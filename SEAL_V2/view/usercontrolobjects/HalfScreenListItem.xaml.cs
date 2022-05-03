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
    /// Interaction logic for HalfScreenListItem.xaml
    /// </summary>
    public partial class HalfScreenListItem : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName;
        private String name;
        private PackIconKind iconShow;
        private PackIconKind iconSelected;
        private long navTo;
        private String sendToString;
        public event EventHandler<StatusMessage> message;
        private bool selected = false;

        public HalfScreenListItem(String name, PackIconKind iconShow, PackIconKind iconSelected, String objName, String navObjName, String sendToObjName)
        {
            InitializeComponent();

            this.name = name;
            this.iconShow = iconShow;
            this.iconSelected = iconSelected;
            this.objectID = ObjectIDManager.objectIDs[objName];
            this.objectName = objName;
            this.navTo = ObjectIDManager.objectIDs[navObjName];
            this.sendToString = sendToObjName;

            setup();
        }

        public void loadObjectID()
        {
            //Taken care of in constructor...
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

        private void ItemGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemSelected();

            sendMessage(createMessage(navTo, sendToString));
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
                //ADD LATER
            }
        }

    }
}
