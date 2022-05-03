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
using SEAL_V2.view.usercontrolobjects;
using MaterialDesignThemes.Wpf;
using SEAL_V2.model;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for MenuBarOverlay.xaml
    /// </summary>
    public partial class MenuBarOverlay : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Menu_Bar";
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;

        public MenuBarOverlay()
        {
            InitializeComponent();
            loadObjectID();
            setupMenu();
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

        private void setupMenu()
        {
            loadMenuItems();
            showMenu();
        }

        private void showMenu()
        {
            
            setMenuLength();
            Storyboard showMenu = this.TryFindResource("ShowMenu") as Storyboard;
            showMenu.Begin();
        }

        private void setMenuLength()
        {
            int lengthOfMenu = 50 + ((MenuButtons.Children.Count - 4) * 20);

            //NEED TO DETERMINE LENGTH OF MENU PER MENU ICON

            updateMenuAnimationWidth(lengthOfMenu);
        }

        private void updateMenuAnimationWidth(int lengthOfMenu)
        {
            EasingDoubleKeyFrame keyFrame = (EasingDoubleKeyFrame)Resources["RectangleWidthAnimation"];
            keyFrame.Value = lengthOfMenu;
        }
        private void loadMenuItems()
        {
            loadMenuItemHome();
            loadMenuItemsViewable();
            //initial load home is current...
            loadMenuItemCurrent(objects[ObjectIDManager.objectIDs["Menu_Bar_Home_Button"]] as MenuItem);
        }


        //SET AS DICTIONARY TO ADD RESPECTIVE BUTTONS TO SUB MENU!
        private void loadMenuItemHome()
        {
            MenuItem home = new MenuItem("Menu_Bar_Home_Button", PackIconKind.House, "Home_Page", false);
            home.message += receiveMessage;
            objects[home.getObjectID()] = home;
            MenuButtons.Children.Add(objects[ObjectIDManager.objectIDs["Menu_Bar_Home_Button"]] as MenuItem);
            Separator left = new Separator("Menu_Bar_Separator_Left");
            objects[left.getObjectID()] = left;
            MenuButtons.Children.Add(objects[left.getObjectID()] as Separator);
        }

        private void loadMenuItemsViewable()
        {
            List<MenuItem> list = new List<MenuItem>();

            list.Add(new MenuItem("Menu_Bar_Capture_Button", PackIconKind.Camera, "Capture_Page", false));
            list.Add(new MenuItem("Menu_Bar_Verify_Button", PackIconKind.Check, "Verify_Page", false));
            list.Add(new MenuItem("Menu_Bar_Automate_Button", PackIconKind.FileReport, "Reports_Page", false));
            list.Add(new MenuItem("Menu_Bar_Settings_Button", PackIconKind.Gear, "Settings_Page", false));
            foreach (MenuItem item in list)
            {
                ingestSubMenuItem(item);
            }
        }

        private void ingestSubMenuItem(MenuItem subMenuItem)
        {
            subMenuItem.message += receiveMessage;
            objects[subMenuItem.getObjectID()] = subMenuItem;
            MenuButtons.Children.Add(objects[ObjectIDManager.objectIDs[subMenuItem.getObjectName()]] as MenuItem);
        }

        //THis should turn into simple function updateCurrent() where item is added as child to end of stackpanel index (replaces)
        private void loadMenuItemCurrent(MenuItem currentPageButton)
        {
            Separator right = new Separator("Menu_Bar_Separator_Right");
            objects[right.getObjectID()] = right;
            MenuButtons.Children.Add(objects[right.getObjectID()] as Separator);
            MenuItem current = new MenuItem("Menu_Bar_Current_Button", currentPageButton.getIcon(), currentPageButton.getObjectNameNav(), false);
            current.message += receiveMessage;
            MenuButtons.Children.Add(current);
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
                sendMessage(receivedMessage);
            }
            else
            {
                //Send down...
                //Relay to objects at current level...
            }
        }


    }
}
