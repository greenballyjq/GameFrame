@echo off
chcp 65001 >nul 2>&1
setlocal enabledelayedexpansion

REM patch-wx-sdk.bat — 为目标项目的 WX-WASM-SDK-V2 打补丁（Windows 版）
REM
REM 包含补丁：
REM   1. MiniGameCommon 插件（游戏圈分享、推送动态）
REM   2. 激励视频广告服务端验证回调（adsByUnitId + setServerSideVerificationData）
REM
REM 用法：
REM   patch-wx-sdk.bat -c ..\goosecatcher\GooseClient
REM   patch-wx-sdk.bat -c ..\HuntingClient
REM
REM 全部操作幂等，重复运行安全。

set "SCRIPT_DIR=%~dp0"
set "PATCH_DIR=%SCRIPT_DIR%client\Assets\GameFramework\Platform\WXPatches"

REM ==================== 参数解析 ====================

set "CLIENT_DIR="

:parse_args
if "%~1"=="" goto check_args
if /i "%~1"=="-c" (
    set "CLIENT_DIR=%~2"
    shift
    shift
    goto parse_args
)
shift
goto parse_args

:check_args
if "%CLIENT_DIR%"=="" (
    echo 用法: %~nx0 -c ^<项目客户端根目录^>
    echo.
    echo 示例:
    echo   %~nx0 -c ..\goosecatcher\GooseClient
    echo   %~nx0 -c ..\HuntingClient
    exit /b 1
)

REM 转为绝对路径
pushd "%CLIENT_DIR%" 2>nul
if errorlevel 1 (
    echo [ERROR] 目录不存在: %CLIENT_DIR%
    exit /b 1
)
set "CLIENT_DIR=%CD%"
popd

set "SDK_RUNTIME=%CLIENT_DIR%\client\Assets\WX-WASM-SDK-V2\Runtime"
set "WECHAT_DEFAULT=%SDK_RUNTIME%\wechat-default"

if not exist "%WECHAT_DEFAULT%" (
    echo [ERROR] 未找到 WX-WASM-SDK-V2 目录: %WECHAT_DEFAULT%
    echo         请确认 -c 参数指向正确的项目客户端根目录
    exit /b 1
)

set STEP=0
set STEP_TOTAL=8

echo =========================================
echo  WX SDK 补丁工具
echo =========================================
echo 补丁源:  %PATCH_DIR%
echo 目标SDK: %SDK_RUNTIME%
echo.
echo == Part A: MiniGameCommon 插件（游戏圈） ==
echo.

REM ==================== A1. 拷贝新文件 ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 拷贝 MiniGameCommon 补丁文件...

copy /y "%PATCH_DIR%\new-files\wechat-default\getPlugin.js" "%WECHAT_DEFAULT%\getPlugin.js" >nul
echo   copied: getPlugin.js

if not exist "%WECHAT_DEFAULT%\unity-sdk" mkdir "%WECHAT_DEFAULT%\unity-sdk"
copy /y "%PATCH_DIR%\new-files\wechat-default\unity-sdk\mini-game-common.js" "%WECHAT_DEFAULT%\unity-sdk\mini-game-common.js" >nul
echo   copied: unity-sdk\mini-game-common.js

if not exist "%SDK_RUNTIME%\Plugins" mkdir "%SDK_RUNTIME%\Plugins"
copy /y "%PATCH_DIR%\new-files\Plugins\MiniGameCommon.jslib" "%SDK_RUNTIME%\Plugins\MiniGameCommon.jslib" >nul
echo   copied: Plugins\MiniGameCommon.jslib

echo   [OK] 文件拷贝完成
echo.

REM ==================== A2. 修改 game.json ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 game.json 插件声明...

set "GAME_JSON=%WECHAT_DEFAULT%\game.json"
findstr /c:"MiniGameCommon" "%GAME_JSON%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] MiniGameCommon 已存在，跳过
) else (
    where python3 >nul 2>&1
    if !errorlevel!==0 (
        where python >nul 2>&1
        if !errorlevel!==0 (
            set "PY_CMD=python"
        ) else (
            where node >nul 2>&1
            if !errorlevel!==0 (
                set "PY_CMD="
            )
        )
    ) else (
        set "PY_CMD=python3"
    )

    if defined PY_CMD (
        !PY_CMD! -c "import json,sys;p=sys.argv[1];d=json.load(open(p));d.setdefault('plugins',{})['MiniGameCommon']={'version':'latest','provider':'wxaed5ace05d92b218','contexts':[{'type':'isolatedContext'}]};open(p,'w').write(json.dumps(d,indent=2,ensure_ascii=False)+'\n')" "%GAME_JSON%"
        echo   [OK] MiniGameCommon 插件声明已注入
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];const d=JSON.parse(fs.readFileSync(p,'utf8'));if(!d.plugins)d.plugins={};d.plugins.MiniGameCommon={version:'latest',provider:'wxaed5ace05d92b218',contexts:[{type:'isolatedContext'}]};fs.writeFileSync(p,JSON.stringify(d,null,2)+'\n')" "%GAME_JSON%"
        echo   [OK] MiniGameCommon 插件声明已注入
    )
)
echo.

