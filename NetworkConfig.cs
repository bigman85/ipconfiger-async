using System;
using System.Collections.Generic;
using IPConfiger.Interfaces;

namespace IPConfiger
{
    /// <summary>
    /// 网络配置信息
    /// </summary>
    public class NetworkConfig : IConfigItem
    {
        public string Name { get; set; } = string.Empty;
        public string AdapterName { get; set; } = string.Empty;
        public bool IsDHCP { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string SubnetMask { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string PrimaryDNS { get; set; } = string.Empty;
        public string SecondaryDNS { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} ({AdapterName})";
        }
    }

    /// <summary>
    /// 网络适配器信息
    /// </summary>
    public class NetworkAdapter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MACAddress { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string CurrentIP { get; set; } = string.Empty;
        public bool IsDHCPEnabled { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Description}";
        }
    }
}