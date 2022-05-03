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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using SEAL_V2.view.usercontrolobjects;
using SEAL_V2.model;
using SEAL_V2.db;
using System.Text.RegularExpressions;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Main_Window";
        private String currentPage;
        private DatabaseInterface db;
        //private StatusBar statusBar;
        //private UserLogin userAccountPopup;
        //private TitleBarButtons titleBar;
        //private MenuBarOverlay menu;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        private long currentPageLong = 0;
        //private Dictionary<long, object> pages = new Dictionary<long, object>();
        private int windowHeight;
        private int windowWidth;
        public event EventHandler<StatusMessage> message;
        private bool pageHeaderLink = false;
        private AuditStatus auditStatus;

        public MainWindow()
        {
            InitializeComponent();

            //Need to turn this into actual license check
            License.licenseSetup();

            CurrentSystem.systemSetup();

            //test.Text = SystemParameters.PrimaryScreenWidth.ToString() + "X" + SystemParameters.PrimaryScreenHeight.ToString();
            initialLoad();
        }

        private void initialLoad()
        {
            //Loads csv object mapping
            ObjectIDManager.LoadList();

            loadObjectID();

            showStatusBar();

            addAuditView();

            addPageObjects();

            initialPageLoad();

            addMenu();

            connectToDb();

            addDialogBoxLogin();
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

        //Creates Menu object
        private void addMenu()
        {
            MenuBarOverlay menu = new MenuBarOverlay();
            menu.message += receiveMessage;
            objects[menu.getObjectID()] = menu;
            MenuBarGrid.Children.Add(objects[menu.getObjectID()] as MenuBarOverlay);
        }

        private void addPageObjects()
        {
            HomePage home = new HomePage();
            home.message += receiveMessage;
            objects[home.getObjectID()] = home;
            SettingsPage settings = new SettingsPage();
            settings.message += receiveMessage;
            objects[settings.getObjectID()] = settings;
            CapturePage capture = new CapturePage();
            capture.message += receiveMessage;
            objects[capture.getObjectID()] = capture;
            VerifyPage verify = new VerifyPage();
            verify.message += receiveMessage;
            objects[verify.getObjectID()] = verify;
            ReportPage report = new ReportPage();
            report.message += receiveMessage;
            objects[report.getObjectID()] = report;
        }

        //Adds User profile info popup
        private void addDialogBoxLogin()
        {
            UserLogin userAccountPopup = new UserLogin();
            //userAccountPopup.returnButton += returnButtonEvent;
            userAccountPopup.message += receiveMessage;
            objects[userAccountPopup.getObjectID()] = userAccountPopup;
            DialogContent.Children.Add(objects[userAccountPopup.getObjectID()] as UserLogin);
        }

        private void returnButtonEvent(object sender, EventArgs e)
        {
            closeAccountPopup();
        }

        private void closeAccountPopup()
        {
            DialogHostBox.IsOpen = false;
        }

        private void connectToDb()
        {
            db = DatabaseInterface.Instance;
            db.dbStatus += sendStatusBarMessage;
            db.requestdbStatus();
        }


        private void showStatusBar()
        {
            showStatusLeft();
            showStatusRight();
            Storyboard showStatusBar = this.TryFindResource("FadeInStatusBar") as Storyboard;
            showStatusBar.Begin();
        }

        private void addAuditView()
        {
            auditStatus = new AuditStatus();
            auditView.Children.Add(auditStatus);

        }

        //Adds Status Bar
        private void showStatusLeft()
        {
            StatusBar statusBar = new StatusBar();

            objects[statusBar.getObjectID()] = statusBar;

            SystemStatusGrid.Children.Add(objects[statusBar.getObjectID()] as StatusBar);
        }

        //Adds title bar
        private void showStatusRight()
        {
            TitleBarButtons titleBarButtonsObject = new TitleBarButtons();

            titleBarButtonsObject.message += receiveMessage;

            objects[titleBarButtonsObject.getObjectID()] = titleBarButtonsObject;

            TitleBarButtonsGrid.Children.Add(objects[titleBarButtonsObject.getObjectID()] as TitleBarButtons);
        }

        private void showUserLogin()
        {
            updateWindowDimensions();

            (objects[ObjectIDManager.objectIDs["Account_Popup"]] as UserLogin).updateWindowSize(windowHeight, windowWidth);

            DialogHostBox.IsOpen = true;
        }

        private void initialPageLoad()
        {
            //Loads homepage for initialload()

            loadPage(ObjectIDManager.objectIDs["Home_Page"]);

        }

        private void updatePageTitle(object passedPage)
        {
            if (pageHeaderLink)
            {
                PageTitleText.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                PreviousPageTitleText.Text = "<" + currentPage;
            }
            else
            {
                PageTitleText.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                PreviousPageTitleText.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                PreviousPageTitleText.Text = currentPage;
            }
            Pages temp = (Pages)passedPage;
            temp.refreshPage();
            currentPage = temp.getName();
            PageTitleText.Text = currentPage;
            Storyboard loadPageNameAnimation = this.TryFindResource("LoadPageTitle") as Storyboard;
            loadPageNameAnimation.Begin();
            pageHeaderLink = false;
        }

        //Dictionary for pages??
        private void loadPage(long pageID)
        {
            if (currentPageLong != pageID)
            {
                currentPageLong = pageID;

                updatePageTitle(objects[pageID]);

                CurrentPage.NavigationService.Navigate(objects[pageID]);
            }
        }

        //This should be replaced wit general relay message method
        private void sendStatusBarMessage(object sender, StatusMessage sentMessage)
        {
            (objects[ObjectIDManager.objectIDs["Status_Bar"]] as StatusBar).receiveMessage(this, sentMessage);
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
            //Execute some action on this object
            if (this.objectID == receivedMessage.getAddress())
            {
                //If message content are Sytem.Int64 type then that is address of page to navigate to
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.Int64)))
                {
                    long pageNav = (long)receivedMessage.readMessage();
                    loadPage(pageNav);
                }
                else if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    String messageString = (String)receivedMessage.readMessage();

                    if (messageString.Equals("LOGIN"))
                    {
                        showUserLogin();
                    }
                    else if (messageString.Equals("CLOSE_ACCOUNT_POPUP"))
                    {
                        closeAccountPopup();
                    }
                    else if (messageString.Equals("USER_LOGGED_OUT"))
                    {
                        userLogOut();
                    }
                    else if (messageString.Equals("SYSTEM_FULL_VIEW"))
                    {
                        setPageHeaderLink();
                    }
                    else if (messageString.Equals("AUDIT_ON"))
                    {
                        auditStatus.toggleOn();
                    }
                    else if (messageString.Equals("AUDIT_OFF"))
                    {
                        auditStatus.toggleOff();
                    }

                }
            }
            //Send message up
            //else if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            //{
                //This is top....will not get sent up. Should remove.
            //}
            else
            {
                //Sends message down
                (objects[MessageRelay.sendDown(receivedMessage.getAddress(), objects)] as MessageProtocol).receiveMessage(this, receivedMessage);
            }
        }


        private void DialogMessage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //getWindowDimensions();
        }

        private void updateWindowDimensions()
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            windowHeight = (int)mainWindow.Height;
            windowWidth = (int)mainWindow.Width;
        }

        private void StatusBarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //updatePageTitle("TEST");
        }

        private void userLogOut()
        {
            DialogContent.Children.Clear();
            addDialogBoxLogin();
            initialPageLoad();
            (objects[ObjectIDManager.objectIDs["Home_Page"]] as HomePage).refreshPage();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Account_Popup"]] as UserLogin).windowChanged((int)this.ActualHeight, (int)this.ActualWidth);
        }

        private void setPageHeaderLink()
        {
            pageHeaderLink = true;
            PageTitleText.Text = "<" + PageTitleText.Text;
            PageTitleText.Foreground = ((Brush)Application.Current.Resources["SecondaryColor"]);
            PreviousPageTitleText.Foreground = ((Brush)Application.Current.Resources["SecondaryColor"]);
        }

        private void CurrentPageViewBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (pageHeaderLink)
            {
                CurrentPageViewBox.Opacity = 0.5;
            }
        }

        private void CurrentPageViewBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (pageHeaderLink)
            {
                CurrentPageViewBox.Opacity = 1;
            }
        }

        private void CurrentPageViewBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (pageHeaderLink)
            {
                (objects[ObjectIDManager.objectIDs["Home_Page"]] as HomePage).receiveMessage(this, createMessage("RETURN_HOME", "Home_Page"));
                CurrentPageViewBox.Opacity = 1;
                PageTitleText.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                PageTitleText.Text = (objects[ObjectIDManager.objectIDs["Home_Page"]] as HomePage).getName();
                pageHeaderLink = false;
            }
        }
    }
}
