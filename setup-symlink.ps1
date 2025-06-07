# 检查管理员权限
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Please run this script as Administrator!"
    exit
}

# 显示帮助信息
function Show-Help {
    Write-Host "Usage: $($MyInvocation.MyCommand.Name) [OPTIONS]"
    Write-Host "Create symlink from GameFrame project to game project"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -FrameworkPath <path>   Specify GameFrame framework path (optional)"
    Write-Host "                          (Usually the client\Assets\GameFramework directory in GameFrame repo)"
    Write-Host "                          (Default is client\Assets\GameFramework in the script directory)"
    Write-Host "  -ClientPath <path>      Specify game client project root directory (required)"
    Write-Host "                          (Script will automatically find Assets folder)"
    Write-Host "  -Help                   Show this help information"
    Write-Host ""
    Write-Host "Directory Relationship:"
    Write-Host "  - FrameworkPath: Source directory, where GameFrame framework code is located"
    Write-Host "  - ClientPath: Target directory, script will create GameFramework symlink in the Assets folder"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  $($MyInvocation.MyCommand.Name) -ClientPath ..\MyGame"
    Write-Host "  $($MyInvocation.MyCommand.Name) -ClientPath ..\GooseCatcherClient"
    Write-Host ""
    Write-Host "Note: If no parameters are provided, script will prompt for input interactively"
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
    Write-Host "Please enter game client project directory (script will find Assets folder automatically)"
    Write-Host "Example: ..\GooseCatcherClient"
    $ClientPath = Read-Host "Client project directory"
    
    # 如果用户仍未提供路径，提示错误
    if (-not $ClientPath) {
        Write-Error "Error: Must provide game client path"
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
    Write-Error "Error: Could not find Assets folder in $ClientPath"
    Write-Host "Directory structure should be:"
    Write-Host "- $ClientPath\Assets or"
    Write-Host "- $ClientPath\client\Assets or"
    Write-Host "- $ClientPath\subdirectory\Assets"
    exit 1
}

Write-Host "Using the following paths:"
Write-Host "- Framework Path: $FrameworkPath (auto-configured)"
Write-Host "- Client Project Directory: $ClientPath"
Write-Host "- Assets Directory Found: $CLIENT_ASSETS_PATH"

# 确认操作
$CONFIRM = Read-Host "Continue? (y/n)"
if ($CONFIRM -ne "y") {
    Write-Host "Operation cancelled"
    exit 0
}

# 确保框架源目录存在
if (-NOT (Test-Path -Path $FrameworkPath)) {
    Write-Error "Error: GameFramework source directory does not exist: $FrameworkPath"
    Write-Host "Please check if the directory exists or use -FrameworkPath to specify the correct path"
    exit 1
}

# 确保客户端Assets目录存在
if (-NOT (Test-Path -Path $CLIENT_ASSETS_PATH)) {
    Write-Warning "Game client Assets directory does not exist: $CLIENT_ASSETS_PATH"
    $CREATE_DIR = Read-Host "Create this directory? (y/n)"
    if ($CREATE_DIR -eq "y") {
        New-Item -ItemType Directory -Path $CLIENT_ASSETS_PATH -Force
        Write-Host "Directory created: $CLIENT_ASSETS_PATH"
    }
    else {
        exit 1
    }
}

# 移除现有的 GameFramework 目录（如果存在）
$CLIENT_FRAMEWORK_PATH = Join-Path -Path $CLIENT_ASSETS_PATH -ChildPath "GameFramework"
if (Test-Path -Path $CLIENT_FRAMEWORK_PATH) {
    Write-Host "Removing existing GameFramework directory or link..."
    Remove-Item -Recurse -Force $CLIENT_FRAMEWORK_PATH
}

# 创建符号链接
Write-Host "Creating symbolic link..."
New-Item -ItemType SymbolicLink -Path $CLIENT_FRAMEWORK_PATH -Target $FrameworkPath

Write-Host "Done! Now $CLIENT_FRAMEWORK_PATH points to $FrameworkPath"
Write-Host "When you modify the GameFramework, changes will automatically be reflected in the game client project" 