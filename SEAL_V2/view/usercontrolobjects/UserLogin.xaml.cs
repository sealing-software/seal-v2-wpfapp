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
using SEAL_V2.db;
using SEAL_V2.model;
using System.Windows.Media.Animation;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    public partial class UserLogin : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Account_Popup";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db;
        private int windowHeight;
        private int windowWidth;
        private Storyboard currentAnimation;
        private Dictionary<long, object> objects = new Dictionary<long, object>();

        public UserLogin()
        {
            InitializeComponent();

            loadObjectID();

            connectToDb();

            statusMessage("");

            loadAccountOptionsList();
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
        }

        public String getObjectName()
        {
            return objectName;
        }

        public long getObjectID()
        {
            return objectID;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            sendMessage(createMessage("CLOSE_ACCOUNT_POPUP", "Main_Window"));
        }

        private void statusMessage(String message)
        {
            loginStatus.Text = message;
        }
        private void connectToDb()
        {
            db = DatabaseInterface.Instance;
            db.requestdbStatus();
        }

        private void queryDb()
        {
            if (db.checkAccountExists(userInputUserName.Text, userInputPassword.Password.ToString()))
            {
                getUserInfo();

                //expandGrid();

                //Send message to account button to update color AND text
                sendMessage(createMessage("SUCCESS", "Title_Bar_Account"));
                //GridBackground.Height = 
                //GridBackground.Width = windowWidth / 1.25;

                //multiThreadStatusColorUpdate(((SolidColorBrush)(Brush)Application.Current.Resources["OnBackground"]).Color, (Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID())));

                //accountButtonAnimation();

                sendMessage(createMessage("REFRESH", "Home_Page"));
            }
            else
            {
                statusMessage("Invalid username or password!");
            }
        }

        private void updateGroupColor()
        {
            multiThreadStatusColorUpdate(((SolidColorBrush)(Brush)Application.Current.Resources["OnBackground"]).Color, (Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID())));
        }

        private void getUserInfo()
        {
            UserDisplayNameText.Text = User.getDisplayName();
            
            UserNameText.Text = User.getUserName();

            AssignedGroupText.Text = User.getAssignedGroupText();
        }

        private void expandGrid()
        {
            EasingDoubleKeyFrame keyFrameHeight = (EasingDoubleKeyFrame)Resources["EndHeight"];
            keyFrameHeight.Value = windowHeight / 1.1;

            EasingDoubleKeyFrame keyFrameWidth = (EasingDoubleKeyFrame)Resources["EndWidth"];
            keyFrameWidth.Value = windowWidth / 1.1;

            Storyboard gridIncrease = this.TryFindResource("LoginUser") as Storyboard;
            gridIncrease.Begin();

            GridLogin.Visibility = Visibility.Hidden;
            GridAccount.Visibility = Visibility.Visible;
        }

        private void updateGridSize(int height, int width)
        {
            EasingDoubleKeyFrame keyFrameStartHeight = (EasingDoubleKeyFrame)Resources["StartHeight"];
            keyFrameStartHeight.Value = windowHeight;

            EasingDoubleKeyFrame keyFrameStartWidth = (EasingDoubleKeyFrame)Resources["StartWidth"];
            keyFrameStartWidth.Value = windowWidth;

            EasingDoubleKeyFrame keyFrameHeight = (EasingDoubleKeyFrame)Resources["EndHeight"];
            keyFrameHeight.Value = height / 1.1;

            EasingDoubleKeyFrame keyFrameWidth = (EasingDoubleKeyFrame)Resources["EndWidth"];
            keyFrameWidth.Value = width / 1.1;

            Storyboard windowSizeChange = this.TryFindResource("WindowSizeUpdate") as Storyboard;
            windowSizeChange.Begin();

            updateWindowSize(height, width);
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            queryDb();
        }

        public void updateWindowSize(int height, int width)
        {
            windowHeight = height;
            windowWidth = width;
        }

        public void windowChanged(int height, int width)
        {            
            if (GridLogin.Visibility == Visibility.Visible)
            {
                updateWindowSize(height, width);
                GridBackground.Height = windowHeight *.3;
                GridBackground.Width = windowWidth * .3;
            }
            else
            {
                updateGridSize(height, width);
            }
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

        //FIX RECEIVE MESSAGE TO INCLUDE SEND DOWN OR RELAY
        public void receiveMessage(object sender, StatusMessage receivedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {

            }
            else
            {
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    String messageString = (String)receivedMessage.readMessage();

                    if (messageString.Equals("GROUP_COLOR_UPDATE"))
                    {
                        updateGroupColor();
                    }
                } else if (receivedMessage.readMessage().GetType().Equals(typeof(System.Int64)))
                {
                    deselectAllItems(receivedMessage.getSender());

                    loadPage((long)receivedMessage.readMessage());
                }
            }
        }

        private void loadPage(long pageID)
        {
            AccountOptionPage.Visibility = Visibility.Visible;

            Pages temp = objects[pageID] as Pages;

            temp.refreshPage();

            AccountOptionPage.NavigationService.Navigate(temp);
        }

        private async void multiThreadStatusColorUpdate(Color startColor, Color endColor)
        {
            await Task.Run(() => Dispatcher.Invoke(() => textColorFade(((SolidColorBrush)(Brush)Application.Current.Resources["OnBackground"]).Color, startColor)));

            await Task.Delay(1950);

            await Task.Run(() => Dispatcher.Invoke(() => textColorFade(startColor, endColor)));
        }

        private void textColorFade(Color fromColor, Color toColor)
        {
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.From = fromColor;
            colorAnimation.To = toColor;
            colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            PropertyPath textForegroundPath = new PropertyPath("(TextBlock.Foreground).(SolidColorBrush.Color)");
            currentAnimation = new Storyboard();
            Storyboard.SetTarget(colorAnimation, AssignedGroupText);
            Storyboard.SetTargetProperty(colorAnimation, textForegroundPath);
            currentAnimation.Children.Add(colorAnimation);

            currentAnimation.Begin();
        }

        private void loadAccountOptionsList()
        {
            AccountOptionItem account = new AccountOptionItem("Account", MaterialDesignThemes.Wpf.PackIconKind.Account, MaterialDesignThemes.Wpf.PackIconKind.AccountOutline, "Account_Popup_Account_Button", "Account_Popup_Account_Page", "Account_Popup");
            account.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[account.getObjectName()]] = account;
            accountoptionslist.Children.Add(account);

            AccountPopupAccount accountPage = new AccountPopupAccount();
            accountPage.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[accountPage.getObjectName()]] = accountPage;

            AccountOptionItem security = new AccountOptionItem("Security", MaterialDesignThemes.Wpf.PackIconKind.Lock, MaterialDesignThemes.Wpf.PackIconKind.UnlockedOutline, "Account_Popup_Security_Button", "Account_Popup_Security_Page", "Account_Popup");
            security.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[security.getObjectName()]] = security;
            accountoptionslist.Children.Add(security);

            AccountPopupSecurity securityPage = new AccountPopupSecurity();
            securityPage.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[securityPage.getObjectName()]] = securityPage;


            AccountOptionItem license = new AccountOptionItem("License", MaterialDesignThemes.Wpf.PackIconKind.License, MaterialDesignThemes.Wpf.PackIconKind.License, "Account_Popup_License_Button", "Account_Popup_License_Page", "Account_Popup");
            license.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[license.getObjectName()]] = license;
            accountoptionslist.Children.Add(license);

            AccountPopupLicense licensePage = new AccountPopupLicense();
            licensePage.message += receiveMessage;
            objects[ObjectIDManager.objectIDs[licensePage.getObjectName()]] = licensePage;
        }

        private async void accountButtonAnimation()
        {
            foreach (AccountOptionItem item in accountoptionslist.Children)
            {
                item.buttonLoadAnimation();
                await Task.Delay(100);
            }       
        }

        private void deselectAllItems(long selectedItem)
        {
            foreach (AccountOptionItem item in accountoptionslist.Children)
            {
                if (item.getObjectID() != selectedItem)
                {
                    item.deselectItem();
                }
                else
                {
                    item.itemSelected();
                }
            }
        }
    }
}
