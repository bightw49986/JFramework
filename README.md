# JFramework
JFramework是一個輕便、易編輯、以 **SOAP（Scriptable Object Architecture Pattern）** 為核心的 Unity 遊戲開發框架，適合用來做一些小型專案或是Prototype的快速驗證，在SOAP的資料驅動、模組化設計與系統解耦的基礎上，實作了如 `ObjectPooling`、`SceneManagement`、`AudioManagment`、`Finite StateMachine` 及數種資料驅動的UI Effect等遊戲開發常見的基礎建設，幫助專案快速展開。

---

## 設計理念

**SOAP** 的概念源自2017年GDC [Talk](https://www.youtube.com/watch?v=raQ3iHhE_Kk)，利用 `ScriptableObject（以下簡稱SO）`是Asset的特性，解決以下Unity專案中常見的痛點：

- **邏輯與資料混雜** — 遊戲邏輯與數值散落在 `MonoBehaviour` 之間，難以測試與重用
    
    → SOAP提出 `ScriptableVariable` 的概念，將資料Asset化，讓不同腳本間不須相依取值，而是透過序列化存取SO上的資料，將相依性反轉至容易操作的編輯器層。
    
- **系統間高度耦合** — 過多單例及UnityEvent的強連結導致系統間耦合難以修改
    
    → SOAP分別利用 `ModularSystem`及 `StaticEvent` 將系統與事件**去中心化**，腳本不再依賴綿長的Singlton manager classes或是 Unity Event 的Function call，而是在自己的序列化欄位引用需要的模組化系統或全域事件，有效降低耦合性。

### 本專案延伸

- 以泛型架構實作 `ScriptableVariable` 及 `StaticEvent`使其容易擴充，並提供日常開發中常見類型之實作

- 在 `ScriptableVariable` 的基礎上擴充ValueChanged等類響應式（Reactive）設計，並實作多種資料綁定驅動的UI效果

- 以樹狀結構實作 `ModularSystem` 的生命週期管理，實現跨場景的模組載入

- 在模組化系統的基礎上實作常見的物件池、音效管理、場景管理等系統

- 補充其它和SOAP無關但開發中仍常使用的功能，如流程管理、有限狀態機等

---

## 架構總覽

```
JFramework
├── SOAP 核心
│   ├── ScriptableVariable<T>        資料容器（含值變化事件）
│   ├── StaticEvent / StaticEvent<T> 全域事件匯流排
│   ├── ValueReference<T>            雙模式資料參考（常數 or ScriptableAsset）
│   ├── ScriptableTask / Routine     可序列化非同步操作
│   └── ModularSystem                SystemRunner + SystemModule 生命週期管理
│
├── 物件池（ObjectPooling）   泛型分層架構，支援 GameObject 與 C# 物件
├── 狀態機（StateMachine）   Simple + Initiative 雙模式
├── 音訊系統（Audio）        分組管理、淡入淡出、動態音量衰減
├── UI 綁定（Binding）       資料驅動文字綁定，支援 TMP
├── 場景管理（SceneManagement） 含自定義過渡效果介面
├── 序列動作（Sequencing）   可組合的遊戲流程編排
├── 工具組件（Transformation / UI Effects / Utility）
└── Inspector 擴充（Attributes + PropertyDrawer）
```

---

## 核心模組說明

### SOAP — ScriptableVariable 與 StaticEvent

SOAP 的核心概念是以 **ScriptableObject 作為資料與事件的載體**，讓場景之間、系統之間可以透過 Asset 溝通，不需直接持有彼此的參考。

**ScriptableVariable** 是帶有值變化事件的泛型資料容器：

```csharp
// 設定數值（自動觸發 ValueChanged 事件）
scoreVariable.Value = 100;

// 訂閱變化
scoreVariable.ValueChanged += (newVal, oldVal) => UpdateUI(newVal);
```

**ValueReference** 讓序列化欄位可以在「直接填常數」與「引用 ScriptableAsset」之間切換，不需改動程式碼：

```csharp
[SerializeField] private FloatReference moveSpeed; // Inspector 中自由切換模式

float current = moveSpeed.Value; // 使用方式完全一致
```

**StaticEvent** 提供型別安全的全域事件，不需單例或靜態類別：

```csharp
// 觸發
onEnemyDied.Raise(sender: this, args: enemyData);

// 訂閱
onEnemyDied.AddListener((sender, data) => UpdateKillCount(data));
```

---

### ModularSystem — 模組化生命週期管理

系統由 **SystemRunner**（MonoBehaviour 單例）與 **SystemModule**（ScriptableObject）組成樹狀結構。每個模組有明確的生命週期回呼：

```
Initialize(GameObject root) → OnUpdate(float dt) → Terminate()
```

**SystemRegistry** 讓場景自行宣告需要的系統，場景卸載時自動移除：

```csharp
// 不同場景掛載不同的 SystemRegistry，各自管理各自的模組
// SystemRunner 跨場景存活，模組動態增減，互不干擾
```

**SystemRunner** 內建訂閱式 Update，取代分散各處的 MonoBehaviour：

```csharp
var sub = SystemRunner.Instance.SubscribeToUpdate(() => DoSomething());
// 不再需要時 Dispose 即自動取消訂閱
sub.Dispose();
```

---

### ObjectPooling — 泛型分層物件池

以泛型基類 `ObjectPoolBase<T, U>` 設計，支援兩種使用情境：

```
ObjectPoolBase<T, U>
├── GameObjectPool        (T = Poolable, GameObject 情境)
└── CSharpObjectPool<T>   (任意可 Dispose 的 C# 物件)
```

**Poolable** 元件透過 `RecycleRequested` 事件自我回收，無需直接持有池的參考：

```csharp
public class Bullet : Poolable
{
    protected override void OnSpawnCustom()  => StartMoving();
    protected override void OnRecycleCustom() => StopMoving();
}

// 任意時機自我回收
RecycleSelf();
```

**ObjectPoolModule** 整合進模組系統，支援**時間分割預熱（Time-Sliced Warmup）**，將大量物件的建立非同步分散到多個 Frame，避免初始化卡頓：

```csharp
// 在 Inspector 設定預熱清單，框架自動於背景執行
warmupConfigs = [ { Prefab = bulletPrefab, Count = 100, SplitIntoFrames = 20 } ]
```

---

### StateMachine — 雙模式狀態機

| | SimpleStateMachine | InitiativeStateMachine |
|---|---|---|
| 驅動方式 | 手動呼叫 `UpdateCurrentState()` | 自動訂閱 SystemRunner Update |
| 適用情境 | 需精確控制更新時機 | 背景自主運行的 AI、流程控制 |

兩者皆支援泛型 Context 在所有狀態間共享資料，以及 **Lock Token** 機制防止在關鍵操作中途切換狀態：

```csharp
if (stateMachine.Lock(out var token))
{
    await PlayCutscene(); // 不可被打斷
    stateMachine.UnLock(token);
}
```

---

### Sequencing — 序列動作系統

將遊戲流程拆解為可組合的 **SequenceAction**，在 Inspector 中視覺化編排：

```
Sequencer
├── WaitAction                  等待 N 秒
├── PlayTimelineAction          播放 Timeline
├── BranchAction                依條件分支
├── MultipleAction              並行執行多個動作
├── ChangeSceneAction           場景切換
├── RunScriptableTaskAction     執行可序列化非同步任務
└── RaiseUnityEventAction       觸發自定義事件
```

---

### Audio — 分組音訊管理

以「組（AudioGroup）」為單位管理音效，支援淡入淡出與多種切換行為（InstantCut / FadeInOut / Overlap）。多音效同時播放時自動套用音量衰減公式（`volume * sqrt(1/n)`），防止混音爆音。

---

### Inspector 擴充

自製 Attribute 讓 Inspector 更具表達力：

```csharp
[InfoBox("此為必填欄位")]
[Required]
[SerializeField] private AudioGroup audioGroup;

[ShowIf(nameof(enableAutoRecycle))]
[SerializeField] private float autoRecycleTime;

[VectorRange(-1f, 1f, -1f, 1f)]
[SerializeField] private Vector2 direction;
```

---

## 關鍵設計決策

**為何 ScriptableObject 而非單例？**
ScriptableObject 是資源，可在 Editor 中直接引用、測試、替換，且天然支援跨場景存活。單例隱含了依賴關係，難以在不同場景或測試環境中替換。

**為何 SystemModule 也用 ScriptableObject？**
讓系統設定成為可序列化的 Asset，企劃可直接在 Inspector 調整參數而不必改程式碼；各模組以 `SystemRunner` 統一管理生命週期，避免初始化順序問題。

**為何 ObjectPool 重構為泛型基類？**
原版 `ObjectPool` 只支援 GameObject，且直接操作介面而非依賴倒置。新版以 `ObjectPoolBase<T, U>` 泛化設計，並透過 `RecycleRequested` 事件解除池與物件的直接耦合。

**為何 UpdateManager 改為 SystemRunner 內建？**
實際使用中發現「需要 Update 訂閱」的情境遠多於「需要完整模組生命週期」的情境。將訂閱 API 直接內建於 SystemRunner，讓任何地方都能以一行程式碼訂閱 Update。

---

## 挑戰

**SOAP框架的資產管理問題：**
大量的資料及事件散落在資產中，經過迭代容易導致維護困難，且SO壞掉時並不會像程式碼編譯報錯，容易有誤刪的風險。

解法：專案提供簡易的SO Reference查找編輯器工具，以及Safe Delete的刪除前檢查功能，不過並沒有針對所有SO資產建立索引，掃描過程長，仍有優化空間。


**SOAP框架的編輯器運行期資料汙染問題：**
這是SO在編輯器環境（Build出來後不會）的特性，當你在Runtime修改SO的值，退出Playmode之後，不同於 `MonoBehaviour`的腳本，該值並不會主動復原回運行前的狀態，這會導致開發上反覆填值的痛苦。

解法：增加editor default value機制，在enable時進行快照，並在退出後還原，該區段用define `UNITY_EDITOR` 包圍起來，確保只在Editor模式運作。（待實作）

**與AI Agent的合作阻力：**
AI Agent在未經指示前難以理解位在代碼範圍外的SO邏輯，導致容易出現幻覺。

解法：目前仍未有一個完善的解法可以全面改善這個問題，畢竟SOAP本來就是較**面向編輯器**的概念；有一說是可以在專案的Context內注入關於SO架構的規則及目錄規範，藉此讓Agent開發時能被路由至正確的位置進行閱讀理解跟CRUD等操作，不過具體成效還未經測試，且可以預見額外的Token開銷跟來回溝通摩擦肯定是有的，仍是一項具體的挑戰。

---

## 專案背景

JFramework 起初作為個人框架開發，後續應用於一個 VR 音樂遊戲專案（VRMusicShooting）的實際客戶交付。在專案開發過程中，框架歷經多次迭代：

- 物件池從單層 GameObject 架構重構為泛型分層架構
- 新增完整的音訊系統、UI 綁定、場景管理與序列動作系統
- Update 管理整合入 SystemRunner，簡化使用介面
- 所有模組補上完整的 Editor 支援與 Inspector 擴充

本 Repository 為將專案中的迭代成果整併回框架本體後的最新版本。

---

## 環境需求

- Unity 2021.3 LTS 以上
- TextMeshPro（Binding / UI 特效模組，選用（Unity6以後改為內建支援））
- Post Processing（SceneTransition 色彩調整過渡，選用）

---

## 授權

Copyright (c) 2026 Jonathan Ho. All rights reserved.