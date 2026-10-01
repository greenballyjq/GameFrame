# GameFramework

Unity 2022.3 下的运行时模块。它负责框架管理器的启动与销毁、事件、资源加载、对象池、界面、音频和特效。玩法规则放在游戏工程里，通过 `GameFrameworkManager.GetManager` 使用这些模块。

## 功能

- 按固定顺序创建并初始化六个管理器，销毁时按相反顺序释放
- 字符串事件与 `EventKey` 事件，以及按作用域批量退订
- `Resources` 与 YooAsset 本地包两种资源加载
- `GameObject` 对象池
- 按类型打开的界面，带层级、生命周期分组和预制体地址
- 音效槽位与三声部音乐，播放失败通过返回值表示
- 一次性、循环和定时特效

## 环境

在 Unity **2022.3.7f1** 中开发。编译依赖：

| 包 | 版本 |
| --- | --- |
| `com.cysharp.unitask` | Git URL，见工程 `Packages/manifest.json` |
| `com.tuyoogame.yooasset` | 2.3.14 |
| `com.unity.ugui` | 1.0.0 |
| `com.unity.inputsystem` | 1.6.3 |

YooAsset 加载器只初始化本地包：编辑器为模拟模式，播放器为离线内置文件系统，默认包名 `DefaultPackage`。

## 快速开始

1. 在常驻物体上添加 `GameFrameworkManager`。
2. 指定 `GameConfig`：
   - `ResourceLoadMode`：`YooAsset` 或 `Resources`
   - `UISettings`：菜单 **GameFramework / UI Settings**
   - `AudioConfig`：菜单 **GameFramework / Audio / Audio Config**，需要混音器和默认总线
   - `loading`：可选，初始化开始时会被激活
3. 进入播放后，`OnSingletonAwake` 会启动初始化。其它脚本等待完成后再取管理器：

```csharp
using Cysharp.Threading.Tasks;
using GameFramework.Core;

public sealed class Boot : UnityEngine.MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        await GameFrameworkManager.Instance.WaitForInitializationAsync();
    }
}
```

初始化顺序为 `EventManager`、`ResourceManager`、`GameObjectPoolManager`、`UIManager`、`AudioManager`、`EffectManager`。某个 `InitAsync` 抛出异常时，后面的管理器不会初始化，`WaitForInitializationAsync` 也不会完成。

应用级流程继承 `GameAppFlow<T>`。`Start` 会调用 `StartAppAsync`：先等待框架初始化，再调用 `RegisterAppManagers`，然后逐个 `InitAsync`。

```csharp
using GameFramework.Game;

public sealed class MyApp : GameAppFlow<MyApp>
{
    protected override void RegisterAppManagers()
    {
        RegisterAppManager(new MyInputManager());
    }
}
```

`IAppManager` 提供 `InitAsync` 与 `Dispose`。需要每帧逻辑时再实现 `IAppUpdatable`。

## 用法

管理器的取法相同：

```csharp
var ui = GameFrameworkManager.Instance.GetManager<UIManager>();
```

`EventManager` 位于全局命名空间，没有 `GameFramework` 前缀。

### 事件

```csharp
using GameFramework.Core;

public static class GameEvents
{
    public static readonly EventKey<ScoreChanged> Score = new EventKey<ScoreChanged>();
}

public sealed class ScoreChanged : EventArgs
{
    public int Value;
}

var events = GameFrameworkManager.Instance.GetManager<EventManager>();
events.AddListener(GameEvents.Score, OnScore);
events.Trigger(GameEvents.Score, new ScoreChanged { Value = 1 });
```

`EventArgs` 是 `GameFramework.Core.EventArgs`，带有 `Sender`。同名的字符串重载为 `AddListener` / `Trigger` / `RemoveListener`，参数个数为零个、一个或两个。

`PushScope` 之后登记的监听，会在对应的 `PopScope` 时退订。作用域栈为空时添加的监听不会进入这张退订表。

### 资源

```csharp
var resources = GameFrameworkManager.Instance.GetManager<ResourceManager>();
var prefab = await resources.LoadAssetAsync<GameObject>("Prefabs/Enemy");
resources.ReleaseAsset("Prefabs/Enemy");
```

`Resources` 模式会去掉开头的 `Assets/`，并去掉最后一个扩展名，再交给 `Resources.LoadAsync`。YooAsset 模式把传入字符串当作包内地址。同一路径在句柄仍然有效时不会再次向包请求。`ReleaseAsset` 会释放该路径上的句柄。

`LoadSceneAsync` 使用路径中的场景名，调用 `SceneManager.LoadSceneAsync`，不经过 `IResourceLoader`。

### 界面

```csharp
using GameFramework.UI;

[UIForm(UILayer.Popup)]
public sealed class UIShop : UIForm
{
    protected override void OnOpen() { }
    protected override void OnClose() { }
}
```

