# CSGOTacticSimulator
CSGO战术模拟器
## 示例
*(v1.1.0 gif)* </br>
https://www.iaders.com/upload/2020/0305/CTSDemo.gif </br>
*(v1.2.0 screenshot)* </br>
![screenshot](https://www.iaders.com/upload/2020/0312/v1.3.0.png)
## 提示
- 支持自动寻路
- 可以通过点击图片获得坐标. 
- 坐标与图片在窗口内的缩放程度无关, 可以任意改变窗口大小而不用修改脚本. 
- 将鼠标移到人物点上, 可以看到该角色信息 (编号, 物品, 坐标). 
- 可在程序内进行地图框架编辑 (自动寻路用). 
## 脚本
### 提示
- 右侧输入框是一个完善的脚本编辑器 (感谢AvalonEdit), 它支持语法高亮, 自动补全和搜索. 
- 行末可使用反斜杠进行换行输入命令. 
- 每条脚本代表某个角色进行了某个动作, 例如购买 (瞬间), 移动 (耗时), 等待. 
- 脚本中角色之间的顺序并不影响最终效果, 例如可以把某个角色的动作集中在一起写出, 也可多个角色一起按时间顺序编写脚本. 
- 在脚本命令等语句前加 "-" 即可注释这行后面的内容. 
### 查询表
#### 战术模拟相关
|语法|解释|实现与否|
|----|----|----|
|set entirety speed {比率值}|设置当前脚本使用的整体速度比率值, 若不设定则使用默认值|√|
|set camp [t, ct]|设置当前队伍|√|
|create character [t, ct] {坐标}|在指定的坐标生成一名角色|√|
|give character {角色编号} weapon {武器}|为某个角色配备指定武器|√|
|give character {角色编号} missile {投掷物} <{投掷物} ...... {投掷物}>|为某个角色配备指定投掷物|√|
|give character {角色编号} props [bomb / defusekit]|为某个角色配备炸弹 / 拆弹器|√|
|set character {角色编号} status [alive / dead]|设定某个角色的存活状态|√|
|set character {角色编号} vertical position [upper / lower]|设定显示某个角色处于三维地图的上方或下方|√|
|action character {角色编号} <from {坐标}> layer {起始层数} auto move {终点坐标} layer {终点层数} [quietly / noisily]|自动寻路|√|
|action character {角色编号} move [run / walk / squat / teleport] {坐标} <{坐标} ...... {坐标}>|将角色移动到某一地点|√ (除了teleport)|
|action character {角色编号} throw [smoke / grenade / flashbang / firebomb / decoy] {坐标} <{坐标} ...... {坐标}>|让角色投掷投掷物到某一坐标 |√|
|action character {角色编号} shoot [{坐标} / {目标编号} [die / live]]|让角色向某坐标或某目标射击|√|
|action character {角色编号} do [plant, defuse]|让角色下 / 拆包|√|
|action character {角色编号} wait until {秒数}|让角色原地等待到指定秒数|√|
|action character {角色编号} wait for {秒数}|让角色原地等待指定秒数|√|
|create comment {秒数} {坐标} {内容}|在指定时间地点创建一个标注|×|
#### 地图框架制作与修改相关
|语法|解释|实现与否|
|----|----|----|
|create map {地图名称}|新建一个地图框架|√|
|create node {坐标} layer {层数}|新建一个节点|√|
|create path {节点编号} to {节点编号} <{节点编号} ...... {节点编号}> limit {移动方式限制} mode [oneway / reversedoneway / twoway] distance {距离}|新建路径|√|
|delete node {节点编号}|删除一个节点|√|
|delete path {节点编号} to {节点编号} <{节点编号} ...... {节点编号}> mode [oneway / reversedoneway / twoway]|删除路径|√|
## 自动寻路
### 层数
- 层数代表当前所表示的点的二维坐标在地图上的层数, 例如MirageA包点和A二楼层数都为0, 而A二楼平台上层数为1, 平台下层数为0. 
- quietly / noisily 代表这次寻路移动是否可以发出声音. 如果选择quietly则哪怕路径允许跑动也会静步移动; 如果路径只允许跑跳则会绕过这条路径. 
## 绘图功能
- 在任意时刻按住LControl键, 即可使用左键在界面上绘图. 
### 相关快捷键
|快捷键|作用|
|----|----|
|LControl + LShift|橡皮擦|
|LControl + Z|撤销|
|LControl + C|选择颜色|
|LControl + S|选择画笔或橡皮擦大小|
|LControl + A|选择透明度|
|LControl + Delete|清除画布|
## TODO
- 可视化地生成命令
## 外部资源使用
|资源|许可证|
|----|----|
|[icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|MIT|
|[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)|MIT|
|[sdl/Multiselect-ComboBox](https://github.com/sdl/Multiselect-ComboBox)|Apache-2.0|
|[ZjzMisaka/CustomizableMessageBox](https://github.com/ZjzMisaka/CustomizableMessageBox)|WTFPL|
|[simpleradar/CSGOMaps](http://simpleradar.com/)|Unknown|