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
    /// Interaction logic for SettingsPageFileStructure.xaml
    /// </summary>
    public partial class SettingsPageFileStructure : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Settings_Page_List_Data_Structure_Page";
        private String name = "File Structure Settings";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;
        public int currentDirectory = 0;
        public DirectoryItem selectedDirectory;
        private DirectoryItem[] directoryItemArray = new DirectoryItem[5];
        private TextBlock[] directoryText = new TextBlock[5];
        private TextBlock[] directorySlash = new TextBlock[4];
        private ToggleItem sequenceToggle;

        public SettingsPageFileStructure()
        {
            InitializeComponent();

            loadObjectID();

            addTextBoxes();

            showMainView();
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
            collapseAllViews();
            collapseAllButtons();
            hideDirectoryInfo();
            currentDirectory = 0;
            resetURLBar();
            loadSubDirectories();
            showMainView();
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

            }
            else
            {

            }
        }

        private void directory1_MouseEnter(object sender, MouseEventArgs e)
        {
            directory1.Opacity = 0.5;
        }

        private void directory1_MouseLeave(object sender, MouseEventArgs e)
        {
            directory1.Opacity = 1;
        }

        private void directory2_MouseEnter(object sender, MouseEventArgs e)
        {
            directory2.Opacity = 0.5;
        }

        private void directory2_MouseLeave(object sender, MouseEventArgs e)
        {
            directory2.Opacity = 1;
        }

        private void directory3_MouseEnter(object sender, MouseEventArgs e)
        {
            directory3.Opacity = 0.5;
        }

        private void directory3_MouseLeave(object sender, MouseEventArgs e)
        {
            directory3.Opacity = 1;
        }

        private void directory4_MouseEnter(object sender, MouseEventArgs e)
        {
            directory4.Opacity = 0.5;
        }

        private void directory4_MouseLeave(object sender, MouseEventArgs e)
        {
            directory4.Opacity = 1;
        }

        private void directory5_MouseEnter(object sender, MouseEventArgs e)
        {
            directory5.Opacity = 0.5;
        }

        private void directory5_MouseLeave(object sender, MouseEventArgs e)
        {
            directory5.Opacity = 1;
        }

        private void collapseAllButtons()
        {
            EditDirectoryButton.Visibility = Visibility.Hidden;
            ReturnButton.Visibility = Visibility.Hidden;
            AddDirectoryButton.Visibility = Visibility.Hidden;
            SaveDirectoryButton.Visibility = Visibility.Hidden;
            GoBackButton.Visibility = Visibility.Hidden;
        }

        private void collapseAllViews()
        {
            directoryList.Visibility = Visibility.Collapsed;
            editDirectory.Visibility = Visibility.Collapsed;
            newDirectory.Visibility = Visibility.Collapsed;
            SequenceGrid.Visibility = Visibility.Hidden;
            deleteprompt.Visibility = Visibility.Hidden;
        }

        private void showMainView()
        {
            collapseAllViews();
            collapseAllButtons();
            revertDirectoryDetailDelete();
            loadSubDirectories();
            if (sequenceToggle != null && sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Visible;
            }
            else if (sequenceToggle != null && !sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Hidden;
            }
            directoryList.Visibility = Visibility.Visible;
            //EditDirectoryButton.Visibility = Visibility.Visible;
            if (directoryItemArray[3] == null)
            {
                AddDirectoryButton.Visibility = Visibility.Visible;
            }
            if (currentDirectory != 0)
            {
                GoBackButton.Visibility = Visibility.Visible;
            }
        }

        private void showEditView()
        {
            collapseAllViews();
            collapseAllButtons();
            //editDirectory.Visibility = Visibility.Visible;
        }

        private void showAddView()
        {
            collapseAllViews();
            collapseAllButtons();
            revertDirectoryDetailDelete();
            if (sequenceToggle != null && sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Visible;
            }
            newDirectory.Visibility = Visibility.Visible;
            ReturnButton.Visibility = Visibility.Visible;
            SaveEditDirectoryButton.Visibility = Visibility.Hidden;
            newDirectoryNameTextBox.Text = "Enter new name";
            newDirectoryNameTextBox.Foreground = Brushes.Gray;
        }

        private void EditDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            showEditView();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            showMainView();
        }

        private void AddDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            showAddView();
        }

        private void SaveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            db.addDirectory(newDirectoryNameTextBox.Text, currentDirectory);
            loadSubDirectories();
            showMainView();
            if (sequenceToggle != null && sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Visible;
            }
            SaveEditDirectoryButton.Visibility = Visibility.Hidden;
        }

        private void newDirectoryNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (newDirectoryNameTextBox.Text.Equals("Enter new name"))
            {
                newDirectoryNameTextBox.Text = "";
                newDirectoryNameTextBox.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
        }

        private void newDirectoryNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (newDirectoryNameTextBox.Text.Equals("Enter new name") || newDirectoryNameTextBox.Text.Equals(""))
            {
                newDirectoryNameTextBox.Text = "Enter new name";
                newDirectoryNameTextBox.Foreground = Brushes.Gray;
            }
        }

        private void newDirectoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SaveDirectoryButton != null)
            {
                if (newDirectoryNameTextBox.Text.Equals("Enter new name") || newDirectoryNameTextBox.Text.Equals(""))
                {
                    SaveDirectoryButton.Visibility = Visibility.Collapsed;
                    newDirectoryError.Text = "";
                }
                else if (db.checksubDirectoryExists(newDirectoryNameTextBox.Text, selectedDirectory))
                {
                    newDirectoryError.Text = "This name already exists within the directory!";
                    SaveDirectoryButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SaveDirectoryButton.Visibility = Visibility.Visible;
                    newDirectoryError.Text = "";
                }
            }
        }

        private void loadSubDirectories()
        {
            directoryList.Children.Clear();

            List<Directory> directories = db.getSubDirectories(currentDirectory);

            foreach (Directory directory in directories)
            {
                directoryList.Children.Add(new DirectoryItem(directory.id, directory.name, directory.parentID, directory.allowSequence, directory.sequenceID));
            }
        }

        private void resetURLBar()
        {
            directory1.Visibility = Visibility.Hidden;
            slash2.Visibility = Visibility.Hidden;
            directory2.Visibility = Visibility.Hidden;
            slash3.Visibility = Visibility.Hidden;
            directory3.Visibility = Visibility.Hidden;
            slash4.Visibility = Visibility.Hidden;
            directory4.Visibility = Visibility.Hidden;
            slash5.Visibility = Visibility.Hidden;
            directory5.Visibility = Visibility.Hidden;

            for (int i = 0; i < directoryItemArray.Length; i++)
            {
                directoryItemArray[i] = null;
            }
        }

        private void directoryList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            for (int i = 0; i < directoryList.Children.Count; i++)
            {
                if ((directoryList.Children[i] as DirectoryItem).selected)
                {
                    selectedDirectory = (directoryList.Children[i] as DirectoryItem);
                }
            }

            loadSelectedDirectory();
        }

        private void loadSelectedDirectory()
        {
            currentDirectory = selectedDirectory.id;
            showMainView();
            loadDirectoryInfo();
            updateURL();
        }

        private void loadToggleDescription()
        {
            toggledescription.Text = "Enabling this option will automatically apply the selected sequence to ALL future captures located within this directory. Future system captures located within subdirectories will not be affected.";
        }

        private void loadDirectoryInfo()
        {
            if (selectedDirectory.id != 0)
            {
                directory_info_card.Visibility = Visibility.Visible;
                editDirectoryName.Visibility = Visibility.Visible;
                deleteprompt.Visibility = Visibility.Hidden;
                editDirectoryNameTextBox.Text = selectedDirectory.name;
                editDirectoryError.Text = "";
                sequenceToggle = new ToggleItem("Apply sequence", selectedDirectory.sequenceApplied);
                sequenceToggleGrid.Children.Add(sequenceToggle);
                loadAvailableSequences();
                if (selectedDirectory.sequenceApplied)
                {
                    loadComboBoxItem(selectedDirectory.sequenceID);
                }
                loadToggleDescription();

                if (sequenceToggle != null && sequenceToggle.isOn)
                {
                    SequenceGrid.Visibility = Visibility.Visible;
                }
                else if (sequenceToggle != null && !sequenceToggle.isOn)
                {
                    SequenceGrid.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                directory_info_card.Visibility = Visibility.Hidden;
            }

        }

        private void hideDirectoryInfo()
        {
            directory_info_card.Visibility = Visibility.Hidden;
        }

        private void addTextBoxes()
        {
            directoryText[0] = directory1;
            directoryText[1] = directory2;
            directoryText[2] = directory3;
            directoryText[3] = directory4;
            directoryText[4] = directory5;

            directorySlash[0] = slash2;
            directorySlash[1] = slash3;
            directorySlash[2] = slash4;
            directorySlash[3] = slash5;
        }

        private void updateURL()
        {
            for (int i = 0; i < directoryText.Length; i++)
            {
                if (directoryItemArray[i] is null)
                {
                    directoryText[i].Text = selectedDirectory.name;
                    directoryText[i].Visibility = Visibility.Visible;

                    if (i < directorySlash.Length)
                    {
                        directorySlash[i].Visibility = Visibility.Visible;
                    }

                    directoryItemArray[i] = selectedDirectory;

                    i = directoryText.Length;
                }
            }
        }

        private void directory1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            navigateBack(0);
        }

        private void directory2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            navigateBack(1);
        }

        private void directory3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            navigateBack(2);
        }

        private void directory4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            navigateBack(3);
        }

        private void directory5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            navigateBack(4);
        }

        private void navigateBack(int passedindex)
        {
            selectedDirectory = directoryItemArray[passedindex];

            currentDirectory = selectedDirectory.id;

            for (int i = passedindex + 1; i < directoryItemArray.Length; i++)
            {
                directoryItemArray[i] = null;
                directoryText[i].Text = "";
                directoryText[i].Visibility = Visibility.Hidden;

                if (i < directorySlash.Length)
                {
                    directorySlash[i].Visibility = Visibility.Hidden;
                }
            }

            showMainView();
            loadDirectoryInfo();
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            int lastValidIndex = 0;

            for (int i = 0; i < directoryItemArray.Length; i++)
            {
                if (directoryItemArray[i] != null)
                {
                    lastValidIndex = i - 1;
                }
            }
            if (lastValidIndex >= -1)
            {
                if (selectedDirectory.parentID == 0)
                {
                    directoryItemArray[0] = null;
                    selectedDirectory = null;
                    currentDirectory = 0;
                    hideDirectoryInfo();
                    resetURLBar();
                    showMainView();
                }
                else
                {
                    navigateBack(lastValidIndex);
                }
            }

            if (sequenceToggle != null && sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Visible;
            }
            else if (sequenceToggle != null && !sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Hidden;
            }
        }

        private void loadAvailableSequences()
        {
            SequenceSelection.ItemsSource = db.getSequences();
        }

        private void sequenceToggleGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sequenceToggle.isOn)
            {
                SequenceGrid.Visibility = Visibility.Visible;
                SaveEditDirectoryButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SequenceGrid.Visibility = Visibility.Hidden;
            }
        }

        private void editDirectoryNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (db.checkSameLevelDirectoryExists(editDirectoryNameTextBox.Text, selectedDirectory) && !editDirectoryNameTextBox.Text.Equals(selectedDirectory.name))
            {
                SaveEditDirectoryButton.Visibility = Visibility.Collapsed;
                editDirectoryError.Text = "This name already exists within the upper directory!";
            }
            else if (editDirectoryNameTextBox.Text.Equals(""))
            {
                SaveEditDirectoryButton.Visibility = Visibility.Collapsed;
                editDirectoryError.Text = "";
            }
            else
            {
                editDirectoryError.Text = "";
                SaveEditDirectoryButton.Visibility = Visibility.Visible;
            }
        }

        private void SaveEditDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            int sequenceAdded = 0;
            int sequenceid = 0;
            if (sequenceToggle.isOn)
            {
                sequenceAdded = 1;
                sequenceid = (SequenceSelection.SelectedItem as Sequence).getID();
                selectedDirectory.sequenceID = sequenceid;
                loadComboBoxItem(sequenceid);
            }

            db.updateDirectory(selectedDirectory.id, editDirectoryNameTextBox.Text, sequenceAdded, sequenceid);
            selectedDirectory.name = editDirectoryNameTextBox.Text;
            selectedDirectory.sequenceApplied = sequenceToggle.isOn;
            loadDirectoryInfo();
            showMainView();
            refreshURLName();
            loadToggleDescription();
        }

        private void loadComboBoxItem(int sequenceID)
        {
            SequenceGrid.Visibility = Visibility.Visible;

            for (int i = 0; i < SequenceSelection.Items.Count; i++)
            {
                Sequence temp = SequenceSelection.Items[i] as Sequence;

                if (temp.getID() == sequenceID)
                {
                    SequenceSelection.SelectedIndex = i;

                    i = SequenceSelection.Items.Count;
                }
            }
        }

        private void refreshURLName()
        {
            for (int i = 0; i < directoryText.Length; i++)
            {
                if (directoryItemArray[i] is null)
                {
                    directoryText[i-1].Text = selectedDirectory.name;
                    directoryText[i-1].Visibility = Visibility.Visible;

                    i = directoryText.Length;
                }
            }
        }

        private void SequenceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sequence typeItem = SequenceSelection.SelectedItem as Sequence;

            if (typeItem == null && sequenceToggle.isOn)
            {
                SaveEditDirectoryButton.Visibility = Visibility.Collapsed;
            }
            else if (sequenceToggle.isOn)
            {
                editDirectoryNameTextBox_TextChanged(null, null);
            }
        }

        private void DeleteDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteDirectoryButton.Visibility = Visibility.Hidden;
            GoBackDeleteDirectoryButton.Visibility = Visibility.Visible;
            SaveEditDirectoryButton.Visibility = Visibility.Hidden;
            editDirectoryName.Visibility = Visibility.Collapsed;
            deleteprompt.Visibility = Visibility.Visible;
            FinalDeleteDirectoryButton.Visibility = Visibility.Hidden;

            if (db.doSubDirectoriesExist(selectedDirectory.id))
            {
                deletePromptText.Text = "The directory '" + selectedDirectory.name + "' contains subdirectories which must be removed before deletion.";
            }
            else
            {
                deletePromptText.Text = "This action will permanently delete the '" + selectedDirectory.name + "' directory.";
                FinalDeleteDirectoryButton.Visibility = Visibility.Visible;
            }
        }

        private void GoBackDeleteDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            revertDirectoryDetailDelete();
        }

        private void revertDirectoryDetailDelete()
        {
            DeleteDirectoryButton.Visibility = Visibility.Visible;
            GoBackDeleteDirectoryButton.Visibility = Visibility.Hidden;
            FinalDeleteDirectoryButton.Visibility = Visibility.Hidden;
            SaveEditDirectoryButton.Visibility = Visibility.Visible;
            editDirectoryName.Visibility = Visibility.Visible;
            deleteprompt.Visibility = Visibility.Hidden;
        }

        private void FinalDeleteDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            db.deleteDirectory(selectedDirectory.id);

            GoBackButton_Click(null, null);
        }
    }
}
