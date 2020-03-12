using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using CSGOTacticSimulator.Model;
using CSGOTacticSimulator.Helper;
using CSGOTacticSimulator.Global;
using CustomizableMessageBox;
using MessageBox = CustomizableMessageBox.MessageBox;
using System.Threading;
using ICSharpCode.AvalonEdit.Search;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace CSGOTacticSimulator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Character> characters = new List<Character>();
        public List<Animation> animations = new List<Animation>();
        private Camp currentCamp;
        double ratio = 1;
        double localSpeedController = -1;
        bool bombDefused = false;
        CompletionWindow completionWindow;

        private List<Thread> listThread = new List<Thread>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //快速搜索功能
            SearchPanel.Install(te_editor.TextArea);
            //设置语法规则
            string highlightPath = GlobalDictionary.highlightPath;
            XmlTextReader reader = new XmlTextReader(highlightPath);
            var xshd = HighlightingLoader.LoadXshd(reader);
            te_editor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);

            te_editor.TextArea.TextEntering += te_editor_TextArea_TextEntering;
            te_editor.TextArea.TextEntered += te_editor_TextArea_TextEntered;

            ImageBrush backgroundBrush = new ImageBrush();
            backgroundBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.backgroundPath));
            backgroundBrush.Stretch = Stretch.Fill;
            this.Background = backgroundBrush;

            ImageBrush minimizeBrush = new ImageBrush();
            minimizeBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.minimizePath));
            minimizeBrush.Stretch = Stretch.Uniform;
            btn_minimize.Background = minimizeBrush;

            ImageBrush runBrush = new ImageBrush();
            runBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.runPath));
            runBrush.Stretch = Stretch.Uniform;
            btn_run.Background = runBrush;

            ImageBrush stopBrush = new ImageBrush();
            stopBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.stopPath));
            stopBrush.Stretch = Stretch.Uniform;
            btn_stop.Background = stopBrush;

            ImageBrush saveBrush = new ImageBrush();
            saveBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.savePath));
            saveBrush.Stretch = Stretch.Uniform;
            btn_save.Background = saveBrush;

            ImageBrush exitBrush = new ImageBrush();
            exitBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.exitPath));
            exitBrush.Stretch = Stretch.Uniform;
            btn_exit.Background = exitBrush;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            int resultIndex = MessageBox.Show(new List<object> { "保存后退出", "直接退出", new ButtonSpacer(200), "取消" }, "是否另存战术脚本", "正在退出程序", MessageBoxImage.Warning);
            if (MessageBox.ButtonList[resultIndex].ToString() == "取消")
            {
                return;
            }
            else if(MessageBox.ButtonList[resultIndex].ToString() == "保存后退出")
            {
                SaveFile();
            }
            Stop();

            Application.Current.Shutdown(-1);
        }

        private void StopAllThread()
        {
            for (int i = listThread.Count - 1; i >= 0; --i)
            {
                listThread[i].Abort();
            }
            listThread.Clear();
        }

        private void i_map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointInMap = new Point(Math.Round((e.GetPosition(i_map).X / ratio), 2), Math.Round((e.GetPosition(i_map).Y / ratio), 2));
            tb_point.Text = pointInMap.ToString();
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
            cb_select_map.Items.Clear();

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (!dir.Exists)
                return;
            DirectoryInfo dirD = dir as DirectoryInfo;
            FileSystemInfo[] files = dirD.GetFileSystemInfos();

            List<string> maps = new List<string>();
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(GlobalDictionary.mapListPath);
            while ((line = file.ReadLine()) != null)
            {
                maps.Add(line);
            }

            foreach (FileSystemInfo fileSystemInfo in files)
            {
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                if (fileInfo != null)
                {
                    foreach (string map in maps)
                    {
                        if (fileInfo.Name.ToUpper().IndexOf(map.ToUpper()) != -1)
                        {
                            ComboBoxItem cbItem = new ComboBoxItem();
                            cbItem.Content = map;
                            cbItem.Tag = fileInfo.FullName;
                            cb_select_map.Items.Add(cbItem);
                        }
                    }
                }
            }
        }

        private void cb_select_map_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)cb_select_map.SelectedItem;
            if (selectedItem != null)
            {
                i_map.Source = new BitmapImage(new Uri(selectedItem.Tag.ToString(), UriKind.Absolute));
                tb_infos.Visibility = Visibility.Visible;
                tb_timer.Visibility = Visibility.Visible;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (i_map.Source != null)
            {
                ratio = i_map.ActualWidth / i_map.Source.Width;
            }

            foreach (Character character in characters)
            {
                Point wndPoint = GetWndPoint(character.MapPoint, ImgType.Character);
                c_canvas.Children.Remove(character.CharacterImg);
                Canvas.SetLeft(character.CharacterImg, wndPoint.X);
                Canvas.SetTop(character.CharacterImg, wndPoint.Y);
                c_canvas.Children.Add(character.CharacterImg);
            }
        }

        public void NewCharacter(Character character, Point mapPoint)
        {
            Point wndPoint = GetWndPoint(mapPoint, ImgType.Character);

            characters.Add(character);

            Canvas.SetLeft(character.CharacterImg, wndPoint.X);
            Canvas.SetTop(character.CharacterImg, wndPoint.Y);
            c_canvas.Children.Add(character.CharacterImg);
        }

        private Point GetWndPoint(Point mapPoint, ImgType imgType)
        {
            Point scaledMapPoint = new Point(mapPoint.X * ratio, mapPoint.Y * ratio);
            Point wndPoint = new Point();
            i_map.Dispatcher.Invoke(() =>
            {
                wndPoint = i_map.TranslatePoint(scaledMapPoint, this);
            });
            int widthAndHeight = 0;
            switch (imgType)
            {
                case ImgType.Character:
                    widthAndHeight = GlobalDictionary.characterWidthAndHeight;
                    break;
                case ImgType.MissileEffect:
                    widthAndHeight = GlobalDictionary.missileEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.explosionEffectWidthAndHeight;
                    break;
                case ImgType.Missile:
                    widthAndHeight = GlobalDictionary.missileWidthAndHeight;
                    break;
                case ImgType.Props:
                    widthAndHeight = GlobalDictionary.propsWidthAndHeight;
                    break;
                case ImgType.Nothing:
                    widthAndHeight = 0;
                    break;
            }
            wndPoint = new Point(wndPoint.X - widthAndHeight / 2, wndPoint.Y - widthAndHeight / 2);
            return wndPoint;
        }

        private Point GetMapPoint(Point wndPoint, ImgType imgType)
        {
            int widthAndHeight = 0;
            switch (imgType)
            {
                case ImgType.Character:
                    widthAndHeight = GlobalDictionary.characterWidthAndHeight;
                    break;
                case ImgType.MissileEffect:
                    widthAndHeight = GlobalDictionary.missileEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.explosionEffectWidthAndHeight;
                    break;
                case ImgType.Props:
                    widthAndHeight = GlobalDictionary.propsWidthAndHeight;
                    break;
                case ImgType.Missile:
                    widthAndHeight = GlobalDictionary.missileWidthAndHeight;
                    break;
                case ImgType.Nothing:
                    widthAndHeight = 0;
                    break;
            }
            wndPoint = new Point(wndPoint.X + widthAndHeight / 2, wndPoint.Y + widthAndHeight / 2);
            Point scaledMapPoint = new Point();
            i_map.Dispatcher.Invoke(() =>
            {
                scaledMapPoint = this.TranslatePoint(wndPoint, i_map);
            });
            Point mapPoint = new Point(scaledMapPoint.X / ratio, scaledMapPoint.Y / ratio);
            return mapPoint;
        }

        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            if(i_map.Source == null)
            {
                return;
            }

            ratio = i_map.ActualWidth / i_map.Source.Width;

            bombDefused = false;

            Stop();

            CommandHelper.GetCommands(te_editor.Text);

            foreach (string command in CommandHelper.commands)
            {
                string processedCommand = command.Replace("\r", "").Trim();
                Command commandType = CommandHelper.AnalysisCommand(command);
                switch (commandType)
                {
                    case Command.SetEntiretySpeed:
                        SetEntiretySpeed(processedCommand);
                        break;
                    case Command.ActionCharacterDo:
                        ActionCharacterDo(processedCommand);
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
                }
            }

            StartTimer();

            TraversalAnimations();
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
            listThread.Add(timerThread);
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
            new Character(isFriendly, isT, mapPoint, this);
        }

        private void SetCharacterStatus(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
            Model.VerticalPosition charactorVerticalPosition = Model.VerticalPosition.Upper;
            for (int i = 0; i < characters.Count; ++i)
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
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
                toMapPoint = VectorHelper.Cast(splitedCmd[4]);
                animations.Add(new Animation(number, Helper.Action.Shoot, toMapPoint));
            }
        }
        private void ActionCharacterWaitUntil(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
            double second = double.Parse(splitedCmd[5]);
            animations.Add(new Animation(number, Helper.Action.WaitUntil, new Point(), second));
        }
        private void ActionCharacterWaitFor(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
            double second = double.Parse(splitedCmd[5]);
            animations.Add(new Animation(number, Helper.Action.WaitFor, new Point(), second));
        }
        private void ActionCharacterDo(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
        private void ActionCharacterMove(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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

            Point endMapPoint = VectorHelper.Cast(splitedCmd[5]);
            animations.Add(new Animation(number, action, endMapPoint));
        }
        private void ActionCharacterThrow(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
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
                mapPoints.Add(VectorHelper.Cast(splitedCmd[i]));
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
                foreach (Character character in characters)
                {
                    if (character.Number == ownerIndex && character.IsRunningAnimation == false)
                    {
                        character.IsRunningAnimation = true;
                        RunAnimation(character, animation);
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
            if(character.Status == Model.Status.Dead)
            {
                return;
            }

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
            listThread.Add(waitUntilThread);
        }

        private void RunAnimationWaitFor(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

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
            listThread.Add(waitForThread);
        }
        private void RunAnimationChangeVerticalPosition(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

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

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            int defuseTime = 10;
            if (characters[characters.IndexOf(character)].Props == Props.DefuseKit)
            {
                defuseTime = 5;
            }

            bool canDefuse = false;

            Image img = null;
            foreach (UIElement obj in c_canvas.Children)
            {
                if (obj is Image)
                {
                    img = (Image)obj;
                    BitmapImage bitmapImage = (BitmapImage)img.Source;
                    if (bitmapImage.UriSource.LocalPath.Contains("props_bomb"))
                    {
                        if (VectorHelper.GetDistance(character.MapPoint, GetMapPoint(new Point(Canvas.GetLeft(c_canvas.Children[c_canvas.Children.IndexOf(obj)]), Canvas.GetTop(c_canvas.Children[c_canvas.Children.IndexOf(obj)])), ImgType.Props)) <= 50)
                        {
                            canDefuse = true;
                        }
                    }
                }
            }
            if (!canDefuse)
            {
                return;
            }
            Thread defuseThread = new Thread(() =>
            {
                Thread.Sleep(defuseTime * 1000);

                c_canvas.Dispatcher.Invoke(() =>
                {
                    c_canvas.Children.Remove(img);
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
            listThread.Add(defuseThread);


            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }

        private void RunAnimationPlant(Character character, Animation animation)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            characters[characters.IndexOf(character)].IsRunningAnimation = true;
            animations[animations.IndexOf(animation)].status = Helper.Status.Running;

            if (characters[characters.IndexOf(character)].Props == Props.Bomb)
            {
                Image bombImage = new Image();
                bombImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/props_bomb.png")));
                bombImage.Width = GlobalDictionary.propsWidthAndHeight;
                bombImage.Height = GlobalDictionary.propsWidthAndHeight;
                bombImage.Opacity = 0.75;
                Point bombWndPoint = GetWndPoint(character.MapPoint, ImgType.Props);

                Image explosionImage = new Image();
                explosionImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/explosion.png")));
                explosionImage.Width = GlobalDictionary.explosionEffectWidthAndHeight;
                explosionImage.Height = GlobalDictionary.explosionEffectWidthAndHeight;
                Point explosionWndPoint = GetWndPoint(character.MapPoint, ImgType.ExplosionEffect);
                Thread explosionThread = null;
                Thread plantThread = new Thread(() =>
                {
                    Thread.Sleep(4000);
                    c_canvas.Dispatcher.Invoke(() =>
                    {
                        Canvas.SetLeft(bombImage, bombWndPoint.X);
                        Canvas.SetTop(bombImage, bombWndPoint.Y);
                        c_canvas.Children.Add(bombImage);
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
                        c_canvas.Dispatcher.Invoke(() =>
                        {
                            c_canvas.Children.Remove(bombImage);
                        });

                        c_canvas.Dispatcher.Invoke(() =>
                        {
                            Canvas.SetLeft(explosionImage, explosionWndPoint.X);
                            Canvas.SetTop(explosionImage, explosionWndPoint.Y);
                            c_canvas.Children.Add(explosionImage);
                        });
                        Thread.Sleep(3000);
                        c_canvas.Dispatcher.Invoke(() =>
                        {
                            c_canvas.Children.Remove(explosionImage);
                        });
                    });
                    explosionThread.Start();
                    listThread.Add(explosionThread);
                });
                plantThread.Start();
                listThread.Add(plantThread);
            }
        }
        private void RunAnimationChangeStatus(Character character, Animation animation)
        {
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

            double speedController = localSpeedController != -1 ? localSpeedController : GlobalDictionary.speedController;

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * ratio * speedController;
            Point startWndPoint = GetWndPoint(character.MapPoint, ImgType.Character);
            Point endWndPoint = GetWndPoint(animation.endMapPoint, ImgType.Character);
            Point unitVector = VectorHelper.GetUnitVector(startWndPoint, endWndPoint);

            Point nowWndPoint = startWndPoint;
            Thread moveThread = null;
            moveThread = new Thread(() =>
            {
                characters[characters.IndexOf(character)].IsRunningAnimation = true;
                animations[animations.IndexOf(animation)].status = Helper.Status.Running;
                while (VectorHelper.GetDistance(VectorHelper.GetUnitVector(nowWndPoint, endWndPoint), unitVector) < 1)
                {
                    nowWndPoint = VectorHelper.Add(nowWndPoint, VectorHelper.Multiply(unitVector, pixelPerFresh));
                    characters[characters.IndexOf(character)].MapPoint = GetMapPoint(nowWndPoint, ImgType.Character);
                    c_canvas.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            c_canvas.Children.Remove(character.CharacterImg);
                            Canvas.SetLeft(character.CharacterImg, nowWndPoint.X);
                            Canvas.SetTop(character.CharacterImg, nowWndPoint.Y);
                            c_canvas.Children.Add(character.CharacterImg);
                        }
                        catch
                        {
                            Stop();
                        }
                    });

                    Thread.Sleep(animationFreshTime);
                }

                characters[characters.IndexOf(character)].IsRunningAnimation = false;
                animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    TraversalAnimations();
                }));

            });
            moveThread.Start();
            listThread.Add(moveThread);
        }

        private void RunAnimationThrow(Character character, Animation animation, double speed)
        {
            if (character.Status == Model.Status.Dead)
            {
                return;
            }

            double speedController = localSpeedController != -1 ? localSpeedController : GlobalDictionary.speedController;

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * ratio * speedController;
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
                    missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_decoy.png")));
                    break;
                case Missile.Firebomb:
                    missileEffectImg = new Image();
                    if (character.IsT)
                    {
                        missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_molotov.png")));
                        missileEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_fire.png")));
                    }
                    else
                    {
                        missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_incgrenade.png")));
                        missileEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_fire.png")));
                    }
                    effectLifeSpan = GlobalDictionary.firebombLifespan;
                    break;
                case Missile.Flashbang:
                    missileEffectImg = new Image();
                    missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_flashbang.png")));
                    missileEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_flash.png")));
                    effectLifeSpan = GlobalDictionary.flashbangLifespan;
                    break;
                case Missile.Grenade:
                    missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_hegrenade.png")));
                    break;
                case Missile.Smoke:
                    missileEffectImg = new Image();
                    missileImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/missile_smokegrenade.png")));
                    missileEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_smoke.png")));
                    effectLifeSpan = GlobalDictionary.smokeLifespan;
                    break;
            }
            missileImg.Width = GlobalDictionary.missileWidthAndHeight;
            missileImg.Height = GlobalDictionary.missileWidthAndHeight;

            if (missileEffectImg != null)
            {
                missileEffectImg.Opacity = 0.85;
                missileEffectImg.Width = GlobalDictionary.missileEffectWidthAndHeight;
                missileEffectImg.Height = GlobalDictionary.missileEffectWidthAndHeight;
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
                    MessageBox.Show(new List<object> { new ButtonSpacer(500), "确定" }, "角色" + character.Number + "未持有道具" + missile.ToString() + ", 可能是没有配备, 或已经被扔出, 因此无法投掷. ", "错误", MessageBoxImage.Warning);
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

                        c_canvas.Dispatcher.Invoke(() =>
                        {
                            if (c_canvas.Children.Contains(missileImg))
                            {
                                c_canvas.Children.Remove(missileImg);
                            }
                            Canvas.SetLeft(missileImg, nowWndPoint.X);
                            Canvas.SetTop(missileImg, nowWndPoint.Y);
                            c_canvas.Children.Add(missileImg);
                        });

                        Thread.Sleep(animationFreshTime);
                    }
                    startWndPoint = nowWndPoint;
                }
                c_canvas.Dispatcher.Invoke(() =>
                {
                    if (c_canvas.Children.Contains(missileImg))
                    {
                        c_canvas.Children.Remove(missileImg);
                    }
                });

                nowWndPoint = GetWndPoint(GetMapPoint(nowWndPoint, ImgType.Missile), ImgType.MissileEffect);
                c_canvas.Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(missileEffectImg, nowWndPoint.X);
                    Canvas.SetTop(missileEffectImg, nowWndPoint.Y);
                    c_canvas.Children.Add(missileEffectImg);
                });
                Thread.Sleep(effectLifeSpan * 1000);
                c_canvas.Dispatcher.Invoke(() =>
                {
                    if (c_canvas.Children.Contains(missileEffectImg))
                    {
                        c_canvas.Children.Remove(missileEffectImg);
                    }
                });
            });
            throwThread.Start();
            listThread.Add(throwThread);

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
            c_canvas.Children.Add(bulletLine);

            Thread shootThread = new Thread(() =>
            {
                Thread.Sleep(500);
                c_canvas.Dispatcher.Invoke(() =>
                {
                    c_canvas.Children.Remove(bulletLine);
                });
            });
            shootThread.Start();
            listThread.Add(shootThread);

            characters[characters.IndexOf(character)].IsRunningAnimation = false;
            animations[animations.IndexOf(animation)].status = Helper.Status.Finished;

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                TraversalAnimations();
            }));
        }

        public void ShowCharacterInfos(object sender, MouseEventArgs e)
        {
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
                    tb_infos.Text =
                        "Number: " + character.Number +
                        "\nPosisiion: " + character.MapPoint.ToString() +
                        "\nWeapon: " + character.Weapon.ToString() +
                        "\nMissile: " + missileListStr +
                        "\nprops: " + character.Props.ToString();
                }
            }
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
            sfd.InitialDirectory = Global.GlobalDictionary.basePath;
            sfd.Filter = "txt file|*.txt";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, te_editor.Text);
            }
        }

        private void btn_select_file_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.InitialDirectory = GlobalDictionary.basePath;
            ofd.DefaultExt = ".txt";
            ofd.Filter = "txt file|*.txt";
            if (ofd.ShowDialog() == true)
            {
                string filePath = ofd.FileName;
                te_editor.Text = File.ReadAllText(filePath);
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            StopAllThread();
            CommandHelper.commands.Clear();
            animations.Clear();
            characters.Clear();
            c_canvas.Children.Clear();
            GlobalDictionary.charatersNumber = 0;
            localSpeedController = -1;
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
                data.Add(new CompletionData("bomb"));
                data.Add(new CompletionData("defusekit"));
                data.Add(new CompletionData("alive"));
                data.Add(new CompletionData("dead"));
                data.Add(new CompletionData("upper"));
                data.Add(new CompletionData("lower"));
                data.Add(new CompletionData("run"));
                data.Add(new CompletionData("walk"));
                data.Add(new CompletionData("squat"));
                data.Add(new CompletionData("teleport"));
                data.Add(new CompletionData("smoke"));
                data.Add(new CompletionData("grenade"));
                data.Add(new CompletionData("flashbang"));
                data.Add(new CompletionData("firebomb"));
                data.Add(new CompletionData("decoy"));
                data.Add(new CompletionData("die"));
                data.Add(new CompletionData("live"));
                data.Add(new CompletionData("plant"));
                data.Add(new CompletionData("defuse"));
                completionWindow.Show();
                completionWindow.Closed += delegate {
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
    }
}
