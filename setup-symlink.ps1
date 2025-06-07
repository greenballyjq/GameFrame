# 检查管理员权限
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "请以管理员身份运行此脚本！"
    exit
}

# 显示帮助信息
function Show-Help {
    Write-Host "用法: $($MyInvocation.MyCommand.Name) [OPTIONS]"
    Write-Host "创建从GameFrame项目到游戏项目的符号链接"
    Write-Host ""
    Write-Host "选项:"
    Write-Host "  -FrameworkPath <path>   指定GameFrame框架项目路径 (可选)"
    Write-Host "                          (通常是GameFrame仓库中的client/Assets/GameFramework目录)"
    Write-Host "                          (默认为脚本所在目录下的client\Assets\GameFramework)"
    Write-Host "  -ClientPath <path>      指定游戏客户端项目根目录 (必填)"
    Write-Host "                          (脚本会自动查找此目录下的Assets文件夹)"
    Write-Host "  -Help                   显示此帮助信息"
    Write-Host ""
    Write-Host "目录关系说明:"
    Write-Host "  - FrameworkPath: 源目录，即GameFrame框架源码所在位置"
    Write-Host "  - ClientPath: 目标目录，脚本将在此目录下的Assets中创建指向FrameworkPath的GameFramework符号链接"
    Write-Host ""
    Write-Host "示例:"
    Write-Host "  $($MyInvocation.MyCommand.Name) -ClientPath ..\MyGame"
    Write-Host "  $($MyInvocation.MyCommand.Name) -ClientPath ..\GooseCatcherClient"
    Write-Host ""
    Write-Host "注意: 如果不提供参数，脚本将在运行时交互式提示您输入"
}

# 查找Assets目录的函数
function Find-AssetsDirectory {
    param (
        [string]$BaseDir
    )
    
    $assetsDir = $null
    
    # 直接检查根目录下是否有Assets
    if (Test-Path -Path (Join-Path -Path $BaseDir -ChildPath "Assets")) {
        $assetsDir = Join-Path -Path $BaseDir -ChildPath "Assets"
    }
    # 检查client子目录下是否有Assets
    elseif (Test-Path -Path (Join-Path -Path $BaseDir -ChildPath "client\Assets")) {
        $assetsDir = Join-Path -Path $BaseDir -ChildPath "client\Assets"
    }
    # 遍历一层子目录查找Assets
    else {
        Get-ChildItem -Path $BaseDir -Directory | ForEach-Object {
            $potentialAssetsDir = Join-Path -Path $_.FullName -ChildPath "Assets"
            if (Test-Path -Path $potentialAssetsDir) {
                $assetsDir = $potentialAssetsDir
                return
            }
        }
    }
    
    return $assetsDir
}

# 参数定义
param (
    [Parameter(Mandatory=$false)]
    [string]$FrameworkPath,

    [Parameter(Mandatory=$false)]
    [string]$ClientPath,

    [Parameter(Mandatory=$false)]
    [switch]$Help
)

# 显示帮助
if ($Help) {
    Show-Help
    exit 0
}

# 获取当前工作目录
$WORKSPACE_DIR = (Get-Location).Path

# 设置默认框架路径
$DEFAULT_FRAMEWORK_PATH = Join-Path -Path $WORKSPACE_DIR -ChildPath "client\Assets\GameFramework"
if (-not $FrameworkPath) {
    $FrameworkPath = $DEFAULT_FRAMEWORK_PATH
}

# 如果未提供客户端路径，交互式提示用户输入
if (-not $ClientPath) {
    Write-Host "请输入游戏客户端项目目录 (脚本会自动查找此目录下的Assets文件夹)"
    Write-Host "示例: ..\GooseCatcherClient"
    $ClientPath = Read-Host "客户端项目目录"
    
    # 如果用户仍未提供路径，提示错误
    if (-not $ClientPath) {
        Write-Error "错误: 必须提供游戏客户端路径"
        exit 1
    }
}

# 转换相对路径为绝对路径
if (-not [System.IO.Path]::IsPathRooted($FrameworkPath)) {
    $FrameworkPath = Join-Path -Path $WORKSPACE_DIR -ChildPath $FrameworkPath
}

if (-not [System.IO.Path]::IsPathRooted($ClientPath)) {
    $ClientPath = Join-Path -Path $WORKSPACE_DIR -ChildPath $ClientPath
}

# 自动查找Assets目录
$CLIENT_ASSETS_PATH = Find-AssetsDirectory -BaseDir $ClientPath

if (-not $CLIENT_ASSETS_PATH) {
    Write-Error "错误: 在 $ClientPath 目录下未找到Assets文件夹"
    Write-Host "目录结构应为："
    Write-Host "- $ClientPath\Assets 或"
    Write-Host "- $ClientPath\client\Assets 或"
    Write-Host "- $ClientPath\子目录\Assets"
    exit 1
}

Write-Host "使用以下路径:"
Write-Host "- 框架路径: $FrameworkPath (自动设置)"
Write-Host "- 客户端项目目录: $ClientPath"
Write-Host "- 找到的Assets目录: $CLIENT_ASSETS_PATH"

# 确认操作
$CONFIRM = Read-Host "是否继续? (y/n)"
if ($CONFIRM -ne "y") {
    Write-Host "操作已取消"
    exit 0
}

# 确保框架源目录存在
if (-NOT (Test-Path -Path $FrameworkPath)) {
    Write-Error "错误: GameFramework 框架源目录不存在: $FrameworkPath"
    Write-Host "请检查目录是否存在，或使用 -FrameworkPath 参数指定正确的框架路径"
    exit 1
}

# 确保客户端Assets目录存在
if (-NOT (Test-Path -Path $CLIENT_ASSETS_PATH)) {
    Write-Warning "游戏客户端Assets目录不存在: $CLIENT_ASSETS_PATH"
    $CREATE_DIR = Read-Host "是否要创建此目录? (y/n)"
    if ($CREATE_DIR -eq "y") {
        New-Item -ItemType Directory -Path $CLIENT_ASSETS_PATH -Force
        Write-Host "已创建目录: $CLIENT_ASSETS_PATH"
    }
    else {
        exit 1
    }
}

# 移除现有的 GameFramework 目录（如果存在）
$CLIENT_FRAMEWORK_PATH = Join-Path -Path $CLIENT_ASSETS_PATH -ChildPath "GameFramework"
if (Test-Path -Path $CLIENT_FRAMEWORK_PATH) {
    Write-Host "移除现有的 GameFramework 目录或链接..."
    Remove-Item -Recurse -Force $CLIENT_FRAMEWORK_PATH
}

# 创建符号链接
Write-Host "创建符号链接..."
New-Item -ItemType SymbolicLink -Path $CLIENT_FRAMEWORK_PATH -Target $FrameworkPath

Write-Host "完成！现在 $CLIENT_FRAMEWORK_PATH 指向 $FrameworkPath"
Write-Host "当您修改 GameFramework 框架项目时，更改将自动反映在游戏客户端项目中" 