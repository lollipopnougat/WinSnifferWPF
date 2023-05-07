using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using SharpPcap.LibPcap;

namespace WinSnifferWPF.CapUtils
{
    /// <summary>
    /// 网卡设备管理器
    /// </summary>
    class AdapterManager
    {
        /// <summary>
        /// 初始化一个设备管理器实例
        /// </summary>
        public AdapterManager()
        {
            DevicesList = LibPcapLiveDeviceList.Instance.ToList();
        }

        /// <summary>
        /// 网卡设备列表
        /// </summary>
        public List<LibPcapLiveDevice> DevicesList { get; }

        /// <summary>
        /// 网卡设备数量
        /// </summary>
        public int AdapterCount => DevicesList.Count;

        /// <summary>
        /// 获取所有网卡的设备名
        /// </summary>
        /// <returns>包含设备名的List</returns>
        public List<string> GetAllAdapterNames()
        {
            return DevicesList.Select(x => x.Name).ToList();
        }

        /// <summary>
        /// 获取所有设备的友好名称
        /// </summary>
        /// <returns>包含友好名称的List</returns>
        public List<string> GetAllAdapterFriendlyNames()
        {
            return DevicesList.Select(x => string.IsNullOrEmpty(x.Interface.FriendlyName) ? x.Interface.Name : x.Interface.FriendlyName).ToList();
        }

        /// <summary>
        /// 获取所有设备的描述
        /// </summary>
        /// <returns>包含描述的List</returns>
        public List<string> GetAllAdapterDesc()
        {
            return DevicesList.Select(x => string.IsNullOrEmpty(x.Interface.Description) ? x.Interface.Name : x.Interface.Description).ToList();
        }

        /// <summary>
        /// 根据友好名称/或者设备名称获取设备
        /// </summary>
        /// <param name="fname">友好名称或设备名称</param>
        /// <returns>设备对象</returns>
        public LibPcapLiveDevice GetDeviceByFName(string fname)
        {
            foreach(var device in DevicesList)
            {
                if (device.Interface.FriendlyName == fname || device.Interface.Name == fname)
                {
                    return device;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据描述/或者设备名称获取设备
        /// </summary>
        /// <param name="desc">描述或设备名称</param>
        /// <returns>设备对象</returns>
        public LibPcapLiveDevice GetDeviceByDesc(string desc)
        {
            foreach (var device in DevicesList)
            {
                if (device.Interface.Description == desc || device.Interface.Name == desc)
                {
                    return device;
                }
            }
            return null;
        }



    }
}
