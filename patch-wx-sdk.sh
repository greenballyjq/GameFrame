#!/bin/bash
#
# patch-wx-sdk.sh — 为目标项目的 WX-WASM-SDK-V2 打补丁
#
# 包含补丁：
#   1. MiniGameCommon 插件（游戏圈分享、推送动态）
#   2. 激励视频广告服务端验证回调（adsByUnitId + setServerSideVerificationData）
#
# 用法：
#   ./patch-wx-sdk.sh -c ../goosecatcher/GooseClient
#   ./patch-wx-sdk.sh -c ../HuntingClient
#
# 全部操作幂等，重复运行安全。

set -euo pipefail

# ==================== 路径计算 ====================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PATCH_DIR="$SCRIPT_DIR/client/Assets/GameFramework/Platform/WXPatches"

usage() {
    echo "用法: $0 -c <项目客户端根目录>"
    echo ""
    echo "示例:"
    echo "  $0 -c ../goosecatcher/GooseClient"
    echo "  $0 -c ../HuntingClient"
    exit 1
}

CLIENT_DIR=""

while getopts "c:" opt; do
    case $opt in
        c) CLIENT_DIR="$OPTARG" ;;
        *) usage ;;
    esac
done

if [ -z "$CLIENT_DIR" ]; then
    usage
fi

# 转为绝对路径
CLIENT_DIR="$(cd "$CLIENT_DIR" && pwd)"

SDK_RUNTIME="$CLIENT_DIR/client/Assets/WX-WASM-SDK-V2/Runtime"
WECHAT_DEFAULT="$SDK_RUNTIME/wechat-default"

if [ ! -d "$WECHAT_DEFAULT" ]; then
    echo "❌ 未找到 WX-WASM-SDK-V2 目录: $WECHAT_DEFAULT"
    echo "   请确认 -c 参数指向正确的项目客户端根目录"
    exit 1
fi

STEP_TOTAL=8
STEP=0
next_step() {
    STEP=$((STEP + 1))
    echo "[$STEP/$STEP_TOTAL] $1"
}

echo "========================================="
echo " WX SDK 补丁工具"
echo "========================================="
echo "补丁源:  $PATCH_DIR"
echo "目标SDK: $SDK_RUNTIME"
echo ""
echo "== Part A: MiniGameCommon 插件（游戏圈） =="
echo ""

# ==================== A1. 拷贝新文件 ====================

next_step "拷贝 MiniGameCommon 补丁文件..."

cp -v "$PATCH_DIR/new-files/wechat-default/getPlugin.js" \
      "$WECHAT_DEFAULT/getPlugin.js"

mkdir -p "$WECHAT_DEFAULT/unity-sdk"
cp -v "$PATCH_DIR/new-files/wechat-default/unity-sdk/mini-game-common.js" \
      "$WECHAT_DEFAULT/unity-sdk/mini-game-common.js"

mkdir -p "$SDK_RUNTIME/Plugins"
cp -v "$PATCH_DIR/new-files/Plugins/MiniGameCommon.jslib" \
      "$SDK_RUNTIME/Plugins/MiniGameCommon.jslib"

echo "  ✅ 文件拷贝完成"
echo ""

# ==================== A2. 修改 game.json ====================

next_step "检查 game.json 插件声明..."

GAME_JSON="$WECHAT_DEFAULT/game.json"

if grep -q '"MiniGameCommon"' "$GAME_JSON"; then
    echo "  ⏭  MiniGameCommon 已存在，跳过"
else
    if command -v python3 &>/dev/null; then
        python3 - "$GAME_JSON" << 'PYEOF'
import json, sys
path = sys.argv[1]
with open(path, "r") as f:
    data = json.load(f)
if "plugins" not in data:
    data["plugins"] = {}
data["plugins"]["MiniGameCommon"] = {
    "version": "latest",
    "provider": "wxaed5ace05d92b218",
    "contexts": [{"type": "isolatedContext"}]
}
with open(path, "w") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
    f.write("\n")
print("  ✅ MiniGameCommon 插件声明已注入")
PYEOF
    elif command -v node &>/dev/null; then
        node -e "
