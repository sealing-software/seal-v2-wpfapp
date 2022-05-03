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
    /// Interaction logic for SettingsPageGroups.xaml
    /// </summary>
    public partial class SettingsPageGroups : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page";
        private String name = "Group Settings";
        public event EventHandler<StatusMessage> message;
        private List<Group> groupList;
        private DatabaseInterface db;
        private DataGridRow selectedRow;
        private Group selectedGroup;
        private SettingsPageGroupsUsers usersList;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        private UserInfo selectedUser;
        public SettingsPageGroups()
        {
            InitializeComponent();

            loadObjectID();

            linkDB();

            addDialogBox();

            refreshGroupList();
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

        private void addDialogBox()
        {
            GroupRemove RemoveGroupPopup = new GroupRemove();
            RemoveGroupPopup.message += receiveMessage;
            objects[RemoveGroupPopup.getObjectID()] = RemoveGroupPopup;
            Dialog_Content.Children.Add(objects[RemoveGroupPopup.getObjectID()] as GroupRemove);
            //userAccountPopup.returnButton += returnButtonEvent;
            //userAccountPopup.message += receiveMessage;
            //objects[userAccountPopup.getObjectID()] = userAccountPopup;
            //Dialog_Box.Children.Add(objects[userAccountPopup.getObjectID()] as UserLogin);
            //Dialog_Content.Children.Add(RemoveGroupPopup);
        }

        public String getName()
        {
            return name;
        }

        private void linkDB()
        {
            db = DatabaseInterface.Instance;
        }

        private void refreshGroupList()
        {
            GroupList.Visibility = Visibility.Visible;

            groupList = db.getGroups();

            GroupList.ItemsSource = groupList;
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
            selectedGroup = (Group)GroupList.SelectedItem;

            showSelectedRowButtons();
        }

        private void showSelectedRowButtons()
        {
            if (!selectedGroup.isAdmin())
            {
                RemoveGroupButton.Visibility = Visibility.Visible;
            }
            else
            {
                RemoveGroupButton.Visibility = Visibility.Hidden;
            }

            EditGroupButton.Visibility = Visibility.Visible;
        }

        public void refreshPage()
        {
            if (selectedRow != null && selectedRow.IsSelected)
            {
                selectedRow.IsSelected = false;

                DataGridRow_MouseLeave(selectedRow, null);
            }
            selectedRow = null;
            selectedGroup = null;
            Dialog_Box.IsOpen = false;
            setupMainView();
            refreshGroupList();
        }


        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            setupEditView();
        }

        private void GroupsReturnTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            GroupsReturnTitle.Opacity = 0.5;
        }

        private void GroupsReturnTitle_MouseLeave(object sender, MouseEventArgs e)
        {
            GroupsReturnTitle.Opacity = 1;
        }

        private void hideAllButtons()
        {
            RemoveGroupButton.Visibility = Visibility.Hidden;
            EditGroupButton.Visibility = Visibility.Hidden;
            AddGroupButton.Visibility = Visibility.Hidden;
            RemoveUserButton.Visibility = Visibility.Hidden;
            SaveColorButton.Visibility = Visibility.Hidden;
            SaveGroupButton.Visibility = Visibility.Hidden;
            EditUserButton.Visibility = Visibility.Hidden;
        }

        private void hideAllViews()
        {
            EditGroup.Visibility = Visibility.Collapsed;
            NewGroup.Visibility = Visibility.Collapsed;
            GroupList.Visibility = Visibility.Collapsed;
        }

        private void setupMainView()
        {
            hideAllButtons();
            hideAllViews();
            GroupsTitle.Text = "Groups";
            GroupsTitle.Foreground = (Brush)Application.Current.Resources["OnBackground"];
            ReturnViewBox.Visibility = Visibility.Hidden;
            GroupList.Visibility = Visibility.Visible;
            mainViewButtons();
        }

        private void updateGroupTitle()
        {
            GroupsTitle.Text = selectedGroup.name;
            GroupsTitle.Foreground = selectedGroup.brushColor;
        }

        private void mainViewButtons()
        {
            AddGroupButton.Visibility = Visibility.Visible;
        }

        private void setupEditView()
        {
            hideAllButtons();
            hideAllViews();
            GroupsSettingsPage.Visibility = Visibility.Collapsed;
            GroupsTitle.Text = selectedGroup.name;
            GroupsTitle.Foreground = selectedGroup.brushColor;
            EditGroup.Visibility = Visibility.Visible;
            ReturnViewBox.Visibility = Visibility.Visible;
            setupGroupsOptions();
            //getUserList();
        }

        private void setupNewGroupView()
        {
            hideAllButtons();
            hideAllViews();
            refreshNewGroupView();
            NewGroup.Visibility = Visibility.Visible;
            ReturnViewBox.Visibility = Visibility.Visible;
            GroupsTitle.Text = "New Group";
        }

        private void refreshNewGroupView()
        {
            GroupNameText.Text = "";
            ClrPcker_Background.SelectedColor = (Color)ColorConverter.ConvertFromString("#FFFFFF");

        }

        private void getUserList()
        {
            //Call from users page object
            //userList = db.getUsersFromGroup(selectedGroup.ID);
            //UserList.ItemsSource = userList;
        }

        private void ReturnViewBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            refreshPage();
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
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.Int64)))
                {
                    deselectAllSettingsItems(receivedMessage.getSender());

                    long pageNav = (long)receivedMessage.readMessage();
                    loadPage(pageNav);
                }
                else if(receivedMessage.readMessage().GetType().Equals(typeof(UserInfo)))
                {
                    selectedUser = (UserInfo)receivedMessage.readMessage();

                    EditUserButton.Visibility = Visibility.Visible;
                    //Save user to local variable, show edit and delete user buttons...
                }
                else if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    if (receivedMessage.readMessage().Equals("COLOR_CHANGE"))
                    {
                        SaveColorButton.Visibility = Visibility.Visible;
                    }
                    else if (receivedMessage.readMessage().Equals("NAME_CHANGE_VALID"))
                    {
                        SaveNameButton.Visibility = Visibility.Visible;
                    }
                    else if (receivedMessage.readMessage().Equals("NAME_CHANGE_INVALID"))
                    {
                        SaveNameButton.Visibility = Visibility.Hidden;
                    }
                    else if (receivedMessage.readMessage().Equals("CLOSE_DIALOG"))
                    {
                        Dialog_Box.IsOpen = false;
                    }
                    else if (receivedMessage.readMessage().Equals("PERMISSION_CHANGE"))
                    {
                        SavePermissionsButton.Visibility = Visibility.Visible;
                    }
                    else if (receivedMessage.readMessage().Equals("DELETE_GROUP"))
                    {
                        db.deleteGroup(selectedGroup);
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

        private void setupGroupsOptions()
        {
            objects.Clear();
            addDialogBox();
            GroupsOptions.Children.Clear();
            HalfScreenListItem usersItem = new HalfScreenListItem("Accounts", MaterialDesignThemes.Wpf.PackIconKind.AccountSupervisorCircle, MaterialDesignThemes.Wpf.PackIconKind.AccountSupervisorCircleOutline, "Settings_Page_List_Groups_Page_List_Accounts", "Settings_Page_List_Groups_Page_List_Accounts_Page", "Settings_Page_List_Groups_Page");
            usersItem.message += receiveMessage;
            objects[usersItem.getObjectID()] = usersItem;
            SettingsPageGroupsUsers users = new SettingsPageGroupsUsers(selectedGroup.ID);
            users.message += receiveMessage;
            objects[users.getObjectID()] = users;
            HalfScreenListItem nameItem = new HalfScreenListItem("Name", MaterialDesignThemes.Wpf.PackIconKind.TagText, MaterialDesignThemes.Wpf.PackIconKind.TagTextOutline, "Settings_Page_List_Groups_Page_List_Name", "Settings_Page_List_Groups_Page_List_Name_Page", "Settings_Page_List_Groups_Page");
            nameItem.message += receiveMessage;
            objects[nameItem.getObjectID()] = nameItem;
            SettingsPageGroupsName name = new SettingsPageGroupsName(selectedGroup);
            name.message += receiveMessage;
            objects[name.getObjectID()] = name;
            HalfScreenListItem colorItem = new HalfScreenListItem("Color", MaterialDesignThemes.Wpf.PackIconKind.Color, MaterialDesignThemes.Wpf.PackIconKind.PaintOutline, "Settings_Page_List_Groups_Page_List_Color", "Settings_Page_List_Groups_Page_List_Color_Page", "Settings_Page_List_Groups_Page");
            colorItem.message += receiveMessage;
            objects[colorItem.getObjectID()] = colorItem;
            SettingsPageGroupsColors colors = new SettingsPageGroupsColors(selectedGroup);
            colors.message += receiveMessage;
            objects[colors.getObjectID()] = colors;
            HalfScreenListItem permissionsItem = new HalfScreenListItem("Permissions", MaterialDesignThemes.Wpf.PackIconKind.BookLock, MaterialDesignThemes.Wpf.PackIconKind.BookLockOpen, "Settings_Page_List_Groups_Page_List_Permissions", "Settings_Page_List_Groups_Page_List_Permissions_Page", "Settings_Page_List_Groups_Page");
            permissionsItem.message += receiveMessage;
            objects[permissionsItem.getObjectID()] = permissionsItem;
            SettingsPageGroupsPermissions permissions = new SettingsPageGroupsPermissions(selectedGroup);
            permissions.message += receiveMessage;
            objects[permissions.getObjectID()] = permissions;

            GroupsOptions.Children.Add(objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Accounts"]] as HalfScreenListItem);
            GroupsOptions.Children.Add(objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Name"]] as HalfScreenListItem);
            GroupsOptions.Children.Add(objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Color"]] as HalfScreenListItem);

            if (!selectedGroup.isAdmin())
            {
                GroupsOptions.Children.Add(objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Permissions"]] as HalfScreenListItem);
            }
        }

        private void loadPage(long pageID)
        {
            hideAllButtons();

            GroupsSettingsPage.Visibility = Visibility.Visible;

            Pages temp = (Pages)objects[pageID];

            temp.refreshPage();

            GroupsSettingsPage.NavigationService.Navigate(objects[pageID]);
        }

        private void deselectAllSettingsItems(long selectedItem)
        {
            foreach (HalfScreenListItem item in GroupsOptions.Children)
            {
                if (item.getObjectID() != selectedItem)
                {
                    item.deselectItem();
                }
                else
                {
                    item.itemSelected();
                }
            }
        }

        private void UserShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SaveColorButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Color_Page"]] as SettingsPageGroupsColors).receiveMessage(this, createMessage("CHANGE_COLOR", "Settings_Page_List_Groups_Page_List_Color_Page"));
            SaveColorButton.Visibility = Visibility.Hidden;
            GroupsTitle.Foreground = (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Color_Page"]] as SettingsPageGroupsColors).getColor();

            //If currently logged in, update useraccount page and icon
            if (User.getAssignedGroupID() == selectedGroup.ID)
            {
                sendMessage(createMessage("GROUP_UPDATE", "Title_Bar_Account"));
                sendMessage(createMessage("GROUP_COLOR_UPDATE", "Account_Popup"));        
            }
        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Name_Page"]] as SettingsPageGroupsName).receiveMessage(this, createMessage("CHANGE_NAME", "Settings_Page_List_Groups_Page_List_Name_Page"));
            SaveNameButton.Visibility = Visibility.Hidden;
            GroupsTitle.Text = (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Name_Page"]] as SettingsPageGroupsName).getNewName();
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            setupNewGroupView();
        }

        private void SaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            db.addGroup(GroupNameText.Text, ClrPcker_Background.SelectedColor.ToString());

            refreshPage();
        }

        private void GroupNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (db.checkGroupNameExists(GroupNameText.Text))
            {
                GroupNameErrorText.Text = "Group name already exists!";
                SaveGroupButton.Visibility = Visibility.Hidden;
            }
            else if (GroupNameText.Text == "")
            {
                SaveGroupButton.Visibility = Visibility.Hidden;
            }
            else
            {
                GroupNameErrorText.Text = "";
                SaveGroupButton.Visibility = Visibility.Visible;
            }
        }

        private void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_Remove_Group"]] as GroupRemove).sentGroup(selectedGroup);
            Dialog_Box.IsOpen = true;
        }

        private void SavePermissionsButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Permissions_Page"]] as SettingsPageGroupsPermissions).receiveMessage(this, createMessage("UPDATE_PERMISSIONS", "Settings_Page_List_Groups_Page_List_Permissions_Page"));
            SavePermissionsButton.Visibility = Visibility.Hidden;
            loadPage(ObjectIDManager.objectIDs["Settings_Page_List_Groups_Page_List_Permissions_Page"]);
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            sendMessage(createMessage(selectedUser, "Settings_Page"));
        }
    }
}
