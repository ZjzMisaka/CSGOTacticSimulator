using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CSGOTacticSimulator
{
    /// <summary>
    /// Selector.xaml 的交互逻辑
    /// </summary>
    public partial class Selector : UserControl
    {
        public enum SelectType
        {
            Color,
            Size,
            Opacity
        }
        public Selector(SelectType selectType, MainWindow parentWindow)
        {
            InitializeComponent();

            if(selectType == SelectType.Color)
            {
                btn_0.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
                btn_0.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("green"));
                btn_1.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("green"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("blue"));
                btn_2.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("blue"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("yellow"));
                btn_3.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("yellow"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("brown"));
                btn_4.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("brown"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_5.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("purple"));
                btn_5.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("purple"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_6.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("black"));
                btn_6.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("black"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_7.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("white"));
                btn_7.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("white"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
            }
            else if(selectType == SelectType.Size)
            {
                btn_0.Content = "1";
                btn_0.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineSize = 1; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_1.Content = "2";
                btn_1.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.pathLineSize = 2; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_2.Content = "3";
                btn_2.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.pathLineSize = 3;
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_3.Content = "4";
                btn_3.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineSize = 4; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_4.Content = "5";
                btn_4.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineSize = 5;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_5.Content = "6";
                btn_5.Click += delegate (object sender, RoutedEventArgs e) 
                { 
                    Global.GlobalDictionary.pathLineSize = 6; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_6.Content = "7";
                btn_6.Click += delegate (object sender, RoutedEventArgs e) 
                { 
                    Global.GlobalDictionary.pathLineSize = 7;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_7.Content = "8";
                btn_7.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.pathLineSize = 8; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
            }
            else if (selectType == SelectType.Opacity)
            {
                btn_0.Content = "0.125";
                btn_0.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.125;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_1.Content = "0.25";
                btn_1.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.25;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_2.Content = "0.375";
                btn_2.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.375;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_3.Content = "0.5";
                btn_3.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.5;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_4.Content = "0.625";
                btn_4.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.625;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_5.Content = "0.75";
                btn_5.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.75;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_6.Content = "0.875";
                btn_6.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 0.875;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_7.Content = "1";
                btn_7.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.pathLineOpacity = 1;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
            }
        }
    }
}
