#!/bin/bash

# ========= 路径配置 =========
PROTOC_PATH="plugins/protoc-21.12-osx-x86_64/bin/protoc"
PROTO_DIR="./protos"
OUTPUT_DIR="./output"

# 给 protoc 加执行权限（只要没权限就加）
if [ ! -x "$PROTOC_PATH" ]; then
  echo "🔧 授予 protoc 执行权限"
  chmod +x "$PROTOC_PATH"
fi

# macOS 专用：解除安全限制
if [[ "$OSTYPE" == "darwin"* ]]; then
  echo "🔓 解除 macOS 安全限制"
  xattr -d com.apple.quarantine "$PROTOC_PATH" 2>/dev/null || true
fi


# ========= 检查 protoc 是否存在 =========
if [ ! -f "$PROTOC_PATH" ]; then
  echo "❌ protoc 不存在: $PROTOC_PATH"
  echo "请确认 plugins/protoc-21.12-osx-x86_64 解压正确"
  exit 1
fi

# 创建输出目录（如果不存在）
mkdir -p "$OUTPUT_DIR"

# 遍历所有 .proto 文件
for proto_file in "$PROTO_DIR"/*.proto; do
  echo "⏳ 正在处理: $proto_file"
  "$PROTOC_PATH" --csharp_out="$OUTPUT_DIR" --proto_path="$PROTO_DIR" "$proto_file"

  if [ $? -ne 0 ]; then
    echo "❌ 处理失败: $proto_file"
  else
    echo "✅ 生成成功: $(basename "$proto_file")"
  fi
done

echo "🎉 所有 .proto 文件已处理完毕。生成目录: $OUTPUT_DIR"
