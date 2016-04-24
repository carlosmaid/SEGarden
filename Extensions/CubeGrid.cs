﻿/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VRage.Game;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Components;
using InGame = Sandbox.ModAPI.Ingame;
using Interfaces = Sandbox.ModAPI.Interfaces;

namespace SEGarden.Extensions {

	/// <summary>
	/// Helper functions for SE grids
	/// </summary>
	public static class CubeGrid {

        public enum GridType {
            Unknown,
            Station,
            LargeShip,
            SmallShip,
        }

		/// <summary>
		/// Gets the first available non-empty cargo container on a grid.  
		/// If optional parameters given, first cargo which can fit that volume.
		/// </summary>
		/// <param name="def">Optional: Builder definition</param>
		/// <param name="count">Optional: Number of items</param>
		/// <returns></returns>
		public static InGame.IMyCargoContainer getAvailableCargo(this IMyCubeGrid grid, VRage.ObjectBuilders.SerializableDefinitionId? def = null, int count = 1) {
			List<IMySlimBlock> containers = new List<IMySlimBlock>();
			grid.GetBlocks(containers, x => x.FatBlock != null && x.FatBlock is InGame.IMyCargoContainer);

			if (containers.Count == 0)
				return null;

			if (def == null) {
				// Don't care about fit, just return the first one
				return containers[0].FatBlock as InGame.IMyCargoContainer;
			} else {
				foreach (IMySlimBlock block in containers) {
					InGame.IMyCargoContainer c = block.FatBlock as InGame.IMyCargoContainer;
					Interfaces.IMyInventoryOwner invo = c as Interfaces.IMyInventoryOwner;
					Interfaces.IMyInventory inv = invo.GetInventory(0);

					// TODO: check for fit
					return c;
				}
			}

			return null;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <returns>Type of the grid</returns>
        public static GRIDTYPE getGridType(IMyCubeGrid grid) {
            if (grid.IsStatic) {
                return GRIDTYPE.STATION;
            }
            else {
                if (grid.GridSizeEnum == MyCubeSize.Large)
                    return GRIDTYPE.LARGESHIP;
                else
                    return GRIDTYPE.SMALLSHIP;
            }
        }

        /// <summary>
        /// Gets the hash value for the grid to identify it
        /// (because apparently DisplayName doesn't work)
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static String gridIdentifier(IMyCubeGrid grid) {
            String id = grid.ToString();
            int start = id.IndexOf('{');
            int end = id.IndexOf('}');
            return id.Substring(start + 1, end - start);
        }

		/// <summary>
		/// Checks whether a grid is owned only by one faction
		/// If a single block is owned by another player, returns false
		/// </summary>
		/// <param name="grid">Grid to check</param>
		/// <returns></returns>
		public static bool ownedBySingleFaction(this IMyCubeGrid grid) {
			// No one owns the grid
			if (grid.BigOwners.Count == 0)
				return false;

			// Guaranteed to have at least 1 owner after previous check
			IMyFactionCollection facs = MyAPIGateway.Session.Factions;
			IMyFaction fac = facs.TryGetPlayerFaction(grid.BigOwners[0]);
			
			// Test big owners
			for (int i = 1; i < grid.BigOwners.Count; ++i) {
				IMyFaction newF = facs.TryGetPlayerFaction(grid.BigOwners[i]);
				if (newF != fac)
					return false;
			}

			// Test small owners
			for (int i = 0; i < grid.SmallOwners.Count; ++i) {
				IMyFaction newF = facs.TryGetPlayerFaction(grid.SmallOwners[i]);
				if (newF != fac)
					return false;
			}

			// Didn't encounter any factions different from the BigOwner[0] faction
			return true;
		}
 * 
 * 
        /// <summary>
        /// Get the main cockpit on the grid
        /// </summary>
        /// <param name="grid"></param>
        /// <remarks>Grids should only have 1 main cockpit</remarks>
        /// <returns>The main cockpit as IMyCubeBlock if found, null otherwise</returns>
        public static IMyCubeBlock getMainCockpit(this IMyCubeGrid grid) {
            List<IMySlimBlock> cockpitBlocks = new List<IMySlimBlock>();

            // Get all cockpit blocks
            grid.GetBlocks(cockpitBlocks, (b => b.FatBlock != null && b.FatBlock is InGame.IMyShipController));

            foreach (IMySlimBlock block in cockpitBlocks) {
                if (TerminalPropertyExtensions.GetValueBool(block.FatBlock as IMyTerminalBlock, "MainCockpit")) {
                    return block.FatBlock;
                }
            }
            return null;
        }


	}

}
*/