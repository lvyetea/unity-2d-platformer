# 更新记录

## 2026-11-09

- PlayerController.cs：Jump 登记请求，FixedUpdate/ProcessJump 统一施加冲量；HasGroundSupport 确认落地后复位，OnDisable 清除待处理输入。空中、攻击、受伤和死亡期间不接受跳跃，也不播放跳跃音效。
- SingleJumpValidation.cs：增加 Unity 2D 物理回归验证入口，覆盖连续输入、落地复位、状态拦截和贴地边界。
- 合并已核对的 Windows 构建入口、Addressables 场景登记和 Sign.cs 平台引用修复。
- 验证：Unity 2022.3.56f1c1 的 17 项 2D 物理回归全部通过，测试期间引擎错误为 0；Addressables 与 Windows x86_64 构建成功，构建错误为 0。有画面试玩和真实设备操作尚未复验。
