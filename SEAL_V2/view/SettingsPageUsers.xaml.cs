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
using SEAL_V2.view.usercontrolobjects;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for SettingsPageUsers.xaml
    /// </summary>
    public partial class SettingsPageUsers : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Users_Page";
        private String name = "User Settings";
        public event EventHandler<StatusMessage> message;
        private List<UserInfo> usersList;
        private DatabaseInterface db = DatabaseInterface.Instance;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        private DataGridRow selectedRow;
        private UserInfo selectedUser;


        public SettingsPageUsers()
        {
            InitializeComponent();

            loadObjectID();

            addDialogBox();

            refreshUserList();
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
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

        private void setupMainView()
        {
            hideAllButtons();
            hideAllViews();
            refreshUserList();
            UsersTitle.Text = "Users";
            ReturnViewBox.Visibility = Visibility.Hidden;
            AddUserButton.Visibility = Visibility.Visible;
            UserList.Visibility = Visibility.Visible;
        }

        private void addDialogBox()
        {
            UserRemove RemoveUserPopup = new UserRemove();
            RemoveUserPopup.message += receiveMessage;
            objects[RemoveUserPopup.getObjectID()] = RemoveUserPopup;
            Dialog_Content.Children.Add(objects[RemoveUserPopup.getObjectID()] as UserRemove);
        }

        private void setupEditView()
        {
            hideAllButtons();
            hideAllViews();
            UsersTitle.Text = selectedUser.userName;
            ReturnViewBox.Visibility = Visibility.Visible;
            UserEdit.Visibility = Visibility.Visible;
            DisplayNameText.Text = selectedUser.name;
            PasswordText.Text = "";

            //Primary admin account should not be able to have group changed.
            if (selectedUser.ID == 1)
            {
                GroupText.Visibility = Visibility.Hidden;
                GroupSelection.Visibility = Visibility.Hidden;
            }
            else
            {
                GroupText.Visibility = Visibility.Visible;
                GroupSelection.Visibility = Visibility.Visible;
                loadGroupComboBox();
            }

            UpdateUserButton.Visibility = Visibility.Hidden;
        }

        private void setupNewUserview()
        {
            hideAllButtons();
            hideAllViews();
            UsersTitle.Text = "New user";
            ReturnViewBox.Visibility = Visibility.Visible;
            NewUser.Visibility = Visibility.Visible;
            UsernameText.Text = "";
            NewUserDisplayNameText.Text = "";
            UsernameErrorText.Text = "";
            NewUserPasswordText.Text = "";
            NewUserPasswordErrorText.Text = "";
            NewUserGroupSelection.ItemsSource = db.getGroups();

            SaveNewUserButton.Visibility = Visibility.Hidden;
        }

        private void hideAllViews()
        {
            UserEdit.Visibility = Visibility.Hidden;
            UserList.Visibility = Visibility.Hidden;
            NewUser.Visibility = Visibility.Hidden;
        }

        private void loadGroupComboBox()
        {
            List<Group> groupList = db.getGroups();

            GroupSelection.ItemsSource = groupList;

            for (int i = 0; i < groupList.Count; i++)
            {
                Group temp = (Group)GroupSelection.Items[i];

                if (temp.ID == selectedUser.groupID)
                {
                    GroupSelection.SelectedIndex = i;
                }
            }
        }

        public void shortcut(UserInfo passedUser)
        {
            selectedUser = passedUser;

            setupEditView();
        }

        private void hideAllButtons()
        {
            AddUserButton.Visibility = Visibility.Hidden;
            RemoveUserButton.Visibility = Visibility.Hidden;
            EditUserButton.Visibility = Visibility.Hidden;
            UpdateUserButton.Visibility = Visibility.Hidden;
            SaveNewUserButton.Visibility = Visibility.Hidden;
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
                if (receivedMessage.readMessage().GetType().Equals(typeof(UserInfo)))
                {
                    selectedUser = (UserInfo)receivedMessage.readMessage();

                    EditUserButton.Visibility = Visibility.Visible;
                    //Save user to local variable, show edit and delete user buttons...
                }
                else if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    if (receivedMessage.readMessage().Equals("CLOSE_DIALOG"))
                    {
                        Dialog_Box.IsOpen = false;
                    }
                    else if (receivedMessage.readMessage().Equals("DELETE_USER"))
                    {
                        db.deleteUser(selectedUser);
                        refreshPage();
                        Dialog_Box.IsOpen = false;
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

        public long getObjectID()
        {
            return objectID;
        }

        public void refreshPage()
        {
            if (selectedRow != null && selectedRow.IsSelected)
            {
                selectedRow.IsSelected = false;

                DataGridRow_MouseLeave(selectedRow, null);
            }
            selectedRow = null;
            selectedUser = null;
            Dialog_Box.IsOpen = false;
            setupMainView();
        }

        public String getName()
        {
            return name;
        }

        private void refreshUserList()
        {
            UserList.Visibility = Visibility.Visible;

            usersList = db.getAllUsers();

            UserList.ItemsSource = usersList;
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

        private void DataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedRow != null && (DataGridRow)sender != selectedRow)
            {
                selectedRow.IsSelected = false;
                DataGridRow_MouseLeave(selectedRow, null);
            }

            selectedRow = (DataGridRow)sender;
            selectedUser = (UserInfo)UserList.SelectedItem;

            showSelectedRowButtons();
        }

        private void showSelectedRowButtons()
        {
            if (!selectedUser.isAdmin())
            {
                RemoveUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                RemoveUserButton.Visibility = Visibility.Hidden;
            }
            EditUserButton.Visibility = Visibility.Visible;

        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            setupEditView();
        }

        private void UsersReturnTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            UsersReturnTitle.Opacity = 0.5;
        }

        private void UsersReturnTitle_MouseLeave(object sender, MouseEventArgs e)
        {
            UsersReturnTitle.Opacity = 1;
        }

        private void UsersReturnTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            setupMainView();
        }

        private void changeMade(object sender, TextChangedEventArgs e)
        {
            UpdateUserButton.Visibility = Visibility.Visible;
        }

        private void DisplayNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeMade(sender, null);
        }

        private void GroupSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeMade(sender, null);
        }

        private void PasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeMade(sender, null);

            if (PasswordText.Text.Equals(""))
            {
                PasswordErrorText.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordErrorText.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            db.updateUser(selectedUser.ID, DisplayNameText.Text, PasswordText.Text, GroupSelection.SelectedItem as Group);
            setupMainView();
        }

        private void AddUserButton_Click_1(object sender, RoutedEventArgs e)
        {
            setupNewUserview();
        }

        private void UsernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (db.checkUsernameExists(UsernameText.Text))
            {
                UsernameErrorText.Text = "The username already exists!";
                SaveNewUserButton.Visibility = Visibility.Hidden;
            }
            else
            {
                UsernameErrorText.Text = "";
                showSaveUserButton();
            }
        }

        private void NewUserDisplayNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            showSaveUserButton();
        }

        private void NewUserPasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NewUserPasswordText.Text.Equals(""))
            {
                NewUserPasswordErrorText.Text = "The password cannot be blank!";
                SaveNewUserButton.Visibility = Visibility.Hidden;
            }
            else
            {
                NewUserPasswordErrorText.Text = "";
                showSaveUserButton();
            }
        }

        private void NewUserGroupSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            showSaveUserButton();
        }

        private void showSaveUserButton()
        {
            if (!UsernameText.Text.Equals("") && !NewUserDisplayNameText.Text.Equals("") && !NewUserPasswordText.Text.Equals("") && NewUserGroupSelection.SelectedItem != null && !db.checkUsernameExists(UsernameText.Text) && !NewUserPasswordText.Text.Equals(""))
            {
                SaveNewUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                SaveNewUserButton.Visibility = Visibility.Hidden;
            }
        }

        private void SaveNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            db.addUser(UsernameText.Text, NewUserDisplayNameText.Text, NewUserPasswordText.Text, NewUserGroupSelection.SelectedItem as Group);
            setupMainView();
        }

        private void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_List_Users_Page_Remove_User"]] as UserRemove).sentUser(selectedUser);
            Dialog_Box.IsOpen = true;
        }
    }
}
