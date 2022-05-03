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
using System.Windows.Media.Animation;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for SettingsPageSequence.xaml
    /// </summary>
    public partial class SettingsPageSequence : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Sequence_Page";
        private String name = "Sequences";
        DatabaseInterface db = DatabaseInterface.Instance;
        private List<GroupContainerBubble> bubbleList = new List<GroupContainerBubble>();
        public event EventHandler<StatusMessage> message;
        public Sequence selectedSequence;
        private DataGridRow selectedRow;
        private GroupContainerBubble selectedDescriptionBubble;
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        private ToggleItem newToggle;
        private ToggleItem editToggle;

        public SettingsPageSequence()
        {
            InitializeComponent();

            loadObjectID();

            addDialogBox();
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
            showMainView();
            Dialog_Box.IsOpen = false;
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
            if (MessageRelay.sendUp(this.objectID, receivedMessage.getAddress()))
            {
                sendMessage(receivedMessage);
            }
            else
            {
                if (receivedMessage.readMessage().GetType().Equals(typeof(GroupContainerBubble)))
                {
                    //GroupContainerBubble newBubble = new GroupContainerBubble((receivedMessage.readMessage() as GroupContainerBubble).getGroup());
                    
                    //newSequenceList.Children.Add(newBubble);
                }
                else if (receivedMessage.readMessage().GetType().Equals(typeof(System.String)))
                {
                    if (receivedMessage.readMessage().Equals("CLOSE_DIALOG"))
                    {
                        Dialog_Box.IsOpen = false;
                    }
                    else if (receivedMessage.readMessage().Equals("DELETE_SEQUENCE"))
                    {
                        Dialog_Box.IsOpen = false;
                        db.deleteSequence(selectedSequence);
                        showMainView();
                    }
                }
            }
        }

        private void addDialogBox()
        {
            SequenceRemove remove = new SequenceRemove();
            remove.message += receiveMessage;
            objects[remove.getObjectID()] = remove;
            Dialog_Content.Children.Add(objects[remove.getObjectID()] as SequenceRemove);
        }

        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow rowHover = (DataGridRow)sender;

            if (!rowHover.IsSelected)
            {
                rowHover.Opacity = 0.5;
            }
        }

        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGridRow rowHover = (DataGridRow)sender;

            if (!rowHover.IsSelected)
            {
                rowHover.Opacity = 1;
            }
        }

        private void DataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedRow != null && (DataGridRow)sender != selectedRow)
            {
                selectedRow.IsSelected = false;

                DataGridRow_MouseLeave(selectedRow, null);
            }

            EditSequenceButton.Visibility = Visibility.Visible;
            DeleteSequenceButton.Visibility = Visibility.Visible;

            selectedRow = (DataGridRow)sender;

            selectedSequence = (Sequence)sequenceList.SelectedItem;
        }

        private void AddSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            showAddView();
        }

        private void hideAllButtons()
        {
            AddSequenceButton.Visibility = Visibility.Hidden;
            SaveNewSequenceButton.Visibility = Visibility.Hidden;
            EditSequenceButton.Visibility = Visibility.Hidden;
            SaveUpdateSequenceButton.Visibility = Visibility.Hidden;
            DeleteSequenceButton.Visibility = Visibility.Hidden;
        }

        private void hideAllViews()
        {
            sequenceList.Visibility = Visibility.Hidden;
            newsequenceview.Visibility = Visibility.Hidden;
            editsequenceview.Visibility = Visibility.Hidden;
        }

        private void showMainView()
        {
            hideAllButtons();
            hideAllViews();
            AddSequenceButton.Visibility = Visibility.Visible;
            sequenceList.Visibility = Visibility.Visible;
            sequencetitle.Text = "Sequences";
            ReturnViewBox.Visibility = Visibility.Hidden;
            selectedSequence = null;
            selectedRow = null;
            reloadSequenceList();
        }

        private void showAddView()
        {
            hideAllButtons();
            hideAllViews();
            hideDescriptionEditView();
            selectedDescriptionBubble = null;
            newsequenceview.Visibility = Visibility.Visible;
            ReturnViewBox.Visibility = Visibility.Visible;
            sequencetitle.Text = "Create Sequence";
            loadGroupList();
            newSequenceList.Children.Clear();
            sequencenametext.Text = "Enter name here";
            sequencedescription.Text = "Enter description here";
            sequencenametext.Foreground = Brushes.Gray;
            sequencedescription.Foreground = Brushes.Gray;
            stepsqty.Text = "0";
            newToggle = new ToggleItem("Lock system(s)", false);
            systemlocktoggle.Children.Add(newToggle);
            toggledescription.Text = "Enabling this option will require the same system(s) to be used throughout the sequence.";
        }

        private void showEditView()
        {
            hideAllButtons();
            hideAllViews();
            hideEditDescriptionEditView();
            selectedDescriptionBubble = null;
            editsequenceview.Visibility = Visibility.Visible;
            ReturnViewBox.Visibility = Visibility.Visible;
            editSequenceList.Children.Clear();
            sequencetitle.Text = "Edit " + selectedSequence.sequenceName + " Sequence";
            editsequencenametext.Text = selectedSequence.sequenceName;
            editsequencedescription.Text = selectedSequence.getDescription();
            loadGroupList();
            loadSequence();
            updateEditCount();
            SaveUpdateSequenceButton.Visibility = Visibility.Visible;

            editToggle = new ToggleItem("Lock system(s)", selectedSequence.isSystemLock());
            systemlockedittoggle.Children.Add(editToggle);

            toggleeditdescription.Text = "Enabling this option will require the same system(s) to be used throughout the sequence.";
        }

        private void updateEditCount()
        {
            editstepsqty.Text = editSequenceList.Children.Count.ToString();
            SaveUpdateSequenceButton.Visibility = Visibility.Visible;
        }

        private void ReturnViewBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showMainView();
        }

        private void SequenceReturnTitle_MouseEnter(object sender, MouseEventArgs e)
        {
            SequenceReturnTitle.Opacity = 0.5;
        }

        private void SequenceReturnTitle_MouseLeave(object sender, MouseEventArgs e)
        {
            SequenceReturnTitle.Opacity = 1;
        }

        private void loadGroupList()
        {
            bubbleList.Clear();
            groupList.Children.Clear();
            editgroupList.Children.Clear();

            List<Group> groups = db.getGroups();

            foreach (Group entry in groups)
            {
                //Need to link messaging protocol...
                GroupContainerBubble bubble = new GroupContainerBubble(entry);
                GroupContainerBubble bubble1 = new GroupContainerBubble(entry);
                //GroupContainerBubble bubble1 = new GroupContainerBubble(entry);
                bubble.message += receiveMessage;
                bubbleList.Add(bubble);
                groupList.Children.Add(bubble);
                editgroupList.Children.Add(bubble1);
            }
        }

        private void newSequenceList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            List<GroupContainerBubble> tempList = new List<GroupContainerBubble>();

            for (int i = 0; i < newSequenceList.Children.Count; i++)
            {
                if (!(newSequenceList.Children[i] as GroupContainerBubble).isMouseOver())
                {
                    tempList.Add(newSequenceList.Children[i] as GroupContainerBubble);
                }
                else
                {
                    if((newSequenceList.Children[i] as GroupContainerBubble) == selectedDescriptionBubble)
                    {
                        hideDescriptionEditView();
                    }
                }
            }

            newSequenceList.Children.Clear();

            foreach (GroupContainerBubble bubble in tempList)
            {
                newSequenceList.Children.Add(bubble);
            }

            updateCurrentCount();
        }

        private void newSequenceList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            fortextboxfix.Focus();

            for (int i = 0; i < newSequenceList.Children.Count; i++)
            {
                if ((newSequenceList.Children[i] as GroupContainerBubble).isMouseOver())
                {
                    selectedDescriptionBubble = (newSequenceList.Children[i] as GroupContainerBubble);
                }
            }

            groupdescription.Visibility = Visibility.Visible;

            showDescriptionEditView();

            updateCurrentCount();
        }

        private void showDescriptionEditView()
        {
            groupseperator.Visibility = Visibility.Visible;
            selectedgroupeditdescription.Visibility = Visibility.Visible;
            selectedGroupName.Text = selectedDescriptionBubble.getGroup().name;
            selectedGroupName.Foreground = selectedDescriptionBubble.getGroup().brushColor;
            if (!selectedDescriptionBubble.getDescription().Equals(""))
            {
                groupdescription.Text = selectedDescriptionBubble.getDescription();
                groupdescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
            else
            {
                groupdescription.Text = "Enter task here";
                groupdescription.Foreground = Brushes.Gray;
            }
        }

        private void hideDescriptionEditView()
        {
            groupseperator.Visibility = Visibility.Hidden;
            selectedDescriptionBubble = null;
            selectedgroupeditdescription.Visibility = Visibility.Hidden;
            selectedGroupName.Text = "";
            groupdescription.Text = "";
        }

        private void groupList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (GroupContainerBubble bubble in groupList.Children)
            {
                if (bubble.isMouseOver())
                {
                    GroupContainerBubble newBubble = new GroupContainerBubble(bubble.getGroup());

                    newSequenceList.Children.Add(newBubble);
                }
            }

            updateCurrentCount();
        }

        private void updateCurrentCount()
        {
            stepsqty.Text = newSequenceList.Children.Count.ToString();
            checkSaveRequirments();
        }

        private void sequencenametext_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sequencenametext.Text.Equals("Enter name here"))
            {
                sequencenametext.Text = "";
                sequencenametext.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
            
        }

        private void sequencenametext_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sequencenametext.Text.Equals(""))
            {
                sequencenametext.Text = "Enter name here";
                sequencenametext.Foreground = Brushes.Gray;
            }
        }

        private void sequencenametext_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveNewSequenceButton != null)
            {
                checkSaveRequirments();
            }
        }

        private void checkSaveRequirments()
        {
            if (sequencenametext.Text.Equals("Enter name here") || sequencenametext.Text.Equals(""))
            {
                SaveNewSequenceButton.Visibility = Visibility.Hidden;
                nameerror.Text = "";
            }
            else if (db.checkSequenceNameExists(sequencenametext.Text))
            {
                SaveNewSequenceButton.Visibility = Visibility.Hidden;
                nameerror.Text = "This name has already been used.";
            }
            else
            {
                nameerror.Text = "";
                if (newSequenceList.Children.Count >= 2)
                {
                    SaveNewSequenceButton.Visibility = Visibility.Visible;
                }
                else
                {
                    SaveNewSequenceButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SaveNewSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            int lockint = 0;
            if (newToggle.isOn)
            {
                lockint = 1;
            }

            db.saveSequence(sequencenametext.Text, sequencedescription.Text, getSequenceString(), lockint);
            showMainView();
        }

        private String getSequenceString()
        {
            String sequence = "" ;

            for (int i = 0; i < newSequenceList.Children.Count; i++)
            {
                sequence = sequence + ((newSequenceList.Children[i] as GroupContainerBubble).getGroupID()).ToString();
                sequence = sequence + "," + (newSequenceList.Children[i] as GroupContainerBubble).getDescription();
                if (newSequenceList.Children.Count - i > 1)
                {
                    sequence = sequence + ",";
                }
            }

            return sequence;
        }

        private String getSequenceEditString()
        {
            String sequence = "";

            for (int i = 0; i < editSequenceList.Children.Count; i++)
            {
                sequence = sequence + ((editSequenceList.Children[i] as GroupContainerBubble).getGroupID()).ToString();
                sequence = sequence + "," + (editSequenceList.Children[i] as GroupContainerBubble).getDescription();
                if (editSequenceList.Children.Count - i > 1)
                {
                    sequence = sequence + ",";
                }
            }

            return sequence;
        }

        public void reloadSequenceList()
        {
            sequenceList.ItemsSource = db.getSequences();
        }

        private void EditSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            showEditView();
        }

        private void sequencedescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sequencedescription.Text.Equals("Enter description here"))
            {
                sequencedescription.Text = "";
                sequencedescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void sequencedescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sequencedescription.Text.Equals(""))
            {
                sequencedescription.Text = "Enter description here";
                sequencedescription.Foreground = Brushes.Gray;
            }
        }

        private void groupdescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (groupdescription.Text.Equals("Enter task here"))
            {
                groupdescription.Text = "";
                groupdescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void groupdescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupdescription.Text.Equals("") || groupdescription.Text.Equals("Enter task here"))
            {
                groupdescription.Text = "Enter task here";
                groupdescription.Foreground = Brushes.Gray;
            }
        }

        private void groupdescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedDescriptionBubble != null)
            {
                if (!groupdescription.Text.Equals("") && !groupdescription.Text.Equals("Enter task here"))
                {
                    selectedDescriptionBubble.addDescription(groupdescription.Text);
                }
            }
        }

        private void editsequencenametext_GotFocus(object sender, RoutedEventArgs e)
        {
            if (editsequencenametext.Text.Equals(selectedSequence.sequenceName))
            {
                editsequencenametext.Text = "";
                editsequencenametext.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void editsequencenametext_LostFocus(object sender, RoutedEventArgs e)
        {
            if (editsequencenametext != null && editsequencenametext.Text.Equals("") || editsequencenametext.Text.Equals(selectedSequence.sequenceName))
            {
                editsequencenametext.Text = selectedSequence.sequenceName;
                editsequencenametext.Foreground = Brushes.Gray;
            }
            else if (editsequencenametext != null)
            {
                editsequencenametext.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void editsequencedescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (editsequencedescription != null && editsequencedescription.Text.Equals(selectedSequence.getDescription()))
            {
                editsequencedescription.Text = "";
                editsequencedescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void editsequencedescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (editsequencedescription != null && editsequencedescription.Text.Equals("") || editsequencedescription.Text.Equals(selectedSequence.getDescription()))
            {
                editsequencedescription.Text = selectedSequence.getDescription();
                editsequencedescription.Foreground = Brushes.Gray;
            }
            else if (editsequencedescription != null)
            {
                editsequencenametext.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void loadSequence()
        {
            String[] split = selectedSequence.getGroupSequenceString().Split(',');

            List<Group> groupList = selectedSequence.getGroupSequence();

            for (int i = 0; i < groupList.Count; i++)
            {
                GroupContainerBubble temp = new GroupContainerBubble(groupList[i]);

                temp.addDescription(split[(i * 2) + 1]);

                editSequenceList.Children.Add(temp);
            }
        }

        private void editSequenceList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            List<GroupContainerBubble> tempList = new List<GroupContainerBubble>();

            for (int i = 0; i < editSequenceList.Children.Count; i++)
            {
                if (!(editSequenceList.Children[i] as GroupContainerBubble).isMouseOver())
                {
                    tempList.Add(editSequenceList.Children[i] as GroupContainerBubble);
                }
                else
                {
                    if ((editSequenceList.Children[i] as GroupContainerBubble) == selectedDescriptionBubble)
                    {
                        hideEditDescriptionEditView();
                    }
                }
            }

            editSequenceList.Children.Clear();

            foreach (GroupContainerBubble bubble in tempList)
            {
                editSequenceList.Children.Add(bubble);
            }

            updateEditCount();

            checkSaveUpdateRequirements();
        }

        private void editSequenceList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            editfortextboxfix.Focus();

            for (int i = 0; i < editSequenceList.Children.Count; i++)
            {
                if ((editSequenceList.Children[i] as GroupContainerBubble).isMouseOver())
                {
                    selectedDescriptionBubble = (editSequenceList.Children[i] as GroupContainerBubble);
                }
            }

            if (!selectedDescriptionBubble.containsNull())
            {
                editselectedgroupeditdescription.Visibility = Visibility.Visible;
                showEditDescriptionEditView();
            }

            updateEditCount();
        }

        private void showEditDescriptionEditView()
        {
            if (!selectedDescriptionBubble.containsNull())
            {
                editgroupseperator.Visibility = Visibility.Visible;
                editselectedgroupeditdescription.Visibility = Visibility.Visible;
                editselectedGroupName.Text = selectedDescriptionBubble.getGroup().name;
                editselectedGroupName.Foreground = selectedDescriptionBubble.getGroup().brushColor;
                if (!selectedDescriptionBubble.getDescription().Equals(""))
                {
                    editgroupdescription.Text = selectedDescriptionBubble.getDescription();
                    if (selectedDescriptionBubble.checkEdited() && !editgroupdescription.Text.Equals("Enter task here"))
                    {
                        editgroupdescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                    }
                    else
                    {
                        editgroupdescription.Foreground = Brushes.Gray;
                    }
                }
                else
                {
                    editgroupdescription.Text = "Enter task here";
                    editgroupdescription.Foreground = Brushes.Gray;
                }
            }
        }

        private void hideEditDescriptionEditView()
        {
            editgroupseperator.Visibility = Visibility.Hidden;
            selectedDescriptionBubble = null;
            editselectedgroupeditdescription.Visibility = Visibility.Hidden;
            editselectedGroupName.Text = "";
            editgroupdescription.Text = "";
        }

        private void editgroupdescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (editgroupdescription.Text.Equals(selectedDescriptionBubble.getDescription()) || editgroupdescription.Text.Equals("Enter task here") && !selectedDescriptionBubble.checkEdited())
            {
                editgroupdescription.Text = "";
                editgroupdescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void editgroupdescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (editgroupdescription.Text.Equals("") || editgroupdescription.Text.Equals(selectedDescriptionBubble.getDescription()))
            {
                if (selectedDescriptionBubble.getDescription().Equals(""))
                {
                    editgroupdescription.Text = "Enter task here";
                }
                else
                {
                    editgroupdescription.Text = selectedDescriptionBubble.getDescription();
                }
                
                if (selectedDescriptionBubble.checkEdited() && !editgroupdescription.Text.Equals("Enter task here"))
                {
                    editgroupdescription.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
                }
                else
                {
                    editgroupdescription.Foreground = Brushes.Gray;
                }      
            }
        }

        private void editgroupdescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedDescriptionBubble != null)
            {
                if (!editgroupdescription.Text.Equals(selectedDescriptionBubble.getDescription()))
                {
                    selectedDescriptionBubble.addDescription(editgroupdescription.Text);
                    selectedDescriptionBubble.edited();
                }              
            }
        }

        private void editgroupList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (GroupContainerBubble bubble in editgroupList.Children)
            {
                if (bubble.isMouseOver())
                {
                    GroupContainerBubble newBubble = new GroupContainerBubble(bubble.getGroup());

                    editSequenceList.Children.Add(newBubble);
                }
            }

            updateEditCount();

            checkSaveUpdateRequirements();
        }

        private void editsequencenametext_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveUpdateSequenceButton != null)
            {
                checkSaveUpdateRequirements();
            }
        }

        private void checkSaveUpdateRequirements()
        {
            if (editsequencenametext.Text.Equals("Enter name here") || editsequencenametext.Text.Equals(""))
            {
                SaveUpdateSequenceButton.Visibility = Visibility.Hidden;
                editnameerror.Text = "";
            }
            else if (db.checkSequenceNameExists(editsequencenametext.Text) && !editsequencenametext.Text.Equals(selectedSequence.sequenceName))
            {
                SaveUpdateSequenceButton.Visibility = Visibility.Hidden;
                editnameerror.Text = "This name has already been used.";
            }
            else
            {
                if (editsequencenametext.Text.Equals(selectedSequence.sequenceName))
                {
                    editsequencenametext.Foreground = Brushes.Gray;
                }

                editnameerror.Text = "";
                if (editSequenceList.Children.Count >= 2)
                {
                    SaveUpdateSequenceButton.Visibility = Visibility.Visible;
                }
                else
                {
                    SaveUpdateSequenceButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SaveUpdateSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            int lockint = 0;
            if (editToggle.isOn)
            {
                lockint = 1;
            }

            db.editSequence(selectedSequence.getID(), editsequencenametext.Text, editsequencedescription.Text, getSequenceEditString(), lockint);
            showMainView();
        }

        private void DeleteSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            (objects[ObjectIDManager.objectIDs["Settings_Page_Sequence_Page_Remove_Sequence"]] as SequenceRemove).sentSequence(selectedSequence);
            Dialog_Box.IsOpen = true;
        }
    }
}
