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

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for PermissionsListItem.xaml
    /// </summary>
    public partial class PermissionsListItem : UserControl, MessageProtocol
    {
        public String permissionName { get; set; }
        public long objectID;
        private bool permitted = false;
        private long permissionID;
        private Group selectedGroup;
        private DatabaseInterface db = DatabaseInterface.Instance;
        private List<PermissionsListItem> subPermissions = new List<PermissionsListItem>();
        public event EventHandler<StatusMessage> message;
        public PermissionsListItem(String permissionName, bool permitted, Group group)
        {
            InitializeComponent();

            this.permissionName = permissionName;
            this.permitted = permitted;
            this.selectedGroup = group;

            permissionID = ObjectIDManager.objectIDs[permissionName];
            setupView();
        }

        private void setupView()
        {
            Description.Text = permissionName;

            if (permitted)
            {
                Check.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckBox;
            }
        }

        private void check_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Check.Kind == MaterialDesignThemes.Wpf.PackIconKind.CheckBox)
            {
                revokePermission();
            }
            else
            {
                setPermission();
            }        
        }

        private void Description_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemExpander.Visibility == Visibility.Visible)
            {
                if (ItemExpander.IsExpanded == false)
                {
                    ItemExpander.IsExpanded = true;
                }
                else
                {
                    ItemExpander.IsExpanded = false;
                }
            }
            else
            {
                check_MouseLeftButtonUp(this, e);
            }

        }

        private void Description_MouseEnter(object sender, MouseEventArgs e)
        {
            Description.Opacity = 0.5;
            Check.Opacity = 0.5;
        }

        private void Description_MouseLeave(object sender, MouseEventArgs e)
        {
            Description.Opacity = 1;
            Check.Opacity = 1;
        }

        public void addSubPermission(PermissionsListItem item)
        {
            ItemExpander.Visibility = Visibility.Visible;
            SubPermissionsList.Children.Add(item);
        }

        public long getPermissionID()
        {
            return permissionID;
        }

        public void setPermission()
        {
            permitted = true;
            Check.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckBox;

            sendMessage(createMessage("PERMISSION_CHANGE", "Settings_Page_List_Groups_Page"));

            if (SubPermissionsList.Children.Count > 0)
            {
                foreach (PermissionsListItem item in SubPermissionsList.Children)
                {
                    item.setPermission();
                }
            }
        }

        public void revokePermission()
        {
            permitted = false;
            Check.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckboxBlankOutline;

            if (SubPermissionsList.Children.Count > 0)
            {
                foreach (PermissionsListItem item in SubPermissionsList.Children)
                {
                    item.revokePermission();
                }
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

        public void receiveMessage(object sender, StatusMessage receivedMessage)
        {
            //NOT NEEDED
        }

        public void updatePermission()
        {
            if (permitted)
            {
                db.addPermission(selectedGroup.ID, permissionName);
            }
            else
            {
                db.removePermission(selectedGroup.ID, permissionName);
            }

            if (SubPermissionsList.Children.Count > 0)
            {
                foreach (PermissionsListItem item in SubPermissionsList.Children)
                {
                    item.updatePermission();
                }
            }
        }
    }
}
