using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace IPConfiger.Services
{
    /// <summary>
    /// System.Text.Json序列化器实现
    /// </summary>
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;
        
        public SystemTextJsonSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
        
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        public async Task<string> SerializeAsync<T>(T obj)
        {
            try
            {
                return await Task.Run(() => JsonSerializer.Serialize(obj, _options));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"序列化失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        public async Task<T> DeserializeAsync<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                    return default(T);
                    
                return await Task.Run(() => JsonSerializer.Deserialize<T>(json, _options));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"反序列化失败: {ex.Message}", ex);
            }
        }
    }
}