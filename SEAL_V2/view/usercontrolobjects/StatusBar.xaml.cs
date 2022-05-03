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
using System.Windows.Threading;
using SEAL_V2.model;

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar : UserControl, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Status_Bar";
        public event EventHandler<StatusMessage> message;

        //THIS IS TOP LEFT OF APPLICATION BAR
        public StatusBar()
        {
            InitializeComponent();

            loadObjectID();

            loadTimeAndDate();

            addAdditionalStatusIcons();
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

        private void loadTimeAndDate()
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateAndTime.Text = DateTime.Now.ToString("MMMM d yyyy hh:mm tt");
        }

        private void addAdditionalStatusIcons()
        {
            addDbStatus();
        }

        //Need better way to ID status icons....
        private void addDbStatus()
        {
            DatabaseStatus dbStatus = new DatabaseStatus();
            statuspanel.Children.Add(dbStatus);
        }

        //Decides which status item to send message to
      //  public void receiveMessage(StatusMessage sentMessage)
        //{
            //long recipient = sentMessage.getAddress();

            //THIS SHOULD BE REDONE AS A DICTIONARY!!
           // if (recipient == 1)
          //  {
           //     DatabaseStatus dbIcon = (DatabaseStatus)statuspanel.Children[recipient];
           //     dbIcon.recieveMessage(sentMessage);
           // }
      //  }

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

            }
            else
            {

            }
        }
    }
}
