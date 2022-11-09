using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CSGOTacticSimulator.Model;
using CSGOTacticSimulator.Helper;
using CSGOTacticSimulator.Global;
using System.Windows.Media.Imaging;
using DemoInfo;
using CSGOTacticSimulator.Controls;

namespace CSGOTacticSimulator.Model
{
    public enum Status
    {
        Alive,
        Dead
    }
    public enum VerticalPosition
    {
        Upper,
        Lower
    }
    public enum DoWithProps
    {
        Plant, 
        Defuse
    }
    public class AdditionalPlayerInformation
    {
        public int Kills { get; internal set; }
        public int Deaths { get; internal set; }
        public int Assists { get; internal set; }
        public int Score { get; internal set; }
        public int MVPs { get; internal set; }
        public int Ping { get; internal set; }
        public string Clantag { get; internal set; }
        public int TotalCashSpent { get; internal set; }
    }

    public class LastAliveInfo
    {
        public int Money { get => money; set => money = value; }
        private int money = 0;
        public List<Equipment> WeaponEquipmentList { get => weaponEquipmentList; set => weaponEquipmentList = value; }
        private List<Equipment> weaponEquipmentList = new List<Equipment>();
        public List<Equipment> MissileEquipList { get => missileEquipList; set => missileEquipList = value; }
        private List<Equipment> missileEquipList = new List<Equipment>();
        public List<Equipment> EquipList { get => equipList; set => equipList = value; }
        private List<Equipment> equipList = new List<Equipment>();
    }

    public class Character
    {
        public string Name { get => name; set => name = value; }
        private string name = "";
        public long SteamId { get => steamId; set => steamId = value; }
        private long steamId = 0;
        public int Hp { get => hp; set => hp = value; }
        private int hp = 0;
        public int Money { get => money; set => money = value; }
        private int money = 0;
        public Weapon Weapon { get => weapon; set => weapon = value; }
        private Weapon weapon = Weapon.Knife;
        public List<Equipment> WeaponEquipmentList { get => weaponEquipmentList; set => weaponEquipmentList = value; }
        private List<Equipment> weaponEquipmentList = new List<Equipment>();
        public List<Equipment> MissileEquipList { get => missileEquipList; set => missileEquipList = value; }
        private List<Equipment> missileEquipList = new List<Equipment>();
        public List<Equipment> EquipList { get => equipList; set => equipList = value; }
        private List<Equipment> equipList = new List<Equipment>();
        public List<Missile> Missiles { get => grenades; set => grenades = value; }
        private List<Missile> grenades = new List<Missile>();
        public Props Props { get => props; set => props = value; }
        private Props props = Props.Nothing;
        public bool IsFriendly { get => isFriendly; set => isFriendly = value; }
        private bool isFriendly = true;
        public bool IsT { get => isT; set => isT = value; }
        private bool isT = true;
        public bool IsAlive { get => isAlive; set => isAlive = value; }
        private bool isAlive = true;
        public Status Status
        {
            get => status;
            set 
            {
                status = value;
                CheckAndSetCharacterImg();
            }
        }
        private Status status = Status.Alive;
        public VerticalPosition VerticalPosition
        {
            get => verticalPosition;
            set
            {
                verticalPosition = value;
                CheckAndSetCharacterImg();
            }
        }
        private VerticalPosition verticalPosition = VerticalPosition.Upper;
        public bool IsUpper { get => isUpper; set => isUpper = value; }
        private bool isUpper = true;
        public bool IsRunningAnimation { get => isRunningAnimation; set => isRunningAnimation = value; }
        private bool isRunningAnimation = false;
        public Point MapPoint { get => mapPoint; set => mapPoint = value; }
        private Point mapPoint;
        public int Number { get => number; set => number = value; }
        private int number = Global.GlobalDictionary.charatersNumber++;
        public TSImage CharacterImg { get => characterImg; set => characterImg = value; }
        private TSImage characterImg = new TSImage();
        public TSImage ActiveWeaponImg { get => activeWeaponImg; set => activeWeaponImg = value; }
        private TSImage activeWeaponImg = new TSImage();
        public TSImage OtherImg { get => otherImg; set => otherImg = value; }
        private TSImage otherImg = new TSImage();
        public TSImage StatusImg { get => statusImg; set => statusImg = value; }
        private TSImage statusImg = new TSImage();
        public TSImage Avatar { get => avatar; set => avatar = value; }
        private TSImage avatar = new TSImage();
        public Label CharacterLabel { get => characterLabel; set => characterLabel = value; }
        private Label characterLabel = new Label();
        public Label NumberLabel { get => numberLabel; set => numberLabel = value; }
        private Label numberLabel = new Label();
        public Label AmmoInMagazine { get => ammoInMagazine; set => ammoInMagazine = value; }
        private Label ammoInMagazine = new Label();
        public AdditionalPlayerInformation AdditionalPlayerInformation { get => additionalPlayerInformation; set => additionalPlayerInformation = value; }
        private AdditionalPlayerInformation additionalPlayerInformation = new AdditionalPlayerInformation();
        public LastAliveInfo LastAliveInfo { get => lastAliveInfo; set => lastAliveInfo = value; }
        private LastAliveInfo lastAliveInfo = new LastAliveInfo();

        public Character(string name, long steamId, bool isFriendly, bool isT, Point mapPoint, MainWindow wnd)
        {
            CharacterImg.MouseEnter += wnd.ShowCharacterInfos;
            CharacterImg.MouseLeftButtonDown += wnd.ShowPov;

            CharacterImg.Width = GlobalDictionary.CharacterWidthAndHeight;
            CharacterImg.Height = GlobalDictionary.CharacterWidthAndHeight;

            OtherImg.Visibility = Visibility.Collapsed;
            StatusImg.Visibility = Visibility.Collapsed;

            if (name != "")
            {
                this.Name = name;
                CharacterHelper.AddIntoNameDic(name, CharacterHelper.GetCharacters().Count);
            }

            this.steamId = steamId;

            this.IsFriendly = isFriendly;
            this.IsT = isT;

            if (isFriendly)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyAliveUpperPath));
            }
            else
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.enemyAliveUpperPath));
            }

            this.MapPoint = mapPoint;
            
            CharacterHelper.AddCharacter(this);
            wnd.NewCharacter(this, mapPoint);
        }

        private void CheckAndSetCharacterImg()
        {
            if (status  == Status.Alive && IsFriendly == true && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyAliveUpperPath));
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Alive && IsFriendly == false && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.enemyAliveUpperPath)); 
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Alive && IsFriendly == true && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyAliveLowerPath));
                CharacterImg.Opacity = 0.4;
            }
            else if (status == Status.Alive && IsFriendly == false && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.enemyAliveLowerPath));
                CharacterImg.Opacity = 0.4;
            }
            else if (status == Status.Dead && IsFriendly == true && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyDeadUpperPath));
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Dead && IsFriendly == true && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.friendlyDeadLowerPath));
                CharacterImg.Opacity = 0.85;
            }
            else if (status == Status.Dead && IsFriendly == false && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.enemyDeadUpperPath));
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Dead && IsFriendly == false && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(GlobalDictionary.enemyDeadLowerPath));
                CharacterImg.Opacity = 0.85;
            }
        }
    }
}
