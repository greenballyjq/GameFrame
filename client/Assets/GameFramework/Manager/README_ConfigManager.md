# 配置管理器架构说明

## 架构设计原则

### 通用功能框架化
- **框架层**：提供通用的Luban配置加载功能和单例管理
- **业务层**：只需定义配置表名称和具体的配置读取方法

## 框架层 (GameFramework)

### 抽象基类 `BaseConfigManager`
- **位置**: `client/Assets/GameFramework/Manager/ConfigManager.cs`
- **职责**: 提供通用的Luban配置加载功能
- **特性**:
  - 实现 `IManager` 接口，符合框架管理器规范
  - 支持多平台配置加载（Editor、Standalone、Android）
  - 异步初始化配置表
  - 统一的错误处理和日志记录
  - 提供配置表容器 `cfg.Tables _tables`

```csharp
// 框架层提供完整的配置加载功能
public abstract class BaseConfigManager : MonoBehaviour, IManager
{
    protected virtual string ConfigPath => $"{Application.streamingAssetsPath}/Bundle/Raw/Configs/bytes";
    protected abstract List<string> TableNames { get; }
    protected cfg.Tables _tables;
    public bool Initialized { get; protected set; }
    
    // 自动异步初始化
    public virtual void Init() { InitializeAsync(); }
    
    // 通用的多平台配置加载
    public virtual async Task LoadTablesAsync() { ... }
    
    // 等待初始化完成
    public virtual async Task WaitForInitializationAsync() { ... }
}
```

### 泛型基类 `ConfigManager<T>`
- **职责**: 提供单例模式和自动创建机制
- **特性**:
  - 继承自 `BaseConfigManager`
  - 提供类型安全的单例实例管理
  - 自动创建机制：如果场景中没有实例会自动创建
  - 自动处理重复实例的销毁

```csharp
public abstract class ConfigManager<T> : BaseConfigManager where T : ConfigManager<T>
{
    public static T Instance { get; } // 自动创建单例
    protected virtual void Awake() { ... } // 单例管理
}
```

## 业务层实现

### `GooseConfigManager` - 极简实现
- **位置**: `client/Assets/Scripts/GooseCatcher/Utility/GooseConfigManager.cs`
- **职责**: 只需定义配置表名称和提供配置读取方法
- **特性**:
  - 继承自 `ConfigManager<GooseConfigManager>`
  - 只需重写 `TableNames` 属性
  - 提供游戏专用的配置读取方法
  - 自动获得所有框架功能

```csharp
public class GooseConfigManager : ConfigManager<GooseConfigManager>
{
    // 只需定义配置表名称
    protected override List<string> TableNames => new List<string>
    {
        "gooseconfig_tbdifficulty",
        "gooseconfig_tbegg",
        "gooseconfig_tbglobal",
        "gooseconfig_tbgoose",
        "gooseconfig_tbwave",
        "demo_tbitem"
    };

    // 提供业务专用的配置读取方法
    public cfg.GooseConfig.Goose ReadGoose(int id) => _tables?.TbGoose[id];
    public List<cfg.GooseConfig.Goose> ReadGooseList() => _tables?.TbGoose.DataList;
    // ... 其他配置读取方法
}
```

## 使用方法

### 在游戏逻辑中使用
```csharp
public class GameLogic : MonoBehaviour
{
    // 访问游戏配置管理器
    protected GooseConfigManager GameConfigManager => GooseConfigManager.Instance;
    
    protected async void Start()
    {
        // 等待配置初始化完成（框架自动处理）
        await GameConfigManager.WaitForInitializationAsync();
        
        // 读取配置
        var goose = GameConfigManager.ReadGoose(1);
        var gooseList = GameConfigManager.ReadGooseList();
    }
}
```

### 自动创建机制
```csharp
// 无需手动创建，框架自动处理
var manager = GooseConfigManager.Instance; // 自动创建并初始化
```

## 架构优势

### 1. 代码复用性极高
- 所有Luban配置加载逻辑都在框架中
- 业务层只需几行代码就能实现完整的配置管理器
- 多平台适配逻辑统一处理

### 2. 开发效率提升
- 新增配置管理器只需继承基类
- 只需定义配置表名称和读取方法
- 框架自动处理初始化、加载、错误处理

### 3. 维护成本降低
- 配置加载逻辑集中在框架中
- 修改加载逻辑只需改一个地方
- 统一的错误处理和日志记录

### 4. 功能完整性
- 自动单例管理
- 自动创建机制
- 异步初始化
- 多平台支持
- 错误处理
- 状态管理

## 扩展示例

### 创建新的配置管理器
```csharp
namespace MyGame.Config
{
    public class ItemConfigManager : ConfigManager<ItemConfigManager>
    {
        // 只需定义配置表名称
        protected override List<string> TableNames => new List<string>
        {
            "item_tbweapon",
            "item_tbarmor",
            "item_tbconsumable"
        };
        
        // 提供专用的配置读取方法
        public cfg.Item.Weapon ReadWeapon(int id) => _tables?.TbWeapon[id];
        public cfg.Item.Armor ReadArmor(int id) => _tables?.TbArmor[id];
        public cfg.Item.Consumable ReadConsumable(int id) => _tables?.TbConsumable[id];
    }
}
```

### 使用新的配置管理器
```csharp
// 自动创建和初始化
var itemManager = ItemConfigManager.Instance;
await itemManager.WaitForInitializationAsync();

// 读取配置
var weapon = itemManager.ReadWeapon(1001);
var armor = itemManager.ReadArmor(2001);
```

## 对比分析

### 重构前 vs 重构后

| 方面 | 重构前 | 重构后 |
|------|--------|--------|
| 代码行数 | GooseConfigManager: 200+ 行 | GooseConfigManager: 70+ 行 |
| 功能实现 | 每个配置管理器都要实现完整逻辑 | 只需定义配置表名称和读取方法 |
| 维护成本 | 每个管理器都需要维护加载逻辑 | 只需维护框架中的通用逻辑 |
| 开发效率 | 新增管理器需要大量重复代码 | 新增管理器只需几行代码 |
| 错误处理 | 每个管理器独立处理 | 框架统一处理 |
| 平台适配 | 每个管理器独立适配 | 框架统一适配 |

## 文件组织

### 框架文件
- `client/Assets/GameFramework/Manager/ConfigManager.cs` - 通用配置管理功能

### 业务文件
- `client/Assets/Scripts/GooseCatcher/Utility/GooseConfigManager.cs` - 游戏专用配置（极简实现）

### 删除的重复代码
- ✅ 移除了200+行重复的配置加载逻辑
- ✅ 移除了重复的单例管理代码
- ✅ 移除了重复的平台适配代码
- ✅ 移除了重复的错误处理代码

## 总结

这种架构实现了真正的"框架化"：
- **框架提供能力**：完整的配置加载、管理、错误处理功能
- **业务实现逻辑**：只需定义配置表和读取方法
- **代码复用最大化**：所有通用逻辑在框架中复用
- **开发效率最高**：新增配置管理器只需几行代码
- **维护成本最低**：通用逻辑集中维护

这是一个真正意义上的配置管理框架！🎯 