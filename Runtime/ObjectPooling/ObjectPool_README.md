# ObjectPool 系統完整指南

## 概述

ObjectPool 系統提供了高效能的物件重用機制，避免頻繁的實例化和銷毀操作，特別適用於遊戲中大量重複使用的物件，如子彈、敵人、粒子效果等。

系統採用**純模組化設計**，遵循依賴注入原則，避免單例模式，提供高效能、易維護且可擴展的解決方案。

## 推薦使用方式

**✅ 推薦：使用 ObjectPoolModule (SerializeField 依賴注入)**

```csharp
public class BulletSpawner : MonoBehaviour 
{
    [SerializeField] private ObjectPoolModule poolModule;
    [SerializeField] private GameObject bulletPrefab;
    
    void Start() 
    {
        // 預創建物件池
        poolModule.CreatePool(bulletPrefab, 50);
    }
    
    void Fire() 
    {
        GameObject bullet = poolModule.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 自動回收（如果物件有 Poolable 組件）
        // 或手動回收：poolModule.Recycle(bullet);
    }
}
```

**⚠️ 已棄用：ObjectPoolHelper (靜態輔助類)**

ObjectPoolHelper 仍然可用但已標記為過時，建議遷移到 ObjectPoolModule 以獲得更好的可測試性和依賴管理。

## 系統架構

### 新架構（推薦）

```
ObjectPoolModule (SystemModule) 
         ↓ (SerializeField 依賴注入)
Your MonoBehaviour Classes
         ↓ (直接調用 poolModule 方法)
ObjectPoolManager (內部實現)
         ↓
ObjectPool (多個，每個Prefab一個)
         ↓
GameObject (實作 IPoolableObject 介面)
```

### 傳統架構（已棄用）

```
ObjectPoolModule (SystemModule)
         ↓ (注入Manager)
ObjectPoolHelper (靜態工具類) [已棄用]
         ↓ (使用Manager實例)
ObjectPoolManager
         ↓
ObjectPool (多個，每個Prefab一個)
         ↓
GameObject (實作 IPoolableObject 介面)
```

**新架構特點**：
- 純模組化設計，避免隱藏的全域依賴
- SerializeField 依賴注入，依賴關係清晰可見
- 更好的可測試性和可維護性
- 支援多個 ObjectPoolModule 實例

## 🔄 遷移指南：從 ObjectPoolHelper 到 ObjectPoolModule

如果您的專案目前使用 ObjectPoolHelper，建議按照以下步驟遷移到新的模組化架構：

### 步驟 1：創建 ObjectPoolModule

```csharp
// 在專案視窗右鍵 → Create → JFramework → System → ObjectPool
// 或在程式碼中直接使用
[SerializeField] private ObjectPoolModule poolModule;
```

### 步驟 2：替換 API 調用

| 舊方式 (ObjectPoolHelper) | 新方式 (ObjectPoolModule) |
|---------------------------|---------------------------|
| `ObjectPoolHelper.Spawn(prefab, pos, rot)` | `poolModule.Spawn(prefab, pos, rot)` |
| `ObjectPoolHelper.Recycle(obj)` | `poolModule.Recycle(obj)` |
| `ObjectPoolHelper.CreatePool(prefab, size)` | `poolModule.CreatePool(prefab, size)` |
| `ObjectPoolHelper.GetPoolStatus(prefab)` | `poolModule.GetPoolStatus(prefab)` |
| `ObjectPoolHelper.ClearPool(prefab)` | `poolModule.ClearPool(prefab)` |
| `ObjectPoolHelper.ClearAllPools()` | `poolModule.ClearAllPools()` |

### 步驟 3：更新 Poolable 組件

如果您的 Poolable 組件使用 ObjectPoolHelper，建議設定 `poolModule` 引用：

```csharp
// 舊方式：依賴靜態 Helper
public class Bullet : MonoBehaviour
{
    void SelfDestruct()
    {
        ObjectPoolHelper.Recycle(gameObject);  // ⚠️ 已棄用
    }
}

// 新方式：使用注入的 Module
public class Bullet : MonoBehaviour
{
    private ObjectPoolModule poolModule;
    
    void Start()
    {
        // 從 Poolable 組件獲取 poolModule 引用
        var poolable = GetComponent<Poolable>();
        poolModule = poolable.PoolModule;
    }
    
    void SelfDestruct()
    {
        if (poolModule != null)
            poolModule.Recycle(gameObject);  // ✅ 推薦
        else
            Destroy(gameObject);  // 備案
    }
}
```

