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
using System.Windows.Media.Animation;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for ToggleItem.xaml
    /// </summary>
    public partial class ToggleItem : UserControl
    {
        public bool isOn { get; set; }
        public String toggleText { get; set; }

        public ToggleItem(String toggleText, bool toggleOn)
        {
            InitializeComponent();

            isOn = toggleOn;

            this.toggleText = toggleText;

            loadInitialToggle();
        }

        private void loadInitialToggle()
        {
            if (isOn)
            {
                FrontCircle.Margin = new Thickness(30, 0, 0, 0);
                BackgroundRectangle.Fill = (Brush)Application.Current.Resources["ToggleButtonBackOn"];
            }

            ToggleText.Text = toggleText;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            toggle();
        }

        public void toggle()
        {
            if (true)
            {
                Thickness marginThickness = FrontCircle.Margin;

                if (marginThickness.Right == 30)
                {
                    isOn = true;
                    //FrontCircle.Margin = new Thickness(30, 0, 0, 0);
                    BackgroundRectangle.Fill = (Brush)Application.Current.Resources["ToggleButtonBackOn"];
                    Storyboard switchToggleOn = this.TryFindResource("ToggleOnAnimation") as Storyboard;
                    switchToggleOn.Begin();
                }
                else if (marginThickness.Left == 30)
                {
                    isOn = false;
                    //FrontCircle.Margin = new Thickness(0, 0, 30, 0);
                    BackgroundRectangle.Fill = (Brush)Application.Current.Resources["ToggleButtonBackOff"];
                    Storyboard switchToggleOff = this.TryFindResource("ToggleOffAnimation") as Storyboard;
                    switchToggleOff.Begin();
                }
            }
            else
            {
                Thickness marginThickness = FrontCircle.Margin;

                if (marginThickness.Right == 30)
                {
                    FrontCircle.Margin = new Thickness(30, 0, 0, 0);
                    BackgroundRectangle.Fill = (Brush)Application.Current.Resources["ToggleButtonBackOn"];
                }
                else if (marginThickness.Left == 30)
                {
                    FrontCircle.Margin = new Thickness(0, 0, 30, 0);
                    BackgroundRectangle.Fill = (Brush)Application.Current.Resources["ToggleButtonBackOff"];
                }
            }
        }
    }
}
