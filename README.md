# moba-core

类王者荣耀的 MOBA 手游核心工程(Unity 6 LTS / C#),单机原型起步,后续接入服务端状态同步。

## 开发环境

- Unity 6000.0.32f1 (LTS)
- C# / .NET Standard 2.1
- 目标平台:Android / iOS

## 目录结构

```
Assets/
  Scripts/
    Core/        GameBootstrap 入口、全局服务注册
    Input/       VirtualJoystick 虚拟摇杆输入层
    Movement/    PlayerMotor 角色移动控制
    Skills/      SkillDefinition / SkillSystem 技能施法系统
    Network/     状态同步快照接口(占位)
  Scenes/        场景资源
Packages/        Unity 包依赖清单
ProjectSettings/ Unity 工程配置
```

## 已落地模块

| 模块 | 说明 |
| --- | --- |
| VirtualJoystick | UGUI 虚拟摇杆,归一化方向输出,死区处理 |
| PlayerMotor | 相机坐标系转换 + 速度平滑移动,预留位置/朝向快照接口 |
| SkillSystem | 施法管线:选点/选方向 → 前摇 → 范围命中判定 → 冷却结算 |

## Roadmap (PR 顺序)

1. `feat: skill casting system with targeting and cooldown` ✅
2. `feat: player virtual joystick movement controller` ✅
3. `feat: server-authoritative state sync snapshot`(固定帧快照 + 回滚)
4. `feat: damage calculation and hit resolution`(伤害结算管线)
5. `feat: minion wave spawning and lane pathing`(兵线系统)

## 说明

当前处于早期工程阶段,以核心玩法模块的可验收切片为单位推进,每个 PR 包含独立可测试的代码范围。