支持 Hollow Knight 1028/1221 Linux 版本

本项目通过修改游戏文件 Assembly-CSharp.dll 获取数据，然后通过 Lua 脚本绘制到画面上，尽量减少对游戏的影响避免 desync。

1. 复制粘贴两个 dll 文件到游戏目录/Manager目录，其中 Assembly-CSharp.dll 需要覆盖。
2. libTAS 中使用菜单 Tools -> Lua -> Excute lua script 打开 HollowKnightDllInfo.lua 即可在右上角显示辅助信息。lua 脚本顶部提供了一些配置项，请根据需要进行修改。目前 libTAS 只能画矩形和点，所以游戏中的多边形 hitbox 是通过点画出来的会影响游戏速度，默认只在录制时显示。
3. （可选）运行游戏后 Manager 目录会生成 HollowKnightTasInfo.config 文件，可以定制需要获取的数据，需要注意如果调用属性或者方法有可能会造成 desync（例如 HeroController.CanJump() 会修改 ledgeBufferSteps 字段)，请查看源码确认是否安全。定制数据格式如下：
    * {UnityObject子类名.字段/属性/方法.字段/属性/方法……}，只支持无参方法()结尾，例如 canAttack: {HeroController.CanAttack()}
    * {GameObjectName.字段/属性/方法.字段/属性/方法……}

假如不想覆盖 Assembly-CSharp.dll 文件害怕造成 desync，可以只使用 HollowKnightMemoryInfo.lua 直接读取内存数据，不过只能显示位置和速度，并且位置只有 tick 帧时才会更新
