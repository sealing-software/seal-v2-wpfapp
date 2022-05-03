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
    /// Interaction logic for GroupRemove.xaml
    /// </summary>
    public partial class GroupRemove : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page_Remove_Group";
        public event EventHandler<StatusMessage> message;
        private Dictionary<long, object> objects = new Dictionary<long, object>();

        public GroupRemove()
        {
            InitializeComponent();

            loadObjectID();
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

        public void sentGroup(Group sentGroup)
        {
            if (sentGroup.qty > 0)
            {
                noDelete(sentGroup);
            }
            else
            {
                delete(sentGroup);
            }
        }

        private void noDelete(Group sentGroup)
        {
            RemoveGroupButton.Visibility = Visibility.Hidden;

            String userPlural = "is a user";

            if (sentGroup.qty > 1)
            {
                userPlural = "are users";
            }

            InfoText.Text = "The " + sentGroup.name + " group cannot be deleted. There " + userPlural + " associated with this group. A group can only be deleted if there are no associated users.";
        }

        private void delete(Group sentGroup)
        {
            RemoveGroupButton.Visibility = Visibility.Visible;

            InfoText.Text = "The " + sentGroup.name + " group will be deleted.";
        }

        private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            sendMessage(createMessage("DELETE_GROUP", "Settings_Page_List_Groups_Page"));
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            sendMessage(createMessage("CLOSE_DIALOG", "Settings_Page_List_Groups_Page"));
        }
    }
}
