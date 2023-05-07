using System;
using WinSnifferWPF.Model;

namespace WinSnifferWPF.CapUtils
{
    /// <summary>
    /// 数据包添加事件参数
    /// </summary>
    class PacketAddEventArgs: EventArgs
    {
        /// <summary>
        /// 初始化一个数据包添加事件参数实例
        /// </summary>
        /// <param name="item"></param>
        public PacketAddEventArgs(PacketItem item) :base()
        {
            PacketItem = item;
        }
        /// <summary>
        /// 数据包
        /// </summary>
        public PacketItem PacketItem { get; set; }
    }
}
