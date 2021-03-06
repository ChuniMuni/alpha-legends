﻿using System;
using Common.Network.Packets;
using WorldServer.Network;
using WorldServer.Game.Objects;
using WorldServer.Game.Managers;
using Common.Helpers;
using WorldServer.Game.Objects.PlayerExtensions;
using System.Linq;
using WorldServer.Game.Objects.PlayerExtensions.Quests;

namespace WorldServer.Packets.Handlers
{
    public static class MovementHandler
    {
        public static void HandleMovementStatus(ref PacketReader packet, ref WorldManager manager)
        {
            Player c = manager.Character;
            if (c == null)
                return;

            Vector prevLoc = manager.Character.Location;
            long pos = packet.BaseStream.Position; //Store position after header

            manager.Character.TransportID = packet.ReadUInt64();
            manager.Character.Transport = packet.ReadVector();
            manager.Character.TransportOrientation = packet.ReadFloat();
            manager.Character.Location = packet.ReadVector();
            manager.Character.Orientation = packet.ReadFloat();
            manager.Character.Pitch = packet.ReadFloat();
            manager.Character.MovementFlags = packet.ReadUInt32();

            packet.BaseStream.Position = pos;
            PacketWriter movementStatus = new PacketWriter(packet.Opcode);
            movementStatus.WriteUInt64(manager.Character.Guid);
            movementStatus.WriteBytes(packet.ReadBytes((int)(packet.BaseStream.Length - pos)));
            GridManager.Instance.SendSurroundingNotMe(movementStatus, manager.Character);

            if (prevLoc != c.Location)
            {
                GridManager.Instance.UpdateObject(c);
                c.FindObjectsInRange();
            }
        }

        public static void HandleZoneUpdate(ref PacketReader packet, ref WorldManager manager)
        {
            uint zone = packet.ReadUInt32();
            Player c = manager.Character;
            if (c == null)
                return;

            c.Zone = zone;
            c.UpdateSurroundingQuestStatus();

            if (c.Group != null)
                c.Group.SendAllPartyStatus();
        }

        public static void HandleRunSpeedCheat(ref PacketReader packet, ref WorldManager manager)
        {
            Player c = manager.Character;
            if (c != null && !c.IsGM)
            {
                MiscHandler.HandleForceSpeedChange(ref c.Client, 7.0f, true);
            }
        }

        public static void HandleSwimSpeedCheat(ref PacketReader packet, ref WorldManager manager)
        {
            Player c = manager.Character;
            if (c != null && !c.IsGM)
            {
                MiscHandler.HandleForceSpeedChange(ref c.Client, 4.7222223f, false);
            }
        }

        // TODO: NOT Working (press space on a mount while being mounted, the should do something). Maybe it was not implemented client side...
        public static void HandleMountSpecialAnim(ref PacketReader packet, ref WorldManager manager)
        {
            PacketWriter animPacket = new PacketWriter(Opcodes.SMSG_MOUNTSPECIAL_ANIM);
            animPacket.WriteUInt64(manager.Character.Guid);
            GridManager.Instance.SendSurrounding(animPacket, manager.Character);
        }
    }
}
