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
    /// Interaction logic for SettingsListItem.xaml
    /// </summary>
    public partial class SettingsListItem : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName;
        private String name;
        private PackIconKind iconShow;
        private PackIconKind iconSelected;
        private String sendToString;
        private String objectNameNav;
        private long sendTo;
        public event EventHandler<StatusMessage> message;
        private bool selected = false;

        public SettingsListItem(String name, PackIconKind iconShow, PackIconKind iconSelected, String objectName, String objectNameNav, String objectNameSend)
        {
            if (User.userAuthorized(objectName) || User.isAdmin())
            {
                InitializeComponent();

                this.name = name;
                this.iconShow = iconShow;
                this.iconSelected = iconSelected;
                this.objectName = objectName;
                this.sendToString = objectNameSend;
                this.objectNameNav = objectNameNav;
                loadObjectID();
                this.sendTo = ObjectIDManager.objectIDs[objectNameSend];

                setup();
            }
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
        }

        public String getObjectName()
        {
            return objectName;
        }

        private void setup()
        {
            icon.Kind = iconShow;
            Description.Text = name;
        }

        private void ItemGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!selected)
            {
                ItemGrid.Opacity = 0.5;
                icon.Kind = iconSelected;
            }
        }

        private void ItemGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!selected)
            {
                ItemGrid.Opacity = 1;
                icon.Kind = iconShow;
            }
        }

        public void shortcut(UserInfo passedUser)
        {
            itemSelected();

            sendMessage(createMessage(ObjectIDManager.objectIDs[objectNameNav], sendToString));
        }

        private void ItemGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemSelected();

            sendMessage(createMessage(ObjectIDManager.objectIDs[objectNameNav], sendToString));
        }

        public void itemSelected()
        {
            selected = true;
            selectedicon.Visibility = Visibility.Visible;

            ItemGrid.Opacity = 0.5;
            icon.Kind = iconSelected;
        }

        public void deselectItem()
        {
            selected = false;
            selectedicon.Visibility = Visibility.Hidden;

            ItemGrid.Opacity = 1;
            icon.Kind = iconShow;

        }

        public long getObjectID()
        {
            return objectID;
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

        public void receiveMessage(object sender, StatusMessage recevievedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, recevievedMessage.getAddress()))
            {
                sendMessage(recevievedMessage);
            }
            else
            {

            }
        }
    }
}
