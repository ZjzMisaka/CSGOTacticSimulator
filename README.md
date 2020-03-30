# CSGOTacticSimulator
CSGO Tactic Simulator <br />
 ![WTFPL](http://www.wtfpl.net/wp-content/uploads/2012/12/wtfpl-badge-1.png) <br />
 [中文ReadMe](https://github.com/ZjzMisaka/CSGOTacticSimulator/blob/master/README_CH.md) <br />
## EXAMPLE
*(v1.1.0 gif)* </br>
https://www.iaders.com/upload/2020/0305/CTSDemo.gif </br>
*(v1.2.0 screenshot)* </br>
![screenshot](https://www.iaders.com/upload/2020/0312/v1.3.0.png)
## PROMPT
- Program supports pathfinding. 
- You can get the coordinates by clicking on the picture.
- The coordinates have nothing to do with the zoom level of the picture in the window, you can change the window size without modifying the script. 
- Move your mouse over the character's point to see the character information (number, item, coordinates).
- You can edit the map frame in the program (for pathfinding).
## SCRIPT
### PROMPT
- Input box supports syntax highlighting, auto-completion, search.
- You can use a backslash at the end of a line for multi-line input commands.
- Each script performs an action on behalf of a character, such as buying (instantly), moving (time-consuming), waiting.
- The order between the characters in the script does not affect the final effect.For example, you can write the scripts of the actions of a character at the same time, or you can write the scripts of multiple characters at the same time.
- You can add "-" before the script command and other statements to comment the content after this line.
### QUERY TABLE
#### ABOUT TACTIC SIMULATION
|Grammar|Explanation|Implementation or Not|
|----|----|----|
|set entirety speed {Ratio value}|Sets the overall speed ratio value used by the current script. If not set, the default value is used.|√|
|set camp [t, ct]|Set current team|√|
|create team [ct, t] [pistol, eco, forcebuy, quasibuy]|Generate a team at the birth point and automatically equip weapons and projectiles
|×|
|create character [t, ct] {Coordinate}|Generate a character at the specified coordinates|√|
|delete character {Character number}|Delete a character|×|
|give character {Character number} weapon {Weapon}|Assign a weapon to a character|√|
|give character {Character number} missile {missile} <{missile} ...... {missile}>|Assign missiles to a character|√|
|give character {Character number} props [bomb / defusekit]|Equip a Character with a Bomb / Defusekit|√|
|set character {Character number} status [alive / dead]|Set the survival status of a role|√|
|set character {Character number} vertical position [upper / lower]|Set to show a character above or below the 3D map|√|
|action character {Character number} <from {Coordinate}> layout {Starting layout} auto move {Finishing coordinates} layout {Finishing layer} [quietly / noisily]|Pathfinding|√|
|action character {Character number} move [run / walk / squat / teleport] {Coordinate} <{Coordinate} ...... {Coordinate}>|Move character to a place|√ (Except teleport)|
|action character {Character number} throw [smoke / grenade / flashbang / firebomb / decoy] {Coordinate} <{Coordinate} ...... {Coordinate}>|Throw a missile to a certain coordinate|√|
|action character {Character number} shoot [{Coordinate} / {Goal number} [die / live]]|Shoot to a coordinate or target|√|
|action character {Character number} do [plant, defuse]|plant / defuse|√|
|action character {Character number} wait until {Seconds}|Wait in place until the specified time|√|
|action character {Character number} wait for {Seconds}|Wait in place for a specified number of seconds|√|
|create comment {Seconds} {Coordinate} {Content}|Create a callout at a specified time and place|×|
#### ABOUT MAP fRAME CREATION AND MODIFICATION
|Grammar|Explanation|Implementation or Not|
|----|----|----|
|create map {Map name}|Create a new map frame|√|
|create node {Coordinate} layout {Layout}|Create a new node|√|
|create path {Node number} to {Node number} <{Node number} ...... {Node number}> limit {Movement limits} mode [oneway / reversedoneway / twoway] distance {Distance}|Create a path|√|
|delete node {Node number}|Delete a node|√|
|delete path {Node number} to {Node number} <{Node number} ...... {Node number}> mode [oneway / reversedoneway / twoway]|Delete a path|√|
## WAYFINDING
### LAYER
- The number of layers represents the number of layers of the two-dimensional coordinates of the currently represented point on the map, 例如MirageA包点和A二楼层数都为0, 而A二楼平台上层数为1, 平台下层数为0. 
- quietly / noisily represents whether the pathfinding movement can make a sound. If quietly is selected, even if the path allows running, it will move silently; if the path only allows running and jumping, the path will be bypassed.
## Drawing function
- You can hold down the LControl key at any time and use left button to draw. 
### RELATED HOT KEYS
|Hot keys|Effect|
|----|----|
|LControl + LShift|Eraser|
|LControl + Z|Revoke|
|LControl + C|Choose the color|
|LControl + S|Choose a brush or eraser size|
|LControl + A|Choose transparency|
|LControl + Delete|Clear canvas|
## TODO
## Use of external resources
|Resource|License|
|----|----|
|[icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|MIT|
|[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)|MIT|
|[ZjzMisaka/CustomizableMessageBox](https://github.com/ZjzMisaka/CustomizableMessageBox)|WTFPL|
|[simpleradar/CSGOMaps](http://simpleradar.com/)|Unknown|
