using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSGOTacticSimulator.Model;
using CSGOTacticSimulator.Helper;
using System.Threading.Tasks;
using System.Windows.Media;

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
        ActionCharacterMove,
        ActionCharacterThrow,
        ActionCharacterShoot,
        ActionCharacterDo,
        ActionCharacterWaitFor,
        ActionCharacterWaitUntil,
        BadOrNotCommand
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
        static public string basePath = exePath.Substring(0, exePath.LastIndexOf("bin"));

        static public int characterWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Character"));
        static public int missileWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Missile"));
        static public int propsWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Props"));
        static public int missileEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "MissileEffect"));
        static public int explosionEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "ExplosionEffect"));
        static public int animationFreshTime = int.Parse(IniHelper.ReadIni("Setting", "AnimationFreshTime"));
        static public double walkToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "WalkToRun"));
        static public double squatToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "SquatToRun"));
        static public double speedController = double.Parse(IniHelper.ReadIni("Ratio", "Entirety"));

        static public string mapListPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "MapList")).Replace("/", "\\");
        static public string highlightPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Highlight")).Replace("/", "\\");
        static public string mapFolderPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "MapFolder")).Replace("/", "\\");
        static public string backgroundPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Background")).Replace("/", "\\");
        static public string runPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Run")).Replace("/", "\\");
        static public string stopPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Stop")).Replace("/", "\\");
        static public string savePath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Save")).Replace("/", "\\");
        static public string exitPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Exit")).Replace("/", "\\");
        static public string minimizePath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Minimize")).Replace("/", "\\");

        static public int firebombLifespan = int.Parse(IniHelper.ReadIni("Missile", "FirebombLifespan"));
        static public int smokeLifespan = int.Parse(IniHelper.ReadIni("Missile", "SmokeLifespan"));
        static public int flashbangLifespan = int.Parse(IniHelper.ReadIni("Missile", "FlashbangLifespan"));
    }
}
