using SEAL_V2.db;
using SEAL_V2.model;
using SEAL_V2.view.usercontrolobjects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page";
        private DatabaseInterface db;
        public String name { get; set; }
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;
        public SettingsPage()
        {
            InitializeComponent();

            loadObjectID();

            setupPageProperties();
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

        private void setupPageProperties()
        {
            name = "Settings";

            SettingsItemPage.Visibility = Visibility.Hidden;

            linkDB();

            loadList();
        }

        public void refreshPage()
        {
            setupPageProperties();
        }


        private void linkDB()
        {
            db = DatabaseInterface.Instance;
        }

        public String getName()
        {
            return name;
        }

        private void loadList()
        {
            //VALIDATE GROUP OF USER!!!!

            //CREATE DICTIONARY OF LIST ITEMS....

            SettingsOptionsList.Children.Clear();
            objects.Clear();

            Dictionary<SettingsListItem, Pages> tempDict = new Dictionary<SettingsListItem, Pages>();

            SettingsListItem users = new SettingsListItem("Users", MaterialDesignThemes.Wpf.PackIconKind.Account, MaterialDesignThemes.Wpf.PackIconKind.AccountOutline, "Settings_Page_List_Users", "Settings_Page_List_Users_Page", "Settings_Page");
            tempDict[users] = new SettingsPageUsers();
            SettingsListItem groups = new SettingsListItem("Groups", MaterialDesignThemes.Wpf.PackIconKind.AccountsGroup, MaterialDesignThemes.Wpf.PackIconKind.AccountGroupOutline, "Settings_Page_List_Groups", "Settings_Page_List_Groups_Page", "Settings_Page");
            tempDict[groups] = new SettingsPageGroups();
            SettingsListItem datastructure = new SettingsListItem("Data Trees", MaterialDesignThemes.Wpf.PackIconKind.FileTree, MaterialDesignThemes.Wpf.PackIconKind.FileTreeOutline, "Settings_Page_List_Data_Structure", "Settings_Page_List_Data_Structure_Page", "Settings_Page");
            tempDict[datastructure] = new SettingsPageFileStructure();
            SettingsListItem themes = new SettingsListItem("Color Themes", MaterialDesignThemes.Wpf.PackIconKind.Color, MaterialDesignThemes.Wpf.PackIconKind.PaintOutline, "Settings_Page_List_Themes", "Settings_Page_List_Themes_Page", "Settings_Page");
            //tempDict[themes] = new SettingsPageUsers();
            SettingsListItem sequences = new SettingsListItem("Sequences", MaterialDesignThemes.Wpf.PackIconKind.ViewSequential, MaterialDesignThemes.Wpf.PackIconKind.ViewSequentialOutline, "Settings_Page_List_Sequence", "Settings_Page_List_Sequence_Page", "Settings_Page");
            tempDict[sequences] = new SettingsPageSequence();

            //SettingsListItem sequences = new SettingsListItem("Sequence", MaterialDesignThemes.Wpf.PackIconKind.ViewSequential, MaterialDesignThemes.Wpf.PackIconKind.ViewSequentialOutline, "Settings_Page_List_Sequence", "Settings_Page_List_Sequence_Page", "Settings_Page");
            //tempDict[sequences] = new SettingsPageSequence();


            //Iterate through temporary dictionary. Only adds authorized items. Load pages
            foreach (var dictItem in tempDict)
            {
                Console.WriteLine(dictItem.Key);
                dictItem.Key.message += receiveMessage;
                (dictItem.Value as MessageProtocol).message += receiveMessage;
                SettingsOptionsList.Children.Add(dictItem.Key);
                objects[dictItem.Key.getObjectID()] = dictItem.Key;
                objects[ObjectIDManager.objectIDs[dictItem.Value.getObjectName()]] = dictItem.Value;

                //Unused for now....
                if (User.userAuthorized(dictItem.Key.getObjectName()))
                {

                }
            }
        }

        private void loadPage(long pageID)
        {
            SettingsItemPage.Visibility = Visibility.Visible;

            Pages temp = objects[pageID] as Pages;

            temp.refreshPage();

            SettingsItemPage.NavigationService.Navigate(temp);
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
            if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {
                sendMessage(receivedMessage);
            }
            else
            {
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.Int64)))
                {
                    deselectAllSettingsItems(receivedMessage.getSender());

                    long pageNav = (long)receivedMessage.readMessage();
                    loadPage(pageNav);
                }
                else if (receivedMessage.readMessage().GetType().Equals(typeof(UserInfo)))
                {
                    (objects[ObjectIDManager.objectIDs["Settings_Page_List_Users"]] as SettingsListItem).shortcut(receivedMessage.readMessage() as UserInfo);

                    SettingsItemPage.Visibility = Visibility.Visible;

                    SettingsPageUsers temp = objects[ObjectIDManager.objectIDs["Settings_Page_List_Users_Page"]] as SettingsPageUsers;

                    temp.refreshPage();

                    temp.shortcut(receivedMessage.readMessage() as UserInfo);

                    SettingsItemPage.NavigationService.Navigate(temp);
                }
            }
        }

        //Resets settings options list
        private void deselectAllSettingsItems(long selectedItem)
        {
            foreach (SettingsListItem item in SettingsOptionsList.Children)
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
    }
}
