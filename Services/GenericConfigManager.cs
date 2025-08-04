using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IPConfiger.Interfaces;

namespace IPConfiger.Services
{
    /// <summary>
    /// 通用配置管理器
    /// </summary>
    /// <typeparam name="T">配置项类型</typeparam>
    public class GenericConfigManager<T> : IConfigManager<T> where T : IConfigItem
    {
        private readonly string _configFilePath;
        private readonly IJsonSerializer _jsonSerializer;
        private List<T> _configs;
        private readonly object _lockObject = new object();
        
        public GenericConfigManager(string fileName, IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "IPConfiger");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _configFilePath = Path.Combine(appFolder, fileName);
            _configs = new List<T>();
            
            // 异步加载配置
            _ = Task.Run(async () => await LoadConfigsAsync());
        }
        
        /// <summary>
        /// 获取所有配置
        /// </summary>
        public async Task<List<T>> GetConfigsAsync()
        {
            await EnsureConfigsLoadedAsync();
            lock (_lockObject)
            {
                return _configs.ToList();
            }
        }
        
        /// <summary>
        /// 添加配置
        /// </summary>
        public async Task AddConfigAsync(T config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            if (string.IsNullOrWhiteSpace(config.Name))
                throw new ArgumentException("配置名称不能为空", nameof(config));
                
            await EnsureConfigsLoadedAsync();
            
            lock (_lockObject)
            {
                if (_configs.Any(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("配置名称已存在");
                    
                config.CreatedTime = DateTime.Now;
                _configs.Add(config);
            }
            
            await SaveConfigsAsync();
        }
        
        /// <summary>
        /// 更新配置
        /// </summary>
        public async Task UpdateConfigAsync(T config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            await EnsureConfigsLoadedAsync();
            
            lock (_lockObject)
            {
                var existingConfig = _configs.FirstOrDefault(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
                if (existingConfig == null)
                    throw new ArgumentException("配置不存在");
                    
                var index = _configs.IndexOf(existingConfig);
                config.CreatedTime = existingConfig.CreatedTime;
                _configs[index] = config;
            }
            
            await SaveConfigsAsync();
        }
        
        /// <summary>
        /// 删除配置
        /// </summary>
        public async Task DeleteConfigAsync(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
                throw new ArgumentException("配置名称不能为空", nameof(configName));
                
            await EnsureConfigsLoadedAsync();
            
            lock (_lockObject)
            {
                var config = _configs.FirstOrDefault(c => c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase));
                if (config == null)
                    throw new ArgumentException("配置不存在");
                    
                _configs.Remove(config);
            }
            
            await SaveConfigsAsync();
        }
        
        /// <summary>
        /// 获取指定配置
        /// </summary>
        public async Task<T> GetConfigAsync(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
                return default(T);
                
            await EnsureConfigsLoadedAsync();
            
            lock (_lockObject)
            {
                return _configs.FirstOrDefault(c => c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        /// <summary>
        /// 导出配置
        /// </summary>
        public async Task<string> ExportConfigsAsync()
        {
            await EnsureConfigsLoadedAsync();
            
            List<T> configsToExport;
            lock (_lockObject)
            {
                configsToExport = _configs.ToList();
            }
            
            return await _jsonSerializer.SerializeAsync(configsToExport);
        }
        
        /// <summary>
        /// 导入配置
        /// </summary>
        public async Task ImportConfigsAsync(string configData)
        {
            if (string.IsNullOrWhiteSpace(configData))
                throw new ArgumentException("配置数据不能为空", nameof(configData));
                
            var importedConfigs = await _jsonSerializer.DeserializeAsync<List<T>>(configData);
            if (importedConfigs == null || !importedConfigs.Any())
                throw new ArgumentException("无效的配置数据");
                
            await EnsureConfigsLoadedAsync();
            
            lock (_lockObject)
            {
                foreach (var config in importedConfigs)
                {
                    if (config != null && !string.IsNullOrWhiteSpace(config.Name))
                    {
                        // 如果配置已存在，则更新；否则添加
                        var existingIndex = _configs.FindIndex(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
                        if (existingIndex >= 0)
                        {
                            var originalCreatedTime = _configs[existingIndex].CreatedTime;
                            config.CreatedTime = originalCreatedTime;
                            _configs[existingIndex] = config;
                        }
                        else
                        {
                            _configs.Add(config);
                        }
                    }
                }
            }
            
            await SaveConfigsAsync();
        }
        
        /// <summary>
        /// 确保配置已加载
        /// </summary>
        private async Task EnsureConfigsLoadedAsync()
        {
            lock (_lockObject)
            {
                if (_configs.Any())
                    return;
            }
            
            await LoadConfigsAsync();
        }
        
        /// <summary>
        /// 加载配置
        /// </summary>
        private async Task LoadConfigsAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    lock (_lockObject)
                    {
                        _configs = new List<T>();
                    }
                    return;
                }
                
                var json = await File.ReadAllTextAsync(_configFilePath);
                var loadedConfigs = await _jsonSerializer.DeserializeAsync<List<T>>(json);
                
                lock (_lockObject)
                {
                    _configs = loadedConfigs ?? new List<T>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
                lock (_lockObject)
                {
                    _configs = new List<T>();
                }
            }
        }
        
        /// <summary>
        /// 保存配置
        /// </summary>
        private async Task SaveConfigsAsync()
        {
            try
            {
                List<T> configsToSave;
                lock (_lockObject)
                {
                    configsToSave = _configs.ToList();
                }
                
                var json = await _jsonSerializer.SerializeAsync(configsToSave);
                await File.WriteAllTextAsync(_configFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"保存配置失败: {ex.Message}", ex);
            }
        }
    }
}