using CSGOTacticSimulator.Global;
using CSGOTacticSimulator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSGOTacticSimulator.Helper
{
    static public class CommandHelper
    {
        static public int previewCharactorCount = 0;
        static public List<string> commands = new List<string>();
        static public List<string> GetCommands(string commandsText)
        {
            commands = new List<string>(commandsText.Split('\n'));
            for (int i = 0; i < commands.Count; ++i)
            {
                commands[i].Trim();
            }
            return commands;
        }

        static public Command AnalysisCommand(string cmd)
        {
            if(cmd.Length > 0 && cmd[0] == '-')
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
                if (cmd.Contains("move"))
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
            else
            {
                return Command.BadOrNotCommand;
            }
        }

        static public List<FrameworkElement> GetPreviewElements(string commandText, MainWindow mainWindow)
        {
            List<FrameworkElement> previewElements = new List<FrameworkElement>();

            Dictionary<int, Point> charactorWndPoints = new Dictionary<int, Point>();

            List<string> commands = GetCommands(commandText);
            foreach(string command in commands)
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
                    characterImg.Width = GlobalDictionary.characterWidthAndHeight;
                    characterImg.Height = GlobalDictionary.characterWidthAndHeight;
                    Point charactorWndPoint = mainWindow.GetWndPoint(mapPoint, ImgType.Character);
                    Canvas.SetLeft(characterImg, charactorWndPoint.X);
                    Canvas.SetTop(characterImg, charactorWndPoint.Y);
                    previewElements.Add(characterImg);
                    charactorWndPoints.Add(previewCharactorCount++, mainWindow.GetWndPoint(mapPoint, ImgType.Nothing));
                }
                else if (AnalysisCommand(command) == Command.ActionCharacterMove)
                {
                    string[] splitedCmd = command.Split(' ');
                    int number = int.Parse(splitedCmd[2]);
                    List<Point> wndPoints = new List<Point>();
                    for (int i = 5; i < splitedCmd.Count(); ++i)
                    {
                        wndPoints.Add(mainWindow.GetWndPoint(VectorHelper.Cast(splitedCmd[i]), ImgType.Nothing));
                    }
                    foreach(Point toWndPoint in wndPoints)
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
                        moveLine.MouseEnter += delegate {
                            mainWindow.tb_infos.Text = command;
                        };
                        previewElements.Add(moveLine);

                        charactorWndPoints[number] = toWndPoint;
                    }
                }
            }

            return previewElements;
        }
    }
}