### 步驟 4：在 Inspector 中指定 Module 引用

確保在所有使用物件池的 MonoBehaviour 中設定 `poolModule` SerializeField 引用，指向您創建的 ObjectPoolModule 資產。

### 遷移優勢

- **🔍 依賴透明**：所有依賴都在 Inspector 中可見
- **🧪 可測試性**：容易進行單元測試和模擬
- **🔧 可維護性**：清晰的模組邊界和職責分離
- **🚀 效能**：避免靜態查找和全域狀態

### 核心組件說明

1. **ObjectPoolModule**: 
   - 繼承自 `SystemModule` 的物件池管理模組
   - 支援 ScriptableObject 創建，可在專案中建立多個配置
   - 使用 Odin Inspector 提供友好的編輯器介面
   - 自動管理池容器生命週期和 DontDestroyOnLoad

2. **ObjectPoolManager**: 
   - 統一管理所有 ObjectPool 實例
   - 提供池狀態查詢和統計功能
   - 負責路由不同 Prefab 到對應的池

3. **ObjectPool**: 
   - 使用 Queue 管理特定 Prefab 的所有實例
   - 自動擴展機制，按需創建新物件
   - 完整的物件生命週期管理

4. **IPoolableObject**: 
   - 定義池化物件必須實作的介面
   - 包含 OnSpawn、OnRecycle 生命週期方法
   - 提供 IsActive 狀態和 OriginalPrefab 參考

5. **Poolable**: 
   - IPoolableObject 的完整實作組件
   - 支援自動回收、Transform/Physics 重置
   - 使用 Odin Inspector 提供直觀的參數配置

6. **ObjectPoolHelper**: 
   - 提供靜態方法的便利工具類
   - 使用 ObjectPoolManager 實例而非 Module，確保在非 JFramework 環境下也能使用
   - 自動創建備用 Manager 和容器
   - 由 ObjectPoolModule 注入 Manager 實例

### 架構設計原則

#### 🎯 模組化設計
- 基於 SystemModule 架構，便於集成到現有系統
- 支援 ScriptableObject 配置，可在專案中建立多套配置
- 清晰的模組邊界和職責分離

#### 🔒 介面導向
- 使用 `IPoolableObject` 介面實現鬆耦合
- 支援自定義池化行為實作
- 提高系統的可測試性和可擴展性

#### ⚡ 效能最佳化
- Queue-based 高效能物件管理
- 預分配機制減少運行時記憶體分配
- 智慧型擴展平衡效能與記憶體使用

#### 🛠️ 開發者友善
- Odin Inspector 整合提供直觀的編輯器體驗
- 豐富的除錯資訊和狀態監控
- 完善的錯誤處理和備案機制

## 快速開始

### 1. 建立 ObjectPoolModule

#### 方式 A：透過 ScriptableObject 建立（推薦）

1. 在專案視窗右鍵 → Create → JFramework → System → ObjectPool
2. 設定 DefaultPoolSize 等參數
3. 在系統管理器中引用並初始化

```csharp
public class SystemManager : MonoBehaviour
{
    [SerializeField] private ObjectPoolModule poolModule;

    void Start()
    {
        poolModule.Initialize(); // 會自動注入 Manager 給 Helper
    }
}
```

### 2. 基本使用（推薦方式）

#### 使用 ObjectPoolModule 進行依賴注入

```csharp
public class BulletManager : MonoBehaviour
{
    [SerializeField] private ObjectPoolModule poolModule;  // 在 Inspector 中拖拽指定
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    
    void Start()
    {
        // 預創建物件池
        poolModule.CreatePool(bulletPrefab, 50);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireBullet();
        }
    }
    
    void FireBullet()
    {
        // 從池中取得物件
        GameObject bullet = poolModule.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 物件會自動回收（如果有 Poolable 組件），或者手動回收：
        // poolModule.Recycle(bullet);
    }
}
```

#### 方式 B：獨立使用 ObjectPoolHelper（已棄用，向後相容）

⚠️ **注意：此方式已棄用，建議使用上面的 ObjectPoolModule 方式**

