using System;
using System.Collections.Generic;

namespace IPConfiger.Services
{
    /// <summary>
    /// 简单的依赖注入容器
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _singletonServices = new();
        private readonly Dictionary<Type, Func<object>> _transientFactories = new();
        
        /// <summary>
        /// 注册单例服务
        /// </summary>
        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _singletonServices[typeof(TInterface)] = new TImplementation();
        }
        
        /// <summary>
        /// 注册单例服务实例
        /// </summary>
        public void RegisterSingleton<TInterface>(TInterface instance)
        {
            _singletonServices[typeof(TInterface)] = instance;
        }
        
        /// <summary>
        /// 注册瞬态服务
        /// </summary>
        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _transientFactories[typeof(TInterface)] = () => new TImplementation();
        }
        
        /// <summary>
        /// 注册瞬态服务工厂
        /// </summary>
        public void RegisterTransient<TInterface>(Func<TInterface> factory)
        {
            _transientFactories[typeof(TInterface)] = () => factory();
        }
        
        /// <summary>
        /// 获取服务实例
        /// </summary>
        public T GetService<T>()
        {
            var serviceType = typeof(T);
            
            // 首先检查单例服务
            if (_singletonServices.TryGetValue(serviceType, out var singletonInstance))
            {
                return (T)singletonInstance;
            }
            
            // 然后检查瞬态服务
            if (_transientFactories.TryGetValue(serviceType, out var factory))
            {
                return (T)factory();
            }
            
            throw new InvalidOperationException($"服务 {serviceType.Name} 未注册");
        }
        
        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        public bool IsRegistered<T>()
        {
            var serviceType = typeof(T);
            return _singletonServices.ContainsKey(serviceType) || _transientFactories.ContainsKey(serviceType);
        }
        
        /// <summary>
        /// 清除所有注册的服务
        /// </summary>
        public void Clear()
        {
            _singletonServices.Clear();
            _transientFactories.Clear();
        }
    }
    
    /// <summary>
    /// 全局服务定位器
    /// </summary>
    public static class ServiceLocator
    {
        private static ServiceContainer _container = new();
        
        /// <summary>
        /// 获取当前容器
        /// </summary>
        public static ServiceContainer Container => _container;
        
        /// <summary>
        /// 设置容器
        /// </summary>
        public static void SetContainer(ServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }
        
        /// <summary>
        /// 获取服务
        /// </summary>
        public static T GetService<T>()
        {
            return _container.GetService<T>();
        }
    }
}