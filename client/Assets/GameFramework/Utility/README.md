# GameFramework.Utility

工具类集合，提供调试日志、性能计时等通用功能。

---

## Log

调试日志封装类，对 Unity 的 `Debug.Log` / `Debug.LogWarning` / `Debug.LogError` 进行封装。**仅在编辑器中生效**，打包发布后相关调用会被编译器移除，不影响运行性能。

同时会将 Info / Warning / Error / Exception 全部写入项目根目录的 `Logs` 文件夹，单次 Play 对应一个时间戳命名的文件（如 `20260225_143052.log`），行格式为 `[HH:mm:ss.fff] [Info] 消息内容`。

### 方法说明

| 方法 | 说明 |
|------|------|
| `Info(object message)` | 输出普通日志 |
| `Info(object message, Object context)` | 输出带上下文的普通日志 |
| `Warning(object message)` | 输出警告日志 |
| `Warning(object message, Object context)` | 输出带上下文的警告日志 |
| `Error(object message)` | 输出错误日志 |
| `Error(object message, Object context)` | 输出带上下文的错误日志 |
| `Exception(Exception exception)` | 输出异常日志 |
| `Exception(Exception exception, Object context)` | 输出带上下文的异常日志 |

### 使用示例

```csharp
using GameFramework.Utility;

// 普通日志
Log.Info("[GameLogic] 开始初始化游戏");
Log.Warning($"[SpeciesSpawner] 物种 {specieId} 配置不存在");
Log.Error("[NetworkManager] 连接超时");

// 带上下文（点击 Unity Console 可定位到对应对象）
Log.Info("[UIGameplay] 界面加载完成", gameObject);
Log.Exception(ex, this);
```

### 日志格式建议

推荐格式：`[ClassName] 描述性消息`，便于在控制台中快速定位来源。

### 文件输出

- **路径**：项目根目录 `Logs` 文件夹（如 `E:/HuntingClient/Logs`）
- **文件名**：`yyyyMMdd_HHmmss.log`，单次 Play 一个文件
- **行格式**：`[HH:mm:ss.fff] [Info] 消息内容`

---

## CodeTimer

代码性能计时类，内部使用 `Stopwatch` 进行高精度计时。配合 `using` 使用，语句块结束时自动停止计时并通过 `Log` 输出结果。

### 方法说明

| 方法 | 说明 |
|------|------|
| `Start(string name, int iterations = 1)` | 开始计时，返回可放入 `using` 的实例 |
| `Dispose()` | 停止计时并输出日志（通常由 `using` 自动调用） |

### 参数说明

- **name**：计时名称，用于日志标识
- **iterations**：代码执行次数，用于计算平均耗时。默认为 1 表示单次执行；若在 `using` 块内循环执行多次，可传入对应次数以得到平均耗时

### 使用示例

**单次执行**

```csharp
using GameFramework.Utility;

using (CodeTimer.Start("初始化测试"))
{
    InitializeGame();
}
// 输出: [CodeTimer] 初始化测试 - 执行了 1 次，耗时 12.34 毫秒
```

**多次执行求平均值**

```csharp
using GameFramework.Utility;

const int count = 1000000;
using (CodeTimer.Start("循环测试", count))
{
    for (int i = 0; i < count; i++)
    {
        DoSomething();
    }
}
// 输出: [CodeTimer] 循环测试 - 执行了 1000000 次，总耗时 12500.00 毫秒，平均耗时 0.0125 毫秒
```

### 注意事项

- `iterations` 需与实际循环次数一致，否则平均耗时将不准确
- 依赖 `Log`，因此仅在编辑器中会输出计时结果