ObjectPoolHelper 仍可用於向後相容，但會顯示過時警告：

```csharp
// 直接使用，Helper會自動創建備用Manager
GameObject bullet = ObjectPoolHelper.Spawn(bulletPrefab, position, rotation);
ObjectPoolHelper.Recycle(bullet);

// 或者手動創建Manager並注入
var container = new GameObject("MyPoolContainer").transform;
var manager = new ObjectPoolManager(container, 10);
ObjectPoolHelper.SetPoolManager(manager);
```

### 3. 設定可池化的 Prefab

#### 使用內建 Poolable 組件（推薦）

1. 在 Prefab 上添加 `Poolable` 組件
2. 配置參數：
   - **Reset Transform On Spawn**: 是否在生成時重置 Transform
   - **Reset Physics On Spawn**: 是否在生成時重置物理狀態
   - **Auto Recycle Time**: 自動回收時間（0 表示不自動回收）
   - **Pool Module**: 可選的 ObjectPoolModule 引用（推薦設定）

#### 實作自定義 IPoolableObject

```csharp
public class CustomPoolable : MonoBehaviour, IPoolableObject
{
    public bool IsActive { get; private set; }
    public GameObject OriginalPrefab { get; set; }

    public void OnSpawn()
    {
        IsActive = true;
        // 自定義初始化邏輯
        gameObject.SetActive(true);
    }

    public void OnRecycle()
    {
        IsActive = false;
        // 自定義清理邏輯
        gameObject.SetActive(false);
    }
}
```

### 4. 使用物件池

#### 推薦方式：使用 ObjectPoolModule

```csharp
public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private ObjectPoolModule poolModule;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    void Start()
    {
        // 預創建物件池
        poolModule.CreatePool(bulletPrefab, 50);
    }

    void Fire()
    {
        // 生成物件
        GameObject bullet = poolModule.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // 回收物件（通常在子彈腳本中自動處理）
        // poolModule.Recycle(bullet);
    }
    
    void ShowPoolStatus()
    {
        var status = poolModule.GetPoolStatus(bulletPrefab);
        Debug.Log($"子彈池狀態 - 可用: {status.Available}, 總數: {status.Total}");
    }
    
    void ClearBulletPool()
    {
        poolModule.ClearPool(bulletPrefab);
    }
}
```

#### 已棄用方式：使用 ObjectPoolHelper

```csharp
// ⚠️ 已棄用 - 使用 ObjectPoolModule 代替

// 生成物件
GameObject bullet = ObjectPoolHelper.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);

// 回收物件
ObjectPoolHelper.Recycle(bullet);

// 物件自主回收
var poolable = bullet.GetComponent<IPoolableObject>();
if (poolable != null)
{
    ObjectPoolHelper.Recycle(bullet);
}
```

#### 進階操作

```csharp
// 預先建立物件池
ObjectPoolHelper.CreatePool(enemyPrefab, 20);

// 取得池狀態資訊
var poolModule = // 取得您的 ObjectPoolModule 實例
PoolStatus status = poolModule.GetPoolStatus(bulletPrefab);
Debug.Log($"子彈池狀態: {status}"); // 輸出: Available: 5, Total: 10, Active: 5
```

#### 進階使用方式

```csharp
// 預先建立池
ObjectPoolHelper.CreatePool(bulletPrefab, 50);

// 檢查池狀態
var (available, total) = ObjectPoolHelper.GetPoolStatus(bulletPrefab);

// 清理池
ObjectPoolHelper.ClearPool(bulletPrefab);
```

## 自定義池化物件

### 繼承 Poolable 類別

```csharp
public class PooledBullet : Poolable
{
    [Header("子彈設定")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 1f;

    private Rigidbody rb;

    protected override void OnSpawnCustom()
    {
        // 物件被生成時的自定義邏輯
        if (rb == null) rb = GetComponent<Rigidbody>();

        // 設定隨機方向和速度
        Vector3 direction = Random.insideUnitSphere.normalized;
        rb.velocity = direction * speed;
    }

    protected override void OnRecycleCustom()
    {
        // 物件被回收時的自定義邏輯
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
```

### 直接實作 IPoolableObject 介面

