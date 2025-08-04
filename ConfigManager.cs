using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace IPConfiger
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configFilePath;
        private List<NetworkConfig> _configs;

        public ConfigManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "IPConfiger");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _configFilePath = Path.Combine(appFolder, "configs.json");
            _configs = new List<NetworkConfig>();
            LoadConfigs();
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public List<NetworkConfig> GetConfigs()
        {
            return _configs.ToList();
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        public void AddConfig(NetworkConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Name))
                throw new ArgumentException("配置名称不能为空");

            if (_configs.Any(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("配置名称已存在");

            config.CreatedTime = DateTime.Now;
            _configs.Add(config);
            SaveConfigs();
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public void UpdateConfig(NetworkConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var existingConfig = _configs.FirstOrDefault(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
            if (existingConfig == null)
                throw new ArgumentException("配置不存在");

            var index = _configs.IndexOf(existingConfig);
            config.CreatedTime = existingConfig.CreatedTime;
            _configs[index] = config;
            SaveConfigs();
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        public void DeleteConfig(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
                throw new ArgumentException("配置名称不能为空");

            var config = _configs.FirstOrDefault(c => c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase));
            if (config == null)
                throw new ArgumentException("配置不存在");

            _configs.Remove(config);
            SaveConfigs();
        }

        /// <summary>
        /// 获取指定配置
        /// </summary>
        public NetworkConfig? GetConfig(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
                return null;

            return _configs.FirstOrDefault(c => c.Name.Equals(configName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取指定适配器的配置
        /// </summary>
        public List<NetworkConfig> GetConfigsForAdapter(string adapterName)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                return new List<NetworkConfig>();

            return _configs.Where(c => c.AdapterName.Equals(adapterName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfigs()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var configs = JsonConvert.DeserializeObject<List<NetworkConfig>>(json);
                    _configs = configs ?? new List<NetworkConfig>();
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，使用空配置列表
                _configs = new List<NetworkConfig>();
                // 可以记录日志或显示警告
                System.Diagnostics.Debug.WriteLine($"加载配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void SaveConfigs()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_configs, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        public void ExportConfigs(string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(_configs, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"导出配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        public void ImportConfigs(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("配置文件不存在");

                var json = File.ReadAllText(filePath);
                var importedConfigs = JsonConvert.DeserializeObject<List<NetworkConfig>>(json);
                
                if (importedConfigs != null)
                {
                    foreach (var config in importedConfigs)
                    {
                        // 检查是否存在同名配置
                        var existingConfig = _configs.FirstOrDefault(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
                        if (existingConfig != null)
                        {
                            // 如果存在，添加后缀
                            var counter = 1;
                            var originalName = config.Name;
                            while (_configs.Any(c => c.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                config.Name = $"{originalName} ({counter})";
                                counter++;
                            }
                        }
                        
                        config.CreatedTime = DateTime.Now;
                        _configs.Add(config);
                    }
                    
                    SaveConfigs();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"导入配置失败: {ex.Message}");
            }
        }
    }
}