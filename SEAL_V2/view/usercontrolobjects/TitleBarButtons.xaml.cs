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

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for TitleBarButtons.xaml
    /// </summary>
    public partial class TitleBarButtons : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Title_Bar";
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;

        public TitleBarButtons()
        {
            InitializeComponent();

            loadObjectID();

            addAccountButton();
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

        private void addAccountButton()
        {
            UserAccountButton userAccountButton = new UserAccountButton();

            userAccountButton.message += receiveMessage;

            objects[userAccountButton.getObjectID()] = userAccountButton;

            TitleBarButtonsStackPanel.Children.Add(objects[userAccountButton.getObjectID()] as UserAccountButton);
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

            }
            else if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {
                sendMessage(receivedMessage);
            }
            else
            {
                (objects[MessageRelay.sendDown(receivedMessage.getAddress(), objects)] as MessageProtocol).receiveMessage(this, receivedMessage);
            }
        }
    }
}