const fs = require('fs');
const path = process.argv[1];
const data = JSON.parse(fs.readFileSync(path, 'utf8'));
if (!data.plugins) data.plugins = {};
data.plugins.MiniGameCommon = {
    version: 'latest',
    provider: 'wxaed5ace05d92b218',
    contexts: [{type: 'isolatedContext'}]
};
fs.writeFileSync(path, JSON.stringify(data, null, 2) + '\n');
console.log('  ✅ MiniGameCommon 插件声明已注入');
" "$GAME_JSON"
    else
        echo "  ⚠️  需要 python3 或 node 来修改 game.json，请手动添加 MiniGameCommon 插件声明"
    fi
fi
echo ""

# ==================== A3. 修改 game.js ====================

next_step "检查 game.js 插件引入..."

GAME_JS="$WECHAT_DEFAULT/game.js"

if grep -q "import './getPlugin'" "$GAME_JS"; then
    echo "  ⏭  getPlugin 引入已存在，跳过"
else
    if grep -q "import './weapp-adapter'" "$GAME_JS"; then
        sed -i.bak "s|import './weapp-adapter';|import './weapp-adapter';\nimport './getPlugin';|" "$GAME_JS"
        rm -f "$GAME_JS.bak"
        echo "  ✅ 已在 weapp-adapter 后插入 import './getPlugin'"
    else
        echo "  ⚠️  未找到 import './weapp-adapter'，请手动添加 import './getPlugin'"
    fi
fi
echo ""

# ==================== A4. 修改 unity-sdk/index.js ====================

next_step "检查 unity-sdk/index.js 模块注册..."

INDEX_JS="$WECHAT_DEFAULT/unity-sdk/index.js"

# 检查 import
if grep -q "import miniGameCommon from './mini-game-common'" "$INDEX_JS"; then
    echo "  ⏭  miniGameCommon import 已存在，跳过"
else
    LAST_IMPORT_LINE=$(grep -n "^import " "$INDEX_JS" | tail -1 | cut -d: -f1)
    if [ -n "$LAST_IMPORT_LINE" ]; then
        sed -i.bak "${LAST_IMPORT_LINE}a\\
import miniGameCommon from './mini-game-common';" "$INDEX_JS"
        rm -f "$INDEX_JS.bak"
        echo "  ✅ 已添加 miniGameCommon import"
    else
        echo "  ⚠️  未找到 import 语句，请手动添加"
    fi
fi

# 检查 spread 注册
if grep -q '\.\.\.miniGameCommon' "$INDEX_JS"; then
    echo "  ⏭  miniGameCommon spread 已存在，跳过"
else
    if grep -q '\.\.\.gyroscope' "$INDEX_JS"; then
        sed -i.bak 's/\.\.\.gyroscope,/...gyroscope,\n    ...miniGameCommon,/' "$INDEX_JS"
        rm -f "$INDEX_JS.bak"
        echo "  ✅ 已添加 ...miniGameCommon 注册"
    else
        echo "  ⚠️  未找到 ...gyroscope，请手动在 WXWASMSDK 对象中添加 ...miniGameCommon"
    fi
fi
echo ""

# ==========================================
echo "== Part B: 激励视频广告服务端验证回调 =="
echo ""

# ==================== B1. 修改 ad.js — adsByUnitId ====================

next_step "检查 ad.js — adsByUnitId 映射表..."

AD_JS="$WECHAT_DEFAULT/unity-sdk/ad.js"

if grep -q 'adsByUnitId' "$AD_JS"; then
    echo "  ⏭  adsByUnitId 已存在，跳过"
else
    # 在 const ads = {}; 后插入 const adsByUnitId = {};
    if grep -q 'const ads = {};' "$AD_JS"; then
        sed -i.bak 's/const ads = {};/const ads = {};\nconst adsByUnitId = {};/' "$AD_JS"
        rm -f "$AD_JS.bak"
        echo "  ✅ 已添加 const adsByUnitId = {}"
    else
        echo "  ⚠️  未找到 'const ads = {};'，请手动添加 const adsByUnitId = {};"
    fi
fi

# ==================== B2. 修改 ad.js — WXCreateRewardedVideoAd 注册 ====================

