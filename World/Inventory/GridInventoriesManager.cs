﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Definitions; // from Sandbox.Game.dll
using Sandbox.Game.Entities; // from Sandbox.Game.dll
using Sandbox.ModAPI;

using VRage; // from VRage.dll and VRage.Library.dll
using VRage.Game; // from VRage.Game.dll
using VRage.Game.Entity; // from VRage.Game.dll
using VRage.Game.ModAPI; // from VRage.Game.dll
using VRage.ModAPI; // from VRage.Game.dll
using VRage.ObjectBuilders;
using VRageMath;

using SEGarden.Logging;

namespace SEGarden.World.Inventory {
    
    /// <summary>
    /// Tracks the counts for a configurable set of items across a 
    /// configurable set of blocks on a given grid.
    /// </summary>
    /// <remarks>
    /// TODO: Filter found inventories by a configurable subtypename
    /// </remarks>
    class GridInventoriesManager {

        private Action<ItemCountsAggregate> ContentsChange;

        public event Action<ItemCountsAggregate> ContentsChanged {
            add { ContentsChange += value; }
            remove { ContentsChange -= value; }
        }

        private void NotifyContentsChanged(ItemCountsAggregate change) {
            if (ContentsChange != null) ContentsChange(change);
        }

        public ItemCountsAggregate Totals;

        readonly Dictionary<MyInventoryBase, ItemCountsAggregate> InventoryTotals;
        readonly List<MyDefinitionId> WatchedItems;
        readonly Logger Log;
        readonly IMyCubeGrid Grid;

        // Would be nice if this was whitelisted!
        //Sandbox.Game.Entities.Inventory.MyInventoryAggregate InventoryAggregate;

        bool SkipNextNotify;

        public GridInventoriesManager(IMyCubeGrid grid, List<MyDefinitionId> watchedItems = null) {
            Grid = grid;
            Log = new Logger("SEGarden.World.Inventory.GridInventoriesManager", (() => Grid.EntityId.ToString()));
            Totals = new ItemCountsAggregate();
            InventoryTotals = new Dictionary<MyInventoryBase, ItemCountsAggregate>();
            //InventoryAggregate = new Sandbox.Game.Entities.Inventory.MyInventoryAggregate();
            WatchedItems = watchedItems; 
        }

        public void Initialize() {
            Grid.OnBlockAdded += BlockAdded;
            Grid.OnBlockRemoved += BlockRemoved;
            //InventoryAggregate.ContentsChanged += OnContentsChanged;
            //InventoryAggregate.BeforeContentsChanged += BeforeContentsChanged;
            
            List<IMySlimBlock> allBlocks = new List<IMySlimBlock>();
            Grid.GetBlocks(allBlocks);
            foreach (var block in allBlocks) {
                BlockAdded(block);
            }            
        }

        public void Terminate() {
            Grid.OnBlockAdded -= BlockAdded;
            Grid.OnBlockRemoved -= BlockRemoved;
            //InventoryAggregate.ContentsChanged -= OnContentsChanged;

            foreach (var inventory in InventoryTotals.Keys) {
                inventory.ContentsChanged -= OnContentsChanged;
            }
        }

        private void BlockAdded(IMySlimBlock slimblock) {
            //Log.Trace(slimblock.ToString() + " added to InventoryManager for " + Grid.DisplayName, "blockAdded");

            if (slimblock == null) {
                Log.Error("Received null slimblock", "blockAdded");
                return;
            }
                
            MyEntity fatEntity = slimblock.FatBlock as MyEntity;
            if (fatEntity == null) return;

            MyInventoryBase inventory;
            if (!fatEntity.TryGetInventory(out inventory)) return;

            Log.Trace("Adding inventory " + slimblock.ToString(), "blockAdded");
            InventoryTotals[inventory] = new ItemCountsAggregate();
            //InventoryAggregate.ChildList.AddComponent(inventory);
            OnContentsChanged(inventory);
            inventory.ContentsChanged += OnContentsChanged;
        }

