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

    public class Character
    {
        public Weapon Weapon { get => weapon; set => weapon = value; }
        private Weapon weapon = Weapon.Knife;
        public List<Missile> Missiles { get => grenades; set => grenades = value; }
        private List<Missile> grenades = new List<Missile>();
        public Props Props { get => props; set => props = value; }
        private Props props = Props.Nothing;
        public bool IsFriendly { get => isFriendly; set => isFriendly = value; }
        private bool isFriendly = true;
        public bool IsT { get => isT; set => isT = value; }
        private bool isT = true;
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
        public Image CharacterImg { get => characterImg; set => characterImg = value; }
        private Image characterImg = new Image();

        public Character(bool isFriendly, bool isT, Point mapPoint, MainWindow wnd)
        {
            CharacterImg.MouseEnter += wnd.ShowCharacterInfos;

            CharacterImg.Width = GlobalDictionary.characterWidthAndHeight;
            CharacterImg.Height = GlobalDictionary.characterWidthAndHeight;

            this.IsFriendly = isFriendly;
            this.IsT = isT;

            if (isFriendly)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/FRIENDLY_ALIVE_UPPER.png")));
            }
            else
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/ENEMY_ALIVE_UPPER.png")));
            }

            this.MapPoint = mapPoint;

            wnd.NewCharacter(this, mapPoint);
        }

        private void CheckAndSetCharacterImg()
        {
            if (status  == Status.Alive && IsFriendly == true && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/FRIENDLY_ALIVE_UPPER.png")));
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Alive && IsFriendly == false && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/ENEMY_ALIVE_UPPER.png"))); 
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Alive && IsFriendly == true && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/FRIENDLY_ALIVE_LOWER.png")));
                CharacterImg.Opacity = 0.4;
            }
            else if (status == Status.Alive && IsFriendly == false && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/ENEMY_ALIVE_LOWER.png")));
                CharacterImg.Opacity = 0.4;
            }
            else if (status == Status.Dead && verticalPosition == VerticalPosition.Upper)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/DEAD_UPPER.png")));
                CharacterImg.Opacity = 1;
            }
            else if (status == Status.Dead && verticalPosition == VerticalPosition.Lower)
            {
                CharacterImg.Source = new BitmapImage(new Uri(Path.Combine(GlobalDictionary.basePath, "img/DEAD_LOWER.png")));
                CharacterImg.Opacity = 0.85;
            }
        }
    }
}
