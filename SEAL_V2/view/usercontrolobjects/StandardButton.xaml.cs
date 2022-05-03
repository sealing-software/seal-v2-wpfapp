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
using MaterialDesignThemes.Wpf;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for StandardButton.xaml
    /// </summary>
    public partial class StandardButton : UserControl
    {
        private String objectName;
        private String buttonName;
        private PackIconKind iconStandard;
        private PackIconKind iconSelected;
        private Brush iconColor;
        private String textForButton;
        public EventHandler clicked;

        public StandardButton(String objectName, String buttonName, PackIconKind iconStandard, PackIconKind iconSelected, Brush iconColor, String Text)
        {
            InitializeComponent();

            this.objectName = objectName;
            this.buttonName = buttonName;
            this.iconStandard = iconStandard;
            this.iconSelected = iconSelected;
            this.iconColor = iconColor;
            this.textForButton = Text;

            setupButton();
        }

        private void setupButton()
        {
            buttonIcon.Kind = iconStandard;
            buttonIcon.Foreground = iconColor;
            buttonText.Text = textForButton;
        }

        private void MainGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            MainGrid.Opacity = 0.5;
            buttonIcon.Kind = iconSelected;
        }

        private void MainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            MainGrid.Opacity = 1;
            buttonIcon.Kind = iconStandard;
        }

        private void MainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (clicked != null)
            {
                clicked(this, e);
            }
        }
    }
}
