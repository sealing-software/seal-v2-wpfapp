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
    /// Interaction logic for AccountPopupLicense.xaml
    /// </summary>
    public partial class AccountPopupLicense : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Account_Popup_License_Page";
        private String name = "License Page";
        private DatabaseInterface db = DatabaseInterface.Instance;
        public event EventHandler<StatusMessage> message;


        public AccountPopupLicense()
        {
            InitializeComponent();

            loadObjectID();

            loadLicenseDetails();
        }

        private void loadLicenseDetails()
        {
            license_name.Text = License.getLicenseName();
            license_status.Text = License.getLicenseStatus();
            license_status.Foreground = License.getLicenseStatusColor();
            status_icon.Kind = License.getIcon();
            status_icon.Foreground = License.getLicenseStatusColor();
            association_name.Text = License.getAssociationName();
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

        }

        public String getName()
        {
            return name;
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
