<#
.SYNOPSIS
    LBoL插件项目创建脚本
.DESCRIPTION
    这个PowerShell脚本用于创建一个新的BepInEx 5插件项目，专门针对LBoL（东方LostWord）游戏
    它会使用dotnet new命令和bepinex5plugin模板来初始化项目结构
.PARAMETER
    无直接参数，脚本内部定义了项目配置变量
.EXAMPLE
    .\createproject.ps1
.NOTES
    执行此脚本前需要确保已安装：
    1. .NET SDK
    2. BepInEx 5插件模板
    3. Unity 2022.3.60 或兼容版本
.LINK
    https://github.com/BepInEx/BepInEx
    https://docs.unity3d.com/Packages/com.unity.template.net/
#>

# ========================================
# 配置变量 - 根据项目需求修改这些参数
# ========================================

# 新创建的插件文件夹名称
# 建议使用有意义的名称，如MyPlugin、SkinMod、NetworkMod等
$foldername = "MyFirstPlugin"

# 目标框架版本 (Target Framework Moniker)
# LBoL通常使用.NET Framework 4.6
# 常用值：net46, net472, net48
$tfm = "net46"

# Unity版本号
# 确保与开发环境的Unity版本一致
# 格式：主版本.次版本.修订号
$unityVersion = "2022.3.60"

# ========================================
# 执行项目创建
# ========================================

Write-Host "开始创建LBoL插件项目..." -ForegroundColor Green
Write-Host "项目名称: $foldername" -ForegroundColor Cyan
Write-Host "目标框架: $tfm" -ForegroundColor Cyan
Write-Host "Unity版本: $unityVersion" -ForegroundColor Cyan
Write-Host ""

# 验证dotnet命令是否可用
try {
    $dotnetVersion = & dotnet --version
    Write-Host "检测到.NET CLI版本: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Error "未找到dotnet命令，请确保已安装.NET SDK"
    exit 1
}

# 构建dotnet new命令的参数
$newCommand = "new bepinex5plugin -n $foldername -T $tfm -U $unityVersion"

Write-Host "执行命令: dotnet $newCommand" -ForegroundColor Yellow

try {
    # 执行项目创建命令
    & dotnet $newCommand

    Write-Host ""
    Write-Host "✅ 项目 '$foldername' 创建成功！" -ForegroundColor Green
    Write-Host ""
    Write-Host "接下来的步骤：" -ForegroundColor Cyan
    Write-Host "1. 进入项目目录: cd $foldername" -ForegroundColor White
    Write-Host "2. 在Visual Studio中打开项目" -ForegroundColor White
    Write-Host "3. 编写插件代码" -ForegroundColor White
    Write-Host "4. 构建项目: dotnet build" -ForegroundColor White
    Write-Host "5. 将生成的DLL文件复制到BepInEx插件目录" -ForegroundColor White
    Write-Host ""
    Write-Host "项目结构预览：" -ForegroundColor Cyan
    Write-Host "├── $foldername/" -ForegroundColor White
    Write-Host "│   ├── $foldername.csproj" -ForegroundColor Gray
    Write-Host "│   ├── Plugin.cs" -ForegroundColor Gray
    Write-Host "│   └── PluginInfo.cs" -ForegroundColor Gray
    Write-Host "└── bin/Debug/ (构建输出目录)" -ForegroundColor Gray
}
catch {
    Write-Error "❌ 项目创建失败：$($_.Exception.Message)"

    # 提供可能的解决方案
    Write-Host ""
    Write-Host "可能的解决方案：" -ForegroundColor Yellow
    Write-Host "1. 确保已安装BepInEx 5插件模板：" -ForegroundColor White
    Write-Host "   dotnet new install BepInEx.Template" -ForegroundColor Gray
    Write-Host "2. 检查Unity版本是否正确" -ForegroundColor White
    Write-Host "3. 确保有足够的磁盘空间" -ForegroundColor White
    Write-Host "4. 检查目标框架版本是否支持" -ForegroundColor White

    exit 1
}

# ========================================
# 项目创建后的建议
# ========================================

Write-Host ""
Write-Host "📚 开发资源：" -ForegroundColor Cyan
Write-Host "- BepInEx文档: https://docs.bepinex.dev/" -ForegroundColor White
Write-Host "- Unity文档: https://docs.unity3d.com/" -ForegroundColor White
Write-Host "- C#编程指南: https://learn.microsoft.com/en-us/dotnet/csharp/" -ForegroundColor White
Write-Host ""
Write-Host "🎮 LBoL开发相关：" -ForegroundColor Cyan
Write-Host "- LBoL MOD开发社区" -ForegroundColor White
Write-Host "- 游戏API文档和示例" -ForegroundColor White
Write-Host "- 其他LBoL MOD项目的参考" -ForegroundColor White