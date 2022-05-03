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
    /// Interaction logic for SettingsPageGroupsColors.xaml
    /// </summary>
    public partial class SettingsPageGroupsColors : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Groups_Page_List_Color_Page";
        private String name;
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db;
        public Group selectedGroup;
        private SolidColorBrush currentColor; 

        public SettingsPageGroupsColors(Group selectedGroup)
        {
            InitializeComponent();
            loadDB();
            loadObjectID();
            this.selectedGroup = selectedGroup;
            setInitialColor();
        }

        private void loadDB()
        {
            db = DatabaseInterface.Instance;
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
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
            setInitialColor();
        }

        private void setInitialColor()
        {
            ClrPcker_Background.SelectedColor = (Color)ColorConverter.ConvertFromString(selectedGroup.colorString);
        }

        public long getObjectID()
        {
            return objectID;
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (selectedGroup != null && ClrPcker_Background.SelectedColor != (Color)ColorConverter.ConvertFromString(selectedGroup.colorString))
            {
                sendMessage(createMessage("COLOR_CHANGE", "Settings_Page_List_Groups_Page"));
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
            if (this.objectID == receivedMessage.getAddress())
            {
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    if (receivedMessage.readMessage().Equals("CHANGE_COLOR"))
                    {
                        setColor();
                    }
                }
            }
            else if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {

            }
            else
            {

            }
        }

        public void setColor()
        {
            db.updateGroupColor(selectedGroup.ID, ClrPcker_Background.SelectedColor.ToString());
            currentColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ClrPcker_Background.SelectedColor.ToString()));
        }

        public SolidColorBrush getColor()
        {
            return currentColor;
        }
    }
}