`UILayer` 为 `Page`、`Hud`、`Fixed`、`Popup`、`Overlay`。未写地址时，预制体路径为 `{UISettings.PrefabRoot}/{类型名}.prefab`。特性里的 `Address` 非空时，使用该字符串本身，不再拼接 `PrefabRoot`。`UILifetime` 为 `App`、`Round`、`Overlay`，默认 `Round`。缺少 `UIFormAttribute` 的界面进入 `Page`，分组为 `Round`。

```csharp
UIShop shop = await ui.OpenAsync<UIShop>();
ui.Hide<UIShop>();
ui.Show<UIShop>();
ui.Close<UIShop>();
ui.Close(UILifetime.Round);
```

带参数的界面继承 `UIForm<TArgs>`，用 `OpenAsync<UIShop, ShopArgs>(args)` 打开，并重写 `OnOpen(TArgs)`。同一具体类型同时只有一个打开实例。实例已经存在时，再次 `OpenAsync` 只保证它处于激活状态，不再次调用 `OnCreate` 和 `OnOpen`。

`UIButton` 在悬停和点击时播放音效。游戏侧实现 `IUIButtonAudioDefaults`，并调用 `UIButton.SetDefaults`。

### 音频

在 **GameFramework / Audio** 下创建 `Audio Bus`、`Sfx Cue` 和 `Music Cue`。音效片段、音量、音高、空间化和溢出策略写在 `SfxCue` 上。音乐片段写在 `MusicCue` 上，播放时循环。

```csharp
using GameFramework.Audio;

var audio = GameFrameworkManager.Instance.GetManager<AudioManager>();
PlayResult result = audio.Play(hitCue, hitPoint);
if (result.TryGetHandle(out PlayHandle handle))
    audio.Stop(handle);

audio.PlayMusic(bgm, 1f);
audio.StopMusic(0.5f);
```

失败写在 `PlayResult.Failure` 里，不抛异常。`PlayHandle.Sequence == 0` 表示无效句柄，对它调用 `Stop` 不会产生效果。音乐使用三个声部，不占用 `AudioConfig.SlotCount` 里的音效槽位。`StopSfx` 不影响音乐，`PauseMusic` 不影响音效。

槽位或单条音效到达上限时，按 `OverflowPolicy` 处理：`RejectNew` 拒绝新播放，`StopOldest` 淘汰优先级数字更大者，优先级相同时淘汰更早开始的实例。配置上的槽位策略为 `RejectNew` 时，单条音效和总线上的 `StopOldest` 不会生效。

### 对象池

```csharp
var pools = GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();
await pools.PrewarmAsync("Prefabs/Bullet", 16);
GameObject bullet = await pools.SpawnAsync("Prefabs/Bullet");
pools.Despawn(bullet);
```

预制体上的组件可以实现 `IPoolItem`。取出后调用 `OnSpawned`，回收前调用 `OnDespawned`。

### 特效

```csharp
using GameFramework.Manager;

var effects = GameFrameworkManager.Instance.GetManager<EffectManager>();
GameObject vfx = effects.PlayOneShot(explosionPrefab, position, rotation);
effects.Stop(vfx, EffectStopMode.Immediate);
```

`PlayLoop` 保持播放，直到 `Stop`。`PlayForDuration` 在给定秒数后按 `EffectStopMode` 停止，默认 `Graceful`。路径重载为 `PlayOneShotAsync`、`PlayLoopAsync` 和 `PlayForDurationAsync`。

## 目录结构

```
Assets/GameFramework
├── Core        单例、管理器接口、事件键、对象池、特效条目
├── Manager     事件、资源、对象池、特效管理器
├── UI          界面管理、表单、按钮、层级和动效
├── Audio       音效、音乐、总线和播放结果
├── Game        应用启动与退出、阶段切换作业
└── Utility     编辑器日志、代码计时
```

`GameFrameworkManager` 在 `Core`。`IResourceLoader` 的命名空间是 `GameFramework.Core`，文件位于 `Manager`。

## 约定

- `GetManager<T>` 的键是注册时的类型参数。六个默认管理器用它们自身的具体类型注册。
- `Update` 在 `IsInitialized` 为真之后，才把 `Time.deltaTime` 传给实现了 `IManagerUpdatable` 的管理器。音频模块内部使用 `Time.unscaledDeltaTime`。
- 资源加载器按路径保留一个句柄，没有整数引用计数。`ReleaseAsset` 一次就会移除该路径。
- `ResourceManager` 在加载器初始化返回 `false` 时写错误日志，框架仍会把整体初始化标为完成。
- 界面首次打开的顺序是登记实例、`OnCreate`、`OnOpen`。首次打开不调用 `OnShow`。`Hide` 之后 `IsOpen` 仍为真。
- `Log` 的公开方法带有 `Conditional("UNITY_EDITOR")`，玩家构建里这些调用会被去掉。
