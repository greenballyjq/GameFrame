@echo off
setlocal

REM ========= 路径配置 =========
set PROTOC_PATH=plugins\protoc-21.12-win64\bin\protoc.exe
set PROTO_DIR=protos
set OUTPUT_DIR=output

REM ========= 检查 protoc 是否存在 =========
if not exist "%PROTOC_PATH%" (
    echo ❌ protoc 不存在: %PROTOC_PATH%
    echo 请确认 plugins\protoc-21.12-win64 解压正确。
    exit /b 1
)

REM 创建输出目录（如果不存在）
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
)

REM 遍历 .proto 文件
for %%F in (%PROTO_DIR%\*.proto) do (
    echo ⏳ 正在处理: %%F
    "%PROTOC_PATH%" --csharp_out="%OUTPUT_DIR%" --proto_path="%PROTO_DIR%" "%%F"
    if errorlevel 1 (
        echo ❌ 处理失败: %%F
    ) else (
        echo ✅ 生成成功: %%~nxF
    )
)

echo 🎉 所有 .proto 文件已处理完毕。生成目录: %OUTPUT_DIR%
endlocal
pause