REM ==================== A3. 修改 game.js ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 game.js 插件引入...

set "GAME_JS=%WECHAT_DEFAULT%\game.js"
findstr /c:"import './getPlugin'" "%GAME_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] getPlugin 引入已存在，跳过
) else (
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];c=open(p).read();c=c.replace(\"import './weapp-adapter';\",\"import './weapp-adapter';\nimport './getPlugin';\");open(p,'w').write(c)" "%GAME_JS%"
        echo   [OK] 已在 weapp-adapter 后插入 import './getPlugin'
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];let c=fs.readFileSync(p,'utf8');c=c.replace(\"import './weapp-adapter';\",\"import './weapp-adapter';\nimport './getPlugin';\");fs.writeFileSync(p,c)" "%GAME_JS%"
        echo   [OK] 已在 weapp-adapter 后插入 import './getPlugin'
    )
)
echo.

REM ==================== A4. 修改 unity-sdk/index.js ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 unity-sdk/index.js 模块注册...

set "INDEX_JS=%WECHAT_DEFAULT%\unity-sdk\index.js"

findstr /c:"import miniGameCommon from './mini-game-common'" "%INDEX_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] miniGameCommon import 已存在，跳过
) else (
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys,re;p=sys.argv[1];lines=open(p).readlines();last=max((i for i,l in enumerate(lines) if l.startswith('import ')),default=-1);lines.insert(last+1,\"import miniGameCommon from './mini-game-common';\n\") if last>=0 else None;open(p,'w').writelines(lines)" "%INDEX_JS%"
        echo   [OK] 已添加 miniGameCommon import
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];const lines=fs.readFileSync(p,'utf8').split('\n');let last=-1;lines.forEach((l,i)=>{if(l.startsWith('import '))last=i});if(last>=0)lines.splice(last+1,0,\"import miniGameCommon from './mini-game-common';\");fs.writeFileSync(p,lines.join('\n'))" "%INDEX_JS%"
        echo   [OK] 已添加 miniGameCommon import
    )
)

findstr /c:"...miniGameCommon" "%INDEX_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] miniGameCommon spread 已存在，跳过
) else (
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];c=open(p).read();c=c.replace('...gyroscope,','...gyroscope,\n    ...miniGameCommon,');open(p,'w').write(c)" "%INDEX_JS%"
        echo   [OK] 已添加 ...miniGameCommon 注册
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];let c=fs.readFileSync(p,'utf8');c=c.replace('...gyroscope,','...gyroscope,\n    ...miniGameCommon,');fs.writeFileSync(p,c)" "%INDEX_JS%"
        echo   [OK] 已添加 ...miniGameCommon 注册
    )
)
echo.

REM ==========================================
echo == Part B: 激励视频广告服务端验证回调 ==
echo.

REM ==================== B1. ad.js — adsByUnitId ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 ad.js — adsByUnitId 映射表...

set "AD_JS=%WECHAT_DEFAULT%\unity-sdk\ad.js"

findstr /c:"adsByUnitId" "%AD_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] adsByUnitId 已存在，跳过
) else (
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];c=open(p).read();c=c.replace('const ads = {};','const ads = {};\nconst adsByUnitId = {};');open(p,'w').write(c)" "%AD_JS%"
        echo   [OK] 已添加 const adsByUnitId = {}
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];let c=fs.readFileSync(p,'utf8');c=c.replace('const ads = {};','const ads = {};\nconst adsByUnitId = {};');fs.writeFileSync(p,c)" "%AD_JS%"
        echo   [OK] 已添加 const adsByUnitId = {}
    )
)

REM ==================== B2. ad.js — WXCreateRewardedVideoAd 注册 ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 ad.js — WXCreateRewardedVideoAd adsByUnitId 注册...

findstr /c:"adsByUnitId[config.adUnitId]" "%AD_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] adsByUnitId 注册已存在，跳过
) else (
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];lines=open(p).read().split('\n');found=False;result=[];inF=False;[result.append(l) or (not found and 'WXCreateRewardedVideoAd' in l and setattr(sys,'_f',True)) or (not found and getattr(sys,'_f',False) and 'ads[key] = ad;' in l and (result.append(l[:len(l)-len(l.lstrip())]+'adsByUnitId[config.adUnitId] = key;'),setattr(sys,'_f',None))) for l in lines];open(p,'w').write('\n'.join(result))" "%AD_JS%" 2>nul
        if !errorlevel! neq 0 (
            !PY! -c "import sys;p=sys.argv[1];lines=open(p).read().split('\n');r=[];f=False;d=False;exec('for l in lines:\n r.append(l)\n if not d:\n  if \"WXCreateRewardedVideoAd\" in l: f=True\n  if f and \"ads[key] = ad;\" in l:\n   ind=l[:len(l)-len(l.lstrip())]\n   r.append(ind+\"adsByUnitId[config.adUnitId] = key;\")\n   d=True');open(p,'w').write('\n'.join(r))" "%AD_JS%"
        )
        echo   [OK] 已在 WXCreateRewardedVideoAd 中添加 adsByUnitId 注册
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];const lines=fs.readFileSync(p,'utf8').split('\n');let inF=false,done=false;const r=[];for(const l of lines){r.push(l);if(!done){if(l.includes('WXCreateRewardedVideoAd'))inF=true;if(inF&&l.includes('ads[key] = ad;')){r.push(l.match(/^(\s*)/)[1]+'adsByUnitId[config.adUnitId] = key;');done=true}}}fs.writeFileSync(p,r.join('\n'))" "%AD_JS%"
        echo   [OK] 已在 WXCreateRewardedVideoAd 中添加 adsByUnitId 注册
    )
)

