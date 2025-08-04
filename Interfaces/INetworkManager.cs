using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPConfiger.Interfaces
{
    /// <summary>
    /// 网络管理器接口
    /// </summary>
    public interface INetworkManager
    {
        /// <summary>
        /// 获取所有网络适配器
        /// </summary>
        Task<List<NetworkAdapter>> GetNetworkAdaptersAsync();
        
        /// <summary>
        /// 应用网络配置
        /// </summary>
        Task<bool> ApplyNetworkConfigAsync(NetworkConfig config);
        
        /// <summary>
        /// 获取当前网络配置
        /// </summary>
        Task<NetworkConfig> GetCurrentNetworkConfigAsync(string adapterName);
        
        /// <summary>
        /// 设置DHCP
        /// </summary>
        Task<bool> SetDHCPAsync(string adapterName);
        
        /// <summary>
        /// 设置静态IP
        /// </summary>
        Task<bool> SetStaticIPAsync(NetworkConfig config);
    }
    
    /// <summary>
    /// 代理管理器接口
    /// </summary>
    public interface IProxyManager
    {
        /// <summary>
        /// 应用代理配置
        /// </summary>
        Task<bool> ApplyProxyConfigAsync(ProxyConfig config);
        
        /// <summary>
        /// 获取当前代理配置
        /// </summary>
        Task<ProxyConfig> GetCurrentProxyConfigAsync();
        
        /// <summary>
        /// 禁用代理
        /// </summary>
        Task<bool> DisableProxyAsync();
    }
}