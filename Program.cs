using System;
using System.Windows.Forms;
using IPConfiger.Services;

namespace IPConfiger
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 初始化依赖注入容器
            ServiceRegistration.InitializeServices();
            
            // 设置全局异常处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => 
                GlobalExceptionHandler.HandleException(e.Exception, "应用程序异常");
            
            Application.Run(new MainForm());
        }
    }
}