```csharp
public class CustomPoolableObject : MonoBehaviour, IPoolableObject
{
    public bool IsActive { get; private set; }

    public void OnSpawn()
    {
        IsActive = true;
        // 完全自定義的初始化邏輯
        ResetToDefaultState();
        StartCustomBehavior();
    }

    public void OnRecycle()
    {
        IsActive = false;
        // 完全自定義的清理邏輯
        StopAllCoroutines();
        ClearCustomState();
    }

    public void RecycleSelf()
    {
        if (IsActive)
        {
            ObjectPoolHelper.Recycle(gameObject);
        }
    }
}
```

## 系統內部架構詳解

### ObjectPool 類別的職責

```csharp
// 封裝完整的Spawn流程
public GameObject Get(Vector3 position = default, Quaternion rotation = default, Transform parent = null)
{
    // 1. 從池中取得物件或創建新物件
    // 2. 設定位置、旋轉、父物件
    // 3. 啟用物件
    // 4. 呼叫IPoolableObject.OnSpawn()
}

// 封裝完整的回收流程
public void Recycle(GameObject obj)
{
    // 1. 驗證物件有效性
    // 2. 呼叫IPoolableObject.OnRecycle()
    // 3. 設定父物件為池容器
    // 4. 停用物件
    // 5. 加入可用佇列
}
```

### ObjectPoolModule 類別的職責

```csharp
// 簡化的Get方法 - 委託給ObjectPool處理
public GameObject Get(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
{
    if (!pools.ContainsKey(prefab))
        CreatePool(prefab, defaultPoolSize);

    return pools[prefab].Get(position, rotation, parent);
}

// 自動找到對應池並委託處理
public void Recycle(GameObject pooledObject)
{
    ObjectPool targetPool = FindPoolForObject(pooledObject);
    targetPool?.Recycle(pooledObject);
}
```

## 🚀 核心效能優化：O(1) 物件回收

### 效能問題與解決方案

#### 原始問題

早期版本的 `Recycle()` 方法使用線性搜尋來找到物件對應的池：

```csharp
// 舊方法 - O(n×m) 複雜度
ObjectPool targetPool = null;
foreach (var pool in pools.Values)  // O(n) - 遍歷所有池
{
    if (pool.Contains(pooledObject))  // O(m) - 遍歷池中所有物件
    {
        targetPool = pool;
        break;
    }
}
```

**效能問題**：

- 時間複雜度：O(n×m)，其中 n 是池數量，m 是每個池的物件數量
- 隨著池數量增加，效能會線性降低
- 在有大量不同類型物件的遊戲中會造成明顯延遲

#### 優化解決方案

通過在 `IPoolableObject` 介面中添加 `OriginalPrefab` 屬性來實現 O(1) 查找：

```csharp
public interface IPoolableObject
{
    void OnSpawn();
    void OnRecycle();
    bool IsActive { get; }

    // 核心優化：直接存放原始Prefab參考
    GameObject OriginalPrefab { get; set; }
}
```

**優化後的回收方法**：

```csharp
// 新方法 - O(1) 複雜度
public void Recycle(GameObject pooledObject)
{
    var poolableComponent = pooledObject.GetComponent<IPoolableObject>();
    var originalPrefab = poolableComponent.OriginalPrefab;

    // 直接索引到對應的池 - O(1)
    var targetPool = pools[originalPrefab];
    targetPool.Recycle(pooledObject);
}
```

#### 自動設定機制

系統會在創建物件時自動設定 `OriginalPrefab`：

```csharp
// 在 ObjectPool.CreateNewObject() 中自動處理
private GameObject CreateNewObject()
{
    GameObject obj = Object.Instantiate(prefab, poolContainer);

    // 自動設定原始Prefab參考
    var poolableComponent = obj.GetComponent<IPoolableObject>();
    if (poolableComponent != null)
    {
        poolableComponent.OriginalPrefab = prefab;
    }

    // ... 其他初始化邏輯
}
```

### 效能提升對比

| 池數量 | 舊方法 (最壞情況) | 新方法 | 改進倍數  |
| ------ | ----------------- | ------ | --------- |
| 10     | O(10)             | O(1)   | **10x**   |
| 100    | O(100)            | O(1)   | **100x**  |
| 1000   | O(1000)           | O(1)   | **1000x** |

### 使用者體驗

#### 完全透明的優化

