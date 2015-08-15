﻿using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Library.Utils;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

//using SEGarden.Core;
using SEGarden.Logic.Common;
using SEGarden.Logging;

namespace SEGarden.Messaging {
	/// <summary>
	/// Client side message hooks.  Processed messages coming from the server.
	/// </summary>
	public abstract class MessageHandlerBase {

		private static Logger Log = new Logger("SEGarden.Messaging.MessageHandlerBase");

        //private ushort MessageDomainId;

        public MessageHandlerBase(ushort messageDomainId) {
            //MessageDomainId = messageDomainId;
            SEGarden.GardenGateway.Messages.AddHandler(messageDomainId, this);
        }

        /// <summary>
        /// Inheriting classes need to define their own, since we won't know how
        /// to deserialize their messages due to limitations in msg byte serialization
        /// (See ByteConverterExtenstions)
        /// </summary>
        public abstract void HandleMessage(ushort MessageTypeId, byte[] body, 
            long senderId, RunLocation sourceType);


        /// <summary>
        /// Only SEGarden.MessageManager needs to call this
        /// </summary>
		public void ReceiveBytes(byte[] buffer) {
            Log.Info("Got message of size " + buffer.Length, "ReceiveBytes");

            // Deserialize base message
            MessageContainer container = MessageContainer.FromBytes(buffer);

            // Call internal handler
            HandleMessage(container.MessageDomainId, container.Body, 
                (long)container.SourceId, container.SourceType);
		}

        /*
         * This should hopefully be unneccesary since we changed up factions,
         * but maybe necessary just in case infrastructure doesn't send it right?
        protected bool IntendedForUs(MessageContainer msg) {
            // Is this message even intended for us?
			if (msg.DestType == BaseResponse.DEST_TYPE.FACTION) {
				IMyFaction fac = MyAPIGateway.Session.Factions.TryGetPlayerFaction(
					MyAPIGateway.Session.Player.PlayerID);
				if (fac == null || !msg.Destination.Contains(fac.FactionId)) {
					return; // Message not meant for us
				}
			} else if (msg.DestType == BaseResponse.DEST_TYPE.PLAYER) {
				long localUserId = (long)MyAPIGateway.Session.Player.PlayerID;
				if (!msg.Destination.Contains(localUserId)) {
					return; // Message not meant for us
				}
			}

            return true;
        }
                     * */

	}
}