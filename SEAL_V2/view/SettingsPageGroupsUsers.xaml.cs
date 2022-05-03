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
    /// Interaction logic for SettingsPageGroupsUsers.xaml
    /// </summary>
    public partial class SettingsPageGroupsUsers : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page_List_Accounts_Page";
        private String name = "Group Users";
        private List<UserInfo> userList;
        private DatabaseInterface db;
        private int groupID;
        public event EventHandler<StatusMessage> message;

        public SettingsPageGroupsUsers(int groupID)
        {
            InitializeComponent();
            loadObjectID();
            linkDB();
            this.groupID = groupID;
            getUserList(groupID);
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
        private void linkDB()
        {
            db = DatabaseInterface.Instance;
        }

        public String getName()
        {
            return name;
        }

        public void refreshPage()
        {
            getUserList(groupID);
        }

        private void getUserList(int groupID)
        {
            userList = db.getUsersFromGroup(groupID);
            UserList.ItemsSource = userList;
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

            }
            else
            {

            }
        }

        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow rowHover = (DataGridRow)sender;

            if (!rowHover.IsSelected)
            {
                rowHover.Opacity = 0.5;
            }
        }

        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow rowHover = (DataGridRow)sender;

            if (!rowHover.IsSelected)
            {
                rowHover.Opacity = 1;
            }
        }

        private void UserList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UserInfo selectedUser = (UserInfo)UserList.SelectedItem;

            sendMessage(createMessage(selectedUser, "Settings_Page_List_Groups_Page"));
        }
    }
}
