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
using SEAL_V2.db;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for SettingsPageGroupsName.xaml
    /// </summary>
    public partial class SettingsPageGroupsName : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page_List_Name_Page";
        private String name = "Group Name";
        private String newName;
        private Group selectedGroup;
        private DatabaseInterface db = DatabaseInterface.Instance;
        public event EventHandler<StatusMessage> message;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public SettingsPageGroupsName(Group group)
        {
            InitializeComponent();

            loadObjectID();

            selectedGroup = group;

            GroupNameText.Text = selectedGroup.name;
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

        public String getName()
        {
            return name;
        }

        public void refreshPage()
        {

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
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    if (receivedMessage.readMessage().Equals("CHANGE_NAME"))
                    {
                        setName();
                    }
                }
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

        private void GroupNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GroupNameText.Text.Equals(selectedGroup.name))
            {
                GroupNameErrorText.Text = "";
                sendMessage(createMessage("NAME_CHANGE_INVALID", "Settings_Page_List_Groups_Page"));
            }
            else if (db.checkGroupNameExists(GroupNameText.Text))
            {
                GroupNameErrorText.Text = "Group name already exists!";
                sendMessage(createMessage("NAME_CHANGE_INVALID", "Settings_Page_List_Groups_Page"));
            }
            else if (GroupNameText.Text.Equals(""))
            {
                GroupNameErrorText.Text = "Group name cannot be blank!";
                sendMessage(createMessage("NAME_CHANGE_INVALID", "Settings_Page_List_Groups_Page"));
            }
            else
            {
                GroupNameErrorText.Text = "";
                sendMessage(createMessage("NAME_CHANGE_VALID", "Settings_Page_List_Groups_Page"));
            }
        }

        public void setName()
        {
            newName = GroupNameText.Text;
            db.updateGroupName(selectedGroup.ID, newName);
        }

        public String getNewName()
        {
            return newName;
        }
    }
}
