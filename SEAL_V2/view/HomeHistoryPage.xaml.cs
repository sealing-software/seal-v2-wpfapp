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
using SEAL_V2.db;
using SEAL_V2.model;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for HomeHistoryPage.xaml
    /// </summary>
    public partial class HomeHistoryPage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Home_History_View";
        private String name = "History";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;

        public HomeHistoryPage()
        {
            InitializeComponent();

            loadObjectID();

            //dummyLoad();
        }

        private void dummyLoad()
        {
            SEAL_V2.model.Action.save("TEST");
            SEAL_V2.model.Action.save("TEST");
            SEAL_V2.model.Action.save("TEST");
            SEAL_V2.model.Action.save("TEST");
            SEAL_V2.model.Action.save("TEST");
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
            loadView();
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

        //FIX RECEIVE MESSAGE TO INCLUDE SEND DOWN OR RELAY
        public void receiveMessage(object sender, StatusMessage receivedMessage)
        {
            if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {
                sendMessage(receivedMessage);
            }
            else if (this.objectID == receivedMessage.getAddress())
            {

            }
        }

        private void hideViews()
        {
            NoHistoryView.Visibility = Visibility.Hidden;
            DataView.Visibility = Visibility.Hidden;
        }

        private void loadView()
        {
            hideViews();

            if (db.getHistory().Count < 1)
            {
                NoHistoryView.Visibility = Visibility.Visible;
            }
            else
            {
                loadData();
            }
        }

        private void loadData()
        {
            DataView.Visibility = Visibility.Visible;
            if (StaticCapture.id > 0)
            {
                HistoryGrid.ItemsSource = SEAL_V2.model.Action.getHistory(StaticCapture.id);
            }
            else
            {
                HistoryGrid.ItemsSource = SEAL_V2.model.Action.getHistory();
            }
        }
    }
}