REM ==================== B3. ad.js — WXSetAdServerSideVerificationData ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 ad.js — WXSetAdServerSideVerificationData 函数...

findstr /c:"WXSetAdServerSideVerificationData" "%AD_JS%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] WXSetAdServerSideVerificationData 已存在，跳过
) else (
    set "PATCH_FILE=%PATCH_DIR%\patches\ad-server-verify.js"
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];pf=sys.argv[2];c=open(p).read();patch=open(pf).read();i=c.rfind('};');c=c[:i]+patch+'\n'+c[i:] if i!=-1 else c;open(p,'w').write(c)" "%AD_JS%" "!PATCH_FILE!"
        echo   [OK] 已添加 WXSetAdServerSideVerificationData 函数
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];const pf=process.argv[2];let c=fs.readFileSync(p,'utf8');const patch=fs.readFileSync(pf,'utf8');const i=c.lastIndexOf('};');if(i!==-1)c=c.slice(0,i)+patch+'\n'+c.slice(i);fs.writeFileSync(p,c)" "%AD_JS%" "!PATCH_FILE!"
        echo   [OK] 已添加 WXSetAdServerSideVerificationData 函数
    )
)
echo.

REM ==================== B4. SDK-Call-JS-Old.jslib ====================

set /a STEP+=1
echo [%STEP%/%STEP_TOTAL%] 检查 SDK-Call-JS-Old.jslib — 桥接函数...

set "JSLIB_OLD=%SDK_RUNTIME%\Plugins\SDK-Call-JS-Old.jslib"

if not exist "%JSLIB_OLD%" (
    echo   [WARN] 未找到 SDK-Call-JS-Old.jslib，跳过
    goto :summary
)

findstr /c:"WXSetAdServerSideVerificationData" "%JSLIB_OLD%" >nul 2>&1
if !errorlevel!==0 (
    echo   [SKIP] WXSetAdServerSideVerificationData 桥接已存在，跳过
) else (
    set "JSLIB_PATCH=%PATCH_DIR%\patches\ad-server-verify.jslib"
    where python3 >nul 2>&1 && set "PY=python3" || (where python >nul 2>&1 && set "PY=python" || set "PY=")
    if defined PY (
        !PY! -c "import sys;p=sys.argv[1];pf=sys.argv[2];c=open(p).read();patch=open(pf).read();m='WXCreateInterstitialAd';i=c.find(m);ls=c.rfind('\n',0,i) if i!=-1 else -1;c=c[:ls+1]+patch+'\n'+c[ls+1:] if ls!=-1 else c;open(p,'w').write(c)" "%JSLIB_OLD%" "!JSLIB_PATCH!"
        echo   [OK] 已在 SDK-Call-JS-Old.jslib 添加桥接函数
    ) else (
        node -e "const fs=require('fs');const p=process.argv[1];const pf=process.argv[2];let c=fs.readFileSync(p,'utf8');const patch=fs.readFileSync(pf,'utf8');const i=c.indexOf('WXCreateInterstitialAd');if(i!==-1){const ls=c.lastIndexOf('\n',i);if(ls!==-1){c=c.slice(0,ls+1)+patch+'\n'+c.slice(ls+1)}}fs.writeFileSync(p,c)" "%JSLIB_OLD%" "!JSLIB_PATCH!"
        echo   [OK] 已在 SDK-Call-JS-Old.jslib 添加桥接函数
    )
)
echo.

REM ==================== 汇总 ====================

:summary
echo =========================================
echo  补丁全部完成！
echo =========================================
echo.
echo Part A — MiniGameCommon 插件（游戏圈）:
echo   getPlugin.js              -^> 插件初始化
echo   mini-game-common.js       -^> JS SDK 模块
echo   MiniGameCommon.jslib       -^> C# / JS 桥接
echo   game.json                  -^> 插件声明
echo   game.js                    -^> import './getPlugin'
echo   unity-sdk/index.js         -^> import + 注册
echo.
echo Part B — 激励视频广告服务端验证:
echo   unity-sdk/ad.js            -^> adsByUnitId + WXSetAdServerSideVerificationData
echo   SDK-Call-JS-Old.jslib       -^> 桥接函数
echo.
echo WXMiniGameCommon.cs 通过 GameFramework symlink 自动可见，无需额外操作。
echo =========================================

endlocal
