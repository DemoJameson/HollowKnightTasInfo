## 简介

本工具通过修改的游戏文件 Assembly-CSharp.dll 获取数据，然后在 libTAS 执行 Lua 脚本显示数据到画面上，尽量减少对游戏的影响避免 desync，不过会稍微降低游戏运行速度。

支持 Hollow Knight 1028/1221 Linux 版本

## Assembly-CSharp.dll 修改的内容

* GameManager 中新增字段 `private static readonly long TasInfoMark = 1234567890` 字段用于辅助内存查找时定位
* GameManager 中新增字段 `public static string TasInfo` 用于 libTAS lua 脚本读取然后绘制到画面上
* GamaManager._instance 字段改为 Public 便于读取，不使用现成的 GamaManager.instance 是因为读取属性也有可能造成 desync
* CameraController.LateUpdate() 方法最后调用 HollowKnightTasInfo.TasInfo.Update() 方法，这样可以保证在其它 Object 更新之后才获取的数据
* CameraController 新增 OnPreRender 和 OnPostRender 方法分别调用 TasInfo 中的同名方法，用于完成镜头居中以及缩放等功能
* PlayMakerUnity2DProxy.Start() 方法中 RefreshImplementation() 前调用 TasInfo 中的同名方法，用于处理新创建的 Object

## 功能

* 小骑士相关信息，包括位置，速度，状态等
* 当前场景名，与 LiveSplit 相同逻辑的`游戏时间`
* 敌人的 HP 与碰撞箱
* 镜头跟随以及缩放
* 自定义附加显示的数据，需要对代码有一定了解
* 通过编辑 `HollowKnightTasInfo.config` 文件，实时开关各项功能

## 使用说明

1. 复制两个 dll 文件到 `游戏目录/hollow_knight_Data/Manager` 目录，其中 `Assembly-CSharp.dll` 需要覆盖，注意备份。
2. libTAS 中使用菜单 `Tools -> Lua -> Execute lua script` 打开 `HollowKnightTasInfo.lua` 即可在读取存档后显示辅助信息。
3. （可选）通过编辑`游戏目录/HollowKnightTasInfo.config`文件，可以实时开关各项功能以及定制需要获取的数据。

## 致谢
* Kilaye 给 [libTAS](https://github.com/clementgallet/libTAS) 增加了画线的 lua api
* 参考 [LiveSplit.HollowKnight](https://github.com/ShootMe/LiveSplit.HollowKnight) 的`游戏时间`计算方式
* 参考 [HollowKnight.Modding](https://github.com/HollowKnight-Modding/HollowKnight.Modding) 的代码找到生成 Object 的方法
* 参考 [HollowKnight.HitboxDraw](https://github.com/seresharp/HollowKnight.HitboxDraw) 绘制 hitbox
* 参考 [DebugMod](https://github.com/seresharp/DebugMod) 获取 HP 和镜头跟随缩放
* cuber_kk、inoki、Zippy Rhys 的测试
