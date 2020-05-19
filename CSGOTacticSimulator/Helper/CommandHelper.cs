using CSGOTacticSimulator.Global;
using CSGOTacticSimulator.Model;
using CustomizableMessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = CustomizableMessageBox.MessageBox;

namespace CSGOTacticSimulator.Helper
{
    static public class CommandHelper
    {
        static private Dictionary<string, int> characterNameDic = new Dictionary<string, int>();
        static public int previewCharactorCount = 0;
        static public List<string> commands = new List<string>();
        static public List<string> GetCommands(string commandsText)
        {
            commands = new List<string>(string.Join(" ", commandsText.Replace("\r", "").Replace("\\\n", " ").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Split('\n'));
            for (int i = 0; i < commands.Count; ++i)
            {
                commands[i].Trim();
            }
            return commands;
        }

        static public Command AnalysisCommand(string cmd)
        {
            if (cmd.Length > 0 && cmd[0] == '-')
            {
                return Command.BadOrNotCommand;
            }

            if (cmd.Contains("set entirety speed"))
            {
                return Command.SetEntiretySpeed;
            }
            else if (cmd.Contains("set camp"))
            {
                return Command.SetCamp;
            }
            else if (cmd.Contains("create team"))
            {
                return Command.CreateTeam;
            }
            else if (cmd.Contains("create character"))
            {
                return Command.CreateCharacter;
            }
            else if (cmd.Contains("create comment"))
            {
                return Command.CreateComment;
            }
            else if (cmd.Contains("give character"))
            {
                if (cmd.Contains("weapon"))
                {
                    return Command.GiveCharacterWeapon;
                }
                else if (cmd.Contains("missile"))
                {
                    return Command.GiveCharacterMissile;
                }
                else if (cmd.Contains("props"))
                {
                    return Command.GiveCharacterProps;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else if (cmd.Contains("set character"))
            {
                if (cmd.Contains("status"))
                {
                    return Command.SetCharacterStatus;
                }
                else if (cmd.Contains("vertical position"))
                {
                    return Command.SetCharacterVerticalPosition;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else if (cmd.Contains("action character"))
            {
                if (cmd.Contains("auto move"))
                {
                    return Command.ActionCharacterAutoMove;
                }
                else if (cmd.Contains("move"))
                {
                    return Command.ActionCharacterMove;
                }
                else if (cmd.Contains("throw"))
                {
                    return Command.ActionCharacterThrow;
                }
                else if (cmd.Contains("shoot"))
                {
                    return Command.ActionCharacterShoot;
                }
                else if (cmd.Contains("do"))
                {
                    return Command.ActionCharacterDo;
                }
                else if (cmd.Contains("wait until"))
                {
                    return Command.ActionCharacterWaitUntil;
                }
                else if (cmd.Contains("wait for"))
                {
                    return Command.ActionCharacterWaitFor;
                }
                else
                {
                    return Command.BadOrNotCommand;
                }
            }
            else if (cmd.Contains("create map"))
            {
                return Command.CreateMap;
            }
            else if (cmd.Contains("create node"))
            {
                return Command.CreateNode;
            }
            else if (cmd.Contains("create path"))
            {
                return Command.CreatePath;
            }
            else if (cmd.Contains("delete node"))
            {
                return Command.DeleteNode;
            }
            else if (cmd.Contains("delete path"))
            {
                return Command.DeletePath;
            }
            else
            {
                return Command.BadOrNotCommand;
            }
        }

        private static MouseEventHandler objMouseMove;
        private static MouseButtonEventHandler objMouseMiddleDown;

        static public List<FrameworkElement> GetPreviewElements(string commandText, MainWindow mainWindow)
        {
            characterNameDic = new Dictionary<string, int>();

            List<FrameworkElement> previewElements = new List<FrameworkElement>();

            Dictionary<int, Point> charactorWndPoints = new Dictionary<int, Point>();

            int characterNumber = 0;

            List<string> commands = GetCommands(commandText);

            foreach (string command in commands)
            {
                try
                {
                    Camp currentCamp = Camp.Ct;
                    if (AnalysisCommand(command) == Command.SetCamp)
                    {
                        string[] splitedCmd = command.Split(' ');
                        currentCamp = splitedCmd[2] == "t" ? Camp.T : Camp.Ct;
                    }
                    else if (AnalysisCommand(command) == Command.CreateCharacter)
                    {
                        string[] splitedCmd = command.Split(' ');
                        bool isT = splitedCmd[2] == "t" ? true : false;
                        bool isFriendly = currentCamp.ToString().ToLower() == splitedCmd[2] ? true : false;
                        Point mapPoint = new Point(Double.Parse(splitedCmd[3].Split(',')[0]), Double.Parse(splitedCmd[3].Split(',')[1]));
                        Image characterImg = new Image();
                        if (isFriendly)
                        {
                            characterImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/FRIENDLY_ALIVE_UPPER.png")));
                        }
                        else
                        {
                            characterImg.Source = new BitmapImage(new Uri(System.IO.Path.Combine(GlobalDictionary.basePath, "img/ENEMY_ALIVE_UPPER.png")));
                        }
                        string name = "";
                        if (splitedCmd.Count() == 6 && Regex.IsMatch(splitedCmd[5], "^[^0-9](.*)$"))
                        {
                            name = splitedCmd[5];
                            characterNameDic.Add(name, characterNumber);
                        }
                        characterImg.Width = GlobalDictionary.CharacterWidthAndHeight;
                        characterImg.Height = GlobalDictionary.CharacterWidthAndHeight;
                        Point charactorWndPoint = mainWindow.GetWndPoint(mapPoint, ImgType.Character);
                        Canvas.SetLeft(characterImg, charactorWndPoint.X);
                        Canvas.SetTop(characterImg, charactorWndPoint.Y);
                        characterImg.Tag = "Number: " + characterNumber + "\n" + "Name: " + name + "\n" + "Posision: " + mapPoint.ToString();
                        characterImg.MouseEnter += mainWindow.ShowCharacterImgInfos;
                        objMouseMove = delegate (object sender, MouseEventArgs e)
                        {
                            Point mousePosition = e.GetPosition(mainWindow.i_map);
                            mainWindow.mouseMovePathInPreview.Add(new Point((mousePosition.X / GlobalDictionary.imageRatio), (mousePosition.Y / GlobalDictionary.imageRatio)));
                        };
                        objMouseMiddleDown = delegate (object sender, MouseButtonEventArgs e)
                        {
                            //if (e.MiddleButton == MouseButtonState.Pressed)
                            //{
                            //}
                            if (e.ChangedButton == MouseButton.Middle)
                            {
                                Point mousePosition = e.GetPosition(mainWindow.i_map);
                                mainWindow.keyDownInPreview.Add(new Point(Math.Round((mousePosition.X / GlobalDictionary.imageRatio), 2), Math.Round((mousePosition.Y / GlobalDictionary.imageRatio), 2)));
                            }
                        };
                        characterImg.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
                        {
                            e.Handled = true;
                            ((UIElement)e.Source).CaptureMouse();
                            mainWindow.mouseMovePathInPreview.Clear();
                            mainWindow.keyDownInPreview.Clear();
                            characterImg.MouseMove += objMouseMove;
                            mainWindow.MouseDown += objMouseMiddleDown;
                        };
                        characterImg.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
                        {
                            ((UIElement)e.Source).ReleaseMouseCapture();
                            characterImg.MouseMove -= objMouseMove;

                            if (mainWindow.mouseMovePathInPreview.Count > 0)
                            {
                                string tag = ((Image)sender).Tag.ToString();
                                int number = int.Parse(tag.Substring(tag.IndexOf("Number: ") + 8, tag.IndexOf("Name: ") - (tag.IndexOf("Number: ") + 9)));
                                mainWindow.CreateCommandInWindow(number, new Point(Math.Round((e.GetPosition(mainWindow.i_map).X / GlobalDictionary.imageRatio), 2), Math.Round((e.GetPosition(mainWindow.i_map).Y / GlobalDictionary.imageRatio), 2)));
                            }
                            mainWindow.MouseDown -= objMouseMiddleDown;
                        };
                        
                        characterImg.MouseRightButtonDown += delegate (object sender, MouseButtonEventArgs e)
                        {
                            string tag = ((Image)sender).Tag.ToString();
                            int number = int.Parse(tag.Substring(tag.IndexOf("Number: ") + 8, tag.IndexOf("Name: ") - (tag.IndexOf("Number: ") + 9)));
                            mainWindow.CreateCommandInWindow(number);
                        };
                        ++characterNumber;
                        previewElements.Add(characterImg);
                        charactorWndPoints.Add(previewCharactorCount++, mainWindow.GetWndPoint(mapPoint, ImgType.Nothing));
                    }
                    else if (AnalysisCommand(command) == Command.ActionCharacterAutoMove)
                    {
                        Map mapFrame = GlobalDictionary.mapDic[mainWindow.cb_select_mapframe.Text];
                        if (mapFrame == null)
                        {
                            return null;
                        }
                        string[] splitedCmd = command.Split(' ');
                        Point startMapPoint = new Point();
                        int startLayer;
                        Point endMapPoint = new Point();
                        int endLayer;
                        VolumeLimit volumeLimit;
                        if (!int.TryParse(splitedCmd[2], out characterNumber))
                        {
                            characterNumber = characterNameDic[splitedCmd[2]];
                        }
                        if (!command.Contains("from"))
                        {
                            startMapPoint = charactorWndPoints[characterNumber];
                            startLayer = int.Parse(splitedCmd[4]);
                            endMapPoint = new Point(double.Parse(splitedCmd[7].Split(',')[0]), double.Parse(splitedCmd[7].Split(',')[1]));
                            endLayer = int.Parse(splitedCmd[9]);
                            volumeLimit = splitedCmd[10] == VolumeLimit.Noisily.ToString().ToLower() ? VolumeLimit.Noisily : VolumeLimit.Quietly;
                        }
                        else
                        {
                            startLayer = int.Parse(splitedCmd[6]);
                            startMapPoint = PathfindingHelper.GetNearestNode(VectorHelper.Parse(splitedCmd[4]), startLayer, mapFrame).nodePoint;
                            endMapPoint = new Point(double.Parse(splitedCmd[9].Split(',')[0]), double.Parse(splitedCmd[9].Split(',')[1]));
                            endLayer = int.Parse(splitedCmd[11]);
                            volumeLimit = splitedCmd[12] == VolumeLimit.Noisily.ToString().ToLower() ? VolumeLimit.Noisily : VolumeLimit.Quietly;
                        }

                        // 寻找与起始点最近的同层的节点
                        MapNode startNode = PathfindingHelper.GetNearestNode(startMapPoint, startLayer, mapFrame);
                        string startCommand = "action character" + " " + characterNumber + " " + "move" + " " + (volumeLimit == VolumeLimit.Noisily ? "run" : "walk") + " " + startNode.nodePoint;
                        // 寻找与结束点最近的同层的节点
                        MapNode endNode = PathfindingHelper.GetNearestNode(endMapPoint, endLayer, mapFrame);

                        List<MapNode> mapPathNodes = PathfindingHelper.GetMapPathNodes(startNode, endNode, mapFrame, volumeLimit);

                        List<Point> wndPoints = new List<Point>();
                        wndPoints.Add(mainWindow.GetWndPoint(startMapPoint, ImgType.Nothing));
                        foreach (MapNode mapNode in mapPathNodes)
                        {
                            wndPoints.Add(mainWindow.GetWndPoint(mapNode.nodePoint, ImgType.Nothing));
                        }
                        wndPoints.Add(mainWindow.GetWndPoint(endMapPoint, ImgType.Nothing));

                        foreach (Point toWndPoint in wndPoints)
                        {
                            Line moveLine = new Line();
                            moveLine.Stroke = Brushes.White;
                            moveLine.StrokeThickness = 2;
                            moveLine.StrokeDashArray = new DoubleCollection() { 2, 3 };
                            moveLine.StrokeDashCap = PenLineCap.Triangle;
                            moveLine.StrokeEndLineCap = PenLineCap.Triangle;
                            moveLine.StrokeStartLineCap = PenLineCap.Square;
                            moveLine.X1 = charactorWndPoints[characterNumber].X;
                            moveLine.Y1 = charactorWndPoints[characterNumber].Y;
                            moveLine.X2 = toWndPoint.X;
                            moveLine.Y2 = toWndPoint.Y;
                            moveLine.MouseEnter += delegate
                            {
                                mainWindow.tb_infos.Text = command;
                            };
                            previewElements.Add(moveLine);

                            charactorWndPoints[characterNumber] = toWndPoint;
                        }
                    }
                    else if (AnalysisCommand(command) == Command.ActionCharacterMove)
                    {
                        string[] splitedCmd = command.Split(' ');
                        int number;
                        if(!int.TryParse(splitedCmd[2], out number))
                        {
                            number = characterNameDic[splitedCmd[2]];
                        }
                        List<Point> wndPoints = new List<Point>();
                        for (int i = 5; i < splitedCmd.Count(); ++i)
                        {
                            wndPoints.Add(mainWindow.GetWndPoint(VectorHelper.Parse(splitedCmd[i]), ImgType.Nothing));
                        }
                        foreach (Point toWndPoint in wndPoints)
                        {
                            Line moveLine = new Line();
                            moveLine.Stroke = Brushes.White;
                            moveLine.StrokeThickness = 2;
                            moveLine.StrokeDashArray = new DoubleCollection() { 2, 3 };
                            moveLine.StrokeDashCap = PenLineCap.Triangle;
                            moveLine.StrokeEndLineCap = PenLineCap.Triangle;
                            moveLine.StrokeStartLineCap = PenLineCap.Square;
                            moveLine.X1 = charactorWndPoints[number].X;
                            moveLine.Y1 = charactorWndPoints[number].Y;
                            moveLine.X2 = toWndPoint.X;
                            moveLine.Y2 = toWndPoint.Y;
                            moveLine.MouseEnter += delegate
                            {
                                mainWindow.tb_infos.Text = command;
                            };
                            previewElements.Add(moveLine);

                            charactorWndPoints[number] = toWndPoint;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GlobalDictionary.propertiesSetter, new List<object> { new ButtonSpacer(250), "确定" }, "解析命令 \"" + command + "\" 时出错\n错误信息: " + ex.Message, "错误", MessageBoxImage.Error);
                }
            }
            return previewElements;
        }

        static private void ClearEvent(FrameworkElement frameworkElement, string eventname)
        {
            if (frameworkElement == null) return;
            if (string.IsNullOrEmpty(eventname)) return;

            BindingFlags mPropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
            BindingFlags mFieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Static;
            Type controlType = frameworkElement.GetType();
            PropertyInfo propertyInfo = controlType.GetProperty("Events", mPropertyFlags);
            if (propertyInfo == null)
            {
                Type baseType = frameworkElement.GetType(); ;
                while ((baseType = baseType.BaseType) != typeof(object) && propertyInfo == null)
                {
                    propertyInfo = baseType.GetProperty("Events", mPropertyFlags);
                }
            }
            EventHandlerList eventHandlerList = (EventHandlerList)propertyInfo.GetValue(frameworkElement, null);
            FieldInfo fieldInfo = frameworkElement.GetType().GetField("Event_" + eventname, mFieldFlags);
            if (fieldInfo == null)
            {
                Type baseType = frameworkElement.GetType();
                while ((baseType = baseType.BaseType) != typeof(object) && fieldInfo == null)
                {
                    fieldInfo = baseType.GetField("Event_" + eventname, mFieldFlags);
                }
            }
            Delegate d = eventHandlerList[fieldInfo.GetValue(frameworkElement)];

            if (d == null) return;
            EventInfo eventInfo = controlType.GetEvent(eventname);

            foreach (Delegate dx in d.GetInvocationList())
                eventInfo.RemoveEventHandler(frameworkElement, dx);
        }
    }
}
