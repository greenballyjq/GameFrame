#!/bin/bash

# 显示帮助信息
show_help() {
    echo "用法: $0 [OPTIONS]"
    echo "创建从GameFrame项目到游戏项目的符号链接"
    echo ""
    echo "选项:"
    echo "  -f, --framework-path <path>   指定GameFrame框架项目路径 (可选)"
    echo "                                (通常是GameFrame仓库中的client/Assets/GameFramework目录)"
    echo "                                (默认为脚本所在目录下的client/Assets/GameFramework)"
    echo "  -c, --client-path <path>      指定游戏客户端项目根目录 (必填)"
    echo "                                (脚本会自动查找此目录下的Assets文件夹)"
    echo "  -h, --help                    显示此帮助信息"
    echo ""
    echo "目录关系说明:"
    echo "  - framework-path: 源目录，即GameFrame框架源码所在位置"
    echo "  - client-path: 目标目录，脚本将在此目录下的Assets中创建指向framework-path的GameFramework符号链接"
    echo ""
    echo "示例:"
    echo "  $0 -c ../MyGame"
    echo "  $0 --client-path ../GooseCatcherClient"
    echo ""
    echo "注意: 如果不提供参数，脚本将在运行时交互式提示您输入"
}

# 清理路径函数 - 去除首尾的单引号或双引号
clean_path() {
    local path="$1"
    # 去除首尾的单引号
    path="${path#\'}"
    path="${path%\'}"
    # 去除首尾的双引号
    path="${path#\"}"
    path="${path%\"}"
    echo "$path"
}

# 查找Assets目录的函数
find_assets_directory() {
    local base_dir="$1"
    local assets_dir=""
    
    # 直接检查根目录下是否有Assets
    if [ -d "$base_dir/Assets" ]; then
        assets_dir="$base_dir/Assets"
    # 检查client子目录下是否有Assets
    elif [ -d "$base_dir/client/Assets" ]; then
        assets_dir="$base_dir/client/Assets"
    # 遍历一层子目录查找Assets
    else
        for dir in "$base_dir"/*; do
            if [ -d "$dir" ] && [ -d "$dir/Assets" ]; then
                assets_dir="$dir/Assets"
                break
            fi
        done
    fi
    
    echo "$assets_dir"
}

# 初始化变量
WORKSPACE_DIR="$(pwd)"
CLIENT_PATH=""

# 自动检测框架路径的函数
detect_framework_path() {
    local script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    local possible_paths=(
        "$script_dir/client/Assets/GameFramework"
        "$WORKSPACE_DIR/GameFrame/client/Assets/GameFramework"
        "$script_dir/../GameFrame/client/Assets/GameFramework"
    )
    
    for path in "${possible_paths[@]}"; do
        if [ -d "$path" ]; then
            echo "$path"
            return
        fi
    done
    
    # 如果都找不到，返回默认路径
    echo "$script_dir/client/Assets/GameFramework"
}

# 设置默认框架路径
DEFAULT_FRAMEWORK_PATH="$(detect_framework_path)"
FRAMEWORK_PATH="$DEFAULT_FRAMEWORK_PATH"

# 解析命令行参数
while [[ $# -gt 0 ]]; do
    key="$1"
    case $key in
        -f|--framework-path)
            FRAMEWORK_PATH="$(clean_path "$2")"
            shift # 跳过参数名
            shift # 跳过参数值
            ;;
        -c|--client-path)
            CLIENT_PATH="$(clean_path "$2")"
            shift # 跳过参数名
            shift # 跳过参数值
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "错误: 未知参数 '$key'"
            show_help
            exit 1
            ;;
    esac
done

# 如果未提供客户端路径，交互式提示用户输入
if [ -z "$CLIENT_PATH" ]; then
    echo "请输入游戏客户端项目目录 (脚本会自动查找此目录下的Assets文件夹)"
    echo "示例: ../GooseCatcherClient"
    read -p "客户端项目目录: " CLIENT_PATH
    
    # 清理用户输入的路径
    CLIENT_PATH="$(clean_path "$CLIENT_PATH")"
    
    # 如果用户仍未提供路径，提示错误
    if [ -z "$CLIENT_PATH" ]; then
        echo "错误: 必须提供游戏客户端路径"
        exit 1
    fi
fi

# 如果路径是相对路径，转换为绝对路径
if [[ ! "$FRAMEWORK_PATH" = /* ]]; then
    FRAMEWORK_PATH="$WORKSPACE_DIR/$FRAMEWORK_PATH"
fi

if [[ ! "$CLIENT_PATH" = /* ]]; then
    CLIENT_PATH="$WORKSPACE_DIR/$CLIENT_PATH"
fi

# 自动查找Assets目录
CLIENT_ASSETS_PATH=$(find_assets_directory "$CLIENT_PATH")

if [ -z "$CLIENT_ASSETS_PATH" ]; then
    echo "错误: 在 $CLIENT_PATH 目录下未找到Assets文件夹"
    echo "目录结构应为："
    echo "- $CLIENT_PATH/Assets 或"
    echo "- $CLIENT_PATH/client/Assets 或"
    echo "- $CLIENT_PATH/子目录/Assets"
    exit 1
fi

echo "使用以下路径:"
echo "- 框架路径: $FRAMEWORK_PATH (自动设置)"
echo "- 客户端项目目录: $CLIENT_PATH"
echo "- 找到的Assets目录: $CLIENT_ASSETS_PATH"

# 确认操作
read -p "是否继续? (y/n): " CONFIRM
if [ "$CONFIRM" != "y" ]; then
    echo "操作已取消"
    exit 0
fi

# 确保框架源目录存在
if [ ! -d "$FRAMEWORK_PATH" ]; then
    echo "错误: GameFramework 框架源目录不存在: $FRAMEWORK_PATH"
    echo "请检查目录是否存在，或使用 -f 参数指定正确的框架路径"
    exit 1
fi

# 确保客户端Assets目录存在
if [ ! -d "$CLIENT_ASSETS_PATH" ]; then
    echo "游戏客户端Assets目录不存在: $CLIENT_ASSETS_PATH"
    echo "是否要创建此目录? (y/n)"
    read CREATE_DIR
    if [ "$CREATE_DIR" = "y" ]; then
        mkdir -p "$CLIENT_ASSETS_PATH"
        echo "已创建目录: $CLIENT_ASSETS_PATH"
    else
        exit 1
    fi
fi

# 移除现有的 GameFramework 目录（如果存在）
if [ -d "$CLIENT_ASSETS_PATH/GameFramework" ] || [ -L "$CLIENT_ASSETS_PATH/GameFramework" ]; then
    echo "移除现有的 GameFramework 目录或链接..."
    rm -rf "$CLIENT_ASSETS_PATH/GameFramework"
fi

# 创建符号链接
echo "创建符号链接..."
ln -s "$FRAMEWORK_PATH" "$CLIENT_ASSETS_PATH/GameFramework"

echo "完成！现在 $CLIENT_ASSETS_PATH/GameFramework 指向 $FRAMEWORK_PATH"
echo "当您修改 GameFramework 框架项目时，更改将自动反映在游戏客户端项目中" 