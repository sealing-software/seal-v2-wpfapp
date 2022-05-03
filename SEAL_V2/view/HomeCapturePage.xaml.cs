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
    /// Interaction logic for HomeCapturePage.xaml
    /// </summary>
    public partial class HomeCapturePage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Home_Capture_View";
        private String name = "Captures";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;
        public List<Capture> captureList = new List<Capture>();

        public HomeCapturePage()
        {
            InitializeComponent();

            loadObjectID();
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
            loadMainView();
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

        private void hideAllViews()
        {
            NoDirectoryView.Visibility = Visibility.Hidden;
            NoCaptureView.Visibility = Visibility.Hidden;
            CaptureSelectedView.Visibility = Visibility.Hidden;
        }

        private void loadMainView()
        {
            hideAllViews();

            if (CurrentSystem.machineID < 1)
            {
                NoDirectoryView.Visibility = Visibility.Visible;
            }
            else if (!db.captureExists(CurrentSystem.machineID))
            {
                NoCaptureView.Visibility = Visibility.Visible;
            }
            else
            {
                CaptureSelectedView.Visibility = Visibility.Visible;
                loadCaptureList();
            }
        }

        public void loadCaptureList()
        {
            //CaptureList.Children.Clear();
            CaptureGrid.ItemsSource = db.getCaptures(CurrentSystem.machineID);
            
            //foreach (Capture item in captureList)
            //{
                //CaptureList.Children.Add(new CaptureItem(item));
            //}

            //if (StaticCapture.id > 0)
            //{
                //foreach(CaptureItem item in CaptureList.Children)
                //{
                    //if (item.captureID == StaticCapture.id)
                    //{
                        //item.setSelected();
                        //selectedCaptureName.Text = StaticCapture.name;
                    //}
                //}
            //}
        }

        private void CaptureGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((CaptureGrid.SelectedItem as Capture) != null)
            {
                StaticCapture.changeCapture((Capture)CaptureGrid.SelectedItem);

                sendMessage(createMessage("CAPTURE_UPDATE", "Home_Page"));
                sendMessage(createMessage("REFRESH", "Home_Page"));
            }
        }
    }
}
