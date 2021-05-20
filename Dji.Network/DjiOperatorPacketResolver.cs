﻿using Dji.Network.Packet;
using Dji.Network.Packet.DjiPackets;
using Dji.Network.Packet.DjiPackets.Base;
using Dji.Network.Packet.Extensions;
using System.Diagnostics;
using System;

namespace Dji.Network
{
    public class DjiOperatorPacketResolver : DjiPacketResolver
    {
        protected override void ProcessNetworkPacket(NetworkPacket networkPacket)
        {
            var packetType = typeof(DjiEmptyPacket);
            byte wifiSize = DjiPacket.GetWifiSize(networkPacket.Payload);

            // we did receive a wifi-packet with cmd data attached.
            // ensure that the delimiter is indeed 0x55 before we declare it a command.
            if (wifiSize < networkPacket.Payload.Length && networkPacket.Payload[wifiSize] == 0x55)
                packetType = typeof(DjiCmdPacket);

            DjiPacket djiPacket = (DjiPacket)Activator.CreateInstance(packetType);

            if (djiPacket.Set(networkPacket.Payload))
            {
                var djiNetworkPacketType = typeof(DjiNetworkPacket<>).MakeGenericType(packetType);
                dynamic djiNetworkPacket = Activator.CreateInstance(djiNetworkPacketType, new object[] { networkPacket, djiPacket });

                Resolve(djiNetworkPacket);
            }
            else
            {
                Trace.TraceError($"Cmd-packet has invalid structure or values: {networkPacket.Payload.ToHexString(false, false)}");
            }
        }
    }
}