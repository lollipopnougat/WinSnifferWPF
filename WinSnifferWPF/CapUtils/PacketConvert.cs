using System;
using System.Collections.Generic;
using WinSnifferWPF.Model;
using PacketDotNet;

namespace WinSnifferWPF.CapUtils
{
    /// <summary>
    /// 数据包转换类
    /// </summary>
    class PacketConvert
    {
        /// <summary>
        /// 将PacketDotNet的数据包转换为PacketItem
        /// </summary>
        /// <param name="packet">PacketDotNet定义的数据包</param>
        /// <param name="nums">编号</param>
        /// <param name="time">数据包时间</param>
        /// <returns></returns>
        public static PacketItem ParsePacket(Packet packet, int nums, DateTime time = default)
        {
            var item = new PacketItem(nums, length: packet.TotalPacketLength)
            {
                Time = time.ToLocalTime(),
                Protocol = "Ethernetv2"
            };
            var pak = packet;
            while (pak.HasPayloadPacket)
            {
                pak = pak.PayloadPacket;
                if (pak is ArpPacket)
                {
                    var pakArp = pak as ArpPacket;
                    item.Protocol = "ARP";
                    item.Source = pakArp.SenderHardwareAddress.ToString();
                    item.Destination = pakArp.TargetHardwareAddress.ToString();
                    item.Info = GetArpInfo(pakArp);
                    item.Data = pakArp.Bytes;
                } 
                else if (pak is IPPacket)
                {
                    var pakIp = pak as IPPacket;
                    item.Protocol = "IP";
                    if (pakIp.Version == IPVersion.IPv4)
                    {
                        var pakIpv4 = (pak as IPv4Packet);
                        item.Source = pakIpv4.SourceAddress.ToString();
                        item.Destination = pakIpv4.DestinationAddress.ToString();
                        item.Data = pakIpv4.Bytes;
                    }
                    else
                    {
                        var pakIpv6 = pak as IPv6Packet;
                        item.Source = pakIpv6.SourceAddress.ToString();
                        item.Destination = pakIpv6.DestinationAddress.ToString();
                        item.Data = pakIpv6.Bytes;
                    }
                }
                else if (pak is IcmpV4Packet)
                {
                    var pakIcmpV4 = pak as IcmpV4Packet;
                    item.Protocol = "ICMPv4";
                    item.Info = $"{pakIcmpV4.TypeCode}, Seq={pakIcmpV4.Sequence}";
                    item.Data = pakIcmpV4.Bytes;
                }
                else if (pak is IcmpV6Packet)
                {
                    var pakIcmpV6 = pak as IcmpV6Packet;
                    item.Protocol = "ICMPv6";
                    item.Info = $"{pakIcmpV6.Type}, Code={pakIcmpV6.Code}";
                    item.Data = pakIcmpV6.Bytes;
                }
                else if (pak is TcpPacket)
                {
                    var pakTcp = pak as TcpPacket;
                    item.Protocol = "TCP";
                    item.Info = GetTCPInfo(pakTcp);
                    item.Data = pakTcp.Bytes;
                }
                else if (pak is UdpPacket)
                {
                    var pakUdp = pak as UdpPacket;
                    item.Protocol = "UDP";
                    item.Info = GetUDPInfo(pakUdp);
                    item.Data = pakUdp.Bytes;
                }
                else 
                {
                    item.Protocol = pak.GetType().Name.Substring(0, pak.GetType().Name.Length - 6).ToUpper();
                    item.Data = pak.Bytes;
                }
            }
            return item;
        }

        /// <summary>
        /// 获取TCP详细信息
        /// </summary>
        /// <param name="tcp">tcp数据包</param>
        /// <returns>详细信息字符串</returns>
        private static string GetTCPInfo(TcpPacket tcp)
        {
            List<string> symbolList = new List<string>();
            if (tcp.Finished)
            {
                symbolList.Add("FIN");
            }
            if (tcp.NonceSum)
            {
                symbolList.Add("NSM");
            }
            if (tcp.Push)
            {
                symbolList.Add("PSH");
            }
            if (tcp.Synchronize)
            {
                symbolList.Add("SYN");
            }
            if (tcp.Reset)
            {
                symbolList.Add("RST");
            }
            if (tcp.Acknowledgment)
            {
                symbolList.Add("ACK");
            }
            if (tcp.Urgent)
            {
                symbolList.Add("UGN");
            }
            string symbols = '[' + string.Join(",", symbolList) + ']';
            string info = $"{tcp.SourcePort} -> {tcp.DestinationPort}, {symbols} Seq={tcp.SequenceNumber}, Ack={tcp.AcknowledgmentNumber}, Win={tcp.WindowSize}, Len={tcp.PayloadData.Length}";
            return info;
        }

        /// <summary>
        /// 获取UDP数据包详细信息
        /// </summary>
        /// <param name="udp">UDP数据包</param>
        /// <returns>详细信息字符串</returns>
        private static string GetUDPInfo(UdpPacket udp)
        {
            return $"Len={udp.TotalPacketLength}";
        }

        /// <summary>
        /// 获取ARP数据包详细信息
        /// </summary>
        /// <param name="arp">ARP数据包</param>
        /// <returns>详细信息字符串</returns>
        private static string GetArpInfo(ArpPacket arp)
        {

            if (arp.Operation == ArpOperation.Request)
            {
                return $"Who has {arp.TargetProtocolAddress}? Tell {arp.SenderProtocolAddress}";
            }
            else if (arp.Operation == ArpOperation.Response)
            {
                return $"{arp.SenderProtocolAddress} is at {arp.SenderHardwareAddress}";
            }
            else
            {
                return $"Op={arp.Operation}, TarIPAddr={arp.TargetProtocolAddress}, SenIPAddr={arp.SenderProtocolAddress}";
            }
        }


    }
}
