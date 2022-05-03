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
using SEAL_V2.view.usercontrolobjects;
using SEAL_V2.db;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for SettingsPageGroupsPermissions.xaml
    /// </summary>
    public partial class SettingsPageGroupsPermissions : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page_List_Permissions_Page";
        private String name = "Permissions";
        public event EventHandler<StatusMessage> message;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        private Group selectedGroup;
        private DatabaseInterface db = DatabaseInterface.Instance;

        public SettingsPageGroupsPermissions(Group sentGroup)
        {
            InitializeComponent();

            loadObjectID();

            selectedGroup = sentGroup;

            loadPermissions();
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
                    if (receivedMessage.readMessage().Equals("UPDATE_PERMISSIONS"))
                    {
                        updatePermissions();
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

        public void loadPermissions()
        {
            List<long> sortedIDLIst = ObjectIDManager.getSortedIDs();

            Dictionary<long, String> reverseDictionary = ObjectIDManager.getIDToName();

            Stack<PermissionsListItem> listStack = new Stack<PermissionsListItem>();

            foreach (long id in sortedIDLIst)
            {
                if (listStack.Count == 0)
                {
                    PermissionsListItem tempPermission = new PermissionsListItem(reverseDictionary[id], db.checkGroupObjectPermission(selectedGroup.ID, reverseDictionary[id]), selectedGroup);
                    tempPermission.message += receiveMessage;
                    listStack.Push(tempPermission);
                }
                else
                {
                    if (getDigitPlaces(listStack.Peek().getPermissionID() - id) <= getZeroes(listStack.Peek().getPermissionID()))
                    {
                        PermissionsListItem tempPermission = new PermissionsListItem(reverseDictionary[id], db.checkGroupObjectPermission(selectedGroup.ID, reverseDictionary[id]), selectedGroup);
                        tempPermission.message += receiveMessage;
                        listStack.Push(tempPermission);
                    }
                    else
                    {
                        while (listStack.Count >= 1 && getDigitPlaces(listStack.Peek().getPermissionID() - id) > getZeroes(listStack.Peek().getPermissionID()))
                        {
                            PermissionsListItem child = listStack.Pop();

                            if (listStack.Count == 0)
                            {
                                PermissionsList.Children.Add(child);
                            }
                            else
                            {
                                PermissionsListItem parent = listStack.Pop();
                                parent.addSubPermission(child);
                                
                                listStack.Push(parent);
                            }
                        }
                        PermissionsListItem tempPermission = new PermissionsListItem(reverseDictionary[id], db.checkGroupObjectPermission(selectedGroup.ID, reverseDictionary[id]), selectedGroup);
                        tempPermission.message += receiveMessage;
                        listStack.Push(tempPermission);
                    }             
                }
            }
        }

        private long getZeroes(long currentID)
        {
            String id = currentID.ToString();
            int zeroes = 0;

            for (int i = id.Length - 1; i >= 0; i--)
            {
                if (id[i].Equals('0'))
                {
                    zeroes++;
                }
                else
                {
                    i = -1;
                }
            }

            return zeroes;
        }

        //Get available digits
        private static long getDigitPlaces(long difference)
        {
            String differenceString = Math.Abs(difference).ToString();

            return differenceString.Length;
        }

        public void updatePermissions()
        {
            foreach (PermissionsListItem permission in PermissionsList.Children)
            {
                permission.updatePermission();
            }
        }
    }
}
