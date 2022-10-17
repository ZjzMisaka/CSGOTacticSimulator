using System;
using System.Collections.Generic;
using CSGOTacticSimulator.Model;
using CSGOTacticSimulator.Helper;
using System.Windows.Media;
using CustomizableMessageBox;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CSGOTacticSimulator.Global
{
    public enum Props
    {
        Bomb, 
        DefuseKit,
        Nothing
    }
    public enum Weapon
    {
        Glock18,
        P2000,
        Usps,
        P250,
        Tec9,
        Dualberettas,
        Fiveseven,
        Cz75auto,
        Deserteagle,
        Galilar,
        Famas,
        Ak47,
        Sg553,
        M4a1s,
        M4a4,
        Aug,
        Ssg08,
        Awp,
        G3sg1,
        Scar20,
        Mac10,
        Ump45,
        Mp9,
        Ppbizon,
        Mp7,
        P90,
        Nova,
        Sawedoff,
        Mag7,
        Xm1014,
        M249,
        Knife,
    }
    public enum Missile
    {
        Smoke,
        Grenade,
        Flashbang,
        Firebomb,
        Decoy,
        Nothing
    }
    public enum Camp
    {
        T,
        Ct
    }
    public enum Command
    {
        SetEntiretySpeed,
        SetCamp,
        CreateTeam,
        CreateCharacter,
        CreateComment,
        GiveCharacterWeapon,
        GiveCharacterMissile,
        GiveCharacterProps,
        SetCharacterStatus,
        SetCharacterVerticalPosition,
        ActionCharacterAutoMove,
        ActionCharacterMove,
        ActionCharacterThrow,
        ActionCharacterShoot,
        ActionCharacterDo,
        ActionCharacterWaitFor,
        ActionCharacterWaitUntil,
        BadOrNotCommand, 

        CreateMap, 
        CreateNode, 
        CreatePath, 
        DeleteNode, 
        DeletePath
    }
    public enum ImgType
    {
        Character,
        Missile,
        MissileEffect,
        ExplosionEffect,
        Props,
        Nothing
    }
    static public class GlobalDictionary
    {
        static public int charatersNumber = 0;
        static public int pathLineSize = 3;
        static public SolidColorBrush pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
        static public double pathLineOpacity = 0.9;

        static public string exePath = AppDomain.CurrentDomain.BaseDirectory;

        private static int characterWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Character"));
        private static int missileWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Missile"));
        private static int propsWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Props"));
        private static int missileEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "MissileEffect"));
        private static int explosionEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "ExplosionEffect"));

        static public int animationFreshTime = int.Parse(IniHelper.ReadIni("Setting", "AnimationFreshTime"));   // 刷新间隔
        static public int forwardTimeSpan = (int)(double.Parse(IniHelper.ReadIni("Setting", "ForwardTimeSpan")) * 1000);
        static public int backwardTimeSpan = (int)(double.Parse(IniHelper.ReadIni("Setting", "BackwardTimeSpan")) * 1000);

        static public double walkToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "WalkToRun"));
        static public double squatToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "SquatToRun"));
        static public double speedController = double.Parse(IniHelper.ReadIni("Ratio", "Entirety"));
        private static double imageRatio = 1;

        public static int CharacterWidthAndHeight { get => (int)(characterWidthAndHeight * ImageRatio); set => characterWidthAndHeight = value; }
        public static int MissileWidthAndHeight { get => (int)(missileWidthAndHeight * ImageRatio); set => missileWidthAndHeight = value; }
        public static int PropsWidthAndHeight { get => (int)(propsWidthAndHeight * ImageRatio); set => propsWidthAndHeight = value; }
        public static int MissileEffectWidthAndHeight { get => (int)(missileEffectWidthAndHeight * ImageRatio); set => missileEffectWidthAndHeight = value; }
        public static int ExplosionEffectWidthAndHeight { get => (int)(explosionEffectWidthAndHeight * ImageRatio); set => explosionEffectWidthAndHeight = value; }
        public static double ImageRatio
        {
            get => imageRatio;
            set
            {
                imageRatio = value == 0 ? 0.1 : value;
            }
        }

        static public string mapListPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "MapList")).Replace("/", "\\");
        static public string codeHighlightPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "CodeHighlight")).Replace("/", "\\");
        static public string logHighlightPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "LogHighlight")).Replace("/", "\\");
        static public string mapFolderPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "MapFolder")).Replace("/", "\\");
        static public string backgroundPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Background")).Replace("/", "\\");
        static public string previewPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Preview")).Replace("/", "\\");
        static public string runPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Run")).Replace("/", "\\");
        static public string pausePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Pause")).Replace("/", "\\");
        static public string resumePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Resume")).Replace("/", "\\");
        static public string stopPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Stop")).Replace("/", "\\");
        static public string savePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Save")).Replace("/", "\\");
        static public string exitPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Exit")).Replace("/", "\\");
        static public string infoPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Info")).Replace("/", "\\");
        static public string warningPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Warning")).Replace("/", "\\");
        static public string questionPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Question")).Replace("/", "\\");
        static public string errorPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Error")).Replace("/", "\\");
        static public string minimizePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Minimize")).Replace("/", "\\");
        static public string restorePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Restore")).Replace("/", "\\");
        static public string forwardPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Forward")).Replace("/", "\\");
        static public string backwardPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Backward")).Replace("/", "\\");
        static public string autoPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Auto")).Replace("/", "\\");
        static public string eyePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Eye")).Replace("/", "\\");
        static public string bombPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Bomb")).Replace("/", "\\");
        static public string defuseKitPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "DefuseKit")).Replace("/", "\\");
        static public string explosionPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Explosion")).Replace("/", "\\");
        static public string decoyPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Decoy")).Replace("/", "\\");
        static public string molotovPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Molotov")).Replace("/", "\\");
        static public string incgrenadePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Incgrenade")).Replace("/", "\\");
        static public string fireEffectPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "FireEffect")).Replace("/", "\\");
        static public string flashbangPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Flashbang")).Replace("/", "\\");
        static public string flashEffectPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "FlashEffect")).Replace("/", "\\");
        static public string hegrenadePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Hegrenade")).Replace("/", "\\");
        static public string heEffectPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "HeEffect")).Replace("/", "\\");
        static public string smokePath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "Smoke")).Replace("/", "\\");
        static public string smokeEffectPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "SmokeEffect")).Replace("/", "\\");
        static public string friendlyAliveUpperPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "FriendlyAliveUpper")).Replace("/", "\\");
        static public string friendlyAliveLowerPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "FriendlyAliveLower")).Replace("/", "\\");
        static public string enemyAliveUpperPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "EnemyAliveUpper")).Replace("/", "\\");
        static public string enemyAliveLowerPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "EnemyAliveLower")).Replace("/", "\\");
        static public string deadUpperPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "DeadUpper")).Replace("/", "\\");
        static public string deadLowerPath = System.IO.Path.Combine(Global.GlobalDictionary.exePath, IniHelper.ReadIni("Path", "DeadLower")).Replace("/", "\\");

        static public ImageBrush backgroundBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.backgroundPath)), Stretch = Stretch.Fill };
        static public ImageBrush minimizeBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.minimizePath)), Stretch = Stretch.Uniform };
        static public ImageBrush restoreBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.restorePath)), Stretch = Stretch.Uniform };
        static public ImageBrush forwardBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.forwardPath)), Stretch = Stretch.Uniform };
        static public ImageBrush backwardBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.backwardPath)), Stretch = Stretch.Uniform };
        static public ImageBrush autoBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.autoPath)), Stretch = Stretch.Uniform };
        static public ImageBrush previewBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.previewPath)), Stretch = Stretch.Uniform };
        static public ImageBrush runBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.runPath)), Stretch = Stretch.Uniform };
        static public ImageBrush pauseBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.pausePath)), Stretch = Stretch.Uniform };
        static public ImageBrush resumeBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.resumePath)), Stretch = Stretch.Uniform };
        static public ImageBrush stopBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.stopPath)), Stretch = Stretch.Uniform };
        static public ImageBrush saveBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.savePath)), Stretch = Stretch.Uniform };
        static public ImageBrush exitBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(GlobalDictionary.exitPath)), Stretch = Stretch.Uniform };

        static public int heLifespan = int.Parse(IniHelper.ReadIni("Missile", "HeLifespan"));
        static public int firebombLifespan = int.Parse(IniHelper.ReadIni("Missile", "FirebombLifespan"));
        static public int smokeLifespan = int.Parse(IniHelper.ReadIni("Missile", "SmokeLifespan"));
        static public int flashbangLifespan = int.Parse(IniHelper.ReadIni("Missile", "FlashbangLifespan"));
        static public int decoyLifespan = int.Parse(IniHelper.ReadIni("Missile", "DecoyLifespan"));

        static public string[] mapCalibrationDatas = IniHelper.ReadIni("MapCalibration", "Data").Split('|');

        static public Dictionary<string, Map> mapDic = new Dictionary<string, Map>();

        static public PropertiesSetter propertiesSetter = new PropertiesSetter()
        {
            ButtonFontSize = 20,
            MessageFontSize = 20,
            TitleFontSize = 20,
            WindowWidth = 500,
            WindowMinHeight = 250,
            ButtonPanelColor = new MessageBoxColor("#F03A3A3A"),
            MessagePanelColor = new MessageBoxColor("#F03A3A3A"),
            TitlePanelColor = new MessageBoxColor("#F03A3A3A"),
            TitlePanelBorderThickness = new Thickness(0, 0, 0, 2),
            TitlePanelBorderColor = new MessageBoxColor("#FFEFE2E2"),
            MessagePanelBorderThickness = new Thickness(0),
            ButtonPanelBorderThickness = new Thickness(0, 1, 0, 0),
            WndBorderThickness = new Thickness(1),
            TitleFontColor = new MessageBoxColor("#FFCBBEBE"),
            MessageFontColor = new MessageBoxColor(Colors.White),
            CloseIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(GlobalDictionary.exitPath)),
            InfoIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(GlobalDictionary.infoPath)),
            WarningIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(GlobalDictionary.warningPath)),
            QuestionIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(GlobalDictionary.questionPath)),
            ErrorIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(GlobalDictionary.errorPath)),
            ButtonStyleList = new List<Style> { CustomizableMessageBox.Prefab.GetButtonStyle(ButtonStyleName.White) },
        };
    }
}
