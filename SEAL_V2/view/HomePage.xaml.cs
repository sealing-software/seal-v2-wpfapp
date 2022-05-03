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

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Home_Page";
        public String name { get; set; }
        public event EventHandler<StatusMessage> message;
        private Dictionary<long, object> objects = new Dictionary<long, object>();


        public HomePage()
        {
            InitializeComponent();

            loadObjectID();

            setupPageProperties();

            loadSubViews();
        }

        public void loadObjectID()
        {
            objectID = ObjectIDManager.objectIDs[objectName];
        }

        private void setupPageProperties()
        {
            name = "Home";
        }

        public void refreshPage()
        {
            hideFullSystemView();
            (objects[ObjectIDManager.objectIDs["Home_System_View"]] as Pages).refreshPage();
            (objects[ObjectIDManager.objectIDs["Home_Sequence_View"]] as Pages).refreshPage();
            (objects[ObjectIDManager.objectIDs["Home_Capture_View"]] as Pages).refreshPage();
            (objects[ObjectIDManager.objectIDs["Home_History_View"]] as Pages).refreshPage();
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
                if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    String messageString = (String)receivedMessage.readMessage();

                    if (messageString.Equals("RETURN_HOME"))
                    {
                        hideFullSystemView();
                    }
                    else if (messageString.Equals("REFRESH"))
                    {
                        refreshOtherPages();
                    }
                    else if (messageString.Equals("CAPTURE_UPDATE"))
                    {
                        CurrentCycle.Text = "| " + StaticCapture.name;
                    }
                }
            }
        }

        private void loadSubViews()
        {
            loadSystemView();
            loadSequenceView();
            loadCaptureView();
            loadHistoryView();
        }

        private void loadSystemView()
        {
            HomeSystemPage homeSystem = new HomeSystemPage();
            objects[homeSystem.getObjectID()] = homeSystem;
            homeSystem.message += receiveMessage;
            SystemCardFrame.NavigationService.Navigate(homeSystem);
        }

        private void loadSequenceView()
        {
            HomeSequencePage homeSequence = new HomeSequencePage();
            objects[homeSequence.getObjectID()] = homeSequence;
            homeSequence.message += receiveMessage;
            SequenceCardFrame.NavigationService.Navigate(homeSequence);
        }

        private void loadCaptureView()
        {
            HomeCapturePage homeCapture = new HomeCapturePage();
            objects[homeCapture.getObjectID()] = homeCapture;
            homeCapture.message += receiveMessage;
            CaptureCardFrame.NavigationService.Navigate(homeCapture);
        }

        private void loadHistoryView()
        {
            HomeHistoryPage homeHistory = new HomeHistoryPage();
            objects[homeHistory.getObjectID()] = homeHistory;
            homeHistory.message += receiveMessage;
            HistoryCardFrame.NavigationService.Navigate(homeHistory);
        }

        private void refreshEvent(object sender, EventArgs e)
        {
            refreshPage();
        }

        private void refreshOtherPages()
        {
            (objects[ObjectIDManager.objectIDs["Home_Sequence_View"]] as Pages).refreshPage();
            (objects[ObjectIDManager.objectIDs["Home_Capture_View"]] as Pages).refreshPage();
            (objects[ObjectIDManager.objectIDs["Home_History_View"]] as Pages).refreshPage();
        }

        private void SystemCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            //hideSubViews();
            //showFullSystemView();
        }

        private void showFullSystemView()
        {
            SystemFullViewCard.Visibility = Visibility.Visible;

            SystemFullViewCardFrame.NavigationService.Navigate(new FullSystemPage());

            sendMessage(createMessage("SYSTEM_FULL_VIEW", "Main_Window"));
        }

        private void hideFullSystemView()
        {
            SystemFullViewCard.Visibility = Visibility.Collapsed;
        }
    }
}
