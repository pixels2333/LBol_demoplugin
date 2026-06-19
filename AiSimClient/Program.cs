using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace AiSimClient;

sealed class Program
{
    // Avalonia 初始化入口，使用经典桌面生命周期。
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = BuildAvaloniaApp();
        // 在框架初始化完成后设置主窗口，避免依赖具体的生命周期重写签名。
        builder.AfterSetup(_ =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();
        });
        builder.StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}