next_step "检查 ad.js — WXCreateRewardedVideoAd adsByUnitId 注册..."

if grep -q 'adsByUnitId\[config\.adUnitId\]' "$AD_JS"; then
    echo "  ⏭  adsByUnitId 注册已存在，跳过"
else
    # 在 WXCreateRewardedVideoAd 中 ads[key] = ad; 后插入
    if grep -q 'WXCreateRewardedVideoAd' "$AD_JS"; then
        # 用 python/node 精确注入（避免 sed 匹配到其他 ads[key] = ad;）
        if command -v python3 &>/dev/null; then
            python3 - "$AD_JS" << 'PYEOF'
import sys
path = sys.argv[1]
with open(path, "r") as f:
    content = f.read()

# 在 WXCreateRewardedVideoAd 函数体内的 ads[key] = ad; 后插入
marker = "WXCreateRewardedVideoAd"
lines = content.split("\n")
in_func = False
done = False
new_lines = []
for line in lines:
    new_lines.append(line)
    if not done:
        if marker in line:
            in_func = True
        if in_func and "ads[key] = ad;" in line:
            indent = line[:len(line) - len(line.lstrip())]
            new_lines.append(f"{indent}adsByUnitId[config.adUnitId] = key;")
            done = True

if done:
    with open(path, "w") as f:
        f.write("\n".join(new_lines))
    print("  ✅ 已在 WXCreateRewardedVideoAd 中添加 adsByUnitId 注册")
else:
    print("  ⚠️  未找到注入点，请手动添加 adsByUnitId[config.adUnitId] = key;")
PYEOF
        elif command -v node &>/dev/null; then
            node -e "
const fs = require('fs');
const path = process.argv[1];
let content = fs.readFileSync(path, 'utf8');
const lines = content.split('\n');
let inFunc = false, done = false;
const result = [];
for (const line of lines) {
    result.push(line);
    if (!done) {
        if (line.includes('WXCreateRewardedVideoAd')) inFunc = true;
        if (inFunc && line.includes('ads[key] = ad;')) {
            const indent = line.match(/^(\s*)/)[1];
            result.push(indent + 'adsByUnitId[config.adUnitId] = key;');
            done = true;
        }
    }
}
if (done) {
    fs.writeFileSync(path, result.join('\n'));
    console.log('  ✅ 已在 WXCreateRewardedVideoAd 中添加 adsByUnitId 注册');
} else {
    console.log('  ⚠️  未找到注入点，请手动添加');
}
" "$AD_JS"
        else
            echo "  ⚠️  需要 python3 或 node，请手动添加"
        fi
    else
        echo "  ⚠️  未找到 WXCreateRewardedVideoAd，请手动添加"
    fi
fi

# ==================== B3. 修改 ad.js — WXSetAdServerSideVerificationData 函数 ====================

next_step "检查 ad.js — WXSetAdServerSideVerificationData 函数..."

if grep -q 'WXSetAdServerSideVerificationData' "$AD_JS"; then
    echo "  ⏭  WXSetAdServerSideVerificationData 已存在，跳过"
else
    # 在 export default { ... } 的最后一个函数后、闭合 }; 前插入
    PATCH_CONTENT=$(cat "$PATCH_DIR/patches/ad-server-verify.js")
    if command -v python3 &>/dev/null; then
        python3 - "$AD_JS" "$PATCH_CONTENT" << 'PYEOF'
import sys
path = sys.argv[1]
patch = sys.argv[2]
with open(path, "r") as f:
    content = f.read()

# 在最后的 }; 前插入（export default 对象的闭合）
# 找到文件末尾的 };
rindex = content.rfind("};")
if rindex != -1:
    content = content[:rindex] + patch + "\n" + content[rindex:]
    with open(path, "w") as f:
        f.write(content)
    print("  ✅ 已添加 WXSetAdServerSideVerificationData 函数")
else:
    print("  ⚠️  未找到 export default 闭合，请手动添加")
PYEOF
    elif command -v node &>/dev/null; then
        node -e "
