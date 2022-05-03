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
using SEAL_V2.model;
using System.Data.SQLite;
using System.Windows.Media.Animation;
using System.Threading;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for DatabaseStatus.xaml
    /// </summary>
    public partial class DatabaseStatus : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Status_Bar_Database";
        private Color currentColor;
        private Storyboard currentAnimation;
        private System.Data.ConnectionState currentDbState;
        public event EventHandler<StatusMessage> message;

        public DatabaseStatus()
        {
            InitializeComponent();
            setDefaultColor();

            //Needs to be updated tp pull from db location.... (Check if local or network!)
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
        }

        public long getObjectID()
        {
            return objectID;
        }

        public String getObjectName()
        {
            return objectName;
        }

        private void setDefaultColor()
        {
            Brush foregroundColor = (Brush)Application.Current.Resources["OnBackground"];
            statusIcon.Foreground = foregroundColor;
            currentColor = ((SolidColorBrush)foregroundColor).Color;
        }

        private void connected()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["Success"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["Success"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseCheck);
            statusInfoMain.Text = "Connected";
            statusInfoSub.Text = "Local";
        }

        private void disconnected()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ErrorStartColor"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ErrorEndColor"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseOff);
            statusInfoMain.Text = "Disconnected";
            statusInfoSub.Text = "N/A";
        }

        private void connecting()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ConnectingStartColor"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ConnectingEndColor"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseArrowUp);
            statusInfoMain.Text = "Connecting";
            statusInfoSub.Text = "N/A";
        }

        private void executingQuery()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ExecutingStartColor"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ExecutingEndColor"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseSearch);
            statusInfoMain.Text = "Querying";
            statusInfoSub.Text = "Local";
        }

        private void error()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ErrorStartColor"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ErrorEndColor"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseAlert);
            statusInfoMain.Text = "Error";
            statusInfoSub.Text = "Local";
        }

        private void fetching()
        {
            Color startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ExecutingStartColor"]).Color;
            Color endColor = ((SolidColorBrush)(Brush)Application.Current.Resources["ExecutingEndColor"]).Color;
            multiThreadStatusColorUpdate(startColor, endColor, PackIconKind.DatabaseExport);
            statusInfoMain.Text = "Fetching";
            statusInfoSub.Text = "Local";
        }

      //  public void recieveMessage(StatusMessage sentMessage)
       // {
            //System.Data.ConnectionState dbState = (System.Data.ConnectionState)sentMessage.readMessage();

            //if (dbState == System.Data.ConnectionState.Open && currentDbState != System.Data.ConnectionState.Open)
            //{
                //connected();
            //}
           //else if (dbState == System.Data.ConnectionState.Closed && currentDbState != System.Data.ConnectionState.Closed)
           // {
           //     disconnected();
          //  }
           // else if (dbState == System.Data.ConnectionState.Connecting && currentDbState != System.Data.ConnectionState.Connecting)
           // {
               // connecting();
            //}
            //else if (dbState == System.Data.ConnectionState.Executing && currentDbState != System.Data.ConnectionState.Executing)
           // {
           //     executingQuery();
          //  }
           // else if (dbState == System.Data.ConnectionState.Fetching && currentDbState != System.Data.ConnectionState.Fetching)
           // {
           //     fetching();
           // }
           // else if (dbState == System.Data.ConnectionState.Broken && currentDbState != System.Data.ConnectionState.Broken)
           // {
           //     error();
           // }

           // currentDbState = dbState;
       // }

        public StatusMessage createMessage(object message, String objectName)
        {
            StatusMessage newMessage = new StatusMessage(ObjectIDManager.objectIDs[objectName], message, this.objectID);

            return newMessage;
        }

        public void sendMessage(StatusMessage newMessage)
        {
            if (message != null)
            {
                this.message(this, newMessage);
            }
        }

        public void receiveMessage(object sender, StatusMessage recevievedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, recevievedMessage.getAddress()))
            {

            }
            else
            {

            }
        }

        private async void multiThreadStatusColorUpdate(Color startColor, Color endColor, PackIconKind icon)
        {
            await Task.Run(() => Dispatcher.Invoke(() => iconColorFade(currentColor, startColor, icon, false, false)));

            currentColor = endColor;

            await Task.Delay(1000);

            await Task.Run(() => Dispatcher.Invoke(() => iconColorFade(startColor, endColor, icon, true, true)));
        }

        private void iconColorFade(Color fromColor, Color toColor, PackIconKind icon, bool reverse, bool repeat)
        {
            statusIcon.Kind = icon;

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.From = fromColor;
            colorAnimation.To = toColor;
            colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            PropertyPath iconForegroundPath = new PropertyPath("(materialdesign:PackIcon.Foreground).(SolidColorBrush.Color)");
            currentAnimation = new Storyboard();
            Storyboard.SetTarget(colorAnimation, statusIcon);
            Storyboard.SetTargetProperty(colorAnimation, iconForegroundPath);
            currentAnimation.Children.Add(colorAnimation);
            if (reverse)
            {
                currentAnimation.AutoReverse = true;
            }

            if (repeat)
            {
                currentAnimation.RepeatBehavior = RepeatBehavior.Forever;
            }

            currentAnimation.Begin();
        }

        private void statusInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            statusPopup.IsOpen = true;
            Storyboard showGrid = this.TryFindResource("showPopup") as Storyboard;
            showGrid.Begin();
        }

        private void statusInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard showGrid = this.TryFindResource("showPopup") as Storyboard;
            showGrid.Begin();
            statusPopup.IsOpen = false;
        }
    }
}
