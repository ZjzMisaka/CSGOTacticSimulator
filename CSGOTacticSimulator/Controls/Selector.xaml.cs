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
            Size
        }
        public Selector(SelectType selectType, MainWindow parentWindow)
        {
            InitializeComponent();

            if(selectType == SelectType.Color)
            {
                btn_0.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
                btn_0.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("green"));
                btn_1.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("green"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("blue"));
                btn_2.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("blue"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("yellow"));
                btn_3.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("yellow"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("brown"));
                btn_4.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("brown"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_5.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("purple"));
                btn_5.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("purple"));
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_6.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("black"));
                btn_6.Click += delegate (object sender, RoutedEventArgs e)
                {
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("black"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_7.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("white"));
                btn_7.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.color = (SolidColorBrush)(new BrushConverter().ConvertFrom("white"));
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
            }
            else if(selectType == SelectType.Size)
            {
                btn_0.Content = "1";
                btn_0.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.size = 1; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_1.Content = "2";
                btn_1.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.size = 2; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_2.Content = "3";
                btn_2.Click += delegate (object sender, RoutedEventArgs e) 
                {
                    Global.GlobalDictionary.size = 3;
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_3.Content = "4";
                btn_3.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.size = 4; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_4.Content = "5";
                btn_4.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.size = 5;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_5.Content = "6";
                btn_5.Click += delegate (object sender, RoutedEventArgs e) 
                { 
                    Global.GlobalDictionary.size = 6; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
                btn_6.Content = "7";
                btn_6.Click += delegate (object sender, RoutedEventArgs e) 
                { 
                    Global.GlobalDictionary.size = 7;
                    parentWindow.c_paintcanvas.Children.Remove(this);
                };
                btn_7.Content = "8";
                btn_7.Click += delegate (object sender, RoutedEventArgs e)
                { 
                    Global.GlobalDictionary.size = 8; 
                    parentWindow.c_paintcanvas.Children.Remove(this); 
                };
            }
        }
    }
}
