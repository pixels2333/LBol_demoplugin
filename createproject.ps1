# 定义变量
$foldername="MyFirstPlugin"
$tfm = "net46"  # 替换为目标框架
$unityVersion = "2022.3.60"  # 替换为 Unity 版本
# 运行 dotnet new 命令
dotnet new bepinex5plugin -n $foldername -T $tfm -U $unityVersion