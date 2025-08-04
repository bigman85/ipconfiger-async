using System;
using IPConfiger.Interfaces;

namespace IPConfiger
{
    /// <summary>
    /// 代理服务器类型
    /// </summary>
    public enum ProxyType
    {
        HTTP = 0,
        HTTPS = 1,
        SOCKS4 = 2,
        SOCKS5 = 3
    }

    /// <summary>
    /// 代理服务器配置信息
    /// </summary>
    public class ProxyConfig : IConfigItem
    {
        public string Name { get; set; } = string.Empty;
        public bool UseProxy { get; set; }
        public ProxyType ProxyType { get; set; } = ProxyType.HTTP;
        public string ProxyServer { get; set; } = string.Empty;
        public int ProxyPort { get; set; } = 8080;
        public bool ProxyRequiresAuth { get; set; }
        public string ProxyUsername { get; set; } = string.Empty;
        public string ProxyPassword { get; set; } = string.Empty;
        public string ProxyBypassList { get; set; } = string.Empty;
        public bool ProxyBypassLocal { get; set; } = true;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            if (UseProxy)
            {
                return $"{Name} [{ProxyType}: {ProxyServer}:{ProxyPort}]";
            }
            return $"{Name} [代理已禁用]";
        }
    }
}