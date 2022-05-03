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
using MaterialDesignThemes.Wpf;
using SEAL_V2.model;
using SEAL_V2.db;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for UserAccountButton.xaml
    /// </summary>
    public partial class UserAccountButton : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Title_Bar_Account";
        private String accountName;
        private bool accountNotifShowing;
        private Storyboard currentAnimation;
        private Color currentColor;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;

        public UserAccountButton()
        {
            InitializeComponent();

            //Check if not account is logged in...
            //noAccountLoggedIn();
            loadObjectID();

            defaultSetup();
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

        //Default setup runs at first boot
        private void defaultSetup()
        {
            accountName = "No Account";

            Brush foregroundColor = (Brush)Application.Current.Resources["OnBackground"];
            AccountIcon.Foreground = foregroundColor;

            //No account logged in until user logs in
            accountNotification(accountName, PackIconKind.AccountCancel);
        }

        private void AccountPopUpBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //AccountPopUpBox.IsPopupOpen = false;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //Method to provide account notifcation and update account icon, slides in from right then reverses
        public void accountNotification(String message, PackIconKind passedIcon)
        {
            AccountNotifications.Text = message;

            Storyboard accountNotificationFade = this.TryFindResource("FadeAccountNotification") as Storyboard;
            accountNotificationFade.Begin();

            AccountIcon.Kind = passedIcon;

        }

        public void accountLoginUpdate(String message, PackIconKind passedIcon)
        {

        }

        public void accountNotificationFastMessageShow(String message)
        {
            if (!accountNotifShowing)
            {
                accountNotifShowing = true;

                AccountNotifications.Text = message;

                Storyboard accountNotificationFade = this.TryFindResource("FadeInAccountNotificationFast") as Storyboard;
                accountNotificationFade.Begin();
            }
        }

        public void accountNotificationFastMessageHide()
        {
            accountNotifShowing = false;

            Storyboard accountNotificationFade = this.TryFindResource("FadeOutAccountNotificationFast") as Storyboard;
            accountNotificationFade.Begin();
        }

        private void PopupBoxIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!accountPopup.IsOpen)
            {
                accountPopup.IsOpen = true;
                popupGrid.Focus();
                Storyboard fadeInPopup = this.TryFindResource("ShowPopup") as Storyboard;
                fadeInPopup.Begin();
            }
        }

        private void PopupBoxIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!accountPopup.IsOpen)
            {
                accountNotificationFastMessageHide();
            }
            AccountIcon.Opacity = 1;
        }

        private void AccountPopUpBox_MouseEnter(object sender, MouseEventArgs e)
        {
            accountNotificationFastMessageShow(accountName);
            AccountIcon.Opacity = 0.5;
        }

        private void loginRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            loginRectangle.Opacity = 0.3;
            AccountRectangle.Opacity = 0.3;
        }

        private void loginRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            loginRectangle.Opacity = 0;
            AccountRectangle.Opacity = 0;
        }

        private void loginRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            accountPopup.IsOpen = false;

            sendMessage(createMessage("LOGIN", "Main_Window"));
        }

        private void popupGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            accountPopup.IsOpen = false;

            accountNotificationFastMessageHide();
        }

        private void accountPopup_Opened(object sender, EventArgs e)
        {
            loginText.Focus();
        }

        private void accountPopup_Closed(object sender, EventArgs e)
        {
            accountNotificationFastMessageHide();
        }

        private void accountLoginSuccesfull()
        {
            accountName = User.getDisplayName();

            accountNotification(accountName, PackIconKind.Account);

            multiThreadStatusColorUpdate((Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID())));

            populateAccountPopup();
        }

        private void populateAccountPopup()
        {
            popupBox.Height = 120;

            NoAccountPopup.Visibility = Visibility.Collapsed;
            AcountPopup.Visibility = Visibility.Visible;
        }
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

        public void receiveMessage(object sender, StatusMessage receivedMessage)
        {
            if (this.objectID == receivedMessage.getAddress())
            {
                if (receivedMessage.readMessage().Equals("SUCCESS"))
                {
                    accountLoginSuccesfull();
                }
                else if (receivedMessage.readMessage().Equals("GROUP_UPDATE"))
                {
                    multiThreadStatusColorUpdate((Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID())));
                }
            }
            else if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {

            }
            else
            {
                //Sends message down
                (objects[MessageRelay.sendDown(receivedMessage.getAddress(), objects)] as MessageProtocol).receiveMessage(this, receivedMessage);
            }
        }

        private async void multiThreadStatusColorUpdate(Color endColor)
        {
            Color startColor;

            if (User.getUserName() == null || User.getUserName().Equals(""))
            {
                startColor = ((SolidColorBrush)(Brush)Application.Current.Resources["OnBackground"]).Color;
            }
            else
            {
                startColor = (Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID()));
            }

            await Task.Delay(1000);

            await Task.Run(() => Dispatcher.Invoke(() => iconColorFade(startColor, startColor)));

            await Task.Delay(1000);

            await Task.Run(() => Dispatcher.Invoke(() => iconColorFade(startColor, endColor)));

        }

        private void iconColorFade(Color fromColor, Color toColor)
        {
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.From = fromColor;
            colorAnimation.To = toColor;
            colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            PropertyPath iconForegroundPath = new PropertyPath("(materialdesign:PackIcon.Foreground).(SolidColorBrush.Color)");
            currentAnimation = new Storyboard();
            Storyboard.SetTarget(colorAnimation, AccountIcon);
            Storyboard.SetTargetProperty(colorAnimation, iconForegroundPath);
            currentAnimation.Children.Add(colorAnimation);

            currentAnimation.Begin();
        }

        private void SignoutRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            SignoutRectangle.Opacity = 0.3;
        }

        private void SignoutRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            SignoutRectangle.Opacity = 0;
        }

        private void SignoutRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            User.userLogOut();
            sendMessage(createMessage("USER_LOGGED_OUT", "Main_Window"));
            userLoggedOut();
        }

        private void userLoggedOut()
        {
            defaultSetup();
            popupBox.Height = 80;
            NoAccountPopup.Visibility = Visibility.Visible;
            AcountPopup.Visibility = Visibility.Collapsed;
            multiThreadStatusColorUpdate((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        private void AccountIcon_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void AccountIcon_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}