        private void BlockRemoved(IMySlimBlock slimblock) {
            //Log.Trace(slimblock.ToString() + " removed from InventoryManager for " + Grid.DisplayName, "blockRemoved");

            if (slimblock == null) {
                Log.Error("Received null slimblock", "blockRemoved");
                return;
            }

            MyEntity fatEntity = slimblock.FatBlock as MyEntity;
            if (fatEntity == null)
                return;

            MyInventoryBase inventory;
            if (!fatEntity.TryGetInventory(out inventory))
                return;
            
            Log.Trace("Removing inventory " + slimblock.ToString(), "blockRemoved");
            if (!InventoryTotals.Remove(inventory)) {
                Log.Error("Received an removal for inventory we're not tracking.", "blockRemoved");
                return;
            }

            //InventoryAggregate.ChildList.RemoveComponent(inventory);
            inventory.ContentsChanged -= OnContentsChanged;
        }

        /*
        private void BeforeContentsChanged(MyInventoryBase inventory) {
            Log.Trace("BeforeContentsChanged called on inventory " + inventory.Entity.ToString(), "BeforeContentsChanged");
        }
        */

        private void OnContentsChanged(MyInventoryBase inventory) {
            Log.Trace("Updating inventory cache with inventory " + inventory.Entity.ToString(), "OnContentsChanged");
          
            ItemCountsAggregate cachedCount;
            if (!InventoryTotals.TryGetValue(inventory, out cachedCount)) {
                Log.Error("Received an update for inventory we're not tracking.", "UpdateInventory");
                return;
            }

            ItemCountsAggregate originalCounts = cachedCount.Copy();
            Totals -= originalCounts;

            if (WatchedItems != null) {
                foreach (var id in WatchedItems) {
                    cachedCount.Set(id, inventory.GetItemAmount(id));
                }
            }
            else {
                foreach (var item in inventory.GetItems()) {
                    cachedCount.Set(item.Content.GetObjectId(), item.Amount);
                }
            }

            Totals += cachedCount;

            if (SkipNextNotify) {
                SkipNextNotify = false;
            }
            else {
                NotifyContentsChanged(cachedCount - originalCounts);
            }
  
            //DebugPrint();
        }

        private void DebugPrint() {
            Log.Debug("Displaying inventory contents for grid: " + Grid.DisplayName, "DebugInventories");

            foreach (var kvp in Totals.Counts) {
                Log.Debug("    " + kvp.Key.ToString() + " - " + kvp.Value, "DebugInventories");
            }

            /*
            foreach (var kvp in Counts) {
                Log.Debug("  contents for block: " + kvp.Key.Entity.ToString(), "DebugInventories");

                foreach (var def in WatchedItems) {
                    Log.Debug("    " + def.DisplayNameText + " - " + kvp.Value.Get(def.Id), "DebugInventories");
                }
            }
             * */
        }

        /// <summary>
        /// Consumes a given selection of items from managed inventories
        /// Adjusts the desired removal amount by how much we actually removed
        /// and returns it.
        /// </summary>
        public void Consume(ref ItemCountsAggregate toRemove, long consumerId = 0) {
            VRage.Exceptions.ThrowIf<ArgumentNullException>(toRemove == null, "toRemove");

            Log.Trace("toRemove: " + toRemove.ToString(), "Consume");

            var items = new List<MyDefinitionId>(toRemove.Counts.Keys);

            var inventories = new List<MyInventoryBase>(InventoryTotals.Keys);

            foreach (var item in items) {

                //remainingToRemove -= InventoryAggregate.RemoveItemsOfType(remainingToRemove, item);

                MyFixedPoint remainingToRemove = toRemove.Get(item);
                Log.Trace(String.Format("Looking to remove: {0} of {1}", remainingToRemove, item), "Consume");
          
                foreach (var inventory in inventories) {
                    if (remainingToRemove <= 0) break;
             
                    MyFixedPoint amountAvailable = inventory.GetItemAmount(item);
                    if (amountAvailable <= 0) continue;

                    MyFixedPoint amountRemoved = amountAvailable < remainingToRemove ? 
                        amountAvailable : remainingToRemove;

                    Log.Trace(String.Format("Removing: {0} from {1}", amountRemoved, inventory.Entity.EntityId), "Consume");
                    SkipNextNotify = true;
                    inventory.RemoveItemsOfType(amountRemoved, item);
                    remainingToRemove -= amountRemoved;
                }

                toRemove.Set(item, remainingToRemove);
            }

            Log.Trace(String.Format("Remaining after attempted removals: {0}", toRemove.ToString()), "Consume");
        }

    }

}
