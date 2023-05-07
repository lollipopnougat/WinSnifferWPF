using System;
using System.Linq;

namespace WinSnifferWPF.Model
{
    /// <summary>
    /// 自定义的数据包格式
    /// </summary>
    [Serializable]
    public class PacketItem
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// 源地址(IP/物理)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 目的地址(IP/物理)
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 数据包的说明信息
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 数据包的时间信息
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 数据包原始数据
        /// </summary>
        public byte[] Data { get; set; }
        //public PacketItem ParentPacket { get; set; }

        /// <summary>
        /// 初始化一个PacketItem对象
        /// </summary>
        public PacketItem()
        {

        }

        /// <summary>
        /// 构造一个使用默认初值的PacketItem
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="source">源地址</param>
        /// <param name="destination">目的地址</param>
        /// <param name="length">长度</param>
        /// <param name="info">信息</param>
        /// <param name="time">时间</param>
        public PacketItem(int id, string protocol = "unknown", string source = "", string destination = "", int length = 0, string info = "", DateTime time = default)
        {
            Id = id;
            Protocol = protocol;
            Source = source;
            Destination = destination;
            Length = length;
            Info = info;
            Time = time;
            Data = new byte[length];
            //ParentPacket = null;
        }

        /// <summary>
        /// 返回此PacketItem对象的string表示
        /// </summary>
        /// <returns>PacketItem对象的string形式表示</returns>
        public override string ToString()
        {
            var dataStr = string.Join("", Data.Select(x => x.ToString("x")));
            
            return $"[{Time}]\nProtocol: {Protocol}\nSource: {Source}\nDestination: {Destination}\nLength: {Length}\nInfomation: {Info}\nRawData(HEX): {dataStr}";
        }

    }
}
