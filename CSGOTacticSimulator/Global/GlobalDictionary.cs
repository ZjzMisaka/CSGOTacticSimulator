using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSGOTacticSimulator.Model;
using CSGOTacticSimulator.Helper;
using System.Threading.Tasks;

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
    public enum Grenade
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
        SetCamp,
        CreateTeam,
        CreateCharacter,
        CreateComment,
        GiveCharacterWeapon,
        GiveCharacterGrenade,
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
        Grenade,
        GrenadeEffect,
        ExplosionEffect,
        Props,
        Nothing
    }
    static public class GlobalDictionary
    {
        static public int charatersNumber = 0;

        static public string exePath = AppDomain.CurrentDomain.BaseDirectory;
        static public string basePath = exePath.Substring(0, exePath.LastIndexOf("bin"));

        static public int characterWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Character"));
        static public int grenadeWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Grenade"));
        static public int propsWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "Props"));
        static public int grenadeEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "GrenadeEffect"));
        static public int explosionEffectWidthAndHeight = int.Parse(IniHelper.ReadIni("View", "ExplosionEffect"));
        static public int animationFreshTime = int.Parse(IniHelper.ReadIni("Setting", "AnimationFreshTime"));
        static public double walkToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "WalkToRun"));
        static public double squatToRunRatio = double.Parse(IniHelper.ReadIni("Ratio", "SquatToRun"));
        static public double speedController = double.Parse(IniHelper.ReadIni("Ratio", "Entirety"));

        static public string mapListPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "MapList"));
        static public string mapFolderPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "MapFolder"));
        static public string backgroundPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Background"));
        static public string runPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Run"));
        static public string stopPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Stop"));
        static public string savePath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Save"));
        static public string exitPath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Exit"));
        static public string minimizePath = System.IO.Path.Combine(Global.GlobalDictionary.basePath, IniHelper.ReadIni("Path", "Minimize"));

        static public int firebombLifespan = int.Parse(IniHelper.ReadIni("Grenade", "FirebombLifespan"));
        static public int smokeLifespan = int.Parse(IniHelper.ReadIni("Grenade", "SmokeLifespan"));
        static public int flashbangLifespan = int.Parse(IniHelper.ReadIni("Grenade", "FlashbangLifespan"));
    }
}
