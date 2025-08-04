using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPConfiger.Services
{
    /// <summary>
    /// 全局异常处理器
    /// </summary>
    public static class GlobalExceptionHandler
    {
        /// <summary>
        /// 处理异常并显示用户友好的错误信息
        /// </summary>
        public static void HandleException(Exception ex, string operation)
        {
            var message = $"操作失败: {operation}\n错误信息: {ex.Message}";
            
            // 记录详细错误信息到调试输出
            System.Diagnostics.Debug.WriteLine($"[ERROR] {operation}: {ex}");
            
            // 显示用户友好的错误信息
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        /// <summary>
        /// 处理异常并返回详细信息
        /// </summary>
        public static string GetErrorMessage(Exception ex, string operation)
        {
            return $"操作失败: {operation}\n错误信息: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 异步操作帮助类
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// 执行异步操作并处理异常
        /// </summary>
        public static async Task ExecuteWithErrorHandling(Func<Task> operation, string operationName)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                GlobalExceptionHandler.HandleException(ex, operationName);
            }
        }
        
        /// <summary>
        /// 执行异步操作并处理异常，返回结果
        /// </summary>
        public static async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> operation, string operationName, T defaultValue = default(T))
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                GlobalExceptionHandler.HandleException(ex, operationName);
                return defaultValue;
            }
        }
        
        /// <summary>
        /// 执行异步操作并处理异常，不显示错误对话框
        /// </summary>
        public static async Task<(bool Success, T Result)> ExecuteWithErrorHandlingSilent<T>(Func<Task<T>> operation, T defaultValue = default(T))
        {
            try
            {
                var result = await operation();
                return (true, result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Silent operation failed: {ex}");
                return (false, defaultValue);
            }
        }
    }
}