- ✅ **API 完全不變**：現有程式碼無需修改
- ✅ **自動化管理**：`OriginalPrefab` 由系統自動設定和管理
- ✅ **向後相容**：現有使用 `Poolable` 組件的程式碼繼續正常工作

```csharp
// 使用方式完全相同，但效能大幅提升
GameObject obj = ObjectPoolHelper.Spawn(prefab, position, rotation);
ObjectPoolHelper.Recycle(obj);  // 現在是 O(1) 效能！
```

#### 自定義實作注意事項

如果實作自定義的 `IPoolableObject`，需要包含新屬性：

```csharp
public class CustomPoolable : MonoBehaviour, IPoolableObject
{
    public bool IsActive { get; private set; }
    public GameObject OriginalPrefab { get; set; }  // 必須實作此屬性

    public void OnSpawn() { /* ... */ }
    public void OnRecycle() { /* ... */ }
}
```

### 適用場景

此優化特別適合：

- 🎮 有大量不同類型池化物件的遊戲
- ⚡ 頻繁回收物件的系統（如子彈、粒子效果）
- 🏆 對效能要求嚴格的即時遊戲
- 📱 資源受限的移動平台遊戲

## 最佳實務

### 1. 池大小設定

```csharp
// 根據遊戲需求設定合適的池大小
// 太小：頻繁建立新物件，影響效能
// 太大：佔用過多記憶體
poolModule.SetDefaultPoolSize(20); // 根據實際需求調整

// 針對特定物件設定不同的池大小
ObjectPoolHelper.CreatePool(bulletPrefab, 100);  // 子彈需要大量
ObjectPoolHelper.CreatePool(enemyPrefab, 20);    // 敵人需要適中
ObjectPoolHelper.CreatePool(effectPrefab, 50);   // 特效需要較多
```

### 2. 自動回收機制

```csharp
// 設定自動回收時間，避免物件永遠不被回收
poolable.SetAutoRecycleTime(5f); // 5秒後自動回收

// 在物件邏輯中使用條件回收
public class Bullet : Poolable
{
    void Update()
    {
        // 超出邊界自動回收
        if (Vector3.Distance(transform.position, Vector3.zero) > 100f)
        {
            RecycleSelf();
        }
    }
}
```

### 3. 預熱策略

```csharp
// 在關鍵時刻前預先建立池，避免遊戲過程中的延遲
void PrewarmPools()
{
    // 遊戲開始前預熱
    ObjectPoolHelper.CreatePool(bulletPrefab, 100);
    ObjectPoolHelper.CreatePool(enemyPrefab, 50);
    ObjectPoolHelper.CreatePool(effectPrefab, 30);
}

// 分階段預熱避免卡頓
IEnumerator PrewarmPoolsGradually()
{
    ObjectPoolHelper.CreatePool(bulletPrefab, 20);
    yield return null; // 等一幀

    ObjectPoolHelper.CreatePool(enemyPrefab, 20);
    yield return null;

    ObjectPoolHelper.CreatePool(effectPrefab, 20);
}
```

### 4. 記憶體管理

```csharp
// 在適當時機清理不需要的池
void OnLevelComplete()
{
    ObjectPoolHelper.ClearPool(enemyPrefab);
    ObjectPoolHelper.ClearPool(bulletPrefab);
    // 保留常用的效果池
}

// 動態調整池大小
void OptimizePoolSizes()
{
    var status = ObjectPoolHelper.GetPoolStatus(bulletPrefab);
    if (status.available > status.total * 0.8f) // 80%都是可用的
    {
        // 考慮縮小池
        poolModule.GetPool(bulletPrefab)?.Shrink(status.total / 2);
    }
}
```

## 效能監控與除錯

### 1. 池狀態監控

```csharp
// 定期檢查池狀態
void MonitorPoolHealth()
{
    foreach (var prefab in monitoredPrefabs)
    {
        var (available, total) = ObjectPoolHelper.GetPoolStatus(prefab);
        float utilization = (total - available) / (float)total;

        if (utilization > 0.9f) // 使用率超過90%
        {
            Debug.LogWarning($"{prefab.name} pool is almost full! Consider expanding.");
        }
    }
}
```

### 2. Editor 工具

