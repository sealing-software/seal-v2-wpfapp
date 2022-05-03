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
using SEAL_V2.model;
using MaterialDesignThemes.Wpf;
using SEAL_V2.db;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for CaptureItem.xaml
    /// </summary>
    public partial class CaptureItem : UserControl
    {
        private Capture passedCapture;
        public int captureID;
        private Brush currentColor;
        private bool selected;
        private DatabaseInterface db = DatabaseInterface.Instance;

        public CaptureItem(Capture passedCapture)
        {
            InitializeComponent();

            this.passedCapture = passedCapture;
            setup();
        }

        private void setup()
        {
            captureName.Text = passedCapture.name;
            captureID = passedCapture.id;
            statusNumberText.Text = (passedCapture.currentStep + 1).ToString() + "/" + (db.getSequence(passedCapture.sequenceid).sequenceLength); 
            showStatus();
        }

        private void showStatus()
        {
            switch(passedCapture.status)
            {
                case 0:
                    statusText.Text = "Not Started";
                    statusicon.Kind = PackIconKind.NewReleases;
                    currentColor = ((Brush)Application.Current.Resources["OnBackground"]);
                    break;
                case 1:
                    statusText.Text = "In Progress";
                    statusicon.Kind = PackIconKind.ProgressClock;
                    currentColor = ((Brush)Application.Current.Resources["InProgress"]);
                    break;
                case 2:
                    statusText.Text = "Reverted";
                    statusicon.Kind = PackIconKind.Error;
                    currentColor = ((Brush)Application.Current.Resources["RemovedItem"]);
                    break;
                case 3:
                    statusText.Text = "Complete";
                    statusicon.Kind = PackIconKind.Check;
                    currentColor = ((Brush)Application.Current.Resources["NewItem"]);
                    break;
            }

            captureName.Foreground = currentColor;
            statusNumberText.Foreground = currentColor;
            statusicon.Foreground = currentColor;
            statusText.Foreground = currentColor;
        }

        private void mainGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!selected)
            {
                rectangleSelect.Fill = currentColor;
            }

        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!selected)
            {
                rectangleSelect.Fill = ((Brush)Application.Current.Resources["12dp"]);
            }

        }

        private void mainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            setSelected();
        }

        public void setSelected()
        {
            selected = true;
            rectangleSelect.Fill = currentColor;
        }
    }
}
