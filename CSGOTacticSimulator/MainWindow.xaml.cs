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
        bool bombDefused = false;

        private List<Thread> listThread = new List<Thread>();

        public MainWindow()
        {
            InitializeComponent();

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
                case ImgType.GrenadeEffect:
                    widthAndHeight = GlobalDictionary.grenadeEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.explosionEffectWidthAndHeight;
                    break;
                case ImgType.Grenade:
                    widthAndHeight = GlobalDictionary.grenadeWidthAndHeight;
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
                case ImgType.GrenadeEffect:
                    widthAndHeight = GlobalDictionary.grenadeEffectWidthAndHeight;
                    break;
                case ImgType.ExplosionEffect:
                    widthAndHeight = GlobalDictionary.explosionEffectWidthAndHeight;
                    break;
                case ImgType.Props:
                    widthAndHeight = GlobalDictionary.propsWidthAndHeight;
                    break;
                case ImgType.Grenade:
                    widthAndHeight = GlobalDictionary.grenadeWidthAndHeight;
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

            CommandHelper.GetCommands(tb_editor.Text);

            foreach (string command in CommandHelper.commands)
            {
                string processedCommand = command.Replace("\r", "").Trim();
                Command commandType = CommandHelper.AnalysisCommand(command);
                switch (commandType)
                {
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
                    case Command.GiveCharacterGrenade:
                        GiveCharacterGrenade(processedCommand);
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
        private void GiveCharacterGrenade(string command)
        {
            string[] splitedCmd = command.Split(' ');
            int number = int.Parse(splitedCmd[2]);
            List<Grenade> grenades = new List<Grenade>();
            for (int i = 4; i < splitedCmd.Count(); ++i)
            {
                foreach (Grenade grenade in Enum.GetValues(typeof(Grenade)))
                {
                    if (splitedCmd[i] == grenade.ToString().ToLower())
                    {
                        grenades.Add(grenade);
                    }
                }
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                if (characters[i].Number == number)
                {
                    foreach (Grenade grenade in grenades)
                    {
                        if (characters[i].Grenades.Count >= 4)
                        {
                            return;
                        }
                        if (grenade != Grenade.Flashbang && !characters[i].Grenades.Contains(grenade))
                        {
                            characters[i].Grenades.Add(grenade);
                        }
                        else if (grenade == Grenade.Flashbang)
                        {
                            int flashbangCount = 0;
                            foreach (Grenade grenadeTemp in characters[i].Grenades)
                            {
                                if (grenadeTemp == Grenade.Flashbang)
                                {
                                    ++flashbangCount;
                                }
                            }
                            if (flashbangCount <= 1)
                            {
                                characters[i].Grenades.Add(grenade);
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
            string grenadeStr = splitedCmd[4];
            Helper.Action action = Helper.Action.Throw;
            Grenade grenade = Grenade.Nothing;
            foreach (Grenade grenadeTemp in Enum.GetValues(typeof(Grenade)))
            {
                if (grenadeTemp.ToString().ToLower() == grenadeStr)
                {
                    grenade = grenadeTemp;
                }
            }
            List<Point> mapPoints = new List<Point>();
            for (int i = 5; i < splitedCmd.Count(); ++i)
            {
                mapPoints.Add(VectorHelper.Cast(splitedCmd[i]));
            }
            animations.Add(new Animation(number, action, new Point(), grenade, mapPoints));
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
            double grenadeSpeed = double.Parse(IniHelper.ReadIni("Grenade", "Speed"));
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
                    RunAnimationThrow(character, animation, grenadeSpeed);
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

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * ratio * GlobalDictionary.speedController;
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

            int animationFreshTime = GlobalDictionary.animationFreshTime;
            double pixelPerFresh = speed / (1000 / GlobalDictionary.animationFreshTime) * ratio * GlobalDictionary.speedController;
            Point startWndPoint = GetWndPoint(character.MapPoint, ImgType.Character);
            Grenade grenade = (Grenade)animation.objectPara[0];
            List<Point> mapPoints = (List<Point>)animation.objectPara[1];
            List<Point> wndPoints = new List<Point>();
            foreach (Point mapPoint in mapPoints)
            {
                wndPoints.Add(GetWndPoint(mapPoint, ImgType.Grenade));
            }

            Image grenadeImg = new Image();
            Image grenadeEffectImg = null;
            int effectLifeSpan = 0;
            switch (grenade)
            {
                case Grenade.Decoy:
                    grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_decoy.png")));
                    break;
                case Grenade.Firebomb:
                    grenadeEffectImg = new Image();
                    if (character.IsT)
                    {
                        grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_molotov.png")));
                        grenadeEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_fire.png")));
                    }
                    else
                    {
                        grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_incgrenade.png")));
                        grenadeEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_fire.png")));
                    }
                    effectLifeSpan = GlobalDictionary.firebombLifespan;
                    break;
                case Grenade.Flashbang:
                    grenadeEffectImg = new Image();
                    grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_flashbang.png")));
                    grenadeEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_flash.png")));
                    effectLifeSpan = GlobalDictionary.flashbangLifespan;
                    break;
                case Grenade.Grenade:
                    grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_hegrenade.png")));
                    break;
                case Grenade.Smoke:
                    grenadeEffectImg = new Image();
                    grenadeImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/grenade_smokegrenade.png")));
                    grenadeEffectImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/effect_smoke.png")));
                    effectLifeSpan = GlobalDictionary.smokeLifespan;
                    break;
            }
            grenadeImg.Width = GlobalDictionary.grenadeWidthAndHeight;
            grenadeImg.Height = GlobalDictionary.grenadeWidthAndHeight;

            if (grenadeEffectImg != null)
            {
                grenadeEffectImg.Opacity = 0.85;
                grenadeEffectImg.Width = GlobalDictionary.grenadeEffectWidthAndHeight;
                grenadeEffectImg.Height = GlobalDictionary.grenadeEffectWidthAndHeight;
            }

            if (characters[characters.IndexOf(character)].Grenades.Contains(grenade))
            {
                characters[characters.IndexOf(character)].Grenades.Remove(grenade);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    Stop();
                    MessageBox.Show(new List<object> { new ButtonSpacer(500), "确定" }, "角色" + character.Number + "未持有道具" + grenade.ToString() + ", 可能是没有配备, 或已经被扔出, 因此无法投掷. ", "错误", MessageBoxImage.Warning);
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
                            if (c_canvas.Children.Contains(grenadeImg))
                            {
                                c_canvas.Children.Remove(grenadeImg);
                            }
                            Canvas.SetLeft(grenadeImg, nowWndPoint.X);
                            Canvas.SetTop(grenadeImg, nowWndPoint.Y);
                            c_canvas.Children.Add(grenadeImg);
                        });

                        Thread.Sleep(animationFreshTime);
                    }
                    startWndPoint = nowWndPoint;
                }
                c_canvas.Dispatcher.Invoke(() =>
                {
                    if (c_canvas.Children.Contains(grenadeImg))
                    {
                        c_canvas.Children.Remove(grenadeImg);
                    }
                });

                nowWndPoint = GetWndPoint(GetMapPoint(nowWndPoint, ImgType.Grenade), ImgType.GrenadeEffect);
                c_canvas.Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(grenadeEffectImg, nowWndPoint.X);
                    Canvas.SetTop(grenadeEffectImg, nowWndPoint.Y);
                    c_canvas.Children.Add(grenadeEffectImg);
                });
                Thread.Sleep(effectLifeSpan * 1000);
                c_canvas.Dispatcher.Invoke(() =>
                {
                    if (c_canvas.Children.Contains(grenadeEffectImg))
                    {
                        c_canvas.Children.Remove(grenadeEffectImg);
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
                    string grenadeListStr = "";
                    foreach (Grenade grenade in character.Grenades)
                    {
                        grenadeListStr += grenade.ToString() + " / ";
                    }
                    if (character.Grenades.Count >= 1)
                    {
                        grenadeListStr = grenadeListStr.Substring(0, grenadeListStr.Length - 3);
                    }
                    else
                    {
                        grenadeListStr = "Nothing";
                    }
                    tb_infos.Text =
                        "Number: " + character.Number +
                        "\nPosisiion: " + character.MapPoint.ToString() +
                        "\nWeapon: " + character.Weapon.ToString() +
                        "\nGrenades: " + grenadeListStr +
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
                File.WriteAllText(sfd.FileName, tb_editor.Text);
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
                tb_editor.Text = File.ReadAllText(filePath);
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush backgroundBrush = new ImageBrush();
            backgroundBrush.ImageSource = new BitmapImage(new Uri(GlobalDictionary.backgroundPath));
            backgroundBrush.Stretch = Stretch.Fill;
            this.Background = backgroundBrush;
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