```csharp
#if UNITY_EDITOR
[Button("顯示所有池狀態")]
private void ShowAllPoolStatus()
{
    // ObjectPoolModule提供的除錯功能
    poolModule.DebugPoolStatus();
}

[Button("清理所有池")]
private void ClearAllPools()
{
    poolModule.ClearAllPools();
}
#endif
```

### 3. Runtime 除錯

```csharp
// 使用Odin Inspector的RuntimeDebugging功能
[ShowInInspector, ReadOnly]
private Dictionary<GameObject, PoolStatus> poolStatusDisplay;

private void UpdatePoolStatusDisplay()
{
    poolStatusDisplay ??= new Dictionary<GameObject, PoolStatus>();
    poolStatusDisplay.Clear();

    foreach (var prefab in trackedPrefabs)
    {
        poolStatusDisplay[prefab] = poolModule.GetPoolStatus(prefab);
    }
}
```

## 常見問題與解決方案

### Q1: 物件無法正確回收怎麼辦？

**症狀**: 呼叫 `ObjectPoolHelper.Recycle(obj)` 但物件沒有回到池中

**可能原因與解決方案**:

1. **物件沒有 IPoolableObject 組件**
   ```csharp
   // 檢查物件是否有正確的組件
   var poolable = obj.GetComponent<IPoolableObject>();
   if (poolable == null)
   {
       Debug.LogError($"{obj.name} 沒有實作 IPoolableObject 介面！");
   }
   ```

2. **OriginalPrefab 參考遺失**
   ```csharp
   // 檢查 OriginalPrefab 是否正確設定
   if (poolable.OriginalPrefab == null)
   {
       Debug.LogError($"{obj.name} 的 OriginalPrefab 參考遺失！");
   }
   ```

3. **物件已經被回收**
   ```csharp
   // 避免重複回收
   if (!poolable.IsActive)
   {
       Debug.LogWarning($"{obj.name} 已經被回收，避免重複回收！");
       return;
   }
   ```

### Q2: 池物件行為異常怎麼處理？

**症狀**: 從池中取得的物件狀態不正確

**解決方案**:

1. **檢查 OnSpawn/OnRecycle 實作**
   ```csharp
   public class CorrectPoolable : Poolable
   {
       public override void OnSpawn()
       {
           base.OnSpawn(); // 重要：呼叫基類方法
           
           // 完全重置物件狀態
           ResetAllComponents();
           InitializeForNewUse();
       }

       public override void OnRecycle()
       {
           // 清理所有狀態
           CleanupBeforeRecycle();
           
           base.OnRecycle(); // 重要：呼叫基類方法
       }
   }
   ```

2. **確保狀態完全重置**
   ```csharp
   private void ResetAllComponents()
   {
       // 重置 Rigidbody
       var rb = GetComponent<Rigidbody>();
       if (rb != null)
       {
           rb.velocity = Vector3.zero;
           rb.angularVelocity = Vector3.zero;
       }

       // 重置動畫
       var animator = GetComponent<Animator>();
       if (animator != null)
       {
           animator.Play("Default", 0, 0f);
       }

       // 重置任何自定義狀態
       ResetCustomStates();
   }
   ```

### Q3: 效能問題如何優化？

**症狀**: 大量物件生成/回收時出現延遲

**優化策略**:

1. **合理的預熱策略**
   ```csharp
   // 分批預熱避免單幀卡頓
   IEnumerator PrewarmPoolsGradually()
   {
       const int batchSize = 10;
       int totalToCreate = 100;

       for (int i = 0; i < totalToCreate; i += batchSize)
       {
           for (int j = 0; j < batchSize && (i + j) < totalToCreate; j++)
           {
               ObjectPoolHelper.CreatePool(prefab, 1);
           }
           yield return null; // 分散到多幀處理
       }
   }
   ```

2. **動態池大小調整**
   ```csharp
   // 根據使用情況動態調整
   private void OptimizePoolSize(GameObject prefab)
   {
       var status = poolModule.GetPoolStatus(prefab);
       float utilizationRate = (float)status.Active / status.Total;

       if (utilizationRate > 0.8f) // 高使用率，擴展池
       {
           poolModule.ExpandPool(prefab, status.Total / 2);
       }
       else if (utilizationRate < 0.2f && status.Total > 10) // 低使用率，縮小池
       {
           poolModule.ShrinkPool(prefab, status.Total / 4);
       }
   }
   ```

