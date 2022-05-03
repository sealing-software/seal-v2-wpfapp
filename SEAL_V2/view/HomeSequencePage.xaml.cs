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
    /// Interaction logic for HomeSequencePage.xaml
    /// </summary>
    public partial class HomeSequencePage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Home_Sequence_View";
        private String name = "Seqeuences";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;
        private Sequence currentSequence;
        private int sequenceStepView = 0;
        private bool newCapture = false;
        private int captureID;
        private Capture currentCapture;

        public HomeSequencePage()
        {
            InitializeComponent();

            loadObjectID();

            loadMainView();
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

        private void hideAllViews()
        {
            NoDirectoryView.Visibility = Visibility.Hidden;
            NoSequenceView.Visibility = Visibility.Hidden;
            SequenceAssignedView.Visibility = Visibility.Hidden;
            SequenceNotAssignedView.Visibility = Visibility.Hidden;
            LeftArrowGrid.Visibility = Visibility.Hidden;
            RightArrowGrid.Visibility = Visibility.Hidden;
            NewCycleView.Visibility = Visibility.Hidden;
            hideAllButtons();
        }

        private void hideAllButtons()
        {
            ButtonPanel.Children.Clear();
        }

        public void loadMainView()
        {
            hideAllViews();
            sequenceStepView = 0;

            if (CurrentSystem.machineID < 1)
            {
                //SequenceAssignedView.Visibility = Visibility.Visible;
                NoDirectoryView.Visibility = Visibility.Visible;
            }
            else if (db.getSequences().Count < 1)
            {
                NoSequenceView.Visibility = Visibility.Visible;
            }
            else if (CurrentSystem.dirAdded > 0)
            {
                if (db.sequenceRequired(db.getDirectory(CurrentSystem.dirID).id))
                {
                    SequenceAssignedView.Visibility = Visibility.Visible;
                    currentSequence = db.getSequence(db.getDirectory(CurrentSystem.dirID).sequenceID);

                    if (StaticCapture.id < 1 && db.captureExists(CurrentSystem.machineID, currentSequence.getID()))
                    {
                        currentCapture = db.getLatestCapture(CurrentSystem.machineID, currentSequence.getID());
                        StaticCapture.changeCapture(currentCapture);
                    }

                    loadSequenceInfo();
                    arrowVisibility();
                }
                else
                {
                    //Check if currentsequence is null,
                    //if null show select view
                    //CODE FOR SEQUENCE NOT BEING REQUIRED FOR DIRECTORY!
                    Console.WriteLine("TEST");
                }
            }
        }

        public void loadNewCycleView()
        {
            hideAllViews();
            cycleName.Text = "";
            NewCycleView.Visibility = Visibility.Visible;
            refreshNewCycleButtons();
        }


        private void arrowVisibility()
        {
            if (currentSequence != null)
            {
                if (sequenceStepView > 0)
                {
                    LeftArrowGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    LeftArrowGrid.Visibility = Visibility.Hidden;
                }

                if (sequenceStepView < currentSequence.sequenceLength - 1)
                {
                    RightArrowGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    RightArrowGrid.Visibility = Visibility.Hidden;
                }
            }
        }


        private void loadSequenceInfo()
        {
            SequenceName.Text = currentSequence.sequenceName;
            sequenceDescription.Text = "The " + db.getDirectory(CurrentSystem.dirID).name + " directory requires this sequence to be applied.";
            
            if (db.captureExists(CurrentSystem.machineID, currentSequence.getID()))
            {
                if (newCapture)
                {
                    newCapture = false;
                    //currentCapture = db.getCapture(captureID);
                    sequenceStepView = StaticCapture.currentStep;
                    loadSequenceGroupBubble(StaticCapture.currentStep);
                    loadSequenceButtons();
                }
                else
                {
                    sequenceStepView = StaticCapture.currentStep;
                    loadSequenceGroupBubble(StaticCapture.currentStep);
                    loadSequenceButtons();
                }
            }
            else
            {
                loadSequenceGroupBubble(0);
                loadSequenceButtons();
            }
        }

        private void loadSequenceGroupBubble(int indexPassed)
        {
            descriptionbubblegrid.Children.Clear();
            descriptionbubblegrid.Children.Add(new GroupContainerBubble(currentSequence.getGroupAtIndex(indexPassed)));
            grouptask.Text = currentSequence.getGroupDescriptionAtIndex(indexPassed);
        }

        private void loadSequenceButtons()
        {
            ButtonPanel.Children.Clear();
            accountpermission.Text = "";

            if (StaticCapture.id > 0)
            {
                if (StaticCapture.status == 3 || StaticCapture.status == 2)
                {

                }
                else
                {
                    if (currentSequence.getGroupAtIndex(StaticCapture.currentStep).ID == User.getAssignedGroupID() && sequenceStepView == StaticCapture.currentStep)
                    {
                        failButton();
                        passButton();
                    }
                    else if (currentSequence.getGroupAtIndex(StaticCapture.currentStep).ID != User.getAssignedGroupID() && sequenceStepView == StaticCapture.currentStep)
                    {
                        accountpermission.Text = "Only a user assigned to the " + currentSequence.getGroupAtIndex(StaticCapture.currentStep).name + " group can execute this step!";
                    }
                }

                if (currentSequence.getGroupAtIndex(0).ID == User.getAssignedGroupID())
                {
                    newCaptureButton();
                }
            }
            else
            {
                if (currentSequence.getGroupAtIndex(0).ID == User.getAssignedGroupID() && sequenceStepView == 0) //AND IF CURRENT STEP IN CAPTURE OBJECT IS CORRECT
                {
                    newCycleButton();
                }
                else if (sequenceStepView == 0 && currentSequence.getGroupAtIndex(0).ID != User.getAssignedGroupID())
                {
                    accountpermission.Text = "Only a user assigned to the " + currentSequence.getGroupAtIndex(0).name + " group can execute this step!";
                }
            }

            loadNavigator();
        }

        private void loadNavigator()
        {
            sequencePosition.Text = (sequenceStepView + 1).ToString() + "/" + currentSequence.sequenceLength.ToString();

            if (StaticCapture.status == 3)
            {
                sequenceCurrent.Text = "This capture has been sealed";

            }
            else if (StaticCapture.status == 2)
            {
                sequenceCurrent.Text = "This capture failed and was sealed";
            }
            else
            {
                if (sequenceStepView == StaticCapture.currentStep)
                {
                    sequenceCurrent.Text = "Current";
                }
                else
                {
                    sequenceCurrent.Text = "";
                }
            }
        }

        private void passButton()
        {
            StandardButton button = new StandardButton("Pass_Button", "Pass", MaterialDesignThemes.Wpf.PackIconKind.Check, MaterialDesignThemes.Wpf.PackIconKind.CheckOutline, (Brush)Application.Current.Resources["NewItem"], "Passed");
            button.clicked += passed;
            ButtonPanel.Children.Add(button);
        }

        private void passed(object sender, EventArgs e)
        {
            StaticCapture.passed(currentSequence.sequenceLength);
            sendMessage(createMessage("REFRESH", "Home_Page"));
        }

        private void failButton()
        {
            StandardButton button = new StandardButton("Fail_Button", "Fail", MaterialDesignThemes.Wpf.PackIconKind.Cancel, MaterialDesignThemes.Wpf.PackIconKind.CancelOutline, (Brush)Application.Current.Resources["RemovedItem"], "Failed");
            button.clicked += failed;
            ButtonPanel.Children.Add(button);
        }

        private void failed(object sender, EventArgs e)
        {
            StaticCapture.failed();
            sendMessage(createMessage("REFRESH", "Home_Page"));
        }

        private void newCycleButton()
        {
            StandardButton button = new StandardButton("NewCycle_Button", "New", MaterialDesignThemes.Wpf.PackIconKind.Play, MaterialDesignThemes.Wpf.PackIconKind.PlayOutline, (Brush)Application.Current.Resources["NewItem"], "Begin");
            button.clicked += newCycle;
            ButtonPanel.Children.Add(button);
        }

        private void newCaptureButton()
        {
            StandardButton button = new StandardButton("NewCapture_Button", "New", MaterialDesignThemes.Wpf.PackIconKind.Plus, MaterialDesignThemes.Wpf.PackIconKind.PlusOutline, (Brush)Application.Current.Resources["NewItem"], "New");
            button.clicked += newCycle;
            ButtonPanel.Children.Add(button);
        }

        public void refreshNewCycleButtons()
        {
            ButtonPanel.Children.Clear();
            cancelButton();
            if (db.captureNameExistForSystem(cycleName.Text, CurrentSystem.machineID))
            {
                nameError.Text = "A cycle with this name already exists!";
            }
            else
            {
                if (!cycleName.Text.Equals(""))
                {
                    saveCycleButton();
                }
                nameError.Text = "";
            }
        }

        public void cancelButton()
        {
            StandardButton button = new StandardButton("Cancel_Button", "Cancel", MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline, (Brush)Application.Current.Resources["RemovedItem"], "Cancel");
            button.clicked += returnToMain;
            ButtonPanel.Children.Add(button);
        }
        public void saveCycleButton()
        {
            StandardButton button = new StandardButton("Save_Button", "Save", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Save");
            button.clicked += saveCycle;
            ButtonPanel.Children.Add(button);
        }

        private void returnToMain(object sender, EventArgs e)
        {
            loadMainView();
        }

        private void newCycle(object sender, EventArgs e)
        {
            loadNewCycleView();
        }

        private void saveCycle(object sender, EventArgs e)
        {
            captureID = db.saveCapture(cycleName.Text, CurrentSystem.machineID, currentSequence.getID(), 0, 0);
            newCapture = true;
            StaticCapture.applyCapture(db.getCapture(captureID));
            sendMessage(createMessage("REFRESH", "Home_Page"));
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

        private void LeftArrowGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            LeftArrowGrid.Opacity = 0.5;
        }

        private void LeftArrowGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            LeftArrowGrid.Opacity = 1;
        }

        private void RightArrowGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            RightArrowGrid.Opacity = 0.5;
        }

        private void RightArrowGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            RightArrowGrid.Opacity = 1;
        }

        private void RightArrowGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sequenceStepView++;
            loadSequenceGroupBubble(sequenceStepView);
            loadSequenceButtons();
            arrowVisibility();
        }

        private void LeftArrowGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sequenceStepView--;
            loadSequenceGroupBubble(sequenceStepView);
            loadSequenceButtons();
            arrowVisibility();
        }

        private void cycleName_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshNewCycleButtons();
        }
    }
}
