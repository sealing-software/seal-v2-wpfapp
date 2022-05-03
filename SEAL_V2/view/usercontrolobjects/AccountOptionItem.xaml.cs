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
using System.Windows.Media.Animation;
using SEAL_V2.model;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for AccountOptionItem.xaml
    /// </summary>
    public partial class AccountOptionItem : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName;
        private String name;
        private PackIconKind iconShow;
        private PackIconKind iconSelected;
        private string sendToString;
        private String objectNameNav;
        private long sendTo;
        public event EventHandler<StatusMessage> message;
        private bool selected = false;

        public AccountOptionItem(String name, PackIconKind iconShow, PackIconKind iconSelected, String objectName, String objectNameNav, String objectNameSend)
        {
            InitializeComponent();

            this.name = name;
            this.iconShow = iconShow;
            this.iconSelected = iconSelected;
            this.objectName = objectName;
            this.sendToString = objectNameSend;
            this.objectNameNav = objectNameNav;
            this.sendTo = ObjectIDManager.objectIDs[objectNameSend];
            loadObjectID();
            setupButton();
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

        private void setupButton()
        {
            icon.Kind = iconShow;
            buttontext.Text = name;
        }

        public void buttonLoadAnimation()
        {
            Storyboard loadButton = this.TryFindResource("ButtonLoad") as Storyboard;
            loadButton.Begin();
        }

        private void maingrid_MouseEnter(object sender, MouseEventArgs e)
        {
            maingrid.Opacity = 0.5;
            icon.Kind = iconSelected;
        }

        private void maingrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!selected)
            {
                maingrid.Opacity = 1;
                icon.Kind = iconShow;
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

        private void maingrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            itemSelected();

            sendMessage(createMessage(ObjectIDManager.objectIDs[objectNameNav], sendToString));
        }

        public void itemSelected()
        {
            selected = true;
        }

        public void deselectItem()
        {
            selected = false;
            maingrid_MouseLeave(null, null);
        }
    }
}
