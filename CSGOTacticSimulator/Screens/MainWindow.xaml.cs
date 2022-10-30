using CSGOTacticSimulator.Controls;
using CSGOTacticSimulator.Global;
using CSGOTacticSimulator.Helper;
using CSGOTacticSimulator.Model;
using CustomizableMessageBox;
using DemoInfo;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Newtonsoft.Json;
using Sdl.MultiSelectComboBox.Themes.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static CustomizableMessageBox.MessageBox;
using MessageBox = CustomizableMessageBox.MessageBox;
using Path = System.IO.Path;

namespace CSGOTacticSimulator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum RunningType
        {
            TXT,
            DEM,
            NONE
        }

        RunningType nowRunningType = RunningType.NONE;
        public List<Animation> animations = new List<Animation>();
        private Camp currentCamp;
        private double localSpeedController = -1;
        private bool bombDefused = false;
        private CompletionWindow completionWindow;
        private Point mouseLastPosition = new Point(-1, -1);
        private Selector selector = null;
        private XshdSyntaxDefinition codeXshd = null;
        private XshdSyntaxDefinition logXshd = null;
        private System.Timers.Timer resizeTimer = new System.Timers.Timer(100) { Enabled = false };
        private Dictionary<UIElement, Point> elementPointDic = new Dictionary<UIElement, Point>();
        private int currentTScore = -1;
        private int currentCTScore = -1;
        public List<Point> mouseMovePathInPreview = new List<Point>();
        public List<Point> keyDownInPreview = new List<Point>();
        public Stopwatch stopWatch = null;
        public Stopwatch stopWatchThisRound = null;
        public List<Stopwatch> stopwatchList = new List<Stopwatch>();
        public int offset = 0;
        public bool isForward = false;
        public bool isBackward = false;
        public List<string> mapList = null;
        public bool isNeedAutomaticGuidance = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = int.Parse(IniHelper.ReadIni("Window", "Width"));
            this.Height = int.Parse(IniHelper.ReadIni("Window", "Height"));

            resizeTimer.Elapsed += new ElapsedEventHandler(ResizingDone);

            //快速搜索功能
            SearchPanel.Install(te_editor.TextArea);
            //设置语法规则
            string codeHighlightPath = GlobalDictionary.codeHighlightPath;
            XmlTextReader codeReader = new XmlTextReader(codeHighlightPath);
            codeXshd = HighlightingLoader.LoadXshd(codeReader);
            string logHighlightPath = GlobalDictionary.logHighlightPath;
            XmlTextReader logReader = new XmlTextReader(logHighlightPath);
            logXshd = HighlightingLoader.LoadXshd(logReader);
            te_editor.SyntaxHighlighting = HighlightingLoader.Load(codeXshd, HighlightingManager.Instance);

            te_editor.TextArea.TextEntering += te_editor_TextArea_TextEntering;
            te_editor.TextArea.TextEntered += te_editor_TextArea_TextEntered;

            this.Background = GlobalDictionary.backgroundBrush;
            btn_minimize.Background = GlobalDictionary.minimizeBrush;
            btn_restore.Background = GlobalDictionary.restoreBrush;
            btn_preview.Background = GlobalDictionary.previewBrush;
            btn_run.Background = GlobalDictionary.runBrush;
            btn_pause.Background = GlobalDictionary.pauseBrush;
            btn_stop.Background = GlobalDictionary.stopBrush;
            btn_save.Background = GlobalDictionary.saveBrush;
            btn_exit.Background = GlobalDictionary.exitBrush;
            btn_forward.Background = GlobalDictionary.forwardBrush;
            btn_backward.Background = GlobalDictionary.backwardBrush;
            btn_auto.Background = GlobalDictionary.autoBrush;

            AddMapsFromFolder(GlobalDictionary.mapFolderPath);
            tb_select_folder.Text = GlobalDictionary.mapFolderPath;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (c_paintcanvas.IsHitTestVisible == false)
                this.DragMove();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            int resultIndex = MessageBox.Show(GlobalDictionary.propertiesSetter, new RefreshList { "保存后退出", "直接退出", new ButtonSpacer(100), "取消" }, "是否另存战术脚本", "正在退出程序", MessageBoxImage.Warning);
            if (MessageBox.ButtonList[resultIndex].ToString() == "取消")
            {
                return;
            }
            else if (MessageBox.ButtonList[resultIndex].ToString() == "保存后退出")
            {
                SaveFile();
            }
            Stop();

            IniHelper.WriteIni("Window", "Width", this.Width.ToString());
            IniHelper.WriteIni("Window", "Height", this.Height.ToString());

            Application.Current.Shutdown(-1);
        }


        private void i_map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (i_map.Source != null)
            {
                GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;
            }
            Point pointInMap = new Point(Math.Round((e.GetPosition(i_map).X / GlobalDictionary.ImageRatio), 2), Math.Round((e.GetPosition(i_map).Y / GlobalDictionary.ImageRatio), 2));
            tb_point.Text = pointInMap.ToString();
            if (me_pov.Visibility == Visibility.Visible)
            {
                me_pov.Stop();
                me_pov.Visibility = Visibility.Collapsed;
                g_povcontroller.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_select_folder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog selectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            selectFolderDialog.SelectedPath = GlobalDictionary.mapFolderPath;
            selectFolderDialog.ShowNewFolderButton = false;
            if (selectFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string folderPath = tb_select_folder.Text = selectFolderDialog.SelectedPath.Trim();

            AddMapsFromFolder(folderPath);
        }

        private void tb_select_folder_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddMapsFromFolder(tb_select_folder.Text.Trim());
        }

        private void AddMapsFromFolder(string folderPath)
        {
            cb_select_mapimg.Items.Clear();
            cb_select_mapframe.Items.Clear();

            DirectoryInfo imgDir = new DirectoryInfo(Path.Combine(folderPath, "mapimg"));
            if (!imgDir.Exists)
                return;
            DirectoryInfo imgDirD = imgDir as DirectoryInfo;
            FileSystemInfo[] imgFiles = imgDirD.GetFileSystemInfos();

            DirectoryInfo frameDir = new DirectoryInfo(Path.Combine(folderPath, "mapframe"));
            if (!imgDir.Exists)
                return;
            DirectoryInfo frameDirD = frameDir as DirectoryInfo;
            FileSystemInfo[] frameFiles = frameDirD.GetFileSystemInfos();

            mapList = new List<string>();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(GlobalDictionary.mapListPath);
            while ((line = file.ReadLine()) != null)
            {
                mapList.Add(line);
            }
            foreach (FileSystemInfo fileSystemInfo in imgFiles)
            {
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                if (fileInfo != null)
                {
                    foreach (string map in mapList)
                    {
                        if (fileInfo.Name.ToUpper().IndexOf(map.ToUpper()) != -1)
                        {
                            ComboBoxItem cbItem = new ComboBoxItem();
                            cbItem.Content = map;
                            cbItem.Tag = fileInfo.FullName;
                            cb_select_mapimg.Items.Add(cbItem);
                        }
                    }
                }
            }

            foreach (FileSystemInfo fileSystemInfo in frameFiles)
            {
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                if (fileInfo != null)
                {
                    ComboBoxItem cbItem = new ComboBoxItem();
                    cbItem.Content = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    cbItem.Tag = fileInfo.FullName;
                    cb_select_mapframe.Items.Add(cbItem);
                }
            }

            if (cb_select_mapimg.Items.Count > 0)
            {
                cb_select_mapimg.SelectedIndex = 0;
            }
        }

        private void cb_select_mapimg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cb_select_mapimg.SelectedItem;
            if (selectedItem != null)
            {
                i_map.Source = new BitmapImage(new Uri(selectedItem.Tag.ToString(), UriKind.Absolute));
                tb_infos.Visibility = Visibility.Visible;
                tb_timer.Visibility = Visibility.Visible;
            }
        }

        private void cb_select_mapframe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cb_select_mapframe.SelectedItem;
            if (selectedItem != null)
            {
                string frameJson;
                frameJson = File.ReadAllText((string)selectedItem.Tag);
                Newtonsoft.Json.JsonConvert.DeserializeObject<Map>(frameJson);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                btn_restore.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_restore.Visibility = Visibility.Visible;
            }

            OnSizeChanged(this, null);
        }

        private Label CreateChacterlabel(Character character, Point wndPoint, int hp)
        {
            Label name = new Label();
            name.IsHitTestVisible = false;
            name.Foreground = new SolidColorBrush(Colors.White);
            name.FontSize *= GlobalDictionary.ImageRatio * 1.3;
            name.Content = character.Name == "" ? character.Number.ToString() : character.Name;
            name.Padding = new Thickness(0);
            name.Margin = new Thickness(0);
            if (hp >= 0)
            {
                name.Content += " [" + hp + "]";
            }
            Canvas.SetLeft(name, wndPoint.X + character.CharacterImg.Width / 2);
            Canvas.SetTop(name, wndPoint.Y + character.CharacterImg.Height);

            character.CharacterLabel = name;

            return name;
        }

        private Label CreateChacterNumberlabel(Character character, Point wndPoint)
        {
            Label number = new Label();
            number.IsHitTestVisible = false;
            number.Foreground = new SolidColorBrush(Colors.White);
            number.FontSize *= GlobalDictionary.ImageRatio * 1.0;
            number.Content = character.Number;
            number.Padding = new Thickness(0);
            number.Margin = new Thickness(0);

            Canvas.SetLeft(number, wndPoint.X);
            Canvas.SetTop(number, wndPoint.Y + character.CharacterImg.Height);

            character.NumberLabel = number;

            return number;
        }

        public void NewCharacter(Character character, Point mapPoint)
        {
            Point wndPoint = GetWndPoint(mapPoint, ImgType.Character);

            Canvas.SetLeft(character.CharacterImg, wndPoint.X);
            Canvas.SetTop(character.CharacterImg, wndPoint.Y);

            if (character.SteamId <= 0)
            {
                c_runcanvas.Children.Add(character.CharacterImg);
                c_runcanvas.Children.Add(CreateChacterlabel(character, wndPoint, -1));
            }
        }

        public Point GetWndPoint(Point mapPoint, ImgType imgType)
        {
            Point scaledMapPoint = new Point(mapPoint.X * GlobalDictionary.ImageRatio, mapPoint.Y * GlobalDictionary.ImageRatio);
            Point wndPoint = new Point();
            i_map.Dispatcher.Invoke(() =>
            {
                wndPoint = i_map.TranslatePoint(scaledMapPoint, c_runcanvas);
            });
            int widthAndHeight = 0;
            switch (imgType)
            {
                case ImgType.Character:
                    widthAndHeight = GlobalDictionary.CharacterWidthAndHeight;
                    break;
                case ImgType.MissileEffect:
                    widthAndHeight = GlobalDictionary.MissileEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.ExplosionEffectWidthAndHeight;
                    break;
                case ImgType.Missile:
                    widthAndHeight = GlobalDictionary.MissileWidthAndHeight;
                    break;
                case ImgType.Props:
                    widthAndHeight = GlobalDictionary.PropsWidthAndHeight;
                    break;
                case ImgType.Nothing:
                    widthAndHeight = 0;
                    break;
            }
            wndPoint = new Point(wndPoint.X - widthAndHeight / 2, wndPoint.Y - widthAndHeight / 2);
            return wndPoint;
        }

        public Point GetMapPoint(Point wndPoint, ImgType imgType)
        {
            int widthAndHeight = 0;
            switch (imgType)
            {
                case ImgType.Character:
                    widthAndHeight = GlobalDictionary.CharacterWidthAndHeight;
                    break;
                case ImgType.MissileEffect:
                    widthAndHeight = GlobalDictionary.MissileEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.ExplosionEffectWidthAndHeight;
                    break;
                case ImgType.Props:
                    widthAndHeight = GlobalDictionary.PropsWidthAndHeight;
                    break;
                case ImgType.Missile:
                    widthAndHeight = GlobalDictionary.MissileWidthAndHeight;
                    break;
                case ImgType.Nothing:
                    widthAndHeight = 0;
                    break;
            }
            wndPoint = new Point(wndPoint.X + widthAndHeight / 2, wndPoint.Y + widthAndHeight / 2);
            Point scaledMapPoint = new Point();
            i_map.Dispatcher.Invoke(() =>
            {
                scaledMapPoint = c_runcanvas.TranslatePoint(wndPoint, i_map);
            });
            Point mapPoint = new Point(scaledMapPoint.X / GlobalDictionary.ImageRatio, scaledMapPoint.Y / GlobalDictionary.ImageRatio);
            return mapPoint;
        }

        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            if (i_map.Source == null)
            {
                return;
            }

            Stop();

            string filePath = tb_select_file.Text;
            if (Path.GetExtension(filePath) == ".txt" || filePath == "")
            {
                nowRunningType = RunningType.TXT;
                te_editor.Dispatcher.Invoke(() =>
                {
                    te_editor.SyntaxHighlighting = HighlightingLoader.Load(codeXshd, HighlightingManager.Instance);
                });

                string processedCommand = null;

                GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;

                bombDefused = false;

                CommandHelper.GetCommands(te_editor.Text);

                try
                {
                    foreach (string command in CommandHelper.commands)
                    {
                        processedCommand = command.Replace("\r", "").Trim();
                        Command commandType = CommandHelper.AnalysisCommand(command);
                        switch (commandType)
                        {
                            case Command.SetEntiretySpeed:
                                SetEntiretySpeed(processedCommand);
                                break;
                            case Command.ActionCharacterDo:
                                ActionCharacterDo(processedCommand);
                                break;
                            case Command.ActionCharacterAutoMove:
                                ActionCharacterAutoMove(processedCommand);
                                break;
                            case Command.ActionCharacterMove:
                                ActionCharacterMove(processedCommand);
                                break;
                            case Command.ActionCharacterShoot:
                                ActionCharacterShoot(processedCommand);
                                break;
                            case Command.ActionCharacterThrow:
                                ActionCharacterThrow(processedCommand);
                                break;
                            case Command.ActionCharacterWaitUntil:
                                ActionCharacterWaitUntil(processedCommand);
                                break;
                            case Command.ActionCharacterWaitFor:
                                ActionCharacterWaitFor(processedCommand);
                                break;
                            case Command.BadOrNotCommand:
                                break;
                            case Command.CreateCharacter:
                                CreateCharacter(processedCommand);
                                break;
                            case Command.CreateComment:
                                break;
                            case Command.CreateTeam:
                                break;
                            case Command.GiveCharacterMissile:
                                GiveCharacterMissile(processedCommand);
                                break;
                            case Command.GiveCharacterProps:
                                GiveCharacterProps(processedCommand);
                                break;
                            case Command.GiveCharacterWeapon:
                                GiveCharacterWeapon(processedCommand);
                                break;
                            case Command.SetCamp:
                                SetCamp(processedCommand);
                                break;
                            case Command.SetCharacterStatus:
                                SetCharacterStatus(processedCommand);
                                break;
                            case Command.SetCharacterVerticalPosition:
                                SetCharacterVerticalPosition(processedCommand);
                                break;

                            case Command.CreateMap:
                                CreateMap(processedCommand);
                                break;
                            case Command.CreateNode:
                                CreateNode(processedCommand);
                                break;
                            case Command.CreatePath:
                                CreatePath(processedCommand);
                                break;
                            case Command.DeleteNode:
                                DeleteNode(processedCommand);
                                break;
                            case Command.DeletePath:
                                DeletePath(processedCommand);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GlobalDictionary.propertiesSetter, new RefreshList { new ButtonSpacer(250), "确定" }, "解析命令 \"" + processedCommand + "\" 时出错\n错误信息: " + ex.Message, "错误", MessageBoxImage.Error);
                }

                StartTimer();

                TraversalAnimations();

                btn_restore.Visibility = Visibility.Collapsed;
                gs_gridsplitter.IsEnabled = false;
                ResizeMode = ResizeMode.NoResize;
                btn_pause.Tag = "R";
            }
            else
            {
                ReadDemo(filePath);
            }
        }
        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {
            if (i_map.Source == null || btn_pause.Tag == null)
            {
                return;
            }

            if (btn_pause.Tag.ToString() == "R")
            {
                ThreadHelper.PauseAllThread();
                btn_pause.Tag = "P";

                if (stopWatch != null)
                {
                    stopWatch.Stop();
                }
                if (stopWatchThisRound != null)
                {
                    stopWatchThisRound.Stop();
                }
                foreach (Stopwatch stopwatch in stopwatchList)
                {
                    if (stopwatch != null)
                    {
                        stopwatch.Stop();
                    }
                }

                btn_pause.Background = GlobalDictionary.resumeBrush;

                if (me_pov.Visibility == Visibility.Visible)
                {
                    me_pov.Pause();
                }
            }
            else
            {
                ThreadHelper.RestartAllThread();
                btn_pause.Tag = "R";

                if (stopWatch != null)
                {
                    stopWatch.Start();
                }
                if (stopWatchThisRound != null)
                {
                    stopWatchThisRound.Start();
                }
                foreach (Stopwatch stopwatch in stopwatchList)
                {
                    if (stopwatch != null)
                    {
                        stopwatch.Start();
                    }
                }

                btn_pause.Background = GlobalDictionary.pauseBrush;

                if (me_pov.Visibility == Visibility.Visible)
                {
                    me_pov.Play();
                }
            }
        }
        private void ReCreateJson(Map map)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(map);
            File.WriteAllText(Path.Combine(tb_select_folder.Text.Trim(), "mapframe", map.mapName + ".json"), json);
            AddMapsFromFolder(tb_select_folder.Text.Trim());
            foreach (ComboBoxItem item in cb_select_mapframe.Items)
            {
                if (item.Content.ToString() == map.mapName)
                {
                    cb_select_mapframe.SelectedItem = item;
                }
            }
        }
        private void CreateMap(string command)
        {
            // create map Mirage
            string[] splitedCmd = command.Split(' ');
            string mapName = splitedCmd[2];
            Map map = new Map(mapName);

            ReCreateJson(map);
            PreviewFrame();
        }
        private void CreateNode(string command)
        {
            // create node 100,100 layer 0
            string mapName = cb_select_mapframe.Text;
            string[] splitedCmd = command.Split(' ');
            Point point = VectorHelper.Parse(splitedCmd[2]);
            int layer = int.Parse(splitedCmd[4]);
            new MapNode(mapName, point, layer);

            ReCreateJson(GlobalDictionary.mapDic[mapName]);
            PreviewFrame();
        }
        private void CreatePath(string command)
        {
            // create path 0 to 1 2 3 4 5 limit allallowed mode twoway distance 100
            string[] splitedCmd = command.Split(' ');
            string mapName = cb_select_mapframe.Text;
            int mapNodeIndex = int.Parse(splitedCmd[2]);
            Point fromPoint = GlobalDictionary.mapDic[mapName].mapNodes[mapNodeIndex].nodePoint;
            int fromLayer = GlobalDictionary.mapDic[mapName].mapNodes[mapNodeIndex].layerNumber;
            string actionLimitStr = splitedCmd[Array.IndexOf(splitedCmd, "limit") + 1];
            ActionLimit actionLimit = ActionLimit.AllAllowed;
            foreach (ActionLimit actionLimitTemp in Enum.GetValues(typeof(ActionLimit)))
            {
                if (actionLimitTemp.ToString().ToLower() == actionLimitStr)
                {
                    actionLimit = actionLimitTemp;
                }
            }
            string directionModeStr = splitedCmd[Array.IndexOf(splitedCmd, "mode") + 1];
            DirectionMode directionMode = DirectionMode.OneWay;
            foreach (DirectionMode directionModeTemp in Enum.GetValues(typeof(DirectionMode)))
            {
                if (directionModeTemp.ToString().ToLower() == directionModeStr)
                {
                    directionMode = directionModeTemp;
                }
            }
            double distance = -1;
            int distanceIndex = Array.IndexOf(splitedCmd, "distance");
            if (distanceIndex != -1)
            {
                distance = double.Parse(splitedCmd[Array.IndexOf(splitedCmd, "distance") + 1]);
            }

            for (int i = 4; i <= Array.IndexOf(splitedCmd, "limit") - 1; ++i)
            {
                int neighbourIndex = int.Parse(splitedCmd[i]);
                new MapNode(mapName, fromPoint, fromLayer, distance, actionLimit, directionMode, GlobalDictionary.mapDic[mapName].mapNodes[neighbourIndex]);
            }

            ReCreateJson(GlobalDictionary.mapDic[mapName]);
            PreviewFrame();
        }
        private void DeleteNode(string command)
        {
            // delete node 0
            string[] splitedCmd = command.Split(' ');
            string mapName = cb_select_mapframe.Text;

            int removeIndex = int.Parse(splitedCmd[2]);
            Map map = GlobalDictionary.mapDic[mapName];

            map.mapNodes.RemoveAt(removeIndex);
            for (int i = 0; i < map.mapNodes.Count; ++i)
            {
                map.mapNodes[i].index = i;
                map.mapNodes[i].neighbourNodes.Remove(removeIndex);
                List<int> keyList = new List<int>(map.mapNodes[i].neighbourNodes.Keys);
                foreach (int key in keyList)
                {
                    if (key > removeIndex)
                    {
                        map.mapNodes[i].neighbourNodes.Add(key - 1, map.mapNodes[i].neighbourNodes[key]);
                        map.mapNodes[i].neighbourNodes.Remove(key);
                    }
                }
            }

            ReCreateJson(map);
            PreviewFrame();
        }
        private void DeletePath(string command)
        {
            // delete path 0 to 1 2 3 4 5 mode twoway
            string[] splitedCmd = command.Split(' ');
            string mapName = cb_select_mapframe.Text;

            int fromIndex = int.Parse(splitedCmd[2]);
            Map map = GlobalDictionary.mapDic[mapName];

            string directionModeStr = splitedCmd[Array.IndexOf(splitedCmd, "mode") + 1];
            DirectionMode directionMode = DirectionMode.OneWay;
            foreach (DirectionMode directionModeTemp in Enum.GetValues(typeof(DirectionMode)))
            {
                if (directionModeTemp.ToString().ToLower() == directionModeStr)
                {
                    directionMode = directionModeTemp;
                }
            }

            if (directionMode != DirectionMode.ReversedOneWay)
            {
                for (int i = 4; i <= Array.IndexOf(splitedCmd, "mode") - 1; ++i)
                {
                    int deleteIndex = int.Parse(splitedCmd[i]);
                    map.mapNodes[fromIndex].neighbourNodes.Remove(deleteIndex);
                }
            }
            if (directionMode != DirectionMode.OneWay)
            {
                foreach (MapNode mapNode in map.mapNodes)
                {
                    for (int i = 4; i <= Array.IndexOf(splitedCmd, "mode") - 1; ++i)
                    {
                        if (mapNode.index == int.Parse(splitedCmd[i]))
                        {
                            mapNode.neighbourNodes.Remove(fromIndex);
                        }
                    }
                }
            }

            ReCreateJson(GlobalDictionary.mapDic[mapName]);
            PreviewFrame();
        }

        private void StartTimer()
        {
            tb_timer.Text = "0";
            double timeDouble = 0;
            Thread timerThread = new Thread(() =>
            {
                while (timeDouble <= 90)
                {
                    Thread.Sleep(GlobalDictionary.animationFreshTime);
                    timeDouble = timeDouble + GlobalDictionary.animationFreshTime / 1000.0;
                    tb_timer.Dispatcher.Invoke(() =>
                    {
                        tb_timer.Text = Math.Round((timeDouble + GlobalDictionary.animationFreshTime / 1000.0), 2).ToString();
                    });
                }
            });
            timerThread.Start();
            ThreadHelper.AddThread(timerThread);
        }

        private void CreateCharacter(string command)
        {
            string[] splitedCmd = command.Split(' ');
            bool isT;
            bool isFriendly;
            Point mapPoint;
            isT = splitedCmd[2] == "t" ? true : false;
            isFriendly = currentCamp.ToString().ToLower() == splitedCmd[2] ? true : false;
            mapPoint = new Point(Double.Parse(splitedCmd[3].Split(',')[0]), Double.Parse(splitedCmd[3].Split(',')[1]));

            string name = "";
            if (splitedCmd.Count() >= 6 && splitedCmd[5].First() == '@')
            {
                name = command.Substring(command.IndexOf('@') + 1);
            }
            new Character(name, -1, isFriendly, isT, mapPoint, this);
        }

        private void SetCharacterStatus(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            Model.Status charactorStatus = Model.Status.Alive;
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Model.Status status in Enum.GetValues(typeof(Model.Status)))
                    {
                        if (status.ToString().ToLower() == splitedCmd[4])
                        {
                            charactorStatus = status;
                        }
                    }
                }
            }
            animations.Add(new Animation(number, Helper.Action.ChangeStatus, new Point(), charactorStatus));
        }

        private void SetCharacterVerticalPosition(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            Model.VerticalPosition charactorVerticalPosition = Model.VerticalPosition.Upper;
            for (int i = 0; i < CharacterHelper.GetCharacters().Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Model.VerticalPosition verticalPosition in Enum.GetValues(typeof(Model.VerticalPosition)))
                    {
                        if (verticalPosition.ToString().ToLower() == splitedCmd[5])
                        {
                            charactorVerticalPosition = verticalPosition;
                        }
                    }
                }
            }
            animations.Add(new Animation(number, Helper.Action.ChangeVerticalPosition, new Point(), charactorVerticalPosition));
        }

        private void SetCamp(string command)
        {
            string[] splitedCmd = command.Split(' ');
            currentCamp = splitedCmd[2] == "t" ? Camp.T : Camp.Ct;
        }
        private void SetEntiretySpeed(string command)
        {
            string[] splitedCmd = command.Split(' ');
            localSpeedController = double.Parse(splitedCmd[3]);
        }
        private void GiveCharacterWeapon(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            string weaponStr = splitedCmd[4];
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Weapon weapon in Enum.GetValues(typeof(Weapon)))
                    {
                        if (weapon.ToString().ToLower() == weaponStr)
                        {
                            characters[i].Weapon = weapon;
                        }
                    }
                }
            }
        }
        private void GiveCharacterMissile(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            List<Missile> missiles = new List<Missile>();
            for (int i = 4; i < splitedCmd.Count(); ++i)
            {
                foreach (Missile missile in Enum.GetValues(typeof(Missile)))
                {
                    if (splitedCmd[i] == missile.ToString().ToLower())
                    {
                        missiles.Add(missile);
                    }
                }
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Missile missile in missiles)
                    {
                        if (characters[i].Missiles.Count >= 4)
                        {
                            return;
                        }
                        if (missile != Missile.Flashbang && !characters[i].Missiles.Contains(missile))
                        {
                            characters[i].Missiles.Add(missile);
                        }
                        else if (missile == Missile.Flashbang)
                        {
                            int flashbangCount = 0;
                            foreach (Missile missileTemp in characters[i].Missiles)
                            {
                                if (missileTemp == Missile.Flashbang)
                                {
                                    ++flashbangCount;
                                }
                            }
                            if (flashbangCount <= 1)
                            {
                                characters[i].Missiles.Add(missile);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void GiveCharacterProps(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            string propsStr = splitedCmd[4];
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Props props in Enum.GetValues(typeof(Props)))
                    {
                        if (props.ToString().ToLower() == propsStr)
                        {
                            characters[i].Props = props;
                        }
                    }
                }
            }
        }
        private void ActionCharacterShoot(string command)
        {
            List<Character> characters = CharacterHelper.GetCharacters();
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            Point toMapPoint = new Point();
            int targetNumber;
            if (int.TryParse(splitedCmd[4], out targetNumber))
            {
                bool targetDie = splitedCmd[5] == "die" ? true : false;
                for (int i = 0; i < characters.Count; ++i)
                {
                    if (characters[i].Number == targetNumber)
                    {
                        animations.Add(new Animation(number, Helper.Action.Shoot, toMapPoint, characters[i], targetDie));
                        return;
                    }
                }
            }
            else
            {
                toMapPoint = VectorHelper.Parse(splitedCmd[4]);
                animations.Add(new Animation(number, Helper.Action.Shoot, toMapPoint));
            }
        }
        private void ActionCharacterWaitUntil(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            double second = double.Parse(splitedCmd[5]);
            animations.Add(new Animation(number, Helper.Action.WaitUntil, new Point(), second));
        }
        private void ActionCharacterWaitFor(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            double second = double.Parse(splitedCmd[5]);
            animations.Add(new Animation(number, Helper.Action.WaitFor, new Point(), second));
        }
        private void ActionCharacterDo(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            string doWithPropsStr = splitedCmd[4];
            DoWithProps doWithProps = DoWithProps.Defuse;
            foreach (DoWithProps doWithPropsTemp in Enum.GetValues(typeof(DoWithProps)))
            {
                if (doWithPropsTemp.ToString().ToLower() == doWithPropsStr)
                {
                    doWithProps = doWithPropsTemp;
                }
            }
            Helper.Action action;
            if (doWithProps == DoWithProps.Defuse)
            {
                action = Helper.Action.Defuse;
            }
            else
            {
                action = Helper.Action.Plant;
            }
            animations.Add(new Animation(number, action, new Point()));
        }
        private void ActionCharacterAutoMove(string command)
        {
            // action character 1 layer 0 auto move 0,0 layer 0 [quietly / noisily]
            // action character 1 from 0,0 layer 0 auto move 0,0 layer 0 [quietly / noisily]
            List<string> replaceCommandList = new List<string>();

            Map mapFrame = GlobalDictionary.mapDic[cb_select_mapframe.Text];
            if (mapFrame == null)
            {
                return;
            }
            string[] splitedCmd = command.Split(' ');
            int characterNumber = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            Point startMapPoint = new Point();
            Character character = null;
            int startLayer;
            Point endMapPoint = new Point();
            int endLayer;
            VolumeLimit volumeLimit;
            if (!command.Contains("from"))
            {
                foreach (Character characterTemp in CharacterHelper.GetCharacters())
                {
                    if (characterTemp.Number == characterNumber)
                    {
                        startMapPoint = characterTemp.MapPoint;
                        character = characterTemp;
                    }
                }
                startLayer = int.Parse(splitedCmd[4]);
                endMapPoint = new Point(double.Parse(splitedCmd[7].Split(',')[0]), double.Parse(splitedCmd[7].Split(',')[1]));
                endLayer = int.Parse(splitedCmd[9]);
                volumeLimit = splitedCmd[10] == VolumeLimit.Noisily.ToString().ToLower() ? VolumeLimit.Noisily : VolumeLimit.Quietly;
            }
            else
            {
                foreach (Character characterTemp in CharacterHelper.GetCharacters())
                {
                    if (characterTemp.Number == characterNumber)
                    {
                        character = characterTemp;
                    }
                }
                startLayer = int.Parse(splitedCmd[6]);
                startMapPoint = PathfindingHelper.GetNearestNode(VectorHelper.Parse(splitedCmd[4]), startLayer, mapFrame).nodePoint;
                endMapPoint = new Point(double.Parse(splitedCmd[9].Split(',')[0]), double.Parse(splitedCmd[9].Split(',')[1]));
                endLayer = int.Parse(splitedCmd[11]);
                volumeLimit = splitedCmd[12] == VolumeLimit.Noisily.ToString().ToLower() ? VolumeLimit.Noisily : VolumeLimit.Quietly;
            }

            // 寻找与起始点最近的同层的节点
            MapNode startNode = PathfindingHelper.GetNearestNode(startMapPoint, startLayer, mapFrame);
            string startCommand = "action character" + " " + characterNumber + " " + "move" + " " + (volumeLimit == VolumeLimit.Noisily ? "run" : "walk") + " " + startNode.nodePoint;
            ActionCharacterMove(startCommand);
            replaceCommandList.Add(startCommand);
            // 寻找与结束点最近的同层的节点
            MapNode endNode = PathfindingHelper.GetNearestNode(endMapPoint, endLayer, mapFrame);
            string endCommand = "action character" + " " + characterNumber + " " + "move" + " " + (volumeLimit == VolumeLimit.Noisily ? "run" : "walk") + " " + endMapPoint;

            List<MapNode> mapPathNodes = PathfindingHelper.GetMapPathNodes(startNode, endNode, mapFrame, volumeLimit);
            for (int i = 0; i < mapPathNodes.Count - 1; ++i)
            {
                string currentCommand;
                MapNode currentStartNode = mapPathNodes[i];
                MapNode currentEndNode = mapPathNodes[i + 1];
                WayInfo wayInfo = currentStartNode.neighbourNodes[currentEndNode.index];

                double runSpeed = double.Parse(IniHelper.ReadIni("RunSpeed", character.Weapon.ToString()));
                double speedController = localSpeedController != -1 ? localSpeedController : GlobalDictionary.speedController;
                if (currentStartNode.nodePoint == currentEndNode.nodePoint)
                {
                    // 爬梯子
                    if (volumeLimit == VolumeLimit.Quietly || wayInfo.actionLimit == ActionLimit.WalkClimb)
                    {
                        // 静音爬梯子
                        // 准备增加爬梯速度的比率, 因此if和else算了两遍速度
                        runSpeed *= GlobalDictionary.walkToRunRatio;
                    }

                    double pixelPerFresh = runSpeed / (1000 / GlobalDictionary.animationFreshTime) * GlobalDictionary.ImageRatio * speedController;
                    double climbTime = (wayInfo.distance / pixelPerFresh) * (GlobalDictionary.animationFreshTime / 1000.0);
                    currentCommand = "action character" + " " + characterNumber + " " + "wait for" + " " + climbTime;
                    ActionCharacterWaitFor(currentCommand);
                }
                else
                {
                    if (volumeLimit == VolumeLimit.Quietly)
                    {
                        // 静音移动
                        if (wayInfo.actionLimit == ActionLimit.SquatOnly)
                        {
                            currentCommand = "action character" + " " + characterNumber + " " + "move" + " " + "squat" + " " + currentEndNode.nodePoint;
                        }
                        else
                        {
                            currentCommand = "action character" + " " + characterNumber + " " + "move" + " " + "walk" + " " + currentEndNode.nodePoint;
                        }
                    }
                    else
                    {
                        // 可以跑动, 取决于地图限制
                        if (wayInfo.actionLimit == ActionLimit.SquatOnly)
                        {
                            currentCommand = "action character" + " " + characterNumber + " " + "move" + " " + "squat" + " " + currentEndNode.nodePoint;
                        }
                        else if (wayInfo.actionLimit == ActionLimit.WalkOnly || wayInfo.actionLimit == ActionLimit.WalkOrSquat)
                        {
                            currentCommand = "action character" + " " + characterNumber + " " + "move" + " " + "walk" + " " + currentEndNode.nodePoint;
                        }
                        else
                        {
                            currentCommand = "action character" + " " + characterNumber + " " + "move" + " " + "run" + " " + currentEndNode.nodePoint;
                        }
                    }
                    ActionCharacterMove(currentCommand);
                }
                replaceCommandList.Add(currentCommand);
            }

            replaceCommandList.Add(endCommand);
            ActionCharacterMove(endCommand);

            te_editor.Text += "\n\n";
            te_editor.Text += "\n--------------------------------------------";
            te_editor.Text += "\n- " + command + ": \n";
            foreach (string commandStr in replaceCommandList)
            {
                te_editor.Text += "\n- " + commandStr;
            }
        }

        private void ActionCharacterMove(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            Helper.Action action = Helper.Action.Run;
            if (splitedCmd[4] == Helper.Action.Run.ToString().ToLower())
            {
                action = Helper.Action.Run;
            }
            else if (splitedCmd[4] == Helper.Action.Walk.ToString().ToLower())
            {
                action = Helper.Action.Walk;
            }
            else if (splitedCmd[4] == Helper.Action.Squat.ToString().ToLower())
            {
                action = Helper.Action.Squat;
            }
            else if (splitedCmd[4] == Helper.Action.WaitUntil.ToString().ToLower())
            {
                action = Helper.Action.WaitUntil;
            }
            else if (splitedCmd[4] == Helper.Action.Teleport.ToString().ToLower())
            {
                action = Helper.Action.Teleport;
            }
            else if (splitedCmd[4] == Helper.Action.ChangeStatus.ToString().ToLower())
            {
                action = Helper.Action.Teleport;
            }

            List<Point> mapPoints = new List<Point>();
            for (int i = 5; i < splitedCmd.Count(); ++i)
            {
                mapPoints.Add(VectorHelper.Parse(splitedCmd[i]));
            }
            animations.Add(new Animation(number, action, new Point(), mapPoints));
        }
        private void ActionCharacterThrow(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = CharacterHelper.GetCharacter(splitedCmd[2]).Number;
            string missileStr = splitedCmd[4];
            Helper.Action action = Helper.Action.Throw;
            Missile missile = Missile.Nothing;
            foreach (Missile missileTemp in Enum.GetValues(typeof(Missile)))
            {
                if (missileTemp.ToString().ToLower() == missileStr)
                {
                    missile = missileTemp;
                }
            }
            List<Point> mapPoints = new List<Point>();
            for (int i = 5; i < splitedCmd.Count(); ++i)
            {
                mapPoints.Add(VectorHelper.Parse(splitedCmd[i]));
            }
            animations.Add(new Animation(number, action, new Point(), missile, mapPoints));
        }
        private void TraversalAnimations()
        {
            foreach (Animation animation in animations)
            {
                if (animation.status != Helper.Status.Waiting)
                {
                    continue;
                }
                int ownerIndex = animation.ownerIndex;
                foreach (Character character in CharacterHelper.GetCharacters())
                {
                    if (character.Number == ownerIndex && character.IsRunningAnimation == false)
                    {
                        character.IsRunningAnimation = true;
                        RunAnimation(character, animation);
                        break;
                    }
                }
            }
        }

        private void RunAnimation(Character character, Animation animation)
        {
            double runSpeed = double.Parse(IniHelper.ReadIni("RunSpeed", character.Weapon.ToString()));
            double missileSpeed = double.Parse(IniHelper.ReadIni("Missile", "Speed"));
            switch (animation.action)
            {
                case Helper.Action.Run:
                    RunAnimationMove(character, animation, runSpeed);
                    break;
                case Helper.Action.Walk:
                    runSpeed *= GlobalDictionary.walkToRunRatio;
                    RunAnimationMove(character, animation, runSpeed);
                    break;
                case Helper.Action.Squat:
                    runSpeed *= GlobalDictionary.squatToRunRatio;
                    RunAnimationMove(character, animation, runSpeed);
                    break;
                case Helper.Action.Teleport:
                    runSpeed = -1;
                    RunAnimationMove(character, animation, runSpeed);
                    break;
                case Helper.Action.WaitUntil:
                    RunAnimationWaitUntil(character, animation);
                    break;
                case Helper.Action.WaitFor:
                    RunAnimationWaitFor(character, animation);
                    break;
                case Helper.Action.Shoot:
                    RunAnimationShoot(character, animation);
                    break;
                case Helper.Action.Throw:
                    RunAnimationThrow(character, animation, missileSpeed);
                    break;
                case Helper.Action.ChangeStatus:
                    RunAnimationChangeStatus(character, animation);
                    break;
                case Helper.Action.ChangeVerticalPosition:
                    RunAnimationChangeVerticalPosition(character, animation);
                    break;
                case Helper.Action.Defuse:
                    RunAnimationDefuse(character, animation);
                    break;
                case Helper.Action.Plant:
                    RunAnimationPlant(character, animation);
                    break;
            }
        }
        private void RunAnimationWaitUntil(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            double second = (double)animation.objectPara[0];
            Thread waitUntilThread = new Thread(() =>
            {
                double nowTime = 0;
                do
                {
                    Thread.Sleep((int)(GlobalDictionary.animationFreshTime));
                    tb_timer.Dispatcher.Invoke(() =>
                    {
                        nowTime = double.Parse(tb_timer.Text);
                    });
                }
                while (nowTime < second);

                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    TraversalAnimations();
                }));
            });
            waitUntilThread.Start();
            ThreadHelper.AddThread(waitUntilThread);
        }

        private void RunAnimationWaitFor(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            double second = (double)animation.objectPara[0];
            Thread waitForThread = new Thread(() =>
            {
                Thread.Sleep((int)(second * 1000));

                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    TraversalAnimations();
                }));
            });
            waitForThread.Start();
            ThreadHelper.AddThread(waitForThread);
        }
        private void RunAnimationChangeVerticalPosition(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            characters[characters.IndexOf(character)].VerticalPosition = (Model.VerticalPosition)animation.objectPara[0];

            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }
        private void RunAnimationDefuse(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            int defuseTime = 10;
            if (characters[characters.IndexOf(character)].Props == Props.DefuseKit)
            {
                defuseTime = 5;
            }

            bool canDefuse = false;

            Image img = null;
            foreach (UIElement obj in c_runcanvas.Children)
            {
                if (obj is Image)
                {
                    img = (Image)obj;
                    BitmapImage bitmapImage = (BitmapImage)img.Source;
                    if (bitmapImage.UriSource.LocalPath.Contains("props_bomb"))
                    {
                        if (VectorHelper.GetDistance(character.MapPoint, GetMapPoint(new Point(Canvas.GetLeft(c_runcanvas.Children[c_runcanvas.Children.IndexOf(obj)]), Canvas.GetTop(c_runcanvas.Children[c_runcanvas.Children.IndexOf(obj)])), ImgType.Props)) <= 50)
                        {
                            canDefuse = true;
                            break;
                        }
                    }
                }
            }
            if (!canDefuse)
            {
                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;
                return;
            }
            Thread defuseThread = new Thread(() =>
            {
                Thread.Sleep(defuseTime * 1000);

                if (character.Status == Model.Status.Dead)
                {
                    characters[characters.IndexOf(character)].IsRunningAnimation = false;
                    animations[animations.IndexOf(animation)].status = Helper.Status.Finished;
                    return;
                }

                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    c_runcanvas.Children.Remove(img);
                    bombDefused = true;
                });

                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    TraversalAnimations();
                }));
            });
            defuseThread.Start();
            ThreadHelper.AddThread(defuseThread);
        }

        private void RunAnimationPlant(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            if (characters[characters.IndexOf(character)].Props == Props.Bomb)
            {
                Image bombImage = new Image();
                bombImage.Source = new BitmapImage(new Uri(GlobalDictionary.bombPath));
                bombImage.Width = GlobalDictionary.PropsWidthAndHeight;
                bombImage.Height = GlobalDictionary.PropsWidthAndHeight;
                bombImage.Opacity = 0.75;
                Point bombWndPoint = GetWndPoint(character.MapPoint, ImgType.Props);

                Image explosionImage = new Image();
                explosionImage.Source = new BitmapImage(new Uri(GlobalDictionary.explosionPath));
                explosionImage.Width = GlobalDictionary.ExplosionEffectWidthAndHeight;
                explosionImage.Height = GlobalDictionary.ExplosionEffectWidthAndHeight;
                Point explosionWndPoint = GetWndPoint(character.MapPoint, ImgType.ExplosionEffect);
                Thread explosionThread = null;
                Thread plantThread = new Thread(() =>
                {
                    Thread.Sleep(4000);

                    if (character.Status == Model.Status.Dead)
                    {
                        characters[characters.IndexOf(character)].IsRunningAnimation = false;
                        animations[animations.IndexOf(animation)].status = Helper.Status.Finished;
                        return;
                    }

                    c_runcanvas.Dispatcher.Invoke(() =>
                    {
                        Canvas.SetLeft(bombImage, bombWndPoint.X);
                        Canvas.SetTop(bombImage, bombWndPoint.Y);
                        c_runcanvas.Children.Add(bombImage);
                    });
                    characters[characters.IndexOf(character)].Props = Props.Nothing;

                    characters[characters.IndexOf(character)].IsRunningAnimation = false;
                    animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        TraversalAnimations();
                    }));

                    explosionThread = new Thread(() =>
                    {
                        Thread.Sleep(30000);
                        if (bombDefused)
                        {
                            return;
                        }
                        c_runcanvas.Dispatcher.Invoke(() =>
                        {
                            c_runcanvas.Children.Remove(bombImage);
                        });

                        c_runcanvas.Dispatcher.Invoke(() =>
                        {
                            Canvas.SetLeft(explosionImage, explosionWndPoint.X);
                            Canvas.SetTop(explosionImage, explosionWndPoint.Y);
                            c_runcanvas.Children.Add(explosionImage);
                        });
                        Thread.Sleep(3000);
                        c_runcanvas.Dispatcher.Invoke(() =>
                        {
                            c_runcanvas.Children.Remove(explosionImage);
                        });
                    });
                    explosionThread.Start();
                    ThreadHelper.AddThread(explosionThread);
                });
                plantThread.Start();
                ThreadHelper.AddThread(plantThread);
            }
        }
        private void RunAnimationChangeStatus(Character character, Animation animation)
        {
            List<Character> characters = CharacterHelper.GetCharacters();

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            characters[characters.IndexOf(character)].Status = (Model.Status)animation.objectPara[0];

            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }
        private void RunAnimationMove(Character character, Animation animation, double speed)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            double speedController = localSpeedController != -1 ? localSpeedController : GlobalDictionary.speedController;

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * GlobalDictionary.ImageRatio * speedController;
            Point startWndPoint = GetWndPoint(character.MapPoint, ImgType.Character);
            // Point endWndPoint = GetWndPoint(animation.endMapPoint, ImgType.Character);
            List<Point> endMapPointList = (List<Point>)animation.objectPara[0];
            List<Point> endWndPointList = new List<Point>();
            foreach (Point endMapPoint in endMapPointList)
            {
                endWndPointList.Add(GetWndPoint(endMapPoint, ImgType.Character));
            }

            if (speed == -1)
            {
                characters[characters.IndexOf(character)].IsRunningAnimation = true;
                animations[animations.IndexOf(animation)].status = Helper.Status.Running;

                try
                {
                    Label label = null;
                    foreach (FrameworkElement frameworkElement in c_runcanvas.Children)
                    {
                        if (frameworkElement is Label)
                        {
                            if ((frameworkElement as Label).Content.ToString() == (character.Name == "" ? character.Number.ToString() : character.Name))
                            {
                                label = (Label)frameworkElement;
                            }
                        }
                    }
                    c_runcanvas.Children.Remove(character.CharacterImg);
                    c_runcanvas.Children.Remove(label);

                    Canvas.SetLeft(character.CharacterImg, endWndPointList.Last().X);
                    Canvas.SetTop(character.CharacterImg, endWndPointList.Last().Y);

                    c_runcanvas.Children.Add(character.CharacterImg);
                    c_runcanvas.Children.Add(CreateChacterlabel(character, endWndPointList.Last(), -1));

                    characters[characters.IndexOf(character)].MapPoint = GetMapPoint(endWndPointList.Last(), ImgType.Character);
                }
                catch
                {
                    Stop();
                }
                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    TraversalAnimations();
                }));
            }
            else
            {
                Point nowWndPoint = startWndPoint;
                Thread moveThread = null;
                moveThread = new Thread(() =>
                {
                    foreach (Point endWndPoint in endWndPointList)
                    {
                        Point unitVector = VectorHelper.GetUnitVector(nowWndPoint, endWndPoint);

                        characters[characters.IndexOf(character)].IsRunningAnimation = true;
                        animations[animations.IndexOf(animation)].status = Helper.Status.Running;
                        while (VectorHelper.GetDistance(VectorHelper.GetUnitVector(nowWndPoint, endWndPoint), unitVector) < 1)
                        {
                            nowWndPoint = VectorHelper.Add(nowWndPoint, VectorHelper.Multiply(unitVector, pixelPerFresh));
                            characters[characters.IndexOf(character)].MapPoint = GetMapPoint(nowWndPoint, ImgType.Character);
                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    Label label = null;
                                    foreach (FrameworkElement frameworkElement in c_runcanvas.Children)
                                    {
                                        if (frameworkElement is Label)
                                        {
                                            if ((frameworkElement as Label).Content.ToString() == (character.Name == "" ? character.Number.ToString() : character.Name))
                                            {
                                                label = (Label)frameworkElement;
                                            }
                                        }
                                    }
                                    c_runcanvas.Children.Remove(character.CharacterImg);
                                    c_runcanvas.Children.Remove(label);

                                    Canvas.SetLeft(character.CharacterImg, nowWndPoint.X);
                                    Canvas.SetTop(character.CharacterImg, nowWndPoint.Y);

                                    c_runcanvas.Children.Add(character.CharacterImg);
                                    c_runcanvas.Children.Add(CreateChacterlabel(character, nowWndPoint, -1));
                                }
                                catch
                                {
                                    Stop();
                                }
                            });

                            Thread.Sleep(animationFreshTime);
                        }


                        nowWndPoint = endWndPoint;
                    }
                    characters[characters.IndexOf(character)].IsRunningAnimation = false;
                    animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                    Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        TraversalAnimations();
                    }));

                });
                moveThread.Start();
                ThreadHelper.AddThread(moveThread);
            }
        }

        private void RunAnimationThrow(Character character, Animation animation, double speed)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            double speedController = localSpeedController != -1 ? localSpeedController : GlobalDictionary.speedController;

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * GlobalDictionary.ImageRatio * speedController;
            Point startWndPoint = GetWndPoint(character.MapPoint, ImgType.Character);
            Missile missile = (Missile)animation.objectPara[0];
            List<Point> mapPoints = (List<Point>)animation.objectPara[1];
            List<Point> wndPoints = new List<Point>();
            foreach (Point mapPoint in mapPoints)
            {
                wndPoints.Add(GetWndPoint(mapPoint, ImgType.Missile));
            }

            Image missileImg = new Image();
            Image missileEffectImg = null;
            int effectLifeSpan = 0;
            switch (missile)
            {
                case Missile.Decoy:
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.decoyPath));
                    break;
                case Missile.Firebomb:
                    missileEffectImg = new Image();
                    if (character.IsT)
                    {
                        missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.molotovPath));
                        missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.fireEffectPath));
                    }
                    else
                    {
                        missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.incgrenadePath));
                        missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.fireEffectPath));
                    }
                    effectLifeSpan = GlobalDictionary.firebombLifespan;
                    break;
                case Missile.Flashbang:
                    missileEffectImg = new Image();
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.flashbangPath));
                    missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.flashEffectPath));
                    effectLifeSpan = GlobalDictionary.flashbangLifespan;
                    break;
                case Missile.Grenade:
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.hegrenadePath));
                    break;
                case Missile.Smoke:
                    missileEffectImg = new Image();
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.smokePath));
                    missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.smokeEffectPath));
                    effectLifeSpan = GlobalDictionary.smokeLifespan;
                    break;
            }
            missileImg.Width = GlobalDictionary.MissileWidthAndHeight;
            missileImg.Height = GlobalDictionary.MissileWidthAndHeight;

            if (missileEffectImg != null)
            {
                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
            }

            if (characters[characters.IndexOf(character)].Missiles.Contains(missile))
            {
                characters[characters.IndexOf(character)].Missiles.Remove(missile);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    Stop();
                    MessageBox.Show(GlobalDictionary.propertiesSetter, new RefreshList { new ButtonSpacer(250), "确定" }, "角色" + character.Number + "未持有道具" + missile.ToString() + ", 可能是没有配备, 或已经被扔出, 因此无法投掷. ", "错误", MessageBoxImage.Warning);
                }));

            }
            Thread throwThread = new Thread(() =>
            {
                Point nowWndPoint = startWndPoint;
                foreach (Point wndPoint in wndPoints)
                {
                    Point unitVector = VectorHelper.GetUnitVector(startWndPoint, wndPoint);
                    while (VectorHelper.GetDistance(VectorHelper.GetUnitVector(nowWndPoint, wndPoint), unitVector) < 1)
                    {
                        nowWndPoint = VectorHelper.Add(nowWndPoint, VectorHelper.Multiply(unitVector, pixelPerFresh));

                        c_runcanvas.Dispatcher.Invoke(() =>
                        {
                            if (c_runcanvas.Children.Contains(missileImg))
                            {
                                c_runcanvas.Children.Remove(missileImg);
                            }
                            Canvas.SetLeft(missileImg, nowWndPoint.X);
                            Canvas.SetTop(missileImg, nowWndPoint.Y);
                            c_runcanvas.Children.Add(missileImg);
                        });

                        Thread.Sleep(animationFreshTime);
                    }
                    startWndPoint = nowWndPoint;
                }
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    if (c_runcanvas.Children.Contains(missileImg))
                    {
                        c_runcanvas.Children.Remove(missileImg);
                    }
                });

                nowWndPoint = GetWndPoint(GetMapPoint(nowWndPoint, ImgType.Missile), ImgType.MissileEffect);
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(missileEffectImg, nowWndPoint.X);
                    Canvas.SetTop(missileEffectImg, nowWndPoint.Y);
                    c_runcanvas.Children.Add(missileEffectImg);
                });
                Thread.Sleep(effectLifeSpan * 1000);
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    if (c_runcanvas.Children.Contains(missileEffectImg))
                    {
                        c_runcanvas.Children.Remove(missileEffectImg);
                    }
                });
            });
            throwThread.Start();
            ThreadHelper.AddThread(throwThread);

            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }

        private void RunAnimationShoot(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            Point fromMapPoint = character.MapPoint;
            Point fromWndPoint = GetWndPoint(fromMapPoint, ImgType.Nothing);
            Point toWndPoint = new Point();

            if (animation.objectPara.Count() > 0)
            {
                Character targetCharacter = (Character)animation.objectPara[0];
                if ((bool)animation.objectPara[1] == false)
                {
                    characters[characters.IndexOf(targetCharacter)].Status = Model.Status.Alive;
                }
                else
                {
                    characters[characters.IndexOf(targetCharacter)].Status = Model.Status.Dead;
                }
                toWndPoint = GetWndPoint(characters[characters.IndexOf(targetCharacter)].MapPoint, ImgType.Nothing);
            }
            else
            {
                toWndPoint = GetWndPoint(animation.endMapPoint, ImgType.Nothing);
            }

            Line bulletLine = new Line();
            bulletLine.Stroke = Brushes.White;
            bulletLine.StrokeThickness = 2;
            bulletLine.StrokeDashArray = new DoubleCollection() { 2, 3 };
            bulletLine.StrokeDashCap = PenLineCap.Triangle;
            bulletLine.StrokeEndLineCap = PenLineCap.Triangle;
            bulletLine.StrokeStartLineCap = PenLineCap.Square;
            bulletLine.X1 = fromWndPoint.X;
            bulletLine.Y1 = fromWndPoint.Y;
            bulletLine.X2 = toWndPoint.X;
            bulletLine.Y2 = toWndPoint.Y;
            c_runcanvas.Children.Add(bulletLine);

            Thread shootThread = new Thread(() =>
            {
                Thread.Sleep(500);
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    c_runcanvas.Children.Remove(bulletLine);
                });
            });
            shootThread.Start();
            ThreadHelper.AddThread(shootThread);

            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }

        public void ShowCharacterInfos(object sender, MouseEventArgs e)
        {
            List<Character> characters = CharacterHelper.GetCharacters();

            foreach (Character character in characters)
            {
                if (character.CharacterImg == sender)
                {
                    string missileListStr = "";
                    foreach (Missile missile in character.Missiles)
                    {
                        missileListStr += missile.ToString() + " / ";
                    }
                    if (character.Missiles.Count >= 1)
                    {
                        missileListStr = missileListStr.Substring(0, missileListStr.Length - 3);
                    }
                    else
                    {
                        missileListStr = "Nothing";
                    }
                    tb_infos.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
                    if (nowRunningType == RunningType.TXT)
                    {
                        tb_infos.Text =
                        "Number: " + character.Number +
                        "\nName: " + character.Name +
                        "\nPosisiion: " + character.MapPoint.ToString() +
                        "\nWeapon: " + character.Weapon.ToString() +
                        "\nMissile: " + missileListStr +
                        "\nprops: " + character.Props.ToString();
                    }
                    else if (nowRunningType == RunningType.DEM)
                    {
                        string weapons = "";
                        string missileEquipments = "";
                        string equips = "";
                        int money = character.Money;
                        foreach (Equipment equipment in character.WeaponEquipmentList)
                        {
                            weapons += equipment.Weapon.ToString() + " ";
                        }
                        if (weapons.Length >= 1)
                        {
                            weapons = weapons.Remove(weapons.Length - 1, 1);
                        }
                        else
                        {
                            weapons = "none";
                        }
                        foreach (Equipment equipment in character.MissileEquipList)
                        {
                            missileEquipments += equipment.Weapon.ToString() + " ";
                        }
                        if (missileEquipments.Length >= 1)
                        {
                            missileEquipments = missileEquipments.Remove(missileEquipments.Length - 1, 1);
                        }
                        else
                        {
                            missileEquipments = "none";
                        }
                        foreach (Equipment equipment in character.EquipList)
                        {
                            equips += equipment.Weapon.ToString() + " ";
                        }
                        if (equips.Length >= 1)
                        {
                            equips = equips.Remove(equips.Length - 1, 1);
                        }
                        else
                        {
                            equips = "none";
                        }

                        tb_infos.Text =
                        "Number: " + character.Number +
                        "\nName: " + character.Name +
                        "\n⚔Weapons: " + weapons +
                        "\n🧨Missiles: " + missileEquipments +
                        "\n⚙Equipment: " + equips +
                        "\n💰Money: " + money +
                        "\n♥HP: " + character.Hp;
                    }

                    break;
                }
            }
        }

        private void ResetInfoPanelFontStyle()
        {
            foreach (UIElement element in g_infos.Children)
            {
                if (element is TextBlock)
                {
                    TextBlock textBlock = (TextBlock)element;
                    if (textBlock != tb_team1 && textBlock != tb_team2)
                    {
                        textBlock.Foreground = Brushes.White;
                        textBlock.TextDecorations = null;
                    }
                }
            }
        }

        private void SetInfos(int tScore, int ctScore)
        {
            if (tb_team1.Tag == null || tb_team2.Tag == null)
            {
                return;
            }

            List<Character> characters = CharacterHelper.GetCharacters();

            int team1Money = 0;
            string team1Camp = "";
            int team1Score = 0;
            int team2Money = 0;
            string team2Camp = "";
            int team2Score = 0;

            foreach (Character character in characters)
            {
                if (character.SteamId == (long)tb_team1.Tag)
                {
                    team1Camp = character.IsT ? "T" : "CT";
                    team1Score = character.IsT ? tScore : ctScore;
                }
                else if (character.SteamId == (long)tb_team2.Tag)
                {
                    team2Camp = character.IsT ? "T" : "CT";
                    team2Score = character.IsT ? tScore : ctScore;
                }

                foreach (UIElement element in g_infos.Children)
                {
                    if (element is TextBlock)
                    {
                        TextBlock textBlock = (TextBlock)element;
                        if (textBlock == tb_team1 || textBlock == tb_team2)
                        {
                            textBlock.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
                            textBlock.Margin = new Thickness(GlobalDictionary.ImageRatio * 10);
                        }
                        else
                        {
                            textBlock.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.0;
                            textBlock.Margin = new Thickness(GlobalDictionary.ImageRatio * 5);
                        }
                        if ((long)textBlock.Tag == character.SteamId && textBlock != tb_team1 && textBlock != tb_team2)
                        {
                            if (!character.IsAlive)
                            {
                                textBlock.Foreground = Brushes.Red;
                                textBlock.TextDecorations = TextDecorations.Strikethrough;
                            }
                            else
                            {
                                if (character.Hp <= 20)
                                {
                                    textBlock.Foreground = Brushes.DeepPink;
                                }
                                else
                                {
                                    textBlock.Foreground = Brushes.White;
                                }
                                textBlock.TextDecorations = null;
                            }
                            if (g_infos.Tag.Equals("DefaultInfo"))
                            {
                                string weapons = "";
                                string missileEquipments = "";
                                string equips = "";
                                int money = character.Money;
                                if (Grid.GetColumn(textBlock) == 0)
                                {
                                    team1Money += money;
                                }
                                else
                                {
                                    team2Money += money;
                                }
                                foreach (Equipment equipment in character.LastAliveInfo.WeaponEquipmentList)
                                {
                                    weapons += equipment.Weapon.ToString() + " ";
                                }
                                if (weapons.Length >= 1)
                                {
                                    weapons = weapons.Remove(weapons.Length - 1, 1);
                                }
                                else
                                {
                                    weapons = "none";
                                }
                                foreach (Equipment equipment in character.LastAliveInfo.MissileEquipList)
                                {
                                    missileEquipments += equipment.Weapon.ToString() + " ";
                                }
                                if (missileEquipments.Length >= 1)
                                {
                                    missileEquipments = missileEquipments.Remove(missileEquipments.Length - 1, 1);
                                }
                                else
                                {
                                    missileEquipments = "none";
                                }
                                foreach (Equipment equipment in character.LastAliveInfo.EquipList)
                                {
                                    equips += equipment.Weapon.ToString() + " ";
                                }
                                if (equips.Length >= 1)
                                {
                                    equips = equips.Remove(equips.Length - 1, 1);
                                }
                                else
                                {
                                    equips = "none";
                                }

                                textBlock.Text =
                                "Number: " + character.Number +
                                "\nName: " + character.Name +
                                "\n⚔Weapons: " + weapons +
                                "\n🧨Missiles: " + missileEquipments +
                                "\n⚙Equipment: " + equips +
                                "\n💰Money: " + money +
                                "\n♥HP: " + character.Hp;
                            }
                            else
                            {
                                textBlock.Text =
                                "Number: " + character.Number +
                                "\nName: " + character.Name +
                                "\nK/D/A: " + character.AdditionalPlayerInformation.Kills + "/" + character.AdditionalPlayerInformation.Deaths + "/" + character.AdditionalPlayerInformation.Assists +
                                "\nKD: " + ((double)character.AdditionalPlayerInformation.Kills / character.AdditionalPlayerInformation.Deaths).ToString("#0.00") +
                                "\nScore: " + character.AdditionalPlayerInformation.Score +
                                "\nMVPs: " + character.AdditionalPlayerInformation.MVPs +
                                "\nPing: " + character.AdditionalPlayerInformation.Ping +
                                "\nTotalCashSpent: " + character.AdditionalPlayerInformation.TotalCashSpent;
                            }
                        }
                    }
                }
            }

            tb_team1.Text = "Camp: " + team1Camp;
            if (g_infos.Tag.Equals("DefaultInfo"))
            {
                tb_team1.Text += "\nScore: " + team1Score +
                "\nEconomy: " + team1Money;
            }

            tb_team2.Text = "Camp: " + team2Camp;
            if (g_infos.Tag.Equals("DefaultInfo"))
            {
                tb_team2.Text += "\nScore: " + team2Score +
                "\nEconomy: " + team2Money;
            }
        }

        public void ShowPov(object sender, MouseEventArgs e)
        {
            List<Character> characters = CharacterHelper.GetCharacters();

            if (e != null)
            {
                isNeedAutomaticGuidance = false;
            }

            foreach (Character character in characters)
            {
                if (character.CharacterImg == sender)
                {
                    if (me_pov.Source != null && me_pov.Source.AbsolutePath.ToLowerInvariant().Contains(character.Name.ToLowerInvariant()))
                    {
                        me_pov.Play();
                        return;
                    }

                    List<string> files = new List<string>();

                    string povsFolder = tb_select_file.Text;
                    DirectoryInfo povFolder = null;
                    povsFolder = povsFolder.Replace(new FileInfo(povsFolder).Name, "");
                    povsFolder = Path.Combine(povsFolder, "povs", tb_timer.Tag.ToString());
                    if (Directory.Exists(povsFolder))
                    {
                        povFolder = new DirectoryInfo(povsFolder);
                        foreach (FileInfo file in povFolder.GetFiles())
                        {
                            if (character.Name.ToLowerInvariant() == file.Name.Replace(file.Extension, "").ToLowerInvariant())
                            {
                                double currentTime = double.Parse(character.CharacterImg.Tag.ToString().Split('|')[1]);
                                me_pov.Tag = 1;
                                PlayPov(file, currentTime);
                                return;
                            }
                        }
                    }

                    povsFolder = tb_select_file.Text;
                    povsFolder = povsFolder.Replace(new FileInfo(povsFolder).Name, "");
                    povsFolder = Path.Combine(povsFolder, "povs");
                    povFolder = new DirectoryInfo(povsFolder);
                    if (povFolder.Exists)
                    {
                        foreach (FileInfo file in povFolder.GetFiles())
                        {
                            if (character.Name.ToLowerInvariant() == file.Name.Replace(file.Extension, "").ToLowerInvariant())
                            {
                                double currentTime = double.Parse(character.CharacterImg.Tag.ToString().Split('|')[0]);
                                me_pov.Tag = 0;
                                PlayPov(file, currentTime);
                                return;
                            }
                        }
                    }

                    break;
                }
            }
        }

        private void PlayPov(FileInfo file, double currentTime)
        {
            me_pov.Visibility = Visibility.Visible;
            g_povcontroller.Visibility = Visibility.Visible;
            me_pov.LoadedBehavior = MediaState.Manual;
            me_pov.Source = new Uri(file.FullName);

            me_pov.Play();
            me_pov.Position = new TimeSpan(0, 0, 0, 0, (int)(currentTime * 1000));
            return;
        }

        public void ShowCharacterImgInfos(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            tb_infos.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
            tb_infos.Text = img.Tag.ToString();
        }
        private void btn_point_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(tb_point.Text);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = Global.GlobalDictionary.exePath;
            sfd.Filter = "txt file|*.txt";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, te_editor.Text);
            }
        }

        private void btn_select_file_Click(object sender, RoutedEventArgs e)
        {
            string folder = "";
            if (tb_select_file.Text.Trim() != "" && Directory.Exists(tb_select_file.Text.Trim()))
            {
                folder = tb_select_file.Text.Trim();
            }
            else if (tb_select_file.Text.Trim() != "" && File.Exists(tb_select_file.Text.Trim()))
            {
                folder = Path.GetDirectoryName(tb_select_file.Text);
            }
            else
            {
                folder = GlobalDictionary.exePath;
            }
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.InitialDirectory = folder;
            ofd.DefaultExt = ".txt";
            ofd.Filter = "脚本或demo (*.txt, *.dem)|*.txt; *.dem";
            if (ofd.ShowDialog() == true)
            {
                string filePath = ofd.FileName;
                if (Path.GetExtension(filePath) == ".txt")
                {
                    if (!File.Exists(filePath))
                    {
                        PropertiesSetter newPropertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
                        newPropertiesSetter.CloseTimer = new MessageBoxCloseTimer(3, 0);
                        MessageBox.Show(newPropertiesSetter, "文件\n" + filePath + "\n不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    tb_select_file.Text = filePath;
                    te_editor.Text = File.ReadAllText(filePath);
                }
                else
                {
                    tb_select_file.Text = filePath;
                    Stop();
                    ReadDemo(filePath);
                }
            }
        }

        private void ReadDemo(string filePath)
        {
            nowRunningType = RunningType.DEM;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            te_editor.Dispatcher.Invoke(() =>
            {
                te_editor.SyntaxHighlighting = HighlightingLoader.Load(logXshd, HighlightingManager.Instance);
            });

            if (i_map.Source == null)
            {
                return;
            }
            if (!File.Exists(filePath))
            {
                PropertiesSetter newPropertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
                newPropertiesSetter.CloseTimer = new MessageBoxCloseTimer(3, 0);
                MessageBox.Show(newPropertiesSetter, "文件\n" + filePath + "\n不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var setting = new JsonSerializerSettings();
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            setting.MissingMemberHandling = MissingMemberHandling.Ignore;

            PropertiesSetter propertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
            propertiesSetter.LoadedEventHandler = new RoutedEventHandler((s, e) =>
            {
                (MessageBox.ButtonList[0] as TextBox).Focus();
            });
            propertiesSetter.KeyDownEventHandler += new KeyEventHandler((s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    MessageBox.CloseNow(3);
                }
            });
            propertiesSetter.EnableCloseButton = true;
            int res = MessageBox.Show(propertiesSetter, new RefreshList {
                new TextBox() { VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 10, 5, 10), Width = 50, Height = 32, FontSize = 20 },
                new CheckBox() { Content = "自动显示信息面板" , VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 10, 5, 10), Width = 150, Foreground = new SolidColorBrush(Colors.White), IsChecked = true},
                new ButtonSpacer(200),
                "OK" }, "需要观看第几回合? ", "选择回合数", MessageBoxImage.Question);

            int roundNumber = 0;
            bool isAutoShowInfoPanel = true;
            if (res == -1)
            {
                return;
            }
            else
            {
                if (!int.TryParse((MessageBox.ButtonList[0] as TextBox).Text, out roundNumber))
                {
                    PropertiesSetter newPropertiesSetter = new PropertiesSetter(propertiesSetter);
                    newPropertiesSetter.CloseTimer = new MessageBoxCloseTimer(3, 0);
                    MessageBox.Show(newPropertiesSetter, "请输入数字", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                isAutoShowInfoPanel = (MessageBox.ButtonList[1] as CheckBox).IsChecked == true ? true : false;
            }

            Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>> eventDic = new Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>>();
            List<Tuple<CurrentInfo, EventArgs, string, int>> eventList = new List<Tuple<CurrentInfo, EventArgs, string, int>>();
            DemoParser parser = new DemoParser(File.OpenRead(filePath));

            bool isRoundAnnounceMatchStarted = false;
            float tickTime = -1;
            float firstFreezetimeEndedTime = -1;

            Dictionary<long, int> dic = new Dictionary<long, int>();

            int nowCanRun = -1;


            foreach (UIElement item in g_infos.Children)
            {
                if (item is TextBlock)
                {
                    (item as TextBlock).Tag = null;
                }
            }


            btn_pause.Tag = "R";
            btn_pause.Background = GlobalDictionary.pauseBrush;
            Thread totalThread = new Thread(() =>
            {
                while (true)
                {
                    while (nowCanRun != roundNumber)
                    {
                        Thread.Sleep(GlobalDictionary.animationFreshTime);
                    }

                    if (!eventDic.ContainsKey(roundNumber))
                    {
                        break;
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        tb_timer.Tag = (roundNumber).ToString();
                    });

                    stopwatchList.Clear();

                    Thread analizeDemoThread = new Thread(AnalizeDemo);
                    analizeDemoThread.Start(new Tuple<Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>>, int, Dictionary<long, int>, float, float, bool>(eventDic, roundNumber, dic, tickTime, firstFreezetimeEndedTime, isAutoShowInfoPanel));
                    ThreadHelper.AddThread(analizeDemoThread);
                    ++roundNumber;
                    analizeDemoThread.Join();

                    this.Dispatcher.Invoke(() =>
                    {
                        c_runcanvas.Children.Clear();
                    });
                }

                roundNumber = -1;
            });
            totalThread.Name = "totalThread";
            totalThread.Start();
            ThreadHelper.AddThread(totalThread);

            List<int> loggedRoundList = new List<int>();

            parser.ParseHeader();
            if (!parser.Map.ToUpper().Contains(cb_select_mapimg.SelectedValue.ToString().ToUpper()))
            {
                foreach (ComboBoxItem item in cb_select_mapimg.Items)
                {
                    if (parser.Map.ToUpper().Contains(item.Content.ToString().ToUpper()))
                    {
                        cb_select_mapimg.SelectedItem = item;
                    }
                }
            }

            int lastPlayerKilledIndex = 0;

            te_editor.Text = "";
            if (float.IsNaN(parser.TickTime))
            {
                int resTickTime = MessageBox.Show(propertiesSetter, new RefreshList { new TextBox() { VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 10, 5, 10), Width = 50, Height = 32, FontSize = 20 }, new ButtonSpacer(350), "OK" }, "该demo是多少ticks的?", "未知ticks", MessageBoxImage.Information);
                if (resTickTime == -1)
                {
                    return;
                }
                else
                {
                    int result;
                    if (int.TryParse((MessageBox.ButtonList[0] as TextBox).Text, out result))
                    {
                        tickTime = 1 / (float)result;
                    }
                    else
                    {
                        PropertiesSetter newPropertiesSetter = new PropertiesSetter(propertiesSetter);
                        newPropertiesSetter.CloseTimer = new MessageBoxCloseTimer(3, 0);
                        MessageBox.Show(newPropertiesSetter, "请输入数字", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
            parser.DecoyNadeEnded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "DecoyNadeEnded", dic[parseE.ThrownBy.SteamID]));
            };
            //parser.FireNadeStarted += (parseSender, parseE) =>
            //{
            //};
            parser.FireNadeWithOwnerStarted += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.ThrownBy = parseE.ThrownBy.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "FireNadeWithOwnerStarted", dic[parseE.ThrownBy.SteamID]));
            };
            parser.FireNadeEnded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "FireNadeEnded", dic[parseE.ThrownBy.SteamID]));
            };
            parser.ExplosiveNadeExploded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "ExplosiveNadeExploded", dic[parseE.ThrownBy.SteamID]));
            };
            parser.FlashNadeExploded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "FlashNadeExploded", dic[parseE.ThrownBy.SteamID]));
            };
            parser.ExplosiveNadeExploded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "ExplosiveNadeExploded", dic[parseE.ThrownBy.SteamID]));
            };
            //parser.NadeReachedTarget += (parseSender, parseE) =>
            //{
            //    if (!isAnalizeToLastRound && ((parseSender as DemoParser).TScore + (parseSender as DemoParser).CTScore + 1) != roundNumber)
            //    {
            //        return;
            //    }
            //    if (!isRoundAnnounceMatchStarted)
            //    {
            //        return;
            //    }
            //    DemoParser parseSenderClone = (parseSender as DemoParser).Clone();
            //    eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "NadeReachedTarget", dic[parseE.ThrownBy.SteamID]));
            //};
            parser.BombBeginPlant += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombBeginPlant", dic[parseE.Player.SteamID]));
            };
            parser.BombAbortPlant += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombAbortPlant", dic[parseE.Player.SteamID]));
            };
            parser.BombExploded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombExploded", dic[parseE.Player.SteamID]));
            };
            parser.BombDefused += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombDefused", dic[parseE.Player.SteamID]));
            };
            parser.BombBeginDefuse += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombBeginDefuse", dic[parseE.Player.SteamID]));
            };
            parser.BombAbortDefuse += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombAbortDefuse", dic[parseE.Player.SteamID]));
            };
            //parser.SayText += (parseSender, parseE) =>
            //{
            //};
            //parser.PlayerDisconnect += (parseSender, parseE) =>
            //{
            //};
            //parser.PlayerBind += (parseSender, parseE) =>
            //{
            //    //TODO
            //};
            parser.DecoyNadeStarted += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.ThrownBy = parseE.ThrownBy.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "DecoyNadeStarted", dic[parseE.ThrownBy.SteamID]));
            };
            parser.Blind += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                parseE.Attacker = parseE.Attacker.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "Blind", dic[parseE.Player.SteamID]));
            };
            //parser.PlayerHurt += (parseSender, parseE) =>
            //{
            //    // TODO
            //};
            parser.BombPlanted += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.Player = parseE.Player.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "BombPlanted", dic[parseE.Player.SteamID]));
            };
            parser.SmokeNadeEnded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "SmokeNadeEnded", dic[parseE.ThrownBy.SteamID]));
            };
            //parser.RoundOfficiallyEnd += (parseSender, parseE) =>
            //{
            //};
            parser.WeaponFired += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                if (parseE.Weapon.Weapon == EquipmentElement.Knife)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                Dictionary<long, List<Equipment>> equipDic = new Dictionary<long, List<Equipment>>();
                List<Equipment> equipList = new List<Equipment>();
                foreach (Equipment weapon in parseE.Shooter.Weapons)
                {
                    for (int n = 1; n <= weapon.ReserveAmmo; ++n)
                    {
                        equipList.Add(new Equipment(weapon.OriginalString));
                    }
                }
                equipDic[parseE.Shooter.SteamID] = equipList;
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants, equipDic);
                parseE.Shooter = parseE.Shooter.Copy();
                parseE.Weapon = new Equipment(parseE.Weapon.OriginalString);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "WeaponFired", dic[parseE.Shooter.SteamID]));
            };
            //parser.HeaderParsed += (parseSender, parseE) =>
            //{
            //};
            //parser.MatchStarted += (parseSender, parseE) =>
            //{
            //};
            parser.RoundAnnounceMatchStarted += (parseSender, parseE) =>
            {
                isRoundAnnounceMatchStarted = true;

                DemoParser demoParser = parseSender as DemoParser;

                IEnumerable<Player> playingParticipants = demoParser.PlayingParticipants.OrderBy(Player => Player.SteamID).OrderBy(Player => Player.Team);
                foreach (Player player in playingParticipants)
                {
                    if (player.Team == Team.Spectate)
                    {
                        continue;
                    }
                    if (dic.ContainsKey(player.SteamID))
                    {
                        continue;
                    }
                    Point mapPoint = DemoPointToMapPoint(player.Position, demoParser.Map);
                    bool camp = false;
                    if (player.Team == Team.Terrorist)
                    {
                        camp = false;
                    }
                    else
                    {
                        camp = true;
                    }

                    Character character = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        character = new Character(player.Name, player.SteamID, camp, camp, mapPoint, this, true);
                    });
                    dic.Add(character.SteamId, character.Number);

                    InitInfoTag(player);
                }

                if (demoParser.TScore + demoParser.CTScore >= 1)
                {
                    eventDic[demoParser.TScore + demoParser.CTScore] = eventList;
                    eventList = new List<Tuple<CurrentInfo, EventArgs, string, int>>();

                    if (eventDic.Count >= 1)
                    {
                        List<int> keyList = new List<int>();
                        foreach (int key in eventDic.Keys)
                        {
                            keyList.Add(key);
                        }
                        foreach (int key in keyList)
                        {
                            if (key < roundNumber)
                            {
                                eventDic[key] = null;
                                eventDic.Remove(key);
                            }
                        }

                        if (demoParser.TScore + demoParser.CTScore == roundNumber && eventDic.ContainsKey(demoParser.TScore + demoParser.CTScore))
                        {
                            nowCanRun = demoParser.TScore + demoParser.CTScore;
                        }
                    }
                }
                te_editor.Dispatcher.Invoke(() =>
                {
                    if (loggedRoundList.Count >= 1 && loggedRoundList.Last() == demoParser.TScore + demoParser.CTScore + 1)
                    {
                        te_editor.Dispatcher.Invoke(() =>
                        {
                            List<string> splitedList = te_editor.Text.Trim().Split("\n".ToCharArray()).ToList();
                            if (splitedList.Count >= 1)
                            {
                                splitedList.RemoveAt(splitedList.Count - 1);
                            }
                            te_editor.Text = String.Join("\n", splitedList);
                            if (te_editor.Text.Length > 0)
                            {
                                te_editor.Text += "\n";
                            }
                            te_editor.ScrollToEnd();
                        });
                    }
                    if ((loggedRoundList.Count >= 1 && loggedRoundList.Last() == demoParser.TScore + demoParser.CTScore + 1) || (!loggedRoundList.Contains(demoParser.TScore + demoParser.CTScore + 1)))
                    {
                        te_editor.Text += "Round " + (demoParser.TScore + demoParser.CTScore + 1) + ": [T: " + demoParser.TScore + "; CT: " + demoParser.CTScore + "]\n";
                        loggedRoundList.Add(demoParser.TScore + demoParser.CTScore + 1);
                        te_editor.ScrollToEnd();
                    }
                });

                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "RoundAnnounceMatchStarted", 0));

            };
            parser.RoundStart += (parseSender, parseE) =>
            {
                DemoParser demoParser = parseSender as DemoParser;

                //CharacterHelper.ClearCharacters();

                IEnumerable<Player> playingParticipants = demoParser.PlayingParticipants.OrderBy(Player => Player.SteamID).OrderBy(Player => Player.Team);
                foreach (Player player in playingParticipants)
                {
                    if (player.Team == Team.Spectate)
                    {
                        continue;
                    }
                    if (dic.ContainsKey(player.SteamID))
                    {
                        continue;
                    }
                    Point mapPoint = DemoPointToMapPoint(player.Position, demoParser.Map);
                    bool camp = false;
                    if (player.Team == Team.Terrorist)
                    {
                        camp = false;
                    }
                    else
                    {
                        camp = true;
                    }

                    Character character = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        character = new Character(player.Name, player.SteamID, camp, camp, mapPoint, this, true);
                    });

                    dic.Add(character.SteamId, character.Number);

                    InitInfoTag(player);
                }
                // 一局开始

                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);

                if (demoParser.TScore + demoParser.CTScore >= 1)
                {
                    eventDic[demoParser.TScore + demoParser.CTScore] = eventList;
                    eventList = new List<Tuple<CurrentInfo, EventArgs, string, int>>();

                    if (eventDic.Count >= 1)
                    {
                        List<int> keyList = new List<int>();
                        foreach (int key in eventDic.Keys)
                        {
                            keyList.Add(key);
                        }
                        foreach (int key in keyList)
                        {
                            if (key < roundNumber)
                            {
                                eventDic[key] = null;
                                eventDic.Remove(key);
                            }
                        }

                        if (demoParser.TScore + demoParser.CTScore == roundNumber && eventDic.ContainsKey(demoParser.TScore + demoParser.CTScore))
                        {
                            nowCanRun = demoParser.TScore + demoParser.CTScore;
                        }
                    }
                }
                te_editor.Dispatcher.Invoke(() =>
                {
                    if (loggedRoundList.Count >= 1 && loggedRoundList.Last() == demoParser.TScore + demoParser.CTScore + 1)
                    {
                        te_editor.Dispatcher.Invoke(() =>
                        {
                            List<string> splitedList = te_editor.Text.Trim().Split("\n".ToCharArray()).ToList();
                            if (splitedList.Count >= 1)
                            {
                                splitedList.RemoveAt(splitedList.Count - 1);
                            }
                            te_editor.Text = String.Join("\n", splitedList);
                            if (te_editor.Text.Length > 0)
                            {
                                te_editor.Text += "\n";
                            }
                            te_editor.ScrollToEnd();
                        });
                    }
                    if ((loggedRoundList.Count >= 1 && loggedRoundList.Last() == demoParser.TScore + demoParser.CTScore + 1) || (!loggedRoundList.Contains(demoParser.TScore + demoParser.CTScore + 1)))
                    {
                        if (isRoundAnnounceMatchStarted)
                        {
                            te_editor.Text += "Round " + (demoParser.TScore + demoParser.CTScore + 1) + ": [T: " + demoParser.TScore + "; CT: " + demoParser.CTScore + "]\n";
                            loggedRoundList.Add(demoParser.TScore + demoParser.CTScore + 1);
                            te_editor.ScrollToEnd();
                        }
                    }
                });

                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "RoundStart", 0));
            };
            parser.SmokeNadeStarted += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                parseE.ThrownBy = parseE.ThrownBy.Copy();
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "SmokeNadeStarted", dic[parseE.ThrownBy.SteamID]));
            };
            //parser.WinPanelMatch += (parseSender, parseE) =>
            //{
            //};
            //parser.RoundFinal += (parseSender, parseE) =>
            //{
            //};
            parser.RoundEnd += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                // 一局结束
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }

                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "RoundEnd", 0));
            };
            //parser.SayText2 += (parseSender, parseE) =>
            //{
            //};
            //parser.RoundMVP += (parseSender, parseE) =>
            //{
            //};
            //parser.BotTakeOver += (parseSender, parseE) =>
            //{
            //};
            parser.FreezetimeEnded += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (firstFreezetimeEndedTime == -1)
                {
                    if (tickTime != -1)
                    {
                        firstFreezetimeEndedTime = tickTime * parser.CurrentTick;
                    }
                    else
                    {
                        firstFreezetimeEndedTime = parser.CurrentTime;
                    }
                }

                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "FreezetimeEnded", 0));
            };
            parser.TickDone += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }
                //for (int i = 1; i < parser.Participants.Count(); ++i)
                //{
                //    dic[parser.Participants.ElementAt(i).SteamID].Add(new Tuple<DemoParser, EventArgs, string>(parseSenderClone, parseE, "TickDone"));
                //}
                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();

                Dictionary<long, List<Equipment>> missileEquipDic = new Dictionary<long, List<Equipment>>();
                Dictionary<long, List<Equipment>> weaponEquipDic = new Dictionary<long, List<Equipment>>();
                Dictionary<long, List<Equipment>> equipmentDic = new Dictionary<long, List<Equipment>>();

                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    Player copiedPlayer = playingParticipant.Copy();
                    copiedPlayer.Money = playingParticipant.Money;
                    copiedPlayer.AdditionaInformations = new DemoInfo.AdditionalPlayerInformation();
                    copiedPlayer.AdditionaInformations.Kills = playingParticipant.AdditionaInformations.Kills;
                    copiedPlayer.AdditionaInformations.Deaths = playingParticipant.AdditionaInformations.Deaths;
                    copiedPlayer.AdditionaInformations.Assists = playingParticipant.AdditionaInformations.Assists;
                    copiedPlayer.AdditionaInformations.Score = playingParticipant.AdditionaInformations.Score;
                    copiedPlayer.AdditionaInformations.MVPs = playingParticipant.AdditionaInformations.MVPs;
                    copiedPlayer.AdditionaInformations.Ping = playingParticipant.AdditionaInformations.Ping;
                    copiedPlayer.AdditionaInformations.Clantag = playingParticipant.AdditionaInformations.Clantag;
                    copiedPlayer.AdditionaInformations.TotalCashSpent = playingParticipant.AdditionaInformations.Assists;
                    nowParticipants.Add(copiedPlayer);

                    List<Equipment> missileEquipList = new List<Equipment>();
                    List<Equipment> weaponEquipList = new List<Equipment>();
                    List<Equipment> equipmentList = new List<Equipment>();

                    foreach (Equipment weapon in playingParticipant.Weapons)
                    {
                        if (weapon.Class == EquipmentClass.Equipment)
                        {
                            equipmentList.Add(new Equipment(weapon.OriginalString));
                        }
                        else if (weapon.Class != EquipmentClass.Grenade)
                        {
                            weaponEquipList.Add(new Equipment(weapon.OriginalString));
                        }
                        else
                        {
                            for (int n = 1; n <= weapon.ReserveAmmo; ++n)
                            {
                                missileEquipList.Add(new Equipment(weapon.OriginalString));
                            }
                        }
                    }
                    missileEquipDic[playingParticipant.SteamID] = missileEquipList;
                    weaponEquipDic[playingParticipant.SteamID] = weaponEquipList;
                    equipmentDic[playingParticipant.SteamID] = equipmentList;
                }

                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants, missileEquipDic, weaponEquipDic, equipmentDic);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "TickDone", 0));
            };
            parser.PlayerKilled += (parseSender, parseE) =>
            {
                if ((parser.CTScore + parser.TScore + 1) < roundNumber)
                {
                    return;
                }
                if (!isRoundAnnounceMatchStarted)
                {
                    return;
                }

                DemoParser nowParser = (parseSender as DemoParser);
                List<Player> nowParticipants = new List<Player>();
                foreach (Player playingParticipant in nowParser.PlayingParticipants)
                {
                    nowParticipants.Add(playingParticipant.Copy());
                }
                CurrentInfo currentInfo = new CurrentInfo(nowParser.TScore, nowParser.CTScore, nowParser.CurrentTick, nowParser.CurrentTime, nowParser.Map, nowParser.TickTime, nowParticipants);
                if (parseE.Victim != null)
                {
                    parseE.Victim = parseE.Victim.Copy();
                }
                if (parseE.Killer != null)
                {
                    parseE.Killer = parseE.Killer.Copy();
                }
                if (parseE.Assister != null)
                {
                    parseE.Assister = parseE.Assister.Copy();
                }
                parseE.Weapon = new Equipment(parseE.Weapon.OriginalString);
                eventList.Add(new Tuple<CurrentInfo, EventArgs, string, int>(currentInfo, parseE, "PlayerKilled", 0));

                if (parseE.Weapon.Weapon == EquipmentElement.Molotov || parseE.Weapon.Weapon == EquipmentElement.HE || parseE.Weapon.Weapon == EquipmentElement.Flash || parseE.Weapon.Weapon == EquipmentElement.Decoy || parseE.Weapon.Weapon == EquipmentElement.Smoke || parseE.Weapon.Weapon == EquipmentElement.Incendiary)
                {
                    return;
                }
                int thisIndex = eventList.Count - 1;
                int middleIndex = (thisIndex + lastPlayerKilledIndex) / 2;
                int ticksCount = 0;
                for (int i = middleIndex; i < thisIndex; ++i)
                {
                    Tuple<CurrentInfo, EventArgs, string, int> currentEvent = eventList[i];

                    EventArgs eventArgs = currentEvent.Item2;
                    if (eventArgs is TickDoneEventArgs)
                    {
                        ++ticksCount;
                    }
                }
                double costTime = 0;
                if (tickTime == -1)
                {
                    costTime = ((parseSender as DemoParser).TickTime * ticksCount);
                }
                else
                {
                    costTime = tickTime * ticksCount;
                }
                if (costTime < 1)
                {
                    return;
                }
                eventList.Insert(middleIndex, new Tuple<CurrentInfo, EventArgs, string, int>(null, parseE, null, -1));
                lastPlayerKilledIndex = eventList.Count - 1;
            };
            //parser.PlayerTeam += (parseSender, parseE) =>
            //{
            //};
            //parser.LastRoundHalf += (parseSender, parseE) =>
            //{
            //};
            //parser.RankUpdate += (parseSender, parseE) =>
            //{
            //};

            te_editor.Text = "";
            Thread analyzeThread = new Thread(() =>
            {
                try
                {
                    while (roundNumber != -1)
                    {
                        if (nowCanRun < roundNumber)
                        {
                            if (!parser.ParseNextTick())
                            {
                                eventDic[parser.TScore + parser.CTScore] = eventList;
                                while (roundNumber != -1)
                                {
                                    if (nowCanRun < roundNumber)
                                    {
                                        ++nowCanRun;
                                    }
                                    else
                                    {
                                        Thread.Sleep(GlobalDictionary.animationFreshTime);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(GlobalDictionary.animationFreshTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is ThreadAbortException))
                    {
                        te_editor.Dispatcher.Invoke(() =>
                        {
                            te_editor.Text += "ERROR: " + ex.Message + "\n";
                            te_editor.ScrollToEnd();
                        });
                    }
                }
            });

            analyzeThread.Name = "analyzeThread";
            analyzeThread.Start();
            ThreadHelper.AddThread(analyzeThread);
        }

        private void AnalizeDemo(object obj)
        {
            Tuple<Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>>, int, Dictionary<long, int>, float, float, bool> tupleTemp = (Tuple<Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>>, int, Dictionary<long, int>, float, float, bool>)obj;
            Dictionary<int, List<Tuple<CurrentInfo, EventArgs, string, int>>> eventDic = tupleTemp.Item1;
            int roundNumber = tupleTemp.Item2;
            Dictionary<long, int> dic = tupleTemp.Item3;
            float tickTime = tupleTemp.Item4;
            float firstFreezetimeEndedTime = tupleTemp.Item5;
            bool isAutoShowInfoPanel = tupleTemp.Item6;
            // double runSpeed = double.Parse(IniHelper.ReadIni("RunSpeed", character.Weapon.ToString()));
            //double missileSpeed = double.Parse(IniHelper.ReadIni("Missile", "Speed"));

            int animationFreshTime = GlobalDictionary.animationFreshTime;

            Dictionary<int, int> usedMissileDic = new Dictionary<int, int>();

            //double lastActionTime = 0;

            bool isFreezetimeEnded = false;

            float startTime = 0;
            float realCostTime = 0;

            stopWatch = new Stopwatch();
            stopWatch.Start();
            float elapsedTimeMillisecondInDemo = 0;

            int thisOffset = 0;
            offset = 0;
            isForward = false;
            isBackward = false;

            bool isNeedRefreshPov = false;
            isNeedAutomaticGuidance = false;

            List<Tuple<CurrentInfo, EventArgs, string, int>> eventList = eventDic[roundNumber];
            List<KeyValuePair<int, TSImage>> missileEffectKeyValuePairList = new List<KeyValuePair<int, TSImage>>();
            List<KeyValuePair<KeyValuePair<int, int>, TSImage>> missileKeyValuePairList = new List<KeyValuePair<KeyValuePair<int, int>, TSImage>>();
            TimeSpan roundTimeSpan = new TimeSpan(0, 1, 56);
            TimeSpan timeSpanWhenBombPlanted = new TimeSpan(0);
            TimeSpan offsetWhenBombPlanted = new TimeSpan(0);

            bool beginPlantOrDefuse = false;

            this.Dispatcher.Invoke(() =>
            {
                GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;
                tb_infos.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
                tb_timer.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
                List<Character> characterList = CharacterHelper.GetCharacters();
                foreach (Character character in characterList)
                {
                    character.CharacterImg.Width = GlobalDictionary.CharacterWidthAndHeight;
                    character.CharacterImg.Height = GlobalDictionary.CharacterWidthAndHeight;
                }
            });

            for (int i = 0; i < eventList.Count; ++i)
            {
                TimeSpan roundLeaveTimeSpan = roundTimeSpan - (stopWatch.Elapsed - timeSpanWhenBombPlanted) - (new TimeSpan(0, 0, 0, 0, offset) - offsetWhenBombPlanted);
                int roundLeaveMinutes = (int)roundLeaveTimeSpan.Minutes;
                int roundLeaveSeconds = roundLeaveTimeSpan.Seconds % 60;
                this.Dispatcher.Invoke(() =>
                {
                    tb_timer.Text = "Round: " + (roundNumber).ToString() + "\n" + roundLeaveMinutes.ToString() + ":" + roundLeaveSeconds.ToString();
                });

                Tuple<CurrentInfo, EventArgs, string, int> currentEvent = eventList[i];

                CurrentInfo currentInfo = currentEvent.Item1;
                EventArgs eventArgs = currentEvent.Item2;
                string eventName = currentEvent.Item3;
                int characterNumber = currentEvent.Item4;

                if (isForward)
                {
                    if (thisOffset < GlobalDictionary.forwardTimeSpan)
                    {
                        if (currentEvent.Item2 is TickDoneEventArgs)
                        {
                            if (currentEvent.Item3 == "TickDone")
                            {
                                if (tickTime == -1)
                                {
                                    thisOffset += (int)(currentInfo.TickTime * 1000);
                                }
                                else
                                {
                                    thisOffset += (int)(tickTime * 1000);
                                }
                            }
                        }
                        else if (currentEvent.Item2 is PlayerKilledEventArgs)
                        {
                            if (currentEvent.Item3 == "PlayerKilled")
                            {
                                PlayerKilledEventArgs playerKilledEventArgs = (currentEvent.Item2 as PlayerKilledEventArgs);
                                if (playerKilledEventArgs.Killer != null && playerKilledEventArgs.Victim != null)
                                {
                                    ShowPlayerKilledLog(playerKilledEventArgs, currentInfo, dic, true);
                                }
                            }
                        }
                        else if (currentEvent.Item3 == "BombPlanted")
                        {
                            BombPlanted(DemoPointToMapPoint((currentEvent.Item2 as BombEventArgs).Player.Position, currentInfo.Map), CharacterHelper.GetCharacter(currentEvent.Item4), out roundTimeSpan, out timeSpanWhenBombPlanted, out offsetWhenBombPlanted);
                        }
                        else if (currentEvent.Item3 == "BombDefused")
                        {
                            BombDefused(CharacterHelper.GetCharacter(currentEvent.Item4));
                        }
                        if (currentEvent.Item3 == "WeaponFired")
                        {
                            if ((currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Flash ||
                                    (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.HE ||
                                    (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Smoke ||
                                    (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Molotov ||
                                    (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Incendiary ||
                                    (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Decoy)
                            {
                                if (ThrowMissile(currentInfo, eventArgs, eventName, characterNumber, eventList, i, usedMissileDic, missileKeyValuePairList, tickTime))
                                {
                                    continue;
                                }
                            }
                        }
                        else if (currentEvent.Item3 == "ExplosiveNadeExploded")
                        {
                            ExplosiveNadeExploded(eventArgs as GrenadeEventArgs, currentInfo);
                        }
                        else if (currentEvent.Item3 == "FlashNadeExploded")
                        {
                            FlashNadeExploded(eventArgs as FlashEventArgs, currentInfo);
                        }
                        else if (currentEvent.Item3 == "SmokeNadeStarted")
                        {
                            SmokeNadeStarted(eventArgs as SmokeEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                        }
                        else if (currentEvent.Item3 == "FireNadeWithOwnerStarted")
                        {
                            FireNadeWithOwnerStarted(eventArgs as FireEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                        }
                        else if (currentEvent.Item3 == "DecoyNadeStarted")
                        {
                            DecoyNadeStarted(eventArgs as DecoyEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            foreach (Character character in CharacterHelper.GetCharacters())
                            {
                                character.OtherImg.Visibility = Visibility.Collapsed;
                                character.StatusImg.Visibility = Visibility.Collapsed;
                            }
                        });

                        continue;
                    }
                    else
                    {
                        isForward = false;
                        isNeedRefreshPov = true;
                        offset += thisOffset;
                        thisOffset = 0;
                    }
                }
                if (isBackward)
                {
                    long elapsedMilliseconds = stopWatchThisRound.ElapsedMilliseconds;
                    if (elapsedMilliseconds + offset - 1000/* 防止计算误差造成的崩溃的魔数 */ <= GlobalDictionary.backwardTimeSpan)
                    {
                        thisOffset = 0;
                        isBackward = false;
                    }
                    if (isBackward)
                    {
                        if (thisOffset < GlobalDictionary.backwardTimeSpan)
                        {
                            i -= 2;
                            if (currentEvent.Item2 is TickDoneEventArgs)
                            {
                                if (currentEvent.Item3 == "TickDone")
                                {
                                    if (tickTime == -1)
                                    {
                                        thisOffset += (int)(currentInfo.TickTime * 1000);
                                    }
                                    else
                                    {
                                        thisOffset += (int)(tickTime * 1000);
                                    }
                                }
                            }
                            if (usedMissileDic.Keys.Contains(i))
                            {
                                usedMissileDic.Remove(i);
                            }
                            this.Dispatcher.Invoke(() =>
                            {
                                foreach (Character character in CharacterHelper.GetCharacters())
                                {
                                    character.OtherImg.Visibility = Visibility.Collapsed;
                                    character.StatusImg.Visibility = Visibility.Collapsed;
                                }
                                c_runcanvas.Children.Clear();
                            });

                            continue;
                        }
                        else
                        {
                            isBackward = false;
                            isNeedRefreshPov = true;
                            offset -= thisOffset;
                            thisOffset = 0;
                        }
                    }
                }

                if (currentEvent.Item1 == null && currentEvent.Item2 is PlayerKilledEventArgs && isNeedAutomaticGuidance)
                {
                    PlayerKilledEventArgs playerKilledEventArgs = (currentEvent.Item2 as PlayerKilledEventArgs);
                    long steamID = playerKilledEventArgs.Killer.SteamID;
                    Character character = CharacterHelper.GetCharacter(dic[steamID]);

                    this.Dispatcher.Invoke(() =>
                    {
                        ShowPov(character.CharacterImg, null);
                    });
                }
                if (currentEvent.Item2 is PlayerKilledEventArgs)
                {
                    if (currentEvent.Item3 == "PlayerKilled")
                    {
                        PlayerKilledEventArgs playerKilledEventArgs = (currentEvent.Item2 as PlayerKilledEventArgs);
                        if (playerKilledEventArgs.Killer == null || playerKilledEventArgs.Victim == null)
                        {
                            continue;
                        }
                        ShowPlayerKilledLog(playerKilledEventArgs, currentInfo, dic);

                        long victimSteamID = playerKilledEventArgs.Victim.SteamID;
                        if (dic.ContainsKey(victimSteamID))
                        {
                            Character victimCharacter = CharacterHelper.GetCharacter(dic[victimSteamID]);
                            if (victimCharacter.OtherImg.Visibility == Visibility.Visible)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    victimCharacter.OtherImg.Visibility = Visibility.Collapsed;
                                });
                            }
                            if (victimCharacter.StatusImg.Visibility == Visibility.Visible)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    victimCharacter.StatusImg.Visibility = Visibility.Collapsed;
                                });
                            }
                        }
                    }
                }
                else if (currentEvent.Item2 is RoundAnnounceMatchStartedEventArgs)
                {
                    if (currentEvent.Item3 == "RoundAnnounceMatchStarted")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        if (isAutoShowInfoPanel)
                        {
                            AutoShowDefaultInfoPanel();
                        }
                    }
                }
                else if (currentEvent.Item2 is RoundStartedEventArgs)
                {
                    if (currentEvent.Item3 == "RoundStart")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        if (isAutoShowInfoPanel)
                        {
                            AutoShowDefaultInfoPanel();
                        }

                        isFreezetimeEnded = false;
                    }
                }
                else if (currentEvent.Item2 is RoundEndedEventArgs)
                {
                    if (currentEvent.Item3 == "RoundEnd")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        if (isAutoShowInfoPanel)
                        {
                            AutoShowPersonalInfoPanel();
                        }
                    }
                }
                else if (currentEvent.Item2 is BlindEventArgs)
                {
                    if (currentEvent.Item3 == "Blind")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        Character character = CharacterHelper.GetCharacter(characterNumber);
                        if (character.Status == Model.Status.Dead)
                        {
                            continue;
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            character.StatusImg.Visibility = Visibility.Visible;
                            character.StatusImg.Source = new BitmapImage(new Uri(GlobalDictionary.eyePath));
                            character.StatusImg.Tag = (currentEvent.Item2 as BlindEventArgs).Attacker.Name;
                        });

                        Thread blindThread = new Thread(() =>
                        {
                            Thread.Sleep((int)((currentEvent.Item2 as BlindEventArgs).FlashDuration * 1000));
                            this.Dispatcher.Invoke(() =>
                            {
                                character.StatusImg.Visibility = Visibility.Collapsed;
                            });
                        });
                        blindThread.Start();
                        ThreadHelper.AddThread(blindThread);
                    }
                }
                else if (currentEvent.Item2 is BombEventArgs)
                {
                    BombEventArgs thisEventArgs = eventArgs as BombEventArgs;
                    Player player = thisEventArgs.Player;
                    Character character = CharacterHelper.GetCharacter(characterNumber);

                    if (currentEvent.Item3 == "BombBeginPlant")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            beginPlantOrDefuse = true;

                            character.OtherImg.Visibility = Visibility.Visible;
                            character.OtherImg.Source = new BitmapImage(new Uri(GlobalDictionary.bombPath));
                            character.OtherImg.Tag = "Plant";
                        });
                    }
                    else if (currentEvent.Item3 == "BombAbortPlant")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            character.OtherImg.Visibility = Visibility.Collapsed;
                        });
                    }
                    else if (currentEvent.Item3 == "BombExploded")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        c_runcanvas.Dispatcher.Invoke(() =>
                        {
                            TSImage bombImage = null;
                            foreach (FrameworkElement frameworkElement in c_runcanvas.Children)
                            {
                                if (frameworkElement is TSImage && (frameworkElement as TSImage).TagStr == "Bomb")
                                {
                                    bombImage = frameworkElement as TSImage;
                                }
                            }
                            if (bombImage == null)
                            {
                                return;
                            }

                            TSImage explosionImage = new TSImage();
                            explosionImage.Source = new BitmapImage(new Uri(GlobalDictionary.explosionPath));
                            explosionImage.Width = GlobalDictionary.ExplosionEffectWidthAndHeight;
                            explosionImage.Height = GlobalDictionary.ExplosionEffectWidthAndHeight;
                            Point explosionWndPoint = bombImage.MapPoint;
                            c_runcanvas.Children.Remove(bombImage);
                            explosionImage.MapPoint = explosionWndPoint;
                            explosionImage.ImgType = ImgType.ExplosionEffect;

                            explosionWndPoint = GetWndPoint(explosionWndPoint, ImgType.ExplosionEffect);
                            Canvas.SetLeft(explosionImage, explosionWndPoint.X);
                            Canvas.SetTop(explosionImage, explosionWndPoint.Y);
                            c_runcanvas.Children.Add(explosionImage);

                            Thread removeExplosionImageThread = new Thread(() =>
                            {
                                Thread.Sleep((int)(GlobalDictionary.heLifespan * 1000));
                                c_runcanvas.Dispatcher.Invoke(() =>
                                {
                                    if (c_runcanvas.Children.Contains(explosionImage))
                                    {
                                        c_runcanvas.Children.Remove(explosionImage);
                                    }
                                });
                            });
                            removeExplosionImageThread.Start();
                            ThreadHelper.AddThread(removeExplosionImageThread);
                        });
                    }
                    else if (currentEvent.Item3 == "BombDefused")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        BombDefused(character);
                    }
                    else if (currentEvent.Item3 == "BombPlanted")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        BombPlanted(DemoPointToMapPoint((currentEvent.Item2 as BombEventArgs).Player.Position, currentInfo.Map), character, out roundTimeSpan, out timeSpanWhenBombPlanted, out offsetWhenBombPlanted);
                    }
                }
                else if (currentEvent.Item2 is BombDefuseEventArgs)
                {
                    BombDefuseEventArgs thisEventArgs = eventArgs as BombDefuseEventArgs;
                    Player player = thisEventArgs.Player;
                    Character character = CharacterHelper.GetCharacter(characterNumber);
                    if (currentEvent.Item3 == "BombBeginDefuse")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            beginPlantOrDefuse = true;
                            character.OtherImg.Visibility = Visibility.Visible;
                            character.OtherImg.Source = new BitmapImage(new Uri(GlobalDictionary.defuseKitPath));
                            character.OtherImg.Tag = "Defuse";
                        });
                    }
                    else if (currentEvent.Item3 == "BombAbortDefuse")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            character.OtherImg.Visibility = Visibility.Collapsed;
                        });
                    }
                }
                else if (currentEvent.Item2 is FreezetimeEndedEventArgs)
                {
                    if (currentEvent.Item3 == "FreezetimeEnded")
                    {
                        if ((currentInfo.TScore + currentInfo.CtScore + 1) != roundNumber)
                        {
                            continue;
                        }

                        stopWatchThisRound = new Stopwatch();
                        stopWatchThisRound.Start();
                        isFreezetimeEnded = true;
                    }
                }
                else if (currentEvent.Item2 is WeaponFiredEventArgs)
                {
                    if (currentEvent.Item3 == "WeaponFired")
                    {
                        if (stopWatchThisRound == null || !isFreezetimeEnded)
                        {
                            continue;
                        }

                        Character character = CharacterHelper.GetCharacter(characterNumber);

                        if ((currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Flash ||
                                (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.HE ||
                                (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Smoke ||
                                (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Molotov ||
                                (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Incendiary ||
                                (currentEvent.Item2 as WeaponFiredEventArgs).Weapon.Weapon == EquipmentElement.Decoy)
                        {
                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                if (character.OtherImg.Visibility == Visibility.Visible && character.OtherImg.Tag != null && character.OtherImg.Tag.ToString() == "Plant")
                                {
                                    character.OtherImg.Visibility = Visibility.Collapsed;
                                }
                            });

                            if (ThrowMissile(currentInfo, eventArgs, eventName, characterNumber, eventList, i, usedMissileDic, missileKeyValuePairList, tickTime))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                if (character.OtherImg.Visibility == Visibility.Visible)
                                {
                                    character.OtherImg.Visibility = Visibility.Collapsed;
                                }
                            });

                            Player player = (currentEvent.Item2 as WeaponFiredEventArgs).Shooter;

                            Line bulletLine = null;
                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                bulletLine = new Line();
                                bulletLine.Stroke = Brushes.White;
                                bulletLine.StrokeThickness = 2;
                                bulletLine.StrokeDashArray = new DoubleCollection() { 2, 3 };
                                bulletLine.StrokeDashCap = PenLineCap.Triangle;
                                bulletLine.StrokeEndLineCap = PenLineCap.Triangle;
                                bulletLine.StrokeStartLineCap = PenLineCap.Square;
                                Point fromMapPoint = DemoPointToMapPoint(player.Position, currentInfo.Map);
                                Point fromWndPoint = GetWndPoint(fromMapPoint, ImgType.Nothing);
                                WeaponFiredEventArgs weaponFiredEventArgs = currentEvent.Item2 as WeaponFiredEventArgs;

                                double tan = Math.Tan(player.ViewDirectionX * Math.PI / 180);
                                Point direction = new Point();
                                if (player.ViewDirectionX < -270 || (player.ViewDirectionX > -90 && player.ViewDirectionX < 90) || player.ViewDirectionX > 270)
                                {
                                    direction = new Point(1, tan);
                                }
                                else if ((player.ViewDirectionX > 90 && player.ViewDirectionX < 270) || (player.ViewDirectionX < -90 && player.ViewDirectionX > -270))
                                {
                                    direction = new Point(-1, -tan);
                                }
                                else if (player.ViewDirectionX == 90 || player.ViewDirectionX == -270)
                                {
                                    direction = new Point(0, 1);
                                }
                                else if (player.ViewDirectionX == 270 || player.ViewDirectionX == -90)
                                {
                                    direction = new Point(0, -1);
                                }

                                direction = VectorHelper.GetUnitVector(new Point(0, 0), direction);

                                Point toMapPoint = DemoPointToMapPoint(player.Position + new DemoInfo.Vector((float)(direction.X * 1000), (float)(direction.Y * 1000), 0), currentInfo.Map);
                                Point toWndPoint = GetWndPoint(toMapPoint, ImgType.Nothing);

                                List<Point> mapPointList = new List<Point>() { fromMapPoint, toMapPoint };
                                bulletLine.Tag = mapPointList;

                                bulletLine.X1 = fromWndPoint.X;
                                bulletLine.Y1 = fromWndPoint.Y;
                                bulletLine.X2 = toWndPoint.X;
                                bulletLine.Y2 = toWndPoint.Y;
                                c_runcanvas.Children.Add(bulletLine);
                            });

                            Thread shootThread = new Thread(() =>
                            {
                                Thread.Sleep(150);
                                c_runcanvas.Dispatcher.Invoke(() =>
                                {
                                    c_runcanvas.Children.Remove(bulletLine);
                                });
                            });
                            shootThread.Start();
                            ThreadHelper.AddThread(shootThread);
                        }
                    }
                }
                else if (currentEvent.Item2 is GrenadeEventArgs)
                {
                    if (currentEvent.Item3 == "ExplosiveNadeExploded")
                    {
                        ExplosiveNadeExploded(eventArgs as GrenadeEventArgs, currentInfo);
                    }
                }
                else if (currentEvent.Item2 is FlashEventArgs)
                {
                    if (currentEvent.Item3 == "FlashNadeExploded")
                    {
                        FlashNadeExploded(eventArgs as FlashEventArgs, currentInfo);
                    }
                }
                else if (currentEvent.Item2 is SmokeEventArgs)
                {
                    if (currentEvent.Item3 == "SmokeNadeStarted")
                    {
                        SmokeNadeStarted(eventArgs as SmokeEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                    }
                }
                else if (currentEvent.Item2 is FireEventArgs)
                {
                    if (currentEvent.Item3 == "FireNadeWithOwnerStarted")
                    {
                        FireNadeWithOwnerStarted(eventArgs as FireEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                    }
                }
                else if (currentEvent.Item2 is DecoyEventArgs)
                {
                    if (currentEvent.Item3 == "DecoyNadeStarted")
                    {
                        DecoyNadeStarted(eventArgs as DecoyEventArgs, currentInfo, characterNumber, eventList, i, usedMissileDic, missileEffectKeyValuePairList);
                    }
                }
                else if (currentEvent.Item2 is TickDoneEventArgs)
                {
                    if (currentEvent.Item3 == "TickDone")
                    {
                        if (stopWatchThisRound == null || !isFreezetimeEnded)
                        {
                            continue;
                        }

                        List<KeyValuePair<int, TSImage>> usedMissileEffectKeyValuePairList = new List<KeyValuePair<int, TSImage>>();
                        foreach (KeyValuePair<int, TSImage> kv in missileEffectKeyValuePairList)
                        {
                            if (kv.Key <= currentEvent.Item1.CurrentTick)
                            {
                                c_runcanvas.Dispatcher.Invoke(() =>
                                {
                                    if (c_runcanvas.Children.Contains(kv.Value))
                                    {
                                        c_runcanvas.Children.Remove(kv.Value);
                                    }
                                });
                                usedMissileEffectKeyValuePairList.Add(kv);
                            }
                        }
                        foreach (KeyValuePair<int, TSImage> kv in usedMissileEffectKeyValuePairList)
                        {
                            missileEffectKeyValuePairList.Remove(kv);
                        }

                        List<KeyValuePair<KeyValuePair<int, int>, TSImage>> usedMissileKeyValuePairList = new List<KeyValuePair<KeyValuePair<int, int>, TSImage>>();
                        foreach (KeyValuePair<KeyValuePair<int, int>, TSImage> kv in missileKeyValuePairList)
                        {
                            if (currentInfo.CurrentTick >= kv.Key.Key && currentInfo.CurrentTick <= kv.Key.Value)
                            {
                                double ratio = ((double)currentInfo.CurrentTick - kv.Key.Key) / (kv.Key.Value - kv.Key.Key);
                                double newX = (kv.Value.EndMapPoint.X - kv.Value.StartMapPoint.X) * ratio + kv.Value.StartMapPoint.X;
                                double newY = (kv.Value.EndMapPoint.Y - kv.Value.StartMapPoint.Y) * ratio + kv.Value.StartMapPoint.Y;
                                kv.Value.MapPoint = new Point(newX, newY);

                                Point newWndPoint = GetWndPoint(kv.Value.MapPoint, ImgType.Missile);

                                c_runcanvas.Dispatcher.Invoke(() =>
                                {
                                    if (c_runcanvas.Children.Contains(kv.Value))
                                    {
                                        c_runcanvas.Children.Remove(kv.Value);
                                    }

                                    Canvas.SetLeft(kv.Value, newWndPoint.X);
                                    Canvas.SetTop(kv.Value, newWndPoint.Y);

                                    c_runcanvas.Children.Add(kv.Value);
                                });
                            }
                            else if (currentInfo.CurrentTick > kv.Key.Value)
                            {
                                usedMissileKeyValuePairList.Add(kv);
                            }
                        }
                        foreach (KeyValuePair<KeyValuePair<int, int>, TSImage> kv in usedMissileKeyValuePairList)
                        {
                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                if (c_runcanvas.Children.Contains(kv.Value))
                                {
                                    c_runcanvas.Children.Remove(kv.Value);
                                }
                            });
                            missileKeyValuePairList.Remove(kv);
                        }

                        foreach (Player player in currentEvent.Item1.Participants)
                        {
                            float nowTime = 0;
                            if (tickTime != -1)
                            {
                                nowTime = tickTime * currentEvent.Item1.CurrentTick;
                            }
                            else
                            {
                                nowTime = currentEvent.Item1.CurrentTime;
                            }
                            float costTime = (nowTime - startTime) * 1000;
                            if (costTime < realCostTime + offset)
                            {
                                break;
                            }

                            if (player.Team == Team.Spectate)
                            {
                                continue;
                            }

                            Label nameLabel = null;
                            Label numberLabel = null;
                            if (!dic.ContainsKey(player.SteamID))
                            {
                                continue;
                            }
                            Character character = CharacterHelper.GetCharacter(dic[player.SteamID]);

                            Point endMapPoint = new Point();
                            Point endWndPoint = new Point();

                            c_runcanvas.Dispatcher.Invoke(() =>
                            {
                                foreach (FrameworkElement frameworkElement in c_runcanvas.Children)
                                {
                                    if (frameworkElement is Label)
                                    {
                                        if ((frameworkElement as Label).Content.ToString().Contains((character.Name == "" ? character.Number.ToString() : character.Name)))
                                        {
                                            nameLabel = (Label)frameworkElement;
                                        }
                                        else if ((frameworkElement as Label).Content.ToString() == character.Number.ToString())
                                        {
                                            numberLabel = (Label)frameworkElement;
                                        }
                                    }
                                }

                                if (player.IsAlive == false)
                                {
                                    if (nameLabel != null)
                                    {
                                        nameLabel.Content = character.Name == "" ? character.Number.ToString() : character.Name + " [" + 0 + "]";
                                    }
                                    character.Status = Model.Status.Dead;
                                }
                                else
                                {
                                    character.Status = Model.Status.Alive;
                                }

                                List<Equipment> missileEquipList = new List<Equipment>();
                                List<Equipment> weaponEquipList = new List<Equipment>();
                                List<Equipment> equipList = new List<Equipment>();
                                foreach (Equipment equipment in currentEvent.Item1.WeaponEquipDic[player.SteamID])
                                {
                                    weaponEquipList.Add(new Equipment(equipment.OriginalString));
                                }
                                foreach (Equipment equipment in currentEvent.Item1.MissileEquipDic[player.SteamID])
                                {
                                    missileEquipList.Add(new Equipment(equipment.OriginalString));
                                }
                                foreach (Equipment equipment in currentEvent.Item1.EquipDic[player.SteamID])
                                {
                                    equipList.Add(new Equipment(equipment.OriginalString));
                                }

                                character.MissileEquipList = missileEquipList;
                                character.WeaponEquipmentList = weaponEquipList;
                                character.EquipList = equipList;
                                character.Hp = player.HP;
                                character.Money = player.Money;
                                character.IsT = player.Team == Team.Terrorist;
                                character.IsAlive = player.IsAlive;
                                character.AdditionalPlayerInformation.Kills = player.AdditionaInformations.Kills;
                                character.AdditionalPlayerInformation.Deaths = player.AdditionaInformations.Deaths;
                                character.AdditionalPlayerInformation.Assists = player.AdditionaInformations.Assists;
                                character.AdditionalPlayerInformation.Score = player.AdditionaInformations.Score;
                                character.AdditionalPlayerInformation.MVPs = player.AdditionaInformations.MVPs;
                                character.AdditionalPlayerInformation.Ping = player.AdditionaInformations.Ping;
                                character.AdditionalPlayerInformation.Clantag = player.AdditionaInformations.Clantag;
                                character.AdditionalPlayerInformation.TotalCashSpent = player.AdditionaInformations.Assists;
                                if (player.IsAlive)
                                {
                                    character.LastAliveInfo.MissileEquipList = missileEquipList;
                                    character.LastAliveInfo.WeaponEquipmentList = weaponEquipList;
                                    character.LastAliveInfo.EquipList = equipList;
                                    character.LastAliveInfo.Money = player.Money;
                                }

                                c_runcanvas.Children.Remove(character.CharacterImg);
                                c_runcanvas.Children.Remove(nameLabel);
                                c_runcanvas.Children.Remove(numberLabel);
                                if (character.OtherImg.Visibility == Visibility.Visible)
                                {
                                    c_runcanvas.Children.Remove(character.OtherImg);
                                }
                                if (character.StatusImg.Visibility == Visibility.Visible)
                                {
                                    c_runcanvas.Children.Remove(character.StatusImg);
                                }

                                endMapPoint = DemoPointToMapPoint(player.IsAlive ? player.Position : player.LastAlivePosition, currentInfo.Map);
                                endWndPoint = GetWndPoint(endMapPoint, ImgType.Character);

                                character.CharacterImg.Tag = (nowTime - firstFreezetimeEndedTime) + "|" + (stopWatchThisRound.ElapsedMilliseconds / 1000.0 + offset / 1000);

                                if (isNeedRefreshPov && me_pov.Visibility == Visibility.Visible)
                                {
                                    double currentTime = double.Parse(CharacterHelper.GetCharacter(characterNumber).CharacterImg.Tag.ToString().Split('|')[(int)me_pov.Tag]);
                                    string filePath = me_pov.Source.LocalPath;
                                    PlayPov(new FileInfo(filePath), currentTime);
                                    isNeedRefreshPov = false;
                                }

                                Canvas.SetLeft(character.CharacterImg, endWndPoint.X);
                                Canvas.SetTop(character.CharacterImg, endWndPoint.Y);

                                if (character.OtherImg.Visibility == Visibility.Visible && character.StatusImg.Visibility == Visibility.Visible)
                                {
                                    character.OtherImg.Width = character.CharacterImg.Width * 1.5;
                                    character.OtherImg.Height = character.CharacterImg.Height * 1.5;
                                    Point otherImgWndPoint = new Point(endWndPoint.X - character.CharacterImg.Width * -3 / 8, endWndPoint.Y - character.CharacterImg.Height);
                                    character.OtherImg.MapPoint = GetMapPoint(otherImgWndPoint, ImgType.Nothing);
                                    character.OtherImg.ImgType = ImgType.Nothing;
                                    Canvas.SetLeft(character.OtherImg, otherImgWndPoint.X);
                                    Canvas.SetTop(character.OtherImg, otherImgWndPoint.Y);

                                    character.StatusImg.Width = character.CharacterImg.Width * 1.5;
                                    character.StatusImg.Height = character.CharacterImg.Height * 1.5;
                                    Point statusImgWndPoint = new Point(endWndPoint.X - character.CharacterImg.Width * 8 / 8, endWndPoint.Y - character.CharacterImg.Height);
                                    character.StatusImg.MapPoint = GetMapPoint(statusImgWndPoint, ImgType.Nothing);
                                    character.StatusImg.ImgType = ImgType.Nothing;
                                    Canvas.SetLeft(character.StatusImg, statusImgWndPoint.X);
                                    Canvas.SetTop(character.StatusImg, statusImgWndPoint.Y);
                                }
                                else
                                {
                                    if (character.OtherImg.Visibility == Visibility.Visible)
                                    {
                                        character.OtherImg.Width = character.CharacterImg.Width * 1.5;
                                        character.OtherImg.Height = character.CharacterImg.Height * 1.5;
                                        Point otherImgWndPoint = new Point(endWndPoint.X - character.CharacterImg.Width * 3 / 8, endWndPoint.Y - character.CharacterImg.Height);
                                        character.OtherImg.MapPoint = GetMapPoint(otherImgWndPoint, ImgType.Nothing);
                                        character.OtherImg.ImgType = ImgType.Nothing;
                                        Canvas.SetLeft(character.OtherImg, otherImgWndPoint.X);
                                        Canvas.SetTop(character.OtherImg, otherImgWndPoint.Y);
                                    }
                                    if (character.StatusImg.Visibility == Visibility.Visible)
                                    {
                                        character.StatusImg.Width = character.CharacterImg.Width * 1.5;
                                        character.StatusImg.Height = character.CharacterImg.Height * 1.5;
                                        Point statusImgWndPoint = new Point(endWndPoint.X - character.CharacterImg.Width * 3 / 8, endWndPoint.Y - character.CharacterImg.Height);
                                        character.StatusImg.MapPoint = GetMapPoint(statusImgWndPoint, ImgType.Nothing);
                                        character.StatusImg.ImgType = ImgType.Nothing;
                                        Canvas.SetLeft(character.StatusImg, statusImgWndPoint.X);
                                        Canvas.SetTop(character.StatusImg, statusImgWndPoint.Y);
                                    }
                                }

                                if (character.IsAlive)
                                {
                                    character.CharacterImg.RenderTransform = new RotateTransform(360 - player.ViewDirectionX, character.CharacterImg.Width / 2, character.CharacterImg.Height / 2);
                                }

                                c_runcanvas.Dispatcher.Invoke(() =>
                                {
                                    if (character.OtherImg.Visibility == Visibility.Visible && !character.CharacterImg.MapPoint.Equals(endMapPoint))
                                    {
                                        if (beginPlantOrDefuse)
                                        {
                                            beginPlantOrDefuse = false;
                                        }
                                        else
                                        {
                                            character.OtherImg.Visibility = Visibility.Collapsed;
                                        }
                                    }
                                });
                                character.CharacterImg.MapPoint = endMapPoint;
                                character.CharacterImg.ImgType = ImgType.Character;
                                c_runcanvas.Children.Add(character.CharacterImg);
                                c_runcanvas.Children.Add(CreateChacterlabel(character, endWndPoint, player.HP));
                                c_runcanvas.Children.Add(CreateChacterNumberlabel(character, endWndPoint));
                                if (character.OtherImg.Visibility == Visibility.Visible)
                                {
                                    c_runcanvas.Children.Add(character.OtherImg);
                                }
                                if (character.StatusImg.Visibility == Visibility.Visible)
                                {
                                    c_runcanvas.Children.Add(character.StatusImg);
                                }
                            });

                            List<Character> characters = CharacterHelper.GetCharacters();
                            characters[characters.IndexOf(character)].MapPoint = endMapPoint;
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            currentTScore = currentInfo.TScore;
                            currentCTScore = currentInfo.CtScore;
                            SetInfos(currentTScore, currentCTScore);
                        });

                        float nowTimeEnd = 0;
                        if (tickTime != -1)
                        {
                            nowTimeEnd = tickTime * currentEvent.Item1.CurrentTick;
                        }
                        else
                        {
                            nowTimeEnd = currentEvent.Item1.CurrentTime;
                        }

                        if (tickTime != -1)
                        {
                            elapsedTimeMillisecondInDemo += tickTime * 1000;
                        }
                        else
                        {
                            elapsedTimeMillisecondInDemo += currentEvent.Item1.TickTime * 1000;
                        }

                        float costTimeEnd = (nowTimeEnd - startTime) * 1000;
                        if (costTimeEnd > realCostTime + offset)
                        {
                            if (startTime == 0)
                            {
                                if (tickTime != -1)
                                {
                                    startTime = tickTime * currentEvent.Item1.CurrentTick;
                                }
                                else
                                {
                                    startTime = currentEvent.Item1.CurrentTime;
                                }
                            }
                            realCostTime += GlobalDictionary.animationFreshTime;

                            long elapsedTimeMillisecond = stopWatch.ElapsedMilliseconds;
                            int sleepTime = (int)(animationFreshTime - (elapsedTimeMillisecond - elapsedTimeMillisecondInDemo));
                            if (sleepTime < 0)
                            {
                                sleepTime = 0;
                            }
                            Thread.Sleep(sleepTime);
                        }
                    }

                }
            }
        }

        private void ExplosiveNadeExploded(GrenadeEventArgs eventArgs, CurrentInfo currentInfo)
        {
            TSImage missileEffectImg = null;
            this.Dispatcher.Invoke(() =>
            {
                missileEffectImg = new TSImage();
                missileEffectImg.MapPoint = DemoPointToMapPoint(eventArgs.Position, currentInfo.Map);
                missileEffectImg.ImgType = ImgType.MissileEffect;
                missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.heEffectPath));

                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                Point wndPoint = GetWndPoint(missileEffectImg.MapPoint, ImgType.MissileEffect);
                Canvas.SetLeft(missileEffectImg, wndPoint.X);
                Canvas.SetTop(missileEffectImg, wndPoint.Y);
                c_runcanvas.Children.Add(missileEffectImg);
            });
            Thread effectThread = new Thread(() =>
            {
                Thread.Sleep((int)(GlobalDictionary.heLifespan * 1000));
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    if (c_runcanvas.Children.Contains(missileEffectImg))
                    {
                        c_runcanvas.Children.Remove(missileEffectImg);
                    }
                });
            });
            effectThread.Start();
            ThreadHelper.AddThread(effectThread);
        }

        private void FlashNadeExploded(FlashEventArgs eventArgs, CurrentInfo currentInfo)
        {
            TSImage missileEffectImg = null;
            this.Dispatcher.Invoke(() =>
            {
                missileEffectImg = new TSImage();
                missileEffectImg.MapPoint = DemoPointToMapPoint((eventArgs as FlashEventArgs).Position, currentInfo.Map);
                missileEffectImg.ImgType = ImgType.MissileEffect;
                missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.flashEffectPath));

                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                Point wndPoint = GetWndPoint(missileEffectImg.MapPoint, ImgType.MissileEffect);
                Canvas.SetLeft(missileEffectImg, wndPoint.X);
                Canvas.SetTop(missileEffectImg, wndPoint.Y);
                c_runcanvas.Children.Add(missileEffectImg);
            });
            Thread effectThread = new Thread(() =>
            {
                Thread.Sleep((int)(GlobalDictionary.flashbangLifespan * 1000));
                c_runcanvas.Dispatcher.Invoke(() =>
                {
                    if (c_runcanvas.Children.Contains(missileEffectImg))
                    {
                        c_runcanvas.Children.Remove(missileEffectImg);
                    }
                });
            });
            effectThread.Start();
            ThreadHelper.AddThread(effectThread);
        }

        private void SmokeNadeStarted(SmokeEventArgs eventArgs, CurrentInfo currentInfo, int characterNumber, List<Tuple<CurrentInfo, EventArgs, string, int>> eventList, int i, Dictionary<int, int> usedMissileDic, List<KeyValuePair<int, TSImage>> missileEffectKeyValuePairList)
        {
            TSImage missileEffectImg = null;
            this.Dispatcher.Invoke(() =>
            {
                missileEffectImg = new TSImage();
                missileEffectImg.MapPoint = DemoPointToMapPoint(eventArgs.Position, currentInfo.Map);
                missileEffectImg.ImgType = ImgType.MissileEffect;
                missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.smokeEffectPath));

                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                Point wndPoint = GetWndPoint(missileEffectImg.MapPoint, ImgType.MissileEffect);
                Canvas.SetLeft(missileEffectImg, wndPoint.X);
                Canvas.SetTop(missileEffectImg, wndPoint.Y);
                c_runcanvas.Children.Add(missileEffectImg);
            });


            for (int m = i + 1; m < eventList.Count(); ++m)
            {
                if (eventList[m].Item1 == null)
                {
                    continue;
                }
                if ((eventList[m].Item1.CtScore + eventList[m].Item1.TScore) != (currentInfo.CtScore + currentInfo.TScore))
                {
                    break;
                }
                if (usedMissileDic.Values.Contains(m))
                {
                    continue;
                }
                if (eventList[m].Item4 != characterNumber)
                {
                    continue;
                }

                if (eventList[m].Item3 == "SmokeNadeEnded")
                {
                    if (((SmokeEventArgs)eventList[m].Item2).ThrownBy.SteamID != eventArgs.ThrownBy.SteamID)
                    {
                        continue;
                    }
                    missileEffectKeyValuePairList.Add(new KeyValuePair<int, TSImage>(eventList[m].Item1.CurrentTick, missileEffectImg));
                    break;
                }
            }
        }

        private void FireNadeWithOwnerStarted(FireEventArgs eventArgs, CurrentInfo currentInfo, int characterNumber, List<Tuple<CurrentInfo, EventArgs, string, int>> eventList, int i, Dictionary<int, int> usedMissileDic, List<KeyValuePair<int, TSImage>> missileEffectKeyValuePairList)
        {
            TSImage missileEffectImg = null;
            this.Dispatcher.Invoke(() =>
            {
                missileEffectImg = new TSImage();
                missileEffectImg.MapPoint = DemoPointToMapPoint(eventArgs.Position, currentInfo.Map);
                missileEffectImg.ImgType = ImgType.MissileEffect;
                missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.fireEffectPath));

                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                Point wndPoint = GetWndPoint(missileEffectImg.MapPoint, ImgType.MissileEffect);
                Canvas.SetLeft(missileEffectImg, wndPoint.X);
                Canvas.SetTop(missileEffectImg, wndPoint.Y);
                c_runcanvas.Children.Add(missileEffectImg);
            });

            for (int m = i + 1; m < eventList.Count(); ++m)
            {
                if (eventList[m].Item1 == null)
                {
                    continue;
                }
                if ((eventList[m].Item1.CtScore + eventList[m].Item1.TScore) != (currentInfo.CtScore + currentInfo.TScore))
                {
                    break;
                }
                if (usedMissileDic.Values.Contains(m))
                {
                    continue;
                }
                if (eventList[m].Item4 != characterNumber)
                {
                    continue;
                }

                if (eventList[m].Item3 == "FireNadeEnded")
                {
                    if (((FireEventArgs)eventList[m].Item2).ThrownBy.SteamID != eventArgs.ThrownBy.SteamID)
                    {
                        continue;
                    }
                    missileEffectKeyValuePairList.Add(new KeyValuePair<int, TSImage>(eventList[m].Item1.CurrentTick, missileEffectImg));
                    break;
                }
            }
        }

        private void DecoyNadeStarted(DecoyEventArgs eventArgs, CurrentInfo currentInfo, int characterNumber, List<Tuple<CurrentInfo, EventArgs, string, int>> eventList, int i, Dictionary<int, int> usedMissileDic, List<KeyValuePair<int, TSImage>> missileEffectKeyValuePairList)
        {
            TSImage missileEffectImg = null;
            this.Dispatcher.Invoke(() =>
            {
                missileEffectImg = new TSImage();
                missileEffectImg.MapPoint = DemoPointToMapPoint(eventArgs.Position, currentInfo.Map);
                missileEffectImg.ImgType = ImgType.MissileEffect;
                missileEffectImg.Source = new BitmapImage(new Uri(GlobalDictionary.decoyEffectPath));

                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                Point wndPoint = GetWndPoint(missileEffectImg.MapPoint, ImgType.MissileEffect);
                Canvas.SetLeft(missileEffectImg, wndPoint.X);
                Canvas.SetTop(missileEffectImg, wndPoint.Y);
                c_runcanvas.Children.Add(missileEffectImg);
            });

            for (int m = i + 1; m < eventList.Count(); ++m)
            {
                if (eventList[m].Item1 == null)
                {
                    continue;
                }
                if ((eventList[m].Item1.CtScore + eventList[m].Item1.TScore) != (currentInfo.CtScore + currentInfo.TScore))
                {
                    break;
                }
                if (usedMissileDic.Values.Contains(m))
                {
                    continue;
                }
                if (eventList[m].Item4 != characterNumber)
                {
                    continue;
                }

                if (eventList[m].Item3 == "DecoyNadeEnded")
                {
                    if (((DecoyEventArgs)eventList[m].Item2).ThrownBy.SteamID != eventArgs.ThrownBy.SteamID)
                    {
                        continue;
                    }
                    missileEffectKeyValuePairList.Add(new KeyValuePair<int, TSImage>(eventList[m].Item1.CurrentTick, missileEffectImg));
                    break;
                }
            }
        }

        private bool ThrowMissile(CurrentInfo currentInfo, EventArgs eventArgs, string eventName, int characterNumber, List<Tuple<CurrentInfo, EventArgs, string, int>> eventList, int i, Dictionary<int, int> usedMissileDic, List<KeyValuePair<KeyValuePair<int, int>, TSImage>> missileKeyValuePairList, float tickTime)
        {
            EquipmentElement weapon = (eventArgs as WeaponFiredEventArgs).Weapon.Weapon;
            Character characterThrow = CharacterHelper.GetCharacter(characterNumber);

            Player playerThrow = (eventArgs as WeaponFiredEventArgs).Shooter;

            // 判断取消投掷
            List<Equipment> equipList = currentInfo.MissileEquipDic[playerThrow.SteamID];
            List<Equipment> equipListNext = null;
            int getTickDoneCount = 0;
            for (int n = i + 1; n < eventList.Count(); ++n)
            {
                if (eventList[n].Item1 == null)
                {
                    continue;
                }
                if ((eventList[n].Item1.CtScore + eventList[n].Item1.TScore) != (currentInfo.CtScore + currentInfo.TScore))
                {
                    break;
                }
                if (eventList[n].Item3 == "TickDone")
                {
                    if (getTickDoneCount <= 30)
                    {
                        ++getTickDoneCount;
                    }
                    else
                    {
                        equipListNext = eventList[n].Item1.MissileEquipDic[playerThrow.SteamID];
                        break;
                    }
                }
            }
            int equipListNextCount = -1;
            if (equipListNext == null)
            {
                equipListNextCount = 0;
            }
            else
            {
                equipListNextCount = equipListNext.Count();
            }
            if (equipList.Count() == equipListNextCount)
            {
                // continue
                return true;
            }

            int endTick = 0;
            double costTime = 0;
            Point missileStartMapPoint = DemoPointToMapPoint(playerThrow.Position, currentInfo.Map);
            Point missileEndMapPoint = new Point();

            for (int n = i + 1; n < eventList.Count(); ++n)
            {
                if (eventList[n].Item1 == null)
                {
                    continue;
                }
                if ((eventList[n].Item1.CtScore + eventList[n].Item1.TScore) != (currentInfo.CtScore + currentInfo.TScore))
                {
                    break;
                }
                if (usedMissileDic.Values.Contains(n))
                {
                    continue;
                }
                if (eventList[n].Item4 != characterNumber)
                {
                    continue;
                }
                if (weapon == EquipmentElement.HE && eventList[n].Item3 == "ExplosiveNadeExploded")
                {
                    if (((GrenadeEventArgs)eventList[n].Item2).ThrownBy.SteamID != playerThrow.SteamID)
                    {
                        continue;
                    }
                    usedMissileDic.Add(i, n);
                    endTick = eventList[n].Item1.CurrentTick;
                    if (tickTime == -1)
                    {
                        costTime = (eventList[n].Item1.CurrentTime - currentInfo.CurrentTime);
                    }
                    else
                    {
                        costTime = tickTime * (eventList[n].Item1.CurrentTick - currentInfo.CurrentTick);
                    }
                    missileEndMapPoint = DemoPointToMapPoint((eventList[n].Item2 as GrenadeEventArgs).Position, currentInfo.Map);
                    break;
                }
                else if (weapon == EquipmentElement.Flash && eventList[n].Item3 == "FlashNadeExploded")
                {
                    if (((FlashEventArgs)eventList[n].Item2).ThrownBy.SteamID != playerThrow.SteamID)
                    {
                        continue;
                    }
                    usedMissileDic.Add(i, n);
                    endTick = eventList[n].Item1.CurrentTick;
                    if (tickTime == -1)
                    {
                        costTime = (eventList[n].Item1.CurrentTime - currentInfo.CurrentTime);
                    }
                    else
                    {
                        costTime = tickTime * (eventList[n].Item1.CurrentTick - currentInfo.CurrentTick);
                    }
                    missileEndMapPoint = DemoPointToMapPoint((eventList[n].Item2 as FlashEventArgs).Position, currentInfo.Map);
                    break;
                }
                else if (weapon == EquipmentElement.Smoke && eventList[n].Item3 == "SmokeNadeStarted")
                {
                    if (((SmokeEventArgs)eventList[n].Item2).ThrownBy.SteamID != playerThrow.SteamID)
                    {
                        continue;
                    }
                    usedMissileDic.Add(i, n);
                    endTick = eventList[n].Item1.CurrentTick;
                    missileEndMapPoint = DemoPointToMapPoint((eventList[n].Item2 as SmokeEventArgs).Position, currentInfo.Map);
                    break;
                }
                else if ((weapon == EquipmentElement.Incendiary || weapon == EquipmentElement.Molotov) && eventList[n].Item3 == "FireNadeWithOwnerStarted")
                {
                    if (((FireEventArgs)eventList[n].Item2).ThrownBy.SteamID != playerThrow.SteamID)
                    {
                        continue;
                    }
                    usedMissileDic.Add(i, n);
                    endTick = eventList[n].Item1.CurrentTick;
                    missileEndMapPoint = DemoPointToMapPoint((eventList[n].Item2 as FireEventArgs).Position, currentInfo.Map);
                    break;
                }
                else if (weapon == EquipmentElement.Decoy && eventList[n].Item3 == "DecoyNadeStarted")
                {
                    if (((DecoyEventArgs)eventList[n].Item2).ThrownBy.SteamID != playerThrow.SteamID)
                    {
                        continue;
                    }
                    usedMissileDic.Add(i, n);
                    endTick = eventList[n].Item1.CurrentTick;
                    missileEndMapPoint = DemoPointToMapPoint((eventList[n].Item2 as DecoyEventArgs).Position, currentInfo.Map);
                    break;
                }
            }

            if (missileEndMapPoint.X == 0 && missileEndMapPoint.Y == 0)
            {
                // continue
                return true;
            }

            Point missileStartWndPoint = GetWndPoint(missileStartMapPoint, ImgType.Missile);
            Point missileEndWndPoint = GetWndPoint(missileEndMapPoint, ImgType.Missile);


            List<Character> characters = CharacterHelper.GetCharacters();

            c_runcanvas.Dispatcher.Invoke(() =>
            {
                TSImage missileImg = new TSImage();
                missileImg.StartMapPoint = missileStartMapPoint;
                missileImg.EndMapPoint = missileEndMapPoint;
                missileImg.MapPoint = missileStartMapPoint;
                missileImg.ImgType = ImgType.Missile;
                if (weapon == EquipmentElement.HE)
                {
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.hegrenadePath));
                }
                else if (weapon == EquipmentElement.Flash)
                {
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.flashbangPath));
                }
                else if (weapon == EquipmentElement.Smoke)
                {
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.smokePath));
                }
                else if (weapon == EquipmentElement.Incendiary || weapon == EquipmentElement.Molotov)
                {
                    if (weapon == EquipmentElement.Incendiary)
                    {
                        missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.incgrenadePath));
                    }
                    else
                    {
                        missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.molotovPath));
                    }
                }
                else if (weapon == EquipmentElement.Decoy)
                {
                    missileImg.Source = new BitmapImage(new Uri(GlobalDictionary.decoyPath));
                }

                missileImg.Width = GlobalDictionary.MissileWidthAndHeight;
                missileImg.Height = GlobalDictionary.MissileWidthAndHeight;
                missileKeyValuePairList.Add(new KeyValuePair<KeyValuePair<int, int>, TSImage>(new KeyValuePair<int, int>(currentInfo.CurrentTick, endTick), missileImg));
            });

            return false;
        }

        private void BombPlanted(Point mapPoint, Character character, out TimeSpan roundTimeSpan, out TimeSpan timeSpanWhenBombPlanted, out TimeSpan offsetWhenBombPlanted)
        {
            roundTimeSpan = new TimeSpan(0, 0, 41);
            timeSpanWhenBombPlanted = stopWatch.Elapsed;
            offsetWhenBombPlanted = new TimeSpan(0, 0, 0, 0, offset);

            c_runcanvas.Dispatcher.Invoke(() =>
            {
                TSImage bombImage = new TSImage();
                bombImage.Source = new BitmapImage(new Uri(GlobalDictionary.bombPath));
                bombImage.Width = GlobalDictionary.PropsWidthAndHeight;
                bombImage.Height = GlobalDictionary.PropsWidthAndHeight;
                bombImage.Opacity = 0.75;
                bombImage.TagStr = "Bomb";
                bombImage.MapPoint = mapPoint;
                bombImage.ImgType = ImgType.Props;
                Point bombWndPoint = GetWndPoint(mapPoint, ImgType.Props);

                character.OtherImg.Visibility = Visibility.Collapsed;

                Canvas.SetLeft(bombImage, bombWndPoint.X);
                Canvas.SetTop(bombImage, bombWndPoint.Y);
                c_runcanvas.Children.Add(bombImage);
            });
        }

        private void BombDefused(Character character)
        {
            c_runcanvas.Dispatcher.Invoke(() =>
            {
                TSImage bombImage = null;
                foreach (FrameworkElement frameworkElement in c_runcanvas.Children)
                {
                    if (frameworkElement is TSImage && (frameworkElement as TSImage).TagStr == "Bomb")
                    {
                        bombImage = frameworkElement as TSImage;
                    }
                }
                if (bombImage == null)
                {
                    return;
                }

                c_runcanvas.Children.Remove(bombImage);

                TSImage explosionImage = new TSImage();
                explosionImage.Source = new BitmapImage(new Uri(GlobalDictionary.explosionPath));
                explosionImage.Width = GlobalDictionary.ExplosionEffectWidthAndHeight;
                explosionImage.Height = GlobalDictionary.ExplosionEffectWidthAndHeight;
                c_runcanvas.Children.Remove(explosionImage);

                character.OtherImg.Visibility = Visibility.Collapsed;
            });
        }

        private void ShowPlayerKilledLog(PlayerKilledEventArgs playerKilledEventArgs, CurrentInfo currentInfo, Dictionary<long, int> dic, bool isSkip = false)
        {
            te_editor.Dispatcher.Invoke(() =>
            {
                string skipInfo = "";
                if (isSkip)
                {
                    skipInfo = " (blind or with flash information is unknown)";
                }

                string headShotStr = "";
                if (playerKilledEventArgs.Headshot)
                {
                    headShotStr += " [headshot]";
                }

                string blindStr = "";
                foreach (long steamId in dic.Keys)
                {
                    if (playerKilledEventArgs.Killer != null && playerKilledEventArgs.Killer.SteamID == steamId)
                    {
                        if (CharacterHelper.GetCharacter(dic[steamId]).StatusImg.Visibility == Visibility.Visible)
                        {
                            blindStr += " [blind]";
                        }
                    }
                }

                string withFlashStr = "";
                foreach (long steamId in dic.Keys)
                {
                    if (playerKilledEventArgs.Victim != null && playerKilledEventArgs.Victim.SteamID == steamId)
                    {
                        if (CharacterHelper.GetCharacter(dic[steamId]).StatusImg.Visibility == Visibility.Visible)
                        {
                            withFlashStr += (" [with flashbang by " + CharacterHelper.GetCharacter(dic[steamId]).StatusImg.Tag.ToString() + "]");
                        }
                    }
                }

                string assisterStr = "";
                if (playerKilledEventArgs.Assister != null)
                {
                    string teammateStr = "";
                    if (playerKilledEventArgs.Assister != null && playerKilledEventArgs.Victim != null && playerKilledEventArgs.Assister.Team == playerKilledEventArgs.Victim.Team)
                    {
                        teammateStr = " (team damage)";
                    }
                    assisterStr = " [with " + playerKilledEventArgs.Assister.Name + teammateStr + "]";
                }

                string teamkillStr = "";
                if (playerKilledEventArgs.Killer != null && playerKilledEventArgs.Victim != null && playerKilledEventArgs.Killer.Team == playerKilledEventArgs.Victim.Team)
                {
                    teamkillStr = " [team kill]";
                }

                string killerWeaponString = playerKilledEventArgs.Weapon.Weapon.ToString();
                te_editor.Text += "Round " + (currentInfo.CtScore + currentInfo.TScore + 1) + ": " + playerKilledEventArgs.Killer.Name + " killed " + playerKilledEventArgs.Victim.Name + " by " + killerWeaponString + headShotStr + assisterStr + blindStr + withFlashStr + teamkillStr + skipInfo + "\n";
                te_editor.ScrollToEnd();
            });
        }

        private void InitInfoTag(Player player)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (player.Team == Team.Terrorist)
                {
                    if (tb_player1.Tag == null)
                    {
                        tb_player1.Tag = player.SteamID;
                        tb_team1.Tag = player.SteamID;
                    }
                    else if (tb_player2.Tag == null)
                    {
                        tb_player2.Tag = player.SteamID;
                    }
                    else if (tb_player3.Tag == null)
                    {
                        tb_player3.Tag = player.SteamID;
                    }
                    else if (tb_player4.Tag == null)
                    {
                        tb_player4.Tag = player.SteamID;
                    }
                    else if (tb_player5.Tag == null)
                    {
                        tb_player5.Tag = player.SteamID;
                    }
                }
                else
                {
                    if (tb_player6.Tag == null)
                    {
                        tb_player6.Tag = player.SteamID;
                        tb_team2.Tag = player.SteamID;
                    }
                    else if (tb_player7.Tag == null)
                    {
                        tb_player7.Tag = player.SteamID;
                    }
                    else if (tb_player8.Tag == null)
                    {
                        tb_player8.Tag = player.SteamID;
                    }
                    else if (tb_player9.Tag == null)
                    {
                        tb_player9.Tag = player.SteamID;
                    }
                    else if (tb_player10.Tag == null)
                    {
                        tb_player10.Tag = player.SteamID;
                    }
                }
            });
        }

        private Point DemoPointToMapPoint(DemoInfo.Vector demoPoint, string mapName)
        {
            string[] splitListStr = null;
            foreach (string map in mapList)
            {
                if (mapName.Contains(map.ToLowerInvariant()))
                {
                    string[] mapCalibrationDatas = GlobalDictionary.mapCalibrationDatas;
                    foreach (string mapData in mapCalibrationDatas)
                    {
                        string[] mapCalibrationData = mapData.Split(':');
                        if (mapCalibrationData[0].ToLowerInvariant() == map.ToLowerInvariant())
                        {
                            splitListStr = mapCalibrationData[1].Split(',');
                            break;
                        }
                    }
                }
            }

            double[] splitList = new double[3];
            for (int i = 0; i < splitListStr.Count(); ++i)
            {
                splitList[i] = double.Parse(splitListStr[i]);
            }
            return new Point((demoPoint.X - splitList[0]) / splitList[2], (demoPoint.Y * (-1) + splitList[1]) / splitList[2]);
        }

        private void Tb_select_file_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string filePath = tb_select_file.Text;
                if (File.Exists(filePath))
                {
                    if (Path.GetExtension(filePath) == ".txt")
                    {
                        te_editor.Text = File.ReadAllText(filePath);
                        tb_select_file.Text = filePath;
                    }
                    else
                    {
                        ReadDemo(filePath);
                    }
                }
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            gs_gridsplitter.IsEnabled = true;
            ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private void Stop(List<string> threadNameList = null)
        {
            ThreadHelper.StopAllThread(threadNameList);
            CommandHelper.commands.Clear();
            animations.Clear();
            CharacterHelper.ClearCharacters();
            c_runcanvas.Children.Clear();
            GlobalDictionary.charatersNumber = 0;
            localSpeedController = -1;

            if (WindowState == WindowState.Maximized)
            {
                btn_restore.Visibility = Visibility.Visible;
            }

            btn_pause.Background = GlobalDictionary.pauseBrush;

            if (me_pov.Visibility == Visibility.Visible)
            {
                me_pov.Stop();
                me_pov.Visibility = Visibility.Collapsed;
                g_povcontroller.Visibility = Visibility.Collapsed;
            }
        }

        void te_editor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == " ")
            {
                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(te_editor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new CompletionData("set"));
                data.Add(new CompletionData("entirety"));
                data.Add(new CompletionData("speed"));
                data.Add(new CompletionData("camp"));
                data.Add(new CompletionData("create"));
                data.Add(new CompletionData("team"));
                data.Add(new CompletionData("character"));
                data.Add(new CompletionData("delete"));
                data.Add(new CompletionData("give"));
                data.Add(new CompletionData("weapon"));
                data.Add(new CompletionData("missile"));
                data.Add(new CompletionData("props"));
                data.Add(new CompletionData("status"));
                data.Add(new CompletionData("vertical"));
                data.Add(new CompletionData("position"));
                data.Add(new CompletionData("action"));
                data.Add(new CompletionData("move"));
                data.Add(new CompletionData("throw"));
                data.Add(new CompletionData("shoot"));
                data.Add(new CompletionData("do"));
                data.Add(new CompletionData("wait"));
                data.Add(new CompletionData("until"));
                data.Add(new CompletionData("for"));
                data.Add(new CompletionData("comment"));
                data.Add(new CompletionData("t"));
                data.Add(new CompletionData("ct"));
                data.Add(new CompletionData("pistol"));
                data.Add(new CompletionData("eco"));
                data.Add(new CompletionData("forcebuy"));
                data.Add(new CompletionData("quasibuy"));
                data.Add(new CompletionData("run"));
                data.Add(new CompletionData("walk"));
                data.Add(new CompletionData("squat"));
                data.Add(new CompletionData("teleport"));
                data.Add(new CompletionData("die"));
                data.Add(new CompletionData("live"));
                data.Add(new CompletionData("layer"));
                data.Add(new CompletionData("auto"));
                data.Add(new CompletionData("from"));
                data.Add(new CompletionData("map"));
                data.Add(new CompletionData("node"));
                data.Add(new CompletionData("path"));
                data.Add(new CompletionData("limit"));
                data.Add(new CompletionData("mode"));
                data.Add(new CompletionData("distance"));
                data.Add(new CompletionData("to"));
                data.Add(new CompletionData("name"));
                foreach (Props props in Enum.GetValues(typeof(Props)))
                {
                    if (props == Props.Nothing)
                    {
                        continue;
                    }
                    data.Add(new CompletionData(props.ToString().ToLower()));
                }
                foreach (VerticalPosition verticalPosition in Enum.GetValues(typeof(VerticalPosition)))
                {
                    data.Add(new CompletionData(verticalPosition.ToString().ToLower()));
                }
                foreach (DoWithProps doWithProps in Enum.GetValues(typeof(DoWithProps)))
                {
                    data.Add(new CompletionData(doWithProps.ToString().ToLower()));
                }
                foreach (Model.Status status in Enum.GetValues(typeof(Model.Status)))
                {
                    data.Add(new CompletionData(status.ToString().ToLower()));
                }
                foreach (Missile missile in Enum.GetValues(typeof(Missile)))
                {
                    if (missile == Missile.Nothing)
                    {
                        continue;
                    }
                    data.Add(new CompletionData(missile.ToString().ToLower()));
                }
                foreach (VolumeLimit volumeLimit in Enum.GetValues(typeof(VolumeLimit)))
                {
                    data.Add(new CompletionData(volumeLimit.ToString().ToLower()));
                }
                foreach (DirectionMode directionMode in Enum.GetValues(typeof(DirectionMode)))
                {
                    data.Add(new CompletionData(directionMode.ToString().ToLower()));
                }
                foreach (ActionLimit actionLimit in Enum.GetValues(typeof(ActionLimit)))
                {
                    data.Add(new CompletionData(actionLimit.ToString().ToLower()));
                }
                foreach (Weapon weapon in Enum.GetValues(typeof(Weapon)))
                {
                    data.Add(new CompletionData(weapon.ToString().ToLower()));
                }
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        void te_editor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btn_restore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void c_paintcanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && e.LeftButton == MouseButtonState.Pressed)
            {
                if (selector != null)
                {
                    c_paintcanvas.Children.Remove(selector);
                    selector = null;
                }

                LineGeometry lineGeometry = new LineGeometry();

                if (mouseLastPosition == new Point(-1, -1))
                {
                    mouseLastPosition = e.GetPosition((FrameworkElement)sender);
                    return;
                }

                lineGeometry.StartPoint = mouseLastPosition;
                lineGeometry.EndPoint = e.GetPosition((FrameworkElement)sender);
                mouseLastPosition = lineGeometry.EndPoint;

                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                path.Stroke = GlobalDictionary.pathLineColor;
                path.StrokeThickness = GlobalDictionary.pathLineSize;
                path.Opacity = GlobalDictionary.pathLineOpacity;
                path.Data = lineGeometry;
                c_paintcanvas.Children.Add(path);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z) && e.LeftButton != MouseButtonState.Pressed)
            {
                if (selector != null)
                {
                    c_paintcanvas.Children.Remove(selector);
                    selector = null;
                }

                if (c_paintcanvas.Children.Count == 0)
                {
                    return;
                }
                mouseLastPosition = new Point(-1, -1);
                c_paintcanvas.Children.RemoveAt(c_paintcanvas.Children.Count - 1);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C) && te_editor.SelectionLength == 0)
            {
                mouseLastPosition = new Point(-1, -1);
                if (selector == null)
                {
                    selector = new Selector(Selector.SelectType.Color, this);
                    Canvas.SetLeft(selector, e.GetPosition((FrameworkElement)sender).X - selector.Width / 2);
                    Canvas.SetTop(selector, e.GetPosition((FrameworkElement)sender).Y - selector.Height / 2);
                    c_paintcanvas.Children.Add(selector);
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S) && te_editor.SelectionLength == 0)
            {
                mouseLastPosition = new Point(-1, -1);
                if (selector == null)
                {
                    selector = new Selector(Selector.SelectType.Size, this);
                    Canvas.SetLeft(selector, e.GetPosition((FrameworkElement)sender).X - selector.Width / 2);
                    Canvas.SetTop(selector, e.GetPosition((FrameworkElement)sender).Y - selector.Height / 2);
                    c_paintcanvas.Children.Add(selector);
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.A) && te_editor.SelectionLength == 0)
            {
                mouseLastPosition = new Point(-1, -1);
                if (selector == null)
                {
                    selector = new Selector(Selector.SelectType.Opacity, this);
                    Canvas.SetLeft(selector, e.GetPosition((FrameworkElement)sender).X - selector.Width / 2);
                    Canvas.SetTop(selector, e.GetPosition((FrameworkElement)sender).Y - selector.Height / 2);
                    c_paintcanvas.Children.Add(selector);
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && e.LeftButton == MouseButtonState.Pressed)
            {
                mouseLastPosition = new Point(-1, -1);
                Point mousePoint = e.GetPosition((FrameworkElement)sender);

                for (int i = 0; i < c_paintcanvas.Children.Count; ++i)
                {
                    if (!(c_paintcanvas.Children[i] is System.Windows.Shapes.Path))
                    {
                        continue;
                    }
                    System.Windows.Shapes.Path path = (System.Windows.Shapes.Path)c_paintcanvas.Children[i];
                    LineGeometry lineGeometry = (LineGeometry)path.Data;
                    if (VectorHelper.GetDistance(lineGeometry.StartPoint, mousePoint) < 2 * GlobalDictionary.pathLineSize)
                    {
                        c_paintcanvas.Children.RemoveAt(i);
                        --i;
                    }
                }
            }
            else
            {
                mouseLastPosition = new Point(-1, -1);
                if (selector != null)
                {
                    c_paintcanvas.Children.Remove(selector);
                    selector = null;
                }
            }
        }
        private void AutoShowDefaultInfoPanel()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!(Keyboard.IsKeyDown(Key.CapsLock) && Keyboard.IsKeyDown(Key.LeftShift)))
                {
                    ResetInfoPanelFontStyle();
                    ShowDefaultInfo();
                    Thread showInfoThread = new Thread(() =>
                    {
                        Thread.Sleep(5 * 1000);
                        this.Dispatcher.Invoke(() =>
                        {
                            HideDefaultInfo();
                        });
                    });
                    showInfoThread.Start();
                    ThreadHelper.AddThread(showInfoThread);
                }
            });
        }

        private void AutoShowPersonalInfoPanel()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!Keyboard.IsKeyDown(Key.CapsLock))
                {
                    ShowPersonalInfo();
                    Thread showInfoThread = new Thread(() =>
                    {
                        Thread.Sleep(5 * 1000);
                        this.Dispatcher.Invoke(() =>
                        {
                            HidePersonalInfo();
                        });
                    });
                    showInfoThread.Start();
                    ThreadHelper.AddThread(showInfoThread);
                }
            });
        }

        private void ShowDefaultInfo()
        {
            g_infos.Visibility = Visibility.Visible;
            g_infos.Tag = "DefaultInfo";
            SetInfos(currentTScore, currentCTScore);
        }
        private void ShowPersonalInfo()
        {
            g_infos.Visibility = Visibility.Visible;
            g_infos.Tag = "PersonalInfo";
            SetInfos(currentTScore, currentCTScore);
        }
        private void HideDefaultInfo()
        {
            if (!Keyboard.IsKeyDown(Key.CapsLock))
            {
                g_infos.Visibility = Visibility.Collapsed;
                g_infos.Tag = "DefaultInfo";
            }
        }
        private void HidePersonalInfo()
        {
            if (g_infos.Visibility == Visibility.Visible && g_infos.Tag.ToString() == "DefaultInfo")
            {
                return;
            }

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (Keyboard.IsKeyDown(Key.CapsLock))
                {
                    g_infos.Visibility = Visibility.Visible;
                }
                else
                {
                    g_infos.Visibility = Visibility.Collapsed;
                }
                g_infos.Tag = "DefaultInfo";
                SetInfos(currentTScore, currentCTScore);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.Delete))
            {
                c_paintcanvas.IsHitTestVisible = true;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Delete))
            {
                c_paintcanvas.Children.Clear();
            }
            else if (e.Key == Key.CapsLock && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                ShowDefaultInfo();
            }
            else if (e.Key == Key.LeftShift && Keyboard.IsKeyDown(Key.CapsLock))
            {
                ShowPersonalInfo();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                c_paintcanvas.IsHitTestVisible = false;
            }
            else if (e.Key == Key.CapsLock)
            {
                HideDefaultInfo();
            }
            else if (e.Key == Key.LeftShift)
            {
                HidePersonalInfo();
            }
        }

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            if (i_map.Source == null)
            {
                return;
            }

            GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;

            if (c_previewcanvas.Children.Count == 0)
            {
                if (te_editor.Text.Length != 0)
                {
                    PreviewAction();
                }
                else if (GlobalDictionary.mapDic.ContainsKey(cb_select_mapframe.Text))
                {
                    PreviewFrame();
                }
            }
            else
            {
                ClearPreviewCanvas();
            }
        }

        private void ClearPreviewCanvas()
        {
            c_previewcanvas.Children.Clear();
            CommandHelper.previewCharactorCount = 0;
        }

        private void PreviewAction()
        {
            ClearPreviewCanvas();

            List<FrameworkElement> previewElements = CommandHelper.GetPreviewElements(te_editor.Text, this);
            foreach (FrameworkElement previewElement in previewElements)
            {
                c_previewcanvas.Children.Add(previewElement);
            }
        }

        private void PreviewFrame()
        {
            ClearPreviewCanvas();

            Map mapFrame = GlobalDictionary.mapDic[cb_select_mapframe.Text];

            List<MapNode> mapNodes = mapFrame.mapNodes;

            foreach (MapNode mapNode in mapNodes)
            {
                Image characterImg = new Image();
                characterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyAliveUpperPath));
                characterImg.Width = GlobalDictionary.CharacterWidthAndHeight;
                characterImg.Height = GlobalDictionary.CharacterWidthAndHeight;
                Point charactorWndPoint = GetWndPoint(mapNode.nodePoint, ImgType.Character);
                Canvas.SetLeft(characterImg, charactorWndPoint.X);
                Canvas.SetTop(characterImg, charactorWndPoint.Y);
                characterImg.Tag += "Current Node -- Index: " + mapNode.index + "\n";
                foreach (KeyValuePair<int, WayInfo> neighbourNode in mapNode.neighbourNodes)
                {
                    characterImg.Tag += "Neighbour Node -- Index: " + neighbourNode.Key + " " + "Action Limit: " + neighbourNode.Value.actionLimit.ToString() + "\n";
                }
                characterImg.MouseEnter += ShowCharacterImgInfos;
                c_previewcanvas.Children.Add(characterImg);
            }
        }

        public void CreateCommandInWindow(Point mapPoint)
        {
            PropertiesSetter propertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
            propertiesSetter.EnableCloseButton = true;
            // 右键单击地图
            // set entirety speed
            // set camp
            // create character
            Button buttonSpeed = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonSpeed.Content = "设置速度";
            Button buttonCamp = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonCamp.Content = "设置阵营";
            Button buttonCreateCharacter = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonCreateCharacter.Content = "创建角色";
            buttonSpeed.Click += delegate (object sender, RoutedEventArgs e)
            {
                MessageBox.ButtonList = new RefreshList { new Label() { Content = "设置速度", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center }, new TextBox() { Width = 60, Height = 32, FontSize = 20, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 10) }, new ButtonSpacer(200), "OK" };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "填写速度比率";
            };
            buttonCamp.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.FontSize = 20;
                comboBox.Width = 60;
                comboBox.Height = 32;
                comboBox.Margin = new Thickness(0, 10, 0, 10);
                comboBox.VerticalContentAlignment = VerticalAlignment.Center;
                comboBox.Items.Add(new ComboBoxItem() { Content = "T", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.Items.Add(new ComboBoxItem() { Content = "CT", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.SelectedIndex = 0;
                MessageBox.ButtonList = new RefreshList { new Label() { Content = "设置阵营", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center }, comboBox, new ButtonSpacer(200), "OK" };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择主视角阵营";
            };
            buttonCreateCharacter.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.FontSize = 20;
                comboBox.Width = 60;
                comboBox.Height = 32;
                comboBox.VerticalContentAlignment = VerticalAlignment.Center;
                comboBox.Items.Add(new ComboBoxItem() { Content = "T", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.Items.Add(new ComboBoxItem() { Content = "CT", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.SelectedIndex = 0;

                TextBox textBoxName = new TextBox();
                textBoxName.FontSize = 20;
                textBoxName.Width = 60;
                textBoxName.Height = 32;
                textBoxName.IsEnabled = false;
                textBoxName.VerticalContentAlignment = VerticalAlignment.Center;
                textBoxName.Height = 32;
                CheckBox checkBoxName = new CheckBox();
                checkBoxName.FontSize = 20;
                checkBoxName.Width = 60;
                checkBoxName.Content = "别名";
                checkBoxName.VerticalContentAlignment = VerticalAlignment.Center;
                checkBoxName.Foreground = new SolidColorBrush(Colors.White);
                checkBoxName.Click += delegate (object cbSender, RoutedEventArgs cbE)
                {
                    if (((CheckBox)cbSender).IsChecked == true)
                    {
                        textBoxName.IsEnabled = true;
                    }
                    else
                    {
                        textBoxName.Text = "";
                        textBoxName.IsEnabled = false;
                    }
                };
                Grid gridName = new Grid();
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) };
                gridName.RowDefinitions.Add(rowDefinitionR0);
                gridName.RowDefinitions.Add(rowDefinitionR1);
                Grid.SetRow(checkBoxName, 0);
                Grid.SetRow(textBoxName, 1);
                gridName.Children.Add(checkBoxName);
                gridName.Children.Add(textBoxName);
                gridName.Width = 150;
                gridName.Height = 50;
                gridName.Margin = new Thickness(5);

                MessageBox.ButtonList = new RefreshList { new Label() { Content = "创建角色", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center }, comboBox, gridName, new ButtonSpacer(50), "OK" };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择角色阵营和坐标";
            };
            int res = MessageBox.Show(propertiesSetter, new RefreshList { buttonSpeed, buttonCamp, buttonCreateCharacter }, "选择命令的种类", "创建命令", MessageBoxImage.Question);
            if (res == -1)
            {
                return;
            }
            switch ((MessageBox.ButtonList[0] as Label).Content)
            {
                case "设置速度":
                    double speedController = -1;
                    try
                    {
                        speedController = double.Parse((MessageBox.ButtonList[1] as TextBox).Text);
                    }
                    catch
                    {
                        PropertiesSetter newPropertiesSetter = new PropertiesSetter(propertiesSetter);
                        newPropertiesSetter.CloseTimer = new MessageBoxCloseTimer(3, 0);
                        MessageBox.Show(newPropertiesSetter, "请输入数字", "错误", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    }
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "set entirety speed " + speedController + "\n";
                    break;
                case "设置阵营":
                    string camp = (MessageBox.ButtonList[1] as ComboBox).Text.ToLower();
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "set camp " + camp + "\n";
                    break;
                case "创建角色":
                    string newCamp = (MessageBox.ButtonList[1] as ComboBox).Text.ToLower();
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "create character " + newCamp + " " + mapPoint;

                    TextBox textBox = (MessageBox.ButtonList[2] as Grid).Children[1] as TextBox;
                    if (textBox.IsEnabled == true)
                    {
                        te_editor.Text += " name " + textBox.Text;
                    }

                    te_editor.Text += "\n";
                    break;
            }
        }
        public void CreateCommandInWindow(int characterNumber)
        {
            PropertiesSetter propertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
            propertiesSetter.EnableCloseButton = true;
            // 右键单击预览
            // give character weapon
            // give character missile
            // give character props
            // set character status
            // set character vertical position
            // do
            // wait for
            // wait until
            Button buttonGive = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonGive.Content = "装备";
            Button buttonSet = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonSet.Content = "设置";
            Button buttonDo = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonDo.Content = "动作";
            Button buttonWait = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonWait.Content = "等待";

            buttonGive.Click += delegate (object sender, RoutedEventArgs e)
            {
                MultiSelectComboBox multiSelectComboBox = new MultiSelectComboBox();
                multiSelectComboBox.SelectedItems = new List<string>();
                multiSelectComboBox.ItemsSource = new List<string>();
                multiSelectComboBox.ItemsSource.Add("烟 - Smoke");
                multiSelectComboBox.ItemsSource.Add("火 - Firebomb");
                multiSelectComboBox.ItemsSource.Add("雷 - Grenade");
                multiSelectComboBox.ItemsSource.Add("闪1 - Flashbang");
                multiSelectComboBox.ItemsSource.Add("闪2 - Flashbang");
                multiSelectComboBox.ItemsSource.Add("诱 - Decoy");
                multiSelectComboBox.Height = 32;
                multiSelectComboBox.FontSize = 20;
                multiSelectComboBox.VerticalContentAlignment = VerticalAlignment.Center;
                multiSelectComboBox.Margin = new Thickness(0, 5, 0, 5);
                //multiSelectComboBox.SelectedItemsChanged += delegate (object s, SelectedItemsChangedEventArgs sice) 
                //{
                //};

                ComboBox comboBoxProps = new ComboBox();
                comboBoxProps.Height = 32;
                comboBoxProps.Margin = new Thickness(0, 5, 0, 5);
                comboBoxProps.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxProps.FontSize = 20;
                comboBoxProps.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (Props props in Enum.GetValues(typeof(Props)))
                {
                    if (props == Props.Nothing)
                    {
                        continue;
                    }
                    comboBoxProps.Items.Add(new ComboBoxItem() { Content = props.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxProps.SelectedIndex = 0;

                ComboBox comboBoxWeapon = new ComboBox();
                comboBoxWeapon.Height = 32;
                comboBoxWeapon.Margin = new Thickness(0, 5, 0, 5);
                comboBoxWeapon.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxWeapon.FontSize = 20;
                comboBoxWeapon.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (Weapon weapon in Enum.GetValues(typeof(Weapon)))
                {
                    comboBoxWeapon.Items.Add(new ComboBoxItem() { Content = weapon.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxWeapon.SelectedIndex = 0;

                Grid grid = new Grid();
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR2 = new RowDefinition() { Height = new GridLength(42) };
                grid.RowDefinitions.Add(rowDefinitionR0);
                grid.RowDefinitions.Add(rowDefinitionR1);
                grid.RowDefinitions.Add(rowDefinitionR2);
                Grid.SetRow(comboBoxWeapon, 0);
                Grid.SetRow(multiSelectComboBox, 1);
                Grid.SetRow(comboBoxProps, 2);
                grid.Children.Add(comboBoxWeapon);
                grid.Children.Add(multiSelectComboBox);
                grid.Children.Add(comboBoxProps);
                grid.Height = 126;
                grid.Width = 350;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "装备", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    grid,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择要给予的装备";
            };
            buttonSet.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBoxStatus = new ComboBox();
                comboBoxStatus.Height = 32;
                comboBoxStatus.Margin = new Thickness(0, 5, 0, 5);
                comboBoxStatus.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxStatus.FontSize = 20;
                comboBoxStatus.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (Model.Status status in Enum.GetValues(typeof(Model.Status)))
                {
                    comboBoxStatus.Items.Add(new ComboBoxItem() { Content = status.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxStatus.SelectedIndex = 0;

                ComboBox comboBoxVerticalPosition = new ComboBox();
                comboBoxVerticalPosition.Height = 32;
                comboBoxVerticalPosition.Margin = new Thickness(0, 5, 0, 5);
                comboBoxVerticalPosition.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxVerticalPosition.FontSize = 20;
                comboBoxVerticalPosition.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (VerticalPosition verticalPosition in Enum.GetValues(typeof(VerticalPosition)))
                {
                    comboBoxVerticalPosition.Items.Add(new ComboBoxItem() { Content = verticalPosition.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxVerticalPosition.SelectedIndex = 0;

                Grid grid = new Grid();
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(42) };
                grid.RowDefinitions.Add(rowDefinitionR0);
                grid.RowDefinitions.Add(rowDefinitionR1);
                Grid.SetRow(comboBoxVerticalPosition, 0);
                Grid.SetRow(comboBoxStatus, 1);
                grid.Children.Add(comboBoxVerticalPosition);
                grid.Children.Add(comboBoxStatus);
                grid.Height = 84;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "设置", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    grid,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择要进行的设置";
            };
            buttonDo.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Height = 32;
                comboBox.FontSize = 20;
                comboBox.Width = 160;
                comboBox.Margin = new Thickness(0, 5, 0, 5);
                comboBox.VerticalContentAlignment = VerticalAlignment.Center;
                foreach (DoWithProps doWithProps in Enum.GetValues(typeof(DoWithProps)))
                {
                    comboBox.Items.Add(new ComboBoxItem() { Content = doWithProps.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBox.SelectedIndex = 0;
                MessageBox.ButtonList = new RefreshList { new Label() { Content = "动作", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center }, comboBox, new ButtonSpacer(200), "OK" };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择要进行的动作";
            };
            buttonWait.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Height = 32;
                comboBox.FontSize = 20;
                comboBox.Width = 80;
                comboBox.Margin = new Thickness(0, 5, 0, 5);
                comboBox.VerticalContentAlignment = VerticalAlignment.Center;
                comboBox.Items.Add(new ComboBoxItem() { Content = "等待", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.Items.Add(new ComboBoxItem() { Content = "等待至", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.SelectedIndex = 0;
                MessageBox.ButtonList = new RefreshList
                {
                    new Label() { Content = "等待", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    comboBox,
                    new TextBox() { Width = 60, FontSize = 20, Height = 32, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 10) },
                    new Label() { Content = "秒", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    new ButtonSpacer(140),
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "选择要进行的动作";
            };
            int res = MessageBox.Show(propertiesSetter, new RefreshList { buttonGive, buttonSet, buttonDo, buttonWait }, "选择命令的种类", "创建命令", MessageBoxImage.Question);
            if (res == -1)
            {
                return;
            }

            List<object> btnList = MessageBox.ButtonList;
            switch ((btnList[0] as Label).Content)
            {
                case "装备":
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    if ((((btnList[1] as Grid).Children[0] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() != "-")
                    {
                        te_editor.Text += "give character " + characterNumber + " weapon " + (((btnList[1] as Grid).Children[0] as ComboBox).SelectedItem as ComboBoxItem).Content + "\n";
                    }
                    if (((btnList[1] as Grid).Children[1] as MultiSelectComboBox).SelectedItems != null && ((btnList[1] as Grid).Children[1] as MultiSelectComboBox).SelectedItems.Count != 0)
                    {
                        te_editor.Text += "give character " + characterNumber + " missile";
                        int i = 1;
                        foreach (string str in ((btnList[1] as Grid).Children[1] as MultiSelectComboBox).SelectedItems)
                        {
                            if (i == 5)
                            {
                                MessageBox.Show(propertiesSetter, "选择了太多投掷物, 因此只取前四个. ", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                break;
                            }
                            te_editor.Text += " " + str.Substring(str.IndexOf("-") + 2).ToLowerInvariant();
                            ++i;
                        }
                        te_editor.Text += "\n";
                    }
                    if ((((btnList[1] as Grid).Children[2] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() != "-")
                    {
                        te_editor.Text += "give character " + characterNumber + " props " + (((btnList[1] as Grid).Children[2] as ComboBox).SelectedItem as ComboBoxItem).Content + "\n";
                    }
                    break;
                case "设置":
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    if ((((btnList[1] as Grid).Children[0] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() != "-")
                    {
                        te_editor.Text += "set character " + characterNumber + " status " + (((btnList[1] as Grid).Children[0] as ComboBox).SelectedItem as ComboBoxItem).Content + "\n";
                    }
                    if ((((btnList[1] as Grid).Children[1] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() != "-")
                    {
                        te_editor.Text += "set character " + characterNumber + " vertical position " + (((btnList[1] as Grid).Children[1] as ComboBox).SelectedItem as ComboBoxItem).Content + "\n";
                    }
                    break;
                case "动作":
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    if (((btnList[1] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() != "-")
                    {
                        te_editor.Text += "action character " + characterNumber + " do " + ((btnList[1] as ComboBox).SelectedItem as ComboBoxItem).Content + "\n";
                    }
                    break;
                case "等待":
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "action character " + characterNumber + (((btnList[1] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() == "等待" ? " wait for " : " wait until ") + (btnList[2] as TextBox).Text + "\n";
                    break;
            }
        }
        public void CreateCommandInWindow(int characterNumber, Point mapPoint)
        {
            PropertiesSetter propertiesSetter = new PropertiesSetter(GlobalDictionary.propertiesSetter);
            propertiesSetter.EnableCloseButton = true;
            // 拖动预览
            // move
            // automove
            // throw
            // shoot
            Button buttonMove = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonMove.Content = "移动";
            Button buttonAutoMove = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonAutoMove.Content = "寻路";
            if (keyDownInPreview.Count < 2)
            {
                buttonAutoMove.IsEnabled = false;
            }
            Button buttonThrow = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonThrow.Content = "投掷";
            Button buttonShoot = new Button() { Visibility = Visibility.Visible, Height = 32, FontSize = 20 };
            buttonShoot.Content = "射击";
            buttonMove.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBoxRecord = new ComboBox();
                comboBoxRecord.Height = 32;
                comboBoxRecord.Margin = new Thickness(0, 5, 0, 5);
                comboBoxRecord.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxRecord.FontSize = 20;
                comboBoxRecord.Items.Add(new ComboBoxItem() { Content = "使用完整路径", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxRecord.Items.Add(new ComboBoxItem() { Content = "仅使用起始点", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxRecord.SelectedIndex = 0;

                ComboBox comboBoxMove = new ComboBox();
                comboBoxMove.Height = 32;
                comboBoxMove.Margin = new Thickness(0, 5, 0, 5);
                comboBoxMove.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxMove.FontSize = 20;
                comboBoxMove.Items.Add(new ComboBoxItem() { Content = "Run", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxMove.Items.Add(new ComboBoxItem() { Content = "Walk", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxMove.Items.Add(new ComboBoxItem() { Content = "Squat", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxMove.Items.Add(new ComboBoxItem() { Content = "Teleport", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBoxMove.SelectedIndex = 0;
                comboBoxMove.SelectionChanged += delegate (object senderSelectionChanged, SelectionChangedEventArgs eSelectionChanged)
                {
                    if ((senderSelectionChanged as ComboBox).SelectedIndex == 3)
                    {
                        comboBoxRecord.SelectedIndex = 1;
                        comboBoxRecord.IsEnabled = false;
                    }
                    else
                    {
                        comboBoxRecord.IsEnabled = true;
                    }
                };

                Grid grid = new Grid();
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(42) };
                grid.RowDefinitions.Add(rowDefinitionR0);
                grid.RowDefinitions.Add(rowDefinitionR1);
                Grid.SetRow(comboBoxRecord, 0);
                grid.Children.Add(comboBoxRecord);
                Grid.SetRow(comboBoxMove, 1);
                grid.Children.Add(comboBoxMove);
                grid.Height = 84;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "移动", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    grid,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "移动设置";
            };
            buttonAutoMove.Click += delegate (object sender, RoutedEventArgs e)
            {
                Label labelStartLayer = new Label() { Content = "起始层数", Foreground = new SolidColorBrush(Colors.White), FontSize = 20, Height = 32 };
                TextBox textBoxStartLayer = new TextBox() { FontSize = 20, Height = 32, Width = 50, VerticalContentAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left, Name = "textBoxStartLayer" };
                Label labelEndLayer = new Label() { Content = "结束层数", Foreground = new SolidColorBrush(Colors.White), FontSize = 20, Height = 32 };
                TextBox textBoxEndLayer = new TextBox() { FontSize = 20, Height = 32, Width = 50, VerticalContentAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left, Name = "textBoxEndLayer" };
                if (keyDownInPreview.Count < 2 || keyDownInPreview.Count > 2)
                {
                    textBoxStartLayer.Text = "0";
                    textBoxEndLayer.Text = "0";
                    textBoxStartLayer.IsReadOnly = true;
                    textBoxEndLayer.IsReadOnly = true;
                }

                ComboBox comboBoxVolumeLimit = new ComboBox();
                comboBoxVolumeLimit.Height = 32;
                comboBoxVolumeLimit.Width = 100;
                comboBoxVolumeLimit.Margin = new Thickness(0, 5, 0, 5);
                comboBoxVolumeLimit.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxVolumeLimit.FontSize = 20;
                comboBoxVolumeLimit.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (VolumeLimit volumeLimit in Enum.GetValues(typeof(VolumeLimit)))
                {
                    comboBoxVolumeLimit.Items.Add(new ComboBoxItem() { Content = volumeLimit.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxVolumeLimit.SelectedIndex = 0;

                Grid grid = new Grid();
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(42) };
                ColumnDefinition columnDefinitionC0 = new ColumnDefinition() { Width = new GridLength(100) };
                ColumnDefinition columnDefinitionC1 = new ColumnDefinition() { };
                grid.RowDefinitions.Add(rowDefinitionR0);
                grid.RowDefinitions.Add(rowDefinitionR1);
                grid.ColumnDefinitions.Add(columnDefinitionC0);
                grid.ColumnDefinitions.Add(columnDefinitionC1);
                Grid.SetRow(labelStartLayer, 0);
                Grid.SetColumn(labelStartLayer, 0);
                grid.Children.Add(labelStartLayer);
                Grid.SetRow(textBoxStartLayer, 0);
                Grid.SetColumn(textBoxStartLayer, 1);
                grid.Children.Add(textBoxStartLayer);
                Grid.SetRow(labelEndLayer, 1);
                Grid.SetColumn(labelEndLayer, 0);
                grid.Children.Add(labelEndLayer);
                Grid.SetRow(textBoxEndLayer, 1);
                Grid.SetColumn(textBoxEndLayer, 1);
                grid.Children.Add(textBoxEndLayer);
                grid.Height = 84;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "寻路", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    grid,
                    comboBoxVolumeLimit,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "寻路设置";
            };
            buttonThrow.Click += delegate (object sender, RoutedEventArgs e)
            {
                ComboBox comboBoxMissile = new ComboBox();
                comboBoxMissile.Height = 32;
                comboBoxMissile.Width = 130;
                comboBoxMissile.Margin = new Thickness(0, 5, 0, 5);
                comboBoxMissile.VerticalContentAlignment = VerticalAlignment.Center;
                comboBoxMissile.FontSize = 20;
                comboBoxMissile.Items.Add(new ComboBoxItem() { Content = "-", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                foreach (Missile missile in Enum.GetValues(typeof(Missile)))
                {
                    comboBoxMissile.Items.Add(new ComboBoxItem() { Content = missile.ToString().ToLower(), FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                }
                comboBoxMissile.SelectedIndex = 0;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "投掷", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    comboBoxMissile,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "投掷设置";
            };
            buttonShoot.Click += delegate (object sender, RoutedEventArgs e)
            {
                CheckBox cb = new CheckBox() { Content = "射击某角色", FontSize = 20, Foreground = new SolidColorBrush(Colors.White), Height = 32 };
                TextBox tb = new TextBox() { Width = 60, VerticalContentAlignment = VerticalAlignment.Center, FontSize = 20, Height = 32, HorizontalAlignment = HorizontalAlignment.Left };
                tb.IsEnabled = false;
                cb.Click += delegate (object cbSender, RoutedEventArgs cbE)
                {
                    if (((CheckBox)cbSender).IsChecked == true)
                    {
                        tb.IsEnabled = true;
                    }
                    else
                    {
                        tb.Text = "";
                        tb.IsEnabled = false;
                    }
                };

                Grid grid = new Grid();
                grid.Margin = new Thickness(0, 10, 0, 10);
                RowDefinition rowDefinitionR0 = new RowDefinition() { Height = new GridLength(42) };
                RowDefinition rowDefinitionR1 = new RowDefinition() { Height = new GridLength(42) };
                grid.RowDefinitions.Add(rowDefinitionR0);
                grid.RowDefinitions.Add(rowDefinitionR1);
                Grid.SetRow(cb, 0);
                grid.Children.Add(cb);
                Grid.SetRow(tb, 1);
                grid.Children.Add(tb);
                grid.Height = 84;

                ComboBox comboBox = new ComboBox();
                comboBox.Margin = new Thickness(0, 10, 0, 10);
                comboBox.VerticalContentAlignment = VerticalAlignment.Center;
                comboBox.FontSize = 20;
                comboBox.Items.Add(new ComboBoxItem() { Content = "Live", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.Items.Add(new ComboBoxItem() { Content = "Die", FontSize = 20, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center });
                comboBox.Height = 32;
                comboBox.SelectedIndex = 0;

                MessageBox.ButtonList = new RefreshList {
                    new Label() { Content = "射击", Width = 60, FontSize = 20, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center },
                    grid,
                    comboBox,
                    "OK"
                };
                MessageBox.MessageBoxImageType = MessageBoxImage.None;
                MessageBox.MessageText = "射击设置";
            };
            int res = MessageBox.Show(propertiesSetter, new RefreshList { buttonMove, buttonAutoMove, buttonThrow, buttonShoot }, "选择命令的种类", "创建命令", MessageBoxImage.Question);
            if (res == -1)
            {
                return;
            }
            switch ((MessageBox.ButtonList[0] as Label).Content)
            {
                case "移动":
                    string pointsStr = "";
                    if ((((MessageBox.ButtonList[1] as Grid).Children[0] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString() == "使用完整路径")
                    {
                        if (keyDownInPreview.Count == 0)
                        {
                            foreach (Point point in mouseMovePathInPreview)
                            {
                                pointsStr += point.ToString() + " ";
                            }
                            if (pointsStr.Count() > 0)
                            {
                                pointsStr = pointsStr.Remove(pointsStr.Count() - 1, 1);
                            }
                        }
                        else
                        {
                            foreach (Point point in keyDownInPreview)
                            {
                                pointsStr += point.ToString() + " ";
                            }
                            if (pointsStr.Count() > 0)
                            {
                                pointsStr = pointsStr.Remove(pointsStr.Count() - 1, 1);
                            }
                        }
                    }
                    else
                    {
                        pointsStr += mapPoint.ToString();
                    }
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "action character " + characterNumber + " move " + (((MessageBox.ButtonList[1] as Grid).Children[1] as ComboBox).SelectedItem as ComboBoxItem).Content.ToString().ToLowerInvariant() + " " + pointsStr + "\n";
                    break;
                case "寻路":
                    Grid grid = MessageBox.ButtonList[1] as Grid;
                    string textBoxStartLayer = "";
                    string textBoxEndLayer = "";
                    foreach (UIElement ue in grid.Children)
                    {
                        if (ue is TextBox)
                        {
                            TextBox tb = (TextBox)ue;
                            if (tb.Name == "textBoxStartLayer")
                            {
                                textBoxStartLayer = tb.Text;
                            }
                            if (tb.Name == "textBoxEndLayer")
                            {
                                textBoxEndLayer = tb.Text;
                            }
                        }
                    }
                    for (int keyDownInPreviewIndex = 0; keyDownInPreview.Count > keyDownInPreviewIndex + 1; ++keyDownInPreviewIndex)
                    {
                        if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                        {
                            te_editor.Text += "\n";
                        }
                        te_editor.Text += "action character " + characterNumber + " from " + keyDownInPreview[keyDownInPreviewIndex] + " layer " + textBoxStartLayer + " auto move " + keyDownInPreview[keyDownInPreviewIndex + 1] + " layer " + textBoxEndLayer + " " + (MessageBox.ButtonList[2] as ComboBox).Text.ToLowerInvariant() + "\n";
                    }
                    break;
                case "投掷":
                    string pointsMissilePathStr = "";
                    if (keyDownInPreview.Count() == 0)
                    {
                        pointsMissilePathStr = mapPoint.ToString();
                    }
                    else
                    {
                        foreach (Point point in keyDownInPreview)
                        {
                            pointsMissilePathStr += point.ToString() + " ";
                        }
                        pointsMissilePathStr.Remove(pointsMissilePathStr.Count() - 1, 1);
                    }
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "action character " + characterNumber + " throw " + (MessageBox.ButtonList[1] as ComboBox).Text.ToLowerInvariant() + " " + pointsMissilePathStr + "\n";
                    break;
                case "射击":
                    string target = "";
                    target = ((MessageBox.ButtonList[1] as Grid).Children[0] as CheckBox).IsChecked == true ? ((MessageBox.ButtonList[1] as Grid).Children[1] as TextBox).Text : mapPoint.ToString();
                    if (te_editor.Text.Count() > 0 && te_editor.Text[te_editor.Text.Count() - 1] != '\n')
                    {
                        te_editor.Text += "\n";
                    }
                    te_editor.Text += "action character " + characterNumber + " shoot " + target + " " + (MessageBox.ButtonList[2] as ComboBox).Text.ToLowerInvariant() + "\n";
                    break;
            }
        }

        private void i_map_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (i_map.Source != null)
            {
                GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;
            }
            CreateCommandInWindow(new Point(Math.Round((e.GetPosition(i_map).X / GlobalDictionary.ImageRatio), 2), Math.Round((e.GetPosition(i_map).Y / GlobalDictionary.ImageRatio), 2)));
        }

        private void Btn_forward_Click(object sender, RoutedEventArgs e)
        {
            string filePath = tb_select_file.Text;
            if (nowRunningType == RunningType.DEM && btn_pause.Tag != null && btn_pause.Tag.ToString() == "R")
            {
                isForward = true;
            }
        }

        private void Btn_backward_Click(object sender, RoutedEventArgs e)
        {
            string filePath = tb_select_file.Text;
            if (nowRunningType == RunningType.DEM && btn_pause.Tag != null && btn_pause.Tag.ToString() == "R")
            {
                isBackward = true;
            }
        }

        private void Btn_auto_Click(object sender, RoutedEventArgs e)
        {
            isNeedAutomaticGuidance = true;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender.Equals(i_map))
            {
                double width = e.NewSize.Width - g_infos.Margin.Left - g_infos.Margin.Right;
                if (width < 0)
                {
                    width = 0;
                }
                g_infos.Width = width;
            }

            tb_timer.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
            tb_infos.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;

            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizingDone(object sender, ElapsedEventArgs e)
        {
            resizeTimer.Stop();

            c_runcanvas.Dispatcher.Invoke(() =>
            {
                if (i_map.Source != null)
                {
                    MainSizeChanged();
                }
            });
        }

        private void MainSizeChanged()
        {
            GlobalDictionary.ImageRatio = i_map.ActualWidth / i_map.Source.Width;

            foreach (UIElement element in g_infos.Children)
            {
                if (element is TextBlock)
                {
                    if (element == tb_team1 || element == tb_team2)
                    {
                        (element as TextBlock).FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
                        (element as TextBlock).Margin = new Thickness(GlobalDictionary.ImageRatio * 10);
                    }
                    else
                    {
                        (element as TextBlock).FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.0;
                        (element as TextBlock).Margin = new Thickness(GlobalDictionary.ImageRatio * 5);
                    }
                }
            }

            tb_infos.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;
            tb_timer.FontSize = (GlobalDictionary.ImageRatio == 0) ? 1 : 15 * GlobalDictionary.ImageRatio * 1.3;

            List<UIElement> uIElementList = new List<UIElement>();
            foreach (UIElement obj in c_runcanvas.Children)
            {
                uIElementList.Add(obj);
            }

            List<Character> characterList = CharacterHelper.GetCharacters();

            foreach (UIElement obj in uIElementList)
            {
                if (obj is Label)
                {
                    c_runcanvas.Children.Remove(obj);
                }
                else if (obj is Line)
                {
                    Line bulletLine = obj as Line;

                    c_runcanvas.Children.Remove(bulletLine);

                    List<Point> mapPointList = (List<Point>)bulletLine.Tag;

                    Point fromWndPoint = GetWndPoint(mapPointList[0], ImgType.Nothing);
                    Point toWndPoint = GetWndPoint(mapPointList[1], ImgType.Nothing);

                    bulletLine.X1 = fromWndPoint.X;
                    bulletLine.Y1 = fromWndPoint.Y;
                    bulletLine.X2 = toWndPoint.X;
                    bulletLine.Y2 = toWndPoint.Y;
                    c_runcanvas.Children.Add(bulletLine);
                }
                else if (obj is TSImage)
                {
                    TSImage img = obj as TSImage;

                    c_runcanvas.Children.Remove(img);

                    Point wndPoint = GetWndPoint(img.MapPoint, img.ImgType);
                    if (img.ImgType == ImgType.Character)
                    {
                        img.Width = GlobalDictionary.CharacterWidthAndHeight;
                        img.Height = GlobalDictionary.CharacterWidthAndHeight;

                        foreach (Character character in characterList)
                        {
                            if (character.CharacterImg.Equals(img))
                            {
                                c_runcanvas.Children.Add(CreateChacterlabel(character, wndPoint, character.Hp));
                                c_runcanvas.Children.Add(CreateChacterNumberlabel(character, wndPoint));
                            }
                        }
                    }
                    else if (img.ImgType == ImgType.ExplosionEffect)
                    {
                        img.Width = GlobalDictionary.ExplosionEffectWidthAndHeight;
                        img.Height = GlobalDictionary.ExplosionEffectWidthAndHeight;
                    }
                    else if (img.ImgType == ImgType.MissileEffect)
                    {
                        img.Width = GlobalDictionary.MissileEffectWidthAndHeight;
                        img.Height = GlobalDictionary.MissileEffectWidthAndHeight;
                    }
                    else if (img.ImgType == ImgType.Missile)
                    {
                        img.Width = GlobalDictionary.MissileWidthAndHeight;
                        img.Height = GlobalDictionary.MissileWidthAndHeight;
                    }
                    else if (img.ImgType == ImgType.Props)
                    {
                        img.Width = GlobalDictionary.PropsWidthAndHeight;
                        img.Height = GlobalDictionary.PropsWidthAndHeight;
                    }

                    Canvas.SetLeft(img, wndPoint.X);
                    Canvas.SetTop(img, wndPoint.Y);

                    c_runcanvas.Children.Add(img);
                }
            }

            foreach (Character character in characterList)
            {
                if (character.StatusImg.Visibility == Visibility.Visible)
                {
                    c_runcanvas.Children.Remove(character.StatusImg);

                    character.StatusImg.Width = character.CharacterImg.Width * 1.5;
                    character.StatusImg.Height = character.CharacterImg.Height * 1.5;

                    Point mapPoint = character.StatusImg.MapPoint;
                    Canvas.SetLeft(character.StatusImg, GetWndPoint(mapPoint, ImgType.Nothing).X);
                    Canvas.SetTop(character.StatusImg, GetWndPoint(mapPoint, ImgType.Nothing).Y);

                    c_runcanvas.Children.Add(character.StatusImg);
                }
                if (character.OtherImg.Visibility == Visibility.Visible)
                {
                    c_runcanvas.Children.Remove(character.OtherImg);

                    character.OtherImg.Width = character.CharacterImg.Width * 1.5;
                    character.OtherImg.Height = character.CharacterImg.Height * 1.5;

                    Point mapPoint = character.OtherImg.MapPoint;
                    Canvas.SetLeft(character.OtherImg, GetWndPoint(mapPoint, ImgType.Nothing).X);
                    Canvas.SetTop(character.OtherImg, GetWndPoint(mapPoint, ImgType.Nothing).Y);

                    c_runcanvas.Children.Add(character.OtherImg);
                }
            }
        }
    }
}