const fs = require('fs');
const path = process.argv[1];
const patch = process.argv[2];
let content = fs.readFileSync(path, 'utf8');
const idx = content.lastIndexOf('};');
if (idx !== -1) {
    content = content.slice(0, idx) + patch + '\n' + content.slice(idx);
    fs.writeFileSync(path, content);
    console.log('  ✅ 已添加 WXSetAdServerSideVerificationData 函数');
} else {
    console.log('  ⚠️  未找到 export default 闭合，请手动添加');
}
" "$AD_JS" "$PATCH_CONTENT"
    else
        echo "  ⚠️  需要 python3 或 node，请手动添加"
    fi
fi
echo ""

# ==================== B4. 修改 SDK-Call-JS-Old.jslib — 桥接函数 ====================

next_step "检查 SDK-Call-JS-Old.jslib — 桥接函数..."

JSLIB_OLD="$SDK_RUNTIME/Plugins/SDK-Call-JS-Old.jslib"

if [ ! -f "$JSLIB_OLD" ]; then
    echo "  ⚠️  未找到 SDK-Call-JS-Old.jslib，跳过"
elif grep -q 'WXSetAdServerSideVerificationData' "$JSLIB_OLD"; then
    echo "  ⏭  WXSetAdServerSideVerificationData 桥接已存在，跳过"
else
    # 在 WXCreateInterstitialAd 前插入
    JSLIB_PATCH=$(cat "$PATCH_DIR/patches/ad-server-verify.jslib")
    if command -v python3 &>/dev/null; then
        python3 - "$JSLIB_OLD" "$JSLIB_PATCH" << 'PYEOF'
import sys
path = sys.argv[1]
patch = sys.argv[2]
with open(path, "r") as f:
    content = f.read()

marker = "WXCreateInterstitialAd"
idx = content.find(marker)
if idx != -1:
    # 向前找到这个函数定义行的开头（找前一个换行+空格）
    line_start = content.rfind("\n", 0, idx)
    if line_start != -1:
        content = content[:line_start + 1] + patch + "\n" + content[line_start + 1:]
        with open(path, "w") as f:
            f.write(content)
        print("  ✅ 已在 SDK-Call-JS-Old.jslib 添加桥接函数")
    else:
        print("  ⚠️  未找到注入点，请手动添加")
else:
    print("  ⚠️  未找到 WXCreateInterstitialAd，请手动添加")
PYEOF
    elif command -v node &>/dev/null; then
        node -e "
const fs = require('fs');
const path = process.argv[1];
const patch = process.argv[2];
let content = fs.readFileSync(path, 'utf8');
const marker = 'WXCreateInterstitialAd';
const idx = content.indexOf(marker);
if (idx !== -1) {
    const lineStart = content.lastIndexOf('\n', idx);
    if (lineStart !== -1) {
        content = content.slice(0, lineStart + 1) + patch + '\n' + content.slice(lineStart + 1);
        fs.writeFileSync(path, content);
        console.log('  ✅ 已在 SDK-Call-JS-Old.jslib 添加桥接函数');
    }
} else {
    console.log('  ⚠️  未找到 WXCreateInterstitialAd，请手动添加');
}
" "$JSLIB_OLD" "$JSLIB_PATCH"
    else
        echo "  ⚠️  需要 python3 或 node，请手动添加"
    fi
fi
echo ""

# ==================== 汇总 ====================

echo "========================================="
echo " 补丁全部完成！"
echo "========================================="
echo ""
echo "Part A — MiniGameCommon 插件（游戏圈）:"
echo "  📄 getPlugin.js              → 插件初始化"
echo "  📄 mini-game-common.js       → JS SDK 模块"
echo "  📄 MiniGameCommon.jslib       → C# ↔ JS 桥接"
echo "  📄 game.json                  → 插件声明"
echo "  📄 game.js                    → import './getPlugin'"
echo "  📄 unity-sdk/index.js         → import + 注册"
echo ""
echo "Part B — 激励视频广告服务端验证:"
echo "  📄 unity-sdk/ad.js            → adsByUnitId + WXSetAdServerSideVerificationData"
echo "  📄 SDK-Call-JS-Old.jslib       → 桥接函数"
echo ""
echo "💡 WXMiniGameCommon.cs 通过 GameFramework symlink 自动可见，无需额外操作。"
echo "========================================="
