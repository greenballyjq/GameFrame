# WX SDK 补丁

为各游戏项目的 WX-WASM-SDK-V2 注入定制功能支持。

## 包含功能

### Part A: MiniGameCommon 插件（游戏圈）
- 分享图片/截图到游戏圈（`shareImageToGameCenter`）
- 推送小游戏动态（`createGameTimeLine` + `pushFeed`）

### Part B: 激励视频广告服务端验证
- `adsByUnitId` 映射表（通过 adUnitId 反查广告实例）
- `WXSetAdServerSideVerificationData` 函数（调用微信原生服务端验证 API）

## 目录结构

```
WXPatches/
├── README.md
├── new-files/                  # 直接拷贝到 WX-WASM-SDK-V2/Runtime/ 的文件
│   ├── wechat-default/
│   │   ├── getPlugin.js        # 插件初始化
│   │   └── unity-sdk/
│   │       └── mini-game-common.js  # JS SDK 模块
│   └── Plugins/
│       └── MiniGameCommon.jslib     # C# ↔ JS 桥接
├── patches/                    # 注入到 SDK 原有文件的代码片段
│   ├── ad-server-verify.js     # ad.js 末尾注入的函数
│   └── ad-server-verify.jslib  # SDK-Call-JS-Old.jslib 注入的桥接
└── cs/
    └── WXMiniGameCommon.cs     # C# 封装（通过 GameFramework symlink 自动到达各项目）
```

## 使用方式

```bash
# 从 GameFrame 根目录运行
./patch-wx-sdk.sh -c ../goosecatcher/GooseClient
./patch-wx-sdk.sh -c ../HuntingClient
```

脚本会自动：

**Part A（游戏圈）：**
1. 拷贝 `getPlugin.js`、`mini-game-common.js`、`MiniGameCommon.jslib`
2. 在 `game.json` 注入 MiniGameCommon 插件声明
3. 在 `game.js` 添加 `import './getPlugin'`
4. 在 `unity-sdk/index.js` 添加 import 和注册

**Part B（广告验证）：**
5. 在 `ad.js` 添加 `adsByUnitId` 映射表
6. 在 `WXCreateRewardedVideoAd` 中注册 adUnitId → key 映射
7. 在 `ad.js` 末尾添加 `WXSetAdServerSideVerificationData` 函数
8. 在 `SDK-Call-JS-Old.jslib` 添加桥接函数

所有操作幂等，重复运行安全。

## SDK 升级后

WX-WASM-SDK-V2 升级后重新运行 `patch-wx-sdk.sh` 即可恢复全部补丁。

## 前置条件

- 微信公众平台已申请 MiniGameCommon 插件权限（AppID: `wxaed5ace05d92b218`）
- 项目已完成 GameFrame 的 symlink 连接
- 广告服务端验证需要基础库版本 >= 3.10.3
