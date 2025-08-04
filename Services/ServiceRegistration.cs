using IPConfiger.Interfaces;
using IPConfiger.Services;

namespace IPConfiger.Services
{
    /// <summary>
    /// 服务注册类
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// 注册所有服务
        /// </summary>
        public static void RegisterServices(ServiceContainer container)
        {
            // 注册JSON序列化器
            container.RegisterSingleton<IJsonSerializer, SystemTextJsonSerializer>();
            
            // 注册网络管理器
            container.RegisterSingleton<INetworkManager, NetworkManager>();
            
            // 注册代理管理器
            container.RegisterSingleton<IProxyManager, ProxyManager>();
            
            // 注册配置管理器
            container.RegisterSingleton<IConfigManager<NetworkConfig>>(
                new GenericConfigManager<NetworkConfig>("configs.json", container.GetService<IJsonSerializer>()));
                
            container.RegisterSingleton<IConfigManager<ProxyConfig>>(
                new GenericConfigManager<ProxyConfig>("proxy_configs.json", container.GetService<IJsonSerializer>()));
        }
        
        /// <summary>
        /// 初始化服务容器
        /// </summary>
        public static ServiceContainer InitializeServices()
        {
            var container = new ServiceContainer();
            RegisterServices(container);
            ServiceLocator.SetContainer(container);
            return container;
        }
    }
}