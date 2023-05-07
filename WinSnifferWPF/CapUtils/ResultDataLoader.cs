using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using WinSnifferWPF.Model;

namespace WinSnifferWPF.CapUtils
{
    /// <summary>
    /// 结果数据加载、保存类
    /// </summary>
    class ResultDataLoader
    {
        /// <summary>
        /// 保存数据包到XML文件
        /// </summary>
        /// <param name="packets">承载数据包的列表</param>
        /// <param name="path">XML文件保存位置</param>
        public static void SavePacketsToFile(List<PacketItem> packets, string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<PacketItem>));
            var sw = new StreamWriter(path, false, new UTF8Encoding(false));
            xmlSerializer.Serialize(sw, packets);
            sw.Close();
        }

        /// <summary>
        /// 从XML文件加载保存的数据包
        /// </summary>
        /// <param name="path">XML文件位置</param>
        /// <returns>承载数据包的列表</returns>
        public static List<PacketItem> GetPacketsFromFile(string path)
        {
            var fs = File.Open(path, FileMode.Open);
            var xmlSerializer = new XmlSerializer(typeof(List<PacketItem>));
            var packets = xmlSerializer.Deserialize(fs) as IEnumerable<PacketItem>;
            return packets.ToList();

        }
    }
}
