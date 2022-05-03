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
using System.Windows.Media;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for AccountPopupAccount.xaml
    /// </summary>
    public partial class AccountPopupAccount : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Account_Popup_Account_Page";
        private String name = "Account Settings";
        private DatabaseInterface db = DatabaseInterface.Instance;
        public event EventHandler<StatusMessage> message;


        public AccountPopupAccount()
        {
            InitializeComponent();

            loadObjectID();

            loadInfo();
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

        public void refreshPage()
        {
            loadInfo();
        }

        public String getName()
        {
            return name;
        }

        public void loadInfo()
        {
            username.Text = User.getUserName();
            displayname.Text = User.getDisplayName();
            group.Text = User.getAssignedGroupText();
            // SolidColorBrush brushColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(db.getGroupHexColor(User.getAssignedGroupID())));
            if (User.isUserLoggedIn())
            {
                String colorString = db.getGroupHexColor(User.getAssignedGroupID());
                SolidColorBrush groupColor = (SolidColorBrush)new BrushConverter().ConvertFromString(colorString);
                group.Foreground = groupColor;
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

    }
}
