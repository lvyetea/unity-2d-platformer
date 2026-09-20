# 2D 横版动作小游戏（Unity / C#）

一个基于 Unity 2022.3 LTS + URP 2D 的横版动作小游戏，完整跑通"进入游戏 → 关卡探索 → 战斗 → 场景传送"的闭环。

项目的重点不在玩法量，而在**架构解耦**：场景加载、音频、UI、转场之间通过 ScriptableObject 事件资产通信，互不直接引用——新增一个关卡或一个音效，不需要改动任何既有玩法代码。


## 技术栈

- **Unity 2022.3 LTS** / URP 14.0.11（2D Renderer）/ C#
- **Addressables 1.22.3**：场景异步加载与卸载
- **Input System 1.11.2**：生成输入类 + 事件回调
- **Cinemachine 2.10.3**：跟随相机 + Confiner2D 动态边界
- **DOTween**：转场淡入淡出
- Tilemap / UGUI

## 已实现功能

### 架构：ScriptableObject 事件总线
定义 6 个事件资产（场景加载 / 音频 / 淡入淡出 / 血量 / 场景数据 / 角色事件），事件侧只持 `UnityAction` 并 `Raise()` 广播，订阅侧统一在 `OnEnable` 注册、`OnDisable` 注销。音频、UI、转场与玩法逻辑之间零直接引用。

### 场景流：常驻场景 + 关卡 Additive 异步加载
`Persistent` 常驻场景承载各管理器；关卡场景按需 `LoadSceneAsync(LoadSceneMode.Additive)` 异步加载，离开时 `UnLoadScene()` 释放。加载完成后回写玩家到目标坐标，并按新场景的 `Bounds` 重新计算 Cinemachine Confiner2D 相机边界（避免传送后相机边界仍停留在上一个场景）。

### 敌人 AI：状态模式
抽象基类 `BaseState` 定义 `OnEnter` / `LogicUpdate` / `PhysicsUpdate` / `OnExit` 四个生命周期方法，宿主经 `Enemy.SwitchState` 切换状态：
- **巡逻 → 追击**：`Physics2D.BoxCast` 朝向前方做盒检测发现玩家
- **追击 → 巡逻**：丢失目标计时 `lostTimeCounter` 归零

### 角色系统
Input System 回调驱动移动 / 跳跃 / 攻击；`Character` 统一管理血量与受伤无敌帧，通过 `UnityEvent`（OnHealthChange / OnTakeDamage / OnDie）向 UI 广播；攻击用 `OnTriggerStay2D` 判定，配合 Animator 的 `StateMachineBehaviour` 在动画结束时收尾攻击 / 受伤状态。

### 交互与转场
`Iinteractable` 接口统一宝箱 / 提示牌 / 传送点的交互入口；传送点广播"场景加载事件"（目标场景 + 坐标 + 是否淡入淡出），转场用 DOTween 做淡入淡出过渡。

## 如何运行

1. 用 Unity Hub 安装 **Unity 2022.3.56f1c1**（同为 2022.3.x 的版本通常也可以）
2. 克隆本仓库，用 Unity 打开该目录
3. 首次打开会自动导入 Addressables 设置与素材资源
4. 打开 `Assets/Scenes/Persistent.unity` 并点击运行

## 项目结构

```
Assets/Script/
├── ScriptableObject/   # 事件总线与数据资产（VoidEventSO / PlayAudioEventSO /
│                       #   GameSceneSO / ScenesLoadEventSO / FadeEventSO / CharacterEventSO）
├── Player/             # PlayerController、PlayerAnimation、攻击与受伤动画收尾、Sign
├── Enemy/              # BaseState、Enemy、Boar、BoarPatrolState、BoarChaseState
├── General/            # Character（血量/无敌帧）、Attack、PhysicsCheck、Chest
├── Transition/         # SceneLoader（Addressables 异步场景流）、TeleportPoint
├── Audio/              # AudioManager、AudioDefination
├── UI/                 # UIManager、PlayerStatBar、Fade Canvas
└── Utilities/          # CameraControl、Iinteractable、Enums
```

## 我的工作

- **程序逻辑全部由我实现**：自研脚本 **32 个 / 约 1410 行**（不含 Input System 自动生成代码与第三方插件）
- 架构设计（事件总线、场景流、状态机）与全部 C# 代码为原创；美术资源使用现成像素素材包，详见下方授权说明

## 已知问题 / 后续计划


- 缺少**存档系统** → 计划加 JSON 序列化存档
- 敌人 AI 只有"巡逻 / 追击"两个状态 → 计划补充"攻击"状态
- 障碍物 / 敌人使用 `Instantiate` + `Destroy` 管理 → 数量上来后应改为**对象池**
- 无音频混音控制（有 AudioMixer 资产但代码未接入）
- 游戏尚未打包，计划实现其他新功能后打包

## 第三方资源与授权

| 资源 | 来源 | 用途 |
|---|---|---|
| Pixel Art Platformer - Village Props | Cainos | 像素美术素材（场景 / 道具） |
| DOTween | Demigiant | 补间动画（转场淡入淡出） |
| Lucid Editor | Cainos | 编辑器扩展 |
| Colourful Hierarchy Category | M Studio | 编辑器扩展 |

> 以上第三方资源的版权归各自原作者所有，在本仓库中仅按各自授权用于演示与学习用途；本仓库不对上述资源主张任何权利。如原作者有异议，可随时移除。
