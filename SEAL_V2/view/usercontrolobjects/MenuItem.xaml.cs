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

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName;
        private long navigateTo;
        private String objectNameNav;
        public bool containsSubItems { get; set; }
        public String name { get; set; }
        public PackIconKind icon { get; set; }
        private Brush foregroundColor { get; set; }
        public event EventHandler<StatusMessage> message;


        public MenuItem(string objectName, PackIconKind icon, String objectNameNav, bool subItems)
        {
            InitializeComponent();

            this.objectName = objectName;
            this.icon = icon;
            loadObjectID();
            this.objectNameNav = objectNameNav;
            this.navigateTo = ObjectIDManager.objectIDs[objectNameNav];
            foregroundColor = (Brush)Application.Current.Resources["MenuButton"];
            containsSubItems = subItems;
            updateIcon();
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

        public String getObjectNameNav()
        {
            return objectNameNav;
        }

        public PackIconKind getIcon()
        {
            return icon;
        }

        private void updateIcon()
        {
            MenuItemIcon.Kind = icon;
        }

        public void itemSelected()
        {
            //ADD COLOR SWITCHING
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
        public void receiveMessage(object sender, StatusMessage recevievedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, recevievedMessage.getAddress()))
            {

            }
            else
            {
 
            }
        }

        private void MenuItemIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            MenuItemIcon.Opacity = 0.5;
        }

        private void MenuItemIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!subMenuPopup.IsOpen)
            {
                MenuItemIcon.Opacity = 1;
            }
        }

        private void MenuItemIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Message to Main_Window to change page
            if (containsSubItems)
            {
                subMenuPopup.IsOpen = true;
            }
            else
            {
                sendMessage(createMessage(navigateTo, "Main_Window"));
            }
        }

        private void subMenuPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            MenuItemIcon.Opacity = 1;
            subMenuPopup.IsOpen = false;
        }
    }
}
