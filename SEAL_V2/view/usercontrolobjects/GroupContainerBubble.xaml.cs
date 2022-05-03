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

namespace SEAL_V2.view.usercontrolobjects
{
    /// <summary>
    /// Interaction logic for GroupContainerBubble.xaml
    /// </summary>
    public partial class GroupContainerBubble : UserControl, MessageProtocol
    {
        private long objectID = 0;
        public event EventHandler<StatusMessage> message;
        private String groupName;
        private Group passedGroup;
        private bool mouseOver = false;
        private bool descriptionAdded = false;
        private String description = "";
        private bool wasEdited = false;
        private bool nullGroup = false;

        public GroupContainerBubble(Group passedGroup)
        {
            InitializeComponent();

            if (passedGroup == null)
            {
                //If sequence is passed in with reference to old/removed group
                this.passedGroup = passedGroup;
                groupName = "GROUP REMOVED";
                groupnametext.Text = groupName;
                groupnametext.Foreground = Brushes.Orange;
                highlightborder.Fill = Brushes.Orange;
                nullGroup = true;
            }
            else
            {
                this.passedGroup = passedGroup;
                groupName = passedGroup.name;
                setupBubble();
            }
        }

        private void setupBubble()
        {
            groupnametext.Text = groupName;
            groupnametext.Foreground = passedGroup.brushColor;
            highlightborder.Fill = passedGroup.brushColor;
            descriptionaddedcircle.Foreground = passedGroup.brushColor;
        }

        private void maingrid_MouseEnter(object sender, MouseEventArgs e)
        {
            maingrid.Opacity = 0.5;
            mouseOver = true;
        }

        private void maingrid_MouseLeave(object sender, MouseEventArgs e)
        {
            maingrid.Opacity = 1;
            mouseOver = false;
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

        private void maingrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        public Group getGroup()
        {
            return passedGroup;
        }

        public int getGroupID()
        {
            return passedGroup.ID;
        }

        public bool isMouseOver()
        {
            return mouseOver;
        }

        public void addDescription(String description)
        {   
            if (description.Equals("") || description.Equals("Enter task here"))
            {
                this.description = description;
                descriptionAdded = false;
            }
            else
            {
                this.description = description;
                descriptionAdded = true;
            }

            descriptionCircle();
        }

        public String getDescription()
        {
            return description;
        }

        private void descriptionCircle()
        {
            if (descriptionAdded)
            {
                descriptionaddedcircle.Visibility = Visibility.Visible;
            }
            else
            {
                descriptionaddedcircle.Visibility = Visibility.Hidden;
            }

        }

        private void maingrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        public void edited()
        {
            wasEdited = true;
        }

        public bool checkEdited()
        {
            return wasEdited;
        }

        public bool containsNull()
        {
            return nullGroup;
        }
    }
}
