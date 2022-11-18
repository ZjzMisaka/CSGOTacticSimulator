# CSGOTacticSimulator
CSGO Tactic Simulator <br />
 ![CSGOTacticSimulator](https://www.iaders.com/wp-content/uploads/2020/05/CTS.png) <br />
 [中文ReadMe](https://github.com/ZjzMisaka/CSGOTacticSimulator/blob/master/README_CH.md) <br />
## multi-language
- [x] 简体中文
- [x] 日本語
- [x] English
##
*(v2.8.0 screenshot)* </br>
![cts110902](https://user-images.githubusercontent.com/16731853/200872444-213ea3da-3776-4338-be3c-f57108c998ce.jpg)
![cts110901](https://user-images.githubusercontent.com/16731853/200872416-337ac92e-f76b-4699-a0c5-37f37f072d17.png)
![cts1109](https://user-images.githubusercontent.com/16731853/200872337-6f84d716-b0ed-41fe-961f-eb2878e7487d.png)
*(v1.1.0 gif)* </br>
https://www.iaders.com/upload/2020/0305/CTSDemo.gif </br>
*(v1.6.1 screenshot)* </br>
![screenshot](https://www.iaders.com/wp-content/uploads/2020/05/1.6.0.png)
## Prompt
- You can read and watch the demo. When watching the demo, click the character point on the left map to play the pov corresponding to the current time. 
- Pathfinding can be performed after selecting the map frame.
- You can edit the map frame in the program (for pathfinding).
- You can get the coordinates by clicking on the picture.
- The coordinates have nothing to do with the zoom level of the picture in the window, you can change the window size without modifying the script. 
- Move your mouse over the character's point to see the character information (number, item, coordinates, etc).
## Watch DEMO&POV
- Enter the path of the demo file into the file input box to load the demo file.
- You can choose to watch a round or watch all rounds.
- You can extract the voice in demo. 
- Press the CapsLock key to view the information panel. 
- Press CapsLock + LeftShift key to switch to the scoreboard. 
- Click the character point on the left map when running the demo then you can watch the pov corresponding to the current time. 
- Pov is divided into the whole game POV or one round POV. If you want to watch only one round, you can only record the POV video of that round. If you want to watch the whole game, you can record the whole game POV video.
- The pov file name should be the same as the player name (case insensitive).
- The whole game POV video files should be placed in the povs folder of the directory to which the demo file belongs, the one round POV video files should be placed in the round number folder in the povs folder of the directory to which the demo file belongs. </br>
![directory](https://www.iaders.com/upload/2020/0709/directory.png)
- The whole game POV video should start with the end of the freezing time of the first round. the one round POV video should start with the end of the freezing time of this round.
## Script
### Prompt
- The input box is a full-fledged script editor, it supports syntax highlighting, auto-completion, and searching. 
- You can use a backslash at the end of a line for multi-line input commands.
- Each script performs an action on behalf of a character, such as buying (instantly), moving (time-consuming), waiting.
- The order between the characters in the script does not affect the final effect. For example, you can write the scripts of the actions of one character at the same time, or you can write the scripts of multiple characters at the same time.
- You can add "-" before the script command and other statements to comment the content after this line.
### Create scripts visually
- Right-click the map
    - Set the speed
        - Sets the overall speed ratio value
    - Set the camp
        - Set the camp
    - Create the character
        - Create the character
- Click the preview button and right click the preview of the character
    - Add equipment for the character
        - weapon
        - missile
        - props
    - Setting character state
        - Set to show a character above or below the 3D map
        - Set the survival status of a role
    - Action
        - Plant / defuse
    - Waiting
        - Wait for / until the specified time
- Click the preview button and drag the preview of the character with the left button
    - Moving
        - Create the moving command, You can set to use the full path or just the start and end point, or set the movement mode
    - Throwing
        - Throw a missile
    - Shooting
        - Shoot to a point
        - Shoot at a character, you can set whether the character being shot survives or not
- Click the preview button and drag the preview of the character with the left button, And click the middle mouse button to create special points while mouse moving
    - Moving
        - Create the moving command by using special points, You can set to use the full path or just the start and end point, or set the movement mode
    - Pathfinding
        - The adjacent special points are used as the starting and ending points for automatic pathfinding. Way finding method can be set. If there are two special points, you can set the starting and ending layer number and wayfinding method
    - Throwing
        - Throw a missile, with special points as the trajectory of the missile, simulate the rebound of the missile
    - Shooting
        - Shoot to a point
        - Shoot to a character, you can set whether the character being shot survives or not
### Query table
#### About tactic simulation
|Grammar|Explanation|Implementation or Not|
|----|----|----|
|set entirety speed {Ratio value}|Sets the overall speed ratio value used by the current script. If not set, the default value is used.|√|
|set camp [t, ct]|Set current team|√|
|create character [t, ct] {Coordinate} <name {name}>|Generate a character at the specified coordinates, and take an alias (do not start with a number)|√|
|give character {Character number} weapon {Weapon}|Assign a weapon to a character|√|
|give character {Character number} missile {missile} <{missile} ...... {missile}>|Assign missiles to a character|√|
|give character {Character number} props [bomb / defusekit]|Equip a Character with a Bomb / Defusekit|√|
|set character {Character number} status [alive / dead]|Set the survival status of a role|√|
|set character {Character number} vertical position [upper / lower]|Set to show a character above or below the 3D map|√|
|action character {Character number} <from {Coordinate}> layer {Starting layer} auto move {Finishing coordinate} layer {Finishing layer} [quietly / noisily]|Pathfinding|√|
|action character {Character number} move [run / walk / squat / teleport] {Coordinate} <{Coordinate} ...... {Coordinate}>|Move character to a place|√|
|action character {Character number} throw [smoke / grenade / flashbang / firebomb / decoy] {Coordinate} <{Coordinate} ...... {Coordinate}>|Throw a missile to a certain coordinate|√|
|action character {Character number} shoot [{Coordinate} / {Goal number} [die / live]]|Shoot to a coordinate or target|√|
|action character {Character number} do [plant, defuse]|Plant / defuse|√|
|action character {Character number} wait until {Seconds}|Wait in place until the specified time|√|
|action character {Character number} wait for {Seconds}|Wait in place for a specified number of seconds|√|
|create comment {Seconds} {Coordinate} {Content}|Create a callout at a specified time and place|×|
#### About map frame creation and modification
|Grammar|Explanation|Implementation or Not|
|----|----|----|
|create map {Map name}|Create a new map frame|√|
|create node {Coordinate} layer {layer}|Create a new node|√|
|create path {Node number} to {Node number} <{Node number} ...... {Node number}> limit {Movement limits} mode [oneway / reversedoneway / twoway] distance {Distance}|Create a path|√|
|delete node {Node number}|Delete a node|√|
|delete path {Node number} to {Node number} <{Node number} ...... {Node number}> mode [oneway / reversedoneway / twoway]|Delete a path|√|
## Wayfinding
### Layer
- The number of layers represents the number of layers of the two-dimensional coordinates of the currently represented point on the map, Take A side of Mirrage as an example: Palace and A Bomb Site have a layer value of 0, while Balcony has a layer value of 1, and Under Balcony has a layer value of 0.
- quietly / noisily represents whether the pathfinding movement can make a sound. If quietly is selected, even if the path allows running, it will move silently; if the path only allows running and jumping, the path will be bypassed.
## Drawing
- You can hold down the LControl key at any time and use left button to draw. 
### Related hot keys
|Hot keys|Effect|
|----|----|
|LControl + LShift|Eraser|
|LControl + Z|Revoke|
|LControl + C|Choose the color|
|LControl + S|Choose size of brush or eraser|
|LControl + A|Choose transparency|
|LControl + Delete|Clear whole canvas|
## Todo
## External resources
|Resource|License|
|----|----|
|[icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|MIT|
|[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)|MIT|
|[sdl/Multiselect-ComboBox](https://github.com/sdl/Multiselect-ComboBox)|Apache-2.0|
|~~[StatsHelix/demoinfo](https://github.com/StatsHelix/demoinfo)~~ [akiver/CSGO-Demos-Manager/demoinfo](https://github.com/akiver/CSGO-Demos-Manager/tree/main/demoinfo)|MIT, GPL-2.0|
|[akiver/csgo-voice-extractor](https://github.com/akiver/csgo-voice-extractor)|MIT|
|[ZjzMisaka/CustomizableMessageBox](https://github.com/ZjzMisaka/CustomizableMessageBox)|WTFPL|
|[simpleradar/CSGOMaps](http://simpleradar.com/)|Unknown|
|[ICONS8](https://icons8.com)|[Link](https://icons8.com/license)|
|[Facepunch/Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks)|MIT|
|[naudio/NAudio](https://github.com/naudio/NAudio)|MIT|
|[ArttuKuikka/HltvSharp](https://github.com/ArttuKuikka/HltvSharp)|[Issue](https://github.com/ArttuKuikka/HltvSharp/issues/9)|
