using System.Threading.Tasks;

namespace IPConfiger.Services
{
    /// <summary>
    /// JSON序列化接口
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        Task<string> SerializeAsync<T>(T obj);
        
        /// <summary>
        /// 反序列化JSON字符串为对象
        /// </summary>
        Task<T> DeserializeAsync<T>(string json);
    }
}