### Q4: 在多場景環境中如何使用？

**解決方案**:

1. **確保 ObjectPoolModule 的生命週期**
   ```csharp
   public class PersistentPoolManager : MonoBehaviour
   {
       private static PersistentPoolManager instance;
       [SerializeField] private ObjectPoolModule poolModule;

       private void Awake()
       {
           if (instance == null)
           {
               instance = this;
               DontDestroyOnLoad(gameObject);
               poolModule.Initialize();
           }
           else
           {
               Destroy(gameObject);
           }
       }
   }
   ```

2. **場景特定物件的管理**
   ```csharp
   public class ScenePoolManager : MonoBehaviour
   {
       [SerializeField] private GameObject[] sceneSpecificPrefabs;

       private void Start()
       {
           // 為場景特定物件建立池
           foreach (var prefab in sceneSpecificPrefabs)
           {
               ObjectPoolHelper.CreatePool(prefab, 10);
           }
       }

       private void OnDestroy()
       {
           // 場景結束時清理場景特定的池
           foreach (var prefab in sceneSpecificPrefabs)
           {
               ObjectPoolHelper.ClearPool(prefab);
           }
       }
   }
   ```

## 進階技巧

### 1. 條件式池化

```csharp
public class ConditionalPoolable : MonoBehaviour, IPoolableObject
{
    [SerializeField] private bool enablePooling = true;
    
    public bool IsActive { get; private set; }
    public GameObject OriginalPrefab { get; set; }

    public void OnSpawn()
    {
        if (!enablePooling) return;
        
        IsActive = true;
        // 池化邏輯
    }

    public void OnRecycle()
    {
        if (!enablePooling)
        {
            Destroy(gameObject); // 不使用池化時直接銷毀
            return;
        }
        
        IsActive = false;
        // 池化邏輯
    }
}
```

### 2. 池化物件的批次操作

```csharp
public static class BatchPoolOperations
{
    public static GameObject[] SpawnMultiple(GameObject prefab, int count, 
        Vector3[] positions = null, Quaternion[] rotations = null)
    {
        var results = new GameObject[count];
        
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = positions?[i] ?? Vector3.zero;
            Quaternion rot = rotations?[i] ?? Quaternion.identity;
            results[i] = ObjectPoolHelper.Spawn(prefab, pos, rot);
        }
        
        return results;
    }

    public static void RecycleMultiple(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                ObjectPoolHelper.Recycle(obj);
            }
        }
    }
}
```

### 3. 自動清理機制

```csharp
public class AutoCleanupPoolable : Poolable
{
    [SerializeField] private float maxLifetime = 30f;
    [SerializeField] private float idleCleanupTime = 10f;
    
    private float lifetimeTimer;
    private float idleTimer;

    protected override void OnSpawnCustom()
    {
        lifetimeTimer = maxLifetime;
        idleTimer = idleCleanupTime;
    }

    private void Update()
    {
        if (!IsActive) return;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0)
        {
            RecycleSelf(); // 生命週期結束
            return;
        }

        // 檢測是否處於閒置狀態
        if (IsIdle())
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                RecycleSelf(); // 閒置太久
            }
        }
        else
        {
            idleTimer = idleCleanupTime; // 重置閒置計時器
        }
    }

    private bool IsIdle()
    {
        // 根據具體需求定義閒置條件
        // 例如：物件靜止、沒有玩家交互等
        return GetComponent<Rigidbody>().velocity.magnitude < 0.1f;
    }
}
```

## 與其他系統整合

### 1. 與事件系統整合

```csharp
public class EventIntegratedPoolable : Poolable
{
    protected override void OnSpawnCustom()
    {
        // 發送物件生成事件
        EventManager.TriggerEvent("ObjectSpawned", new ObjectSpawnedEventArgs
        {
            SpawnedObject = gameObject,
            PrefabType = OriginalPrefab.name
        });
    }

    protected override void OnRecycleCustom()
    {
        // 發送物件回收事件
        EventManager.TriggerEvent("ObjectRecycled", new ObjectRecycledEventArgs
        {
            RecycledObject = gameObject,
            PrefabType = OriginalPrefab.name
        });
    }
}
```

### 2. 與存檔系統整合

