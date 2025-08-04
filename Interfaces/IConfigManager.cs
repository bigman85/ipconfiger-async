using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPConfiger.Interfaces
{
    /// <summary>
    /// 配置管理器接口
    /// </summary>
    /// <typeparam name="T">配置项类型</typeparam>
    public interface IConfigManager<T> where T : IConfigItem
    {
        /// <summary>
        /// 获取所有配置
        /// </summary>
        Task<List<T>> GetConfigsAsync();
        
        /// <summary>
        /// 添加配置
        /// </summary>
        Task AddConfigAsync(T config);
        
        /// <summary>
        /// 更新配置
        /// </summary>
        Task UpdateConfigAsync(T config);
        
        /// <summary>
        /// 删除配置
        /// </summary>
        Task DeleteConfigAsync(string name);
        
        /// <summary>
        /// 获取指定配置
        /// </summary>
        Task<T> GetConfigAsync(string name);
        
        /// <summary>
        /// 导出配置
        /// </summary>
        Task<string> ExportConfigsAsync();
        
        /// <summary>
        /// 导入配置
        /// </summary>
        Task ImportConfigsAsync(string configData);
    }
    
    /// <summary>
    /// 配置项接口
    /// </summary>
    public interface IConfigItem
    {
        string Name { get; set; }
        DateTime CreatedTime { get; set; }
        string Description { get; set; }
    }
}