```csharp
[System.Serializable]
public class PoolableObjectSaveData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public bool isActive;
    // 其他需要保存的狀態
}

public class SaveablePoolable : Poolable, ISaveable
{
    public PoolableObjectSaveData GetSaveData()
    {
        return new PoolableObjectSaveData
        {
            prefabName = OriginalPrefab.name,
            position = transform.position,
            rotation = transform.rotation,
            isActive = IsActive
        };
    }

    public void LoadFromSaveData(PoolableObjectSaveData data)
    {
        transform.position = data.position;
        transform.rotation = data.rotation;
        
        if (data.isActive && !IsActive)
        {
            // 從存檔中恢復為激活狀態
            OnSpawn();
        }
    }
}
```

## 總結

ObjectPool 系統提供了一個完整、高效能且易於使用的物件池化解決方案。主要特點包括：

### ✅ 優勢

- **高效能**: O(1) 物件回收，Queue-based 管理
- **易於使用**: 簡潔的 API 和 Helper 類別
- **模組化設計**: 基於 SystemModule，易於整合
- **豐富功能**: 自動回收、狀態監控、Editor 整合
- **可擴展性**: 介面導向設計，支援自定義實作
- **開發者友善**: Odin Inspector 整合，直觀的編輯器體驗

### 🎯 適用場景

- 遊戲中大量重複使用的物件（子彈、敵人、粒子效果）
- 需要頻繁創建/銷毀物件的系統
- 對記憶體管理有嚴格要求的專案
- 需要高效能即時表現的遊戲

### 🚀 開始使用

1. 建立 ObjectPoolModule ScriptableObject
2. 在 Prefab 上添加 Poolable 組件
3. 使用 ObjectPoolHelper.Spawn() 和 Recycle() 方法
4. 根據需求調整池大小和參數

通過遵循本指南的最佳實務和使用建議，您可以充分發揮 ObjectPool 系統的效能優勢，為您的遊戲提供流暢的物件管理體驗。
}

[Button("強制回收所有物件")]
private void ForceRecycleAll()
{
    // 測試用途
    foreach (var pool in pools.Values)
    {
        pool.RecycleAll();
    }
}
#endif
```

## 常見問題與解答

### Q: 如何選擇使用 Poolable 還是直接實作 IPoolableObject？

A:

- **使用 Poolable**: 大多數情況，需要 Transform 重置、物理重置等常見功能
- **實作 IPoolableObject**: 需要完全客製化或極輕量的池化物件

### Q: 物件池會自動擴展嗎？

A: 是的，當池中沒有可用物件時會自動創建新物件，但建議透過預熱避免運行時分配

### Q: 如何處理複雜的物件初始化？

A: 在 OnSpawn()中進行完整的初始化，可以接受參數或透過 public 方法設定狀態

### Q: 池化物件可以有子物件嗎？

A: 可以，但要確保子物件的狀態也在 OnSpawn/OnRecycle 中正確處理

### Q: 如何除錯池相關問題？

A: 使用 ObjectPoolModule 的 Editor 按鈕、檢查物件命名（會顯示 Pool #編號）、監控池狀態

## 效能注意事項

1. **避免過度池化**: 只對頻繁使用（>10 次/秒）的物件使用池化
2. **合理設定池大小**: 根據實際需求平衡記憶體和效能
3. **及時回收**: 物件使用完畢立即回收，避免記憶體洩漏
4. **避免循環參考**: 池化物件不應持有其他物件的強參考
5. **預熱策略**: 在 Loading 時預熱重要的池，避免遊戲中的延遲

## 範例專案

檢查以下範例類別了解完整用法：

- **ObjectPoolExampleUpdated**: 基本使用範例
- **PooledBullet**: 自定義池化物件範例
- **ObjectPoolHelper**: 便捷 API 使用方式

## 設計優勢總結

這個 ObjectPool 系統提升了：

- **🔧 可維護性**: 清晰的責任分離和介面設計
- **🧪 可測試性**: 介面導向設計便於單元測試
- **📈 可擴展性**: 遵循 SOLID 原則，易於擴展新功能
- **🔄 可重用性**: 低耦合高內聚的模組化設計
- **⚡ 效能**: 高效的 Queue 管理和智能預分配機制

同時保持 API 的向後相容性，現有程式碼無需修改即可受益於這些改進。
