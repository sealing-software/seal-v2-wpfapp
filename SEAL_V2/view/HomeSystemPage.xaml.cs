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
using SEAL_V2.view.usercontrolobjects;
using SEAL_V2.db;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for HomeSystemPage.xaml
    /// </summary>
    public partial class HomeSystemPage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Home_System_View";
        private String name = "System view";
        public event EventHandler<StatusMessage> message;
        private DatabaseInterface db = DatabaseInterface.Instance;
        private int curDirID = 0;
        private DirectoryItem curDirItem;
        private DirectoryItem[] dirArray = new DirectoryItem[5];
        private bool nicknameToggleOn = false;
        private List<String> regValues = new List<string>();
        private bool registryFound = false;
        public event EventHandler refresh;
        public HomeSystemPage()
        {
            InitializeComponent();

            loadObjectID();

            setupInfo();
            loadButtons();
            loadComboBox();
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

        public long getObjectID()
        {
            return objectID;
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

        private void setupInfo()
        {
            machineNameText.Text = CurrentSystem.machineName;
            modelText.Text = CurrentSystem.model;
            manufacturerText.Text = CurrentSystem.manufacturer;
            serialText.Text = CurrentSystem.serial;

            if (CurrentSystem.dirAdded > 0)
            {
                assignStatus.Text = (db.getDirectory(CurrentSystem.dirID)).name;
                assignStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Folder;
                assignStatusIcon.Foreground = ((Brush)Application.Current.Resources["OnBackground"]);
            }
            else
            {
                assignStatus.Text = "Not Assigned";
                assignStatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Warning;
                assignStatusIcon.Foreground = Brushes.Yellow;
            }

            if (CurrentSystem.nicknameAdded > 0)
            {
                customNameView.Visibility = Visibility.Visible;
                customNameText.Text = CurrentSystem.nickname;
            }
            else
            {
                customNameView.Visibility = Visibility.Collapsed;
            }

            if (CurrentSystem.regadded > 0)
            {
                customVersionView.Visibility = Visibility.Visible;
                customVersionText.Text = CurrentSystem.getRegVersion();
            }
            else
            {
                customVersionView.Visibility = Visibility.Collapsed;
            }

            if (db.howManyModelInstances(CurrentSystem.model) > 1)
            {
                duplicateModelGrid.Visibility = Visibility.Visible;
            }
            else
            {
                duplicateModelGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void setupToggle()
        {
            nicknameToggleOn = CurrentSystem.customVersionExists();

            RegistryVersionGrid.Visibility = Visibility.Hidden;
            registryToggleGrid.Children.Clear();
            registryToggleGrid.Children.Add(new ToggleItem("Custom version", nicknameToggleOn));
            toggledescription.Text = "A custom version may be linked to this model by linking to a registery key.";

            if(nicknameToggleOn)
            {
                RegistryVersionGrid.Visibility = Visibility.Visible;
                RegistryKey.Text = CurrentSystem.regKey;
                RegistryValue.Text = CurrentSystem.regValue;
                loadRegButton();
            }
        }

        private void hideAllViews()
        {
            MainView.Visibility = Visibility.Hidden;
            AssignView.Visibility = Visibility.Hidden;
            EditView.Visibility = Visibility.Hidden;
            DeleteView.Visibility = Visibility.Hidden;
            AuditView.Visibility = Visibility.Hidden;
            hideAllButtons();
        }

        private void hideAllButtons()
        {
            ButtonGrid.Visibility = Visibility.Hidden;
            AssignButtonGrid.Visibility = Visibility.Hidden;
            EditButtonGrid.Visibility = Visibility.Hidden;
            DeleteButtonGrid.Visibility = Visibility.Hidden;
            AuditButtonGrid.Visibility = Visibility.Hidden;
        }

        private void loadMainView()
        {
            hideAllViews();
            setupInfo();
            MainView.Visibility = Visibility.Visible;
            ButtonGrid.Visibility = Visibility.Visible;
            loadButtons();
            sendMessage(createMessage("REFRESH", "Home_Page"));
        }

        private void loadAuditView()
        {
            hideAllViews();
            AuditView.Visibility = Visibility.Visible;
            AuditButtonGrid.Visibility = Visibility.Visible;
            SystemsGrid.ItemsSource = db.getSystems();
            loadAuditViewButtons();

        }

        private void loadButtons()
        {
            ButtonPanel.Children.Clear();

            loadAuditButton(); //Should check if any other models/machines exist in db...

            if (CurrentSystem.dirAdded > 0)
            {
                if (true) //Check if any software captures are tied to system
                {
                    loadReassignButton();
                }
            }
            else
            {
                if (db.getSubDirectories(0).Count > 0)
                {
                    loadAssignButton();
                }
            }

            loadEditButton();         

            if (CurrentSystem.dirAdded > 0)
            {
                if (true) //Check if any software captures are tied to system
                {
                    loadDeleteButton();
                }
            }
        }

        private void loadAuditButton()
        {
            StandardButton button = new StandardButton("Audit_Button", "Assign", MaterialDesignThemes.Wpf.PackIconKind.Eye, MaterialDesignThemes.Wpf.PackIconKind.EyeOutline, (Brush)Application.Current.Resources["EditItem"], "Audit");
            button.clicked += auditClicked;
            ButtonPanel.Children.Add(button);
        }

        private void loadAssignButton()
        {
            StandardButton button = new StandardButton("Assign_Button", "Assign", MaterialDesignThemes.Wpf.PackIconKind.Assignment, MaterialDesignThemes.Wpf.PackIconKind.AssignmentTurnedIn, (Brush)Application.Current.Resources["NewItem"], "Assign");
            button.clicked += assignClicked;
            ButtonPanel.Children.Add(button);
        }

        private void loadReassignButton()
        {
            StandardButton button = new StandardButton("Reassign_Button", "Reassign", MaterialDesignThemes.Wpf.PackIconKind.AssignmentReturned, MaterialDesignThemes.Wpf.PackIconKind.AssignmentReturnedOutline, (Brush)Application.Current.Resources["EditItem"], "Reassign");
            button.clicked += reassignClicked;
            ButtonPanel.Children.Add(button);
        }

        private void loadDeleteButton()
        {
            StandardButton button = new StandardButton("Delete_Button", "Delete", MaterialDesignThemes.Wpf.PackIconKind.Delete, MaterialDesignThemes.Wpf.PackIconKind.DeleteOutline, (Brush)Application.Current.Resources["RemovedItem"], "Delete");
            button.clicked += deleteClicked;
            ButtonPanel.Children.Add(button);
        }
        private void auditClicked(object sender, EventArgs e)
        {
            loadAuditView();
        }


        private void assignClicked(object sender, EventArgs e)
        {
            loadAssignView();
        }

        private void reassignClicked(object sender, EventArgs e)
        {
            //loadAssignView();
        }

        private void deleteClicked(object sender, EventArgs e)
        {
            loadDeleteView();
        }

        private void loadAssignView()
        {
            hideAllViews();
            clearArray();
            AssignView.Visibility = Visibility.Visible;
            AssignButtonGrid.Visibility = Visibility.Visible;
            loadAssignViewButtons();
            curDirID = 0;
            updateDirectoryView();
        }

        private void loadEditView()
        {
            hideAllViews();
            EditView.Visibility = Visibility.Visible;
            EditButtonGrid.Visibility = Visibility.Visible;
            setupToggle();
            loadEditButtons();
        }

        private void loadDeleteView()
        {
            hideAllViews();
            DeleteView.Visibility = Visibility.Visible;
            DeleteButtonGrid.Visibility = Visibility.Visible;
            loadDeleteButtons();
        }

        private void clearArray()
        {
            dirArray = new DirectoryItem[5];
        }

        private void loadAssignViewButtons()
        {
            AssignButtonPanel.Children.Clear();
            loadAssignCancelButton();

            if (curDirID != 0 && !db.doesSystemModelExistinDir(CurrentSystem.model, curDirID))
            {
                loadAssignSaveButton();
            }
        }

        private void loadAuditViewButtons()
        {
            AuditButtonPanel.Children.Clear();

            loadAuditCancelButton();
        }

        private void loadAuditCancelButton()
        {
            StandardButton cancelButton = new StandardButton("Cancel_Button", "Cancel", MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline, (Brush)Application.Current.Resources["RemovedItem"], "Cancel");
            cancelButton.clicked += cancelButtonClicked;
            AuditButtonPanel.Children.Add(cancelButton);
        }

        private void loadAssignCancelButton()
        {
            StandardButton cancelButton = new StandardButton("Cancel_Button", "Cancel", MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline, (Brush)Application.Current.Resources["RemovedItem"], "Cancel");
            cancelButton.clicked += cancelButtonClicked;
            AssignButtonPanel.Children.Add(cancelButton);
        }

        private void loadAssignSaveButton()
        {
            if (!CurrentSystem.audit)
            {
                StandardButton button = new StandardButton("Save_Button", "Save", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Save");
                button.clicked += saveAssignButtonClicked;
                AssignButtonPanel.Children.Add(button);
            }
        }

        private void saveAssignButtonClicked(object sender, EventArgs e)
        {
            CurrentSystem.dirAdded = 1;
            CurrentSystem.dirID = curDirID;
            if (CurrentSystem.machineID > 0)
            {
                db.updateSystem(CurrentSystem.machineID, CurrentSystem.model, CurrentSystem.nicknameAdded, CurrentSystem.nickname, 1, curDirID, CurrentSystem.regadded, CurrentSystem.regKey, CurrentSystem.regValue);
            }
            else
            {
                db.addSystem(CurrentSystem.model, CurrentSystem.nicknameAdded, CurrentSystem.nickname, 1, curDirID, CurrentSystem.regadded, CurrentSystem.regKey, CurrentSystem.regValue);
            }

            loadMainView();
        }

        private void cancelButtonClicked(object sender, EventArgs e)
        {
            loadMainView();
        }

        private void loadDirectories(int parentID)
        {
            directoryPanel.Children.Clear();

            List<Directory> directoryList = db.getSubDirectories(parentID);

            foreach (Directory directory in directoryList)
            {
                directoryPanel.Children.Add(new DirectoryItem(directory.id, directory.name, directory.parentID, directory.allowSequence, directory.sequenceID));
            }
        }


        private void loadEditButton()
        {
            StandardButton button = new StandardButton("Edit_Button", "Edit", MaterialDesignThemes.Wpf.PackIconKind.Edit, MaterialDesignThemes.Wpf.PackIconKind.EditOutline, (Brush)Application.Current.Resources["EditItem"], "Edit");
            button.clicked += editClicked;
            ButtonPanel.Children.Add(button);
        }

        private void editClicked(object sender, EventArgs e)
        {
            loadEditView();
        }

        private void loadEditButtons()
        {
            EditButtonPanel.Children.Clear();
            addEditCancelButton();

            if (nicknameToggleOn)
            {
                loadRegButton();

                if (registryFound)
                {
                    addSaveEditButton();
                }
            }
            else
            {
                addSaveEditButton();
            }

        }

        private void addEditCancelButton()
        {
            StandardButton cancelButton = new StandardButton("Cancel_Button", "Cancel", MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline, (Brush)Application.Current.Resources["RemovedItem"], "Cancel");
            cancelButton.clicked += cancelButtonClicked;
            EditButtonPanel.Children.Add(cancelButton);
        }

        private void addSaveEditButton()
        {
            if (!CurrentSystem.audit)
            {
                StandardButton saveButton = new StandardButton("Save_Button", "Save", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Save");
                saveButton.clicked += saveEditClicked;
                EditButtonPanel.Children.Add(saveButton);
            }    
        }

        private void saveEditClicked(object sender, EventArgs e)
        {
            if (nicknameText.Text.Equals(""))
            {
                CurrentSystem.nicknameAdded = 0;
                CurrentSystem.nickname = "";
            }
            else
            {
                CurrentSystem.nicknameAdded = 1;
                CurrentSystem.nickname = nicknameText.Text;
            }

            if (nicknameToggleOn)
            {
                CurrentSystem.regadded = 1;
                CurrentSystem.regKey = RegistryKey.Text;
                CurrentSystem.regValue = RegistryValue.Text;
            }
            else
            {
                CurrentSystem.regadded = 0;
                CurrentSystem.regKey = "";
                CurrentSystem.regValue = "";
            }

            if (CurrentSystem.machineID > 0)
            {
                db.updateSystem(CurrentSystem.machineID, CurrentSystem.model, CurrentSystem.nicknameAdded, CurrentSystem.nickname, CurrentSystem.dirAdded, CurrentSystem.dirID, CurrentSystem.regadded, CurrentSystem.regKey, CurrentSystem.regValue);
            }
            else
            {
                db.addSystem(CurrentSystem.model, CurrentSystem.nicknameAdded, CurrentSystem.nickname, CurrentSystem.dirAdded, CurrentSystem.dirID, CurrentSystem.regadded, CurrentSystem.regKey, CurrentSystem.regValue);
            }

            loadMainView();
        }

        private void URLBackButton_MouseEnter(object sender, MouseEventArgs e)
        {
            URLBackButton.Opacity = 0.5;
        }

        private void URLBackButton_MouseLeave(object sender, MouseEventArgs e)
        {
            URLBackButton.Opacity = 1;
        }

        private void directoryPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < directoryPanel.Children.Count; i++)
            {
                if ((directoryPanel.Children[i] as DirectoryItem).selected)
                {
                    curDirItem = (directoryPanel.Children[i] as DirectoryItem);
                    curDirID = curDirItem.id;
                    addToArray(curDirItem);
                    updateDirectoryView();
                    loadAssignViewButtons();
                }
            }
        }

        private void addToArray(DirectoryItem dirObject)
        {
            for (int i = 0; i < dirArray.Length; i++)
            {
                if (dirArray[i] == null)
                {
                    dirArray[i] = dirObject;

                    i = dirArray.Length;
                }
            }
        }

        private void updateDirectoryView()
        {
            URLText.Text = " / ";

            for (int i = 0; i < dirArray.Length; i++)
            {
                if (dirArray[i] != null)
                {
                    URLText.Text = URLText.Text + (dirArray[i] as DirectoryItem).name;

                    if (i < dirArray.Length - 1)
                    {
                        URLText.Text = URLText.Text + " / ";
                    }
                }
            }


            loadDirectories(curDirID);
        }

        private void assignButtonCheck()
        {
            AssignButtonPanel.Children.Clear();

            loadAssignCancelButton();
        }

        private void URLBackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DirectoryItem item = null;

            if (dirArray[0] != null)
            {
                for (int i = 0; i < dirArray.Length; i++)
                {
                    if (dirArray[i] == null)
                    {
                        dirArray[i - 1] = null;

                        if (i > 1)
                        {
                            item = dirArray[i - 2];
                            curDirID = item.id;
                        }

                        i = dirArray.Length;
                    }
                }
            }

            if (dirArray[4] != null)
            {
                dirArray[4] = null;

                item = dirArray[3];
                curDirID = item.id;
            }

            if (dirArray[0] == null)
            {
                curDirID = 0;
                curDirItem = null;
            }

            loadAssignViewButtons();
            updateDirectoryView();
        }

        private void registryToggleGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (nicknameToggleOn)
            {
                nicknameToggleOn = false;
                RegistryVersionGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                nicknameToggleOn = true;
                RegistryVersionGrid.Visibility = Visibility.Visible;
            }

            loadEditButtons();
        }

        private void loadComboBox()
        {
            regValues.Add("Any");
            regValues.Add("REG_BINARY");
            regValues.Add("REG_DWORD");
            regValues.Add("REG_EXPAND_SZ");
            regValues.Add("REG_MULTI_SZ");
            regValues.Add("REG_SZ");


            RegValues.ItemsSource = regValues;
            RegValues.SelectedIndex = 0;
        }

        private void loadRegButton()
        {
            GetValueGrid.Children.Clear();
            StandardButton regButton = new StandardButton("Registery_Button", "Registery", MaterialDesignThemes.Wpf.PackIconKind.Download, MaterialDesignThemes.Wpf.PackIconKind.DownloadOutline, (Brush)Application.Current.Resources["OnBackground"], "Get value");
            regButton.clicked += cancelButtonClicked;
            GetValueGrid.Children.Add(regButton);
        }

        private void printRegValue()
        {
            registryFound = false;

            if (!RegistryKey.Text.Equals("") && !RegistryValue.Text.Equals(""))
            {

                versionText.Visibility = Visibility.Visible;
                try
                {
                    using (var rootKey = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        using (var key = rootKey.OpenSubKey(RegistryKey.Text, false))
                        {
                            if (key != null)
                            {
                                string found = Convert.ToString(key.GetValue(RegistryValue.Text));

                                if (found.Equals(""))
                                {
                                    versionText.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    versionText.Text = "Version found: " + found;
                                    registryFound = true;
                                }
                            }
                            else
                            {
                                versionText.Visibility = Visibility.Hidden;
                            }
                        }
                    }           
                    
                    if (!registryFound)
                    {
                        versionText.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
                {
                    versionText.Text = "Registry lookup error";
                }
            }
            else
            {
                versionText.Visibility = Visibility.Hidden;
            }

            loadEditButtons();

        }

        private void RegistryKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            printRegValue();
        }

        private void RegistryValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            printRegValue();
        }

        private void loadDeleteButtons()
        {
            DeleteButtonPanel.Children.Clear();
            loadDeleteCancelButton();
            loadFinalDeleteButton();
        }

        private void loadDeleteCancelButton()
        {
            StandardButton cancelButton = new StandardButton("Cancel_Button", "Cancel", MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline, (Brush)Application.Current.Resources["RemovedItem"], "Cancel");
            cancelButton.clicked += cancelButtonClicked;
            DeleteButtonPanel.Children.Add(cancelButton);
        }

        private void loadFinalDeleteButton()
        {
            StandardButton button = new StandardButton("Delete_Button", "Delete", MaterialDesignThemes.Wpf.PackIconKind.Delete, MaterialDesignThemes.Wpf.PackIconKind.DeleteOutline, (Brush)Application.Current.Resources["RemovedItem"], "Delete");
            button.clicked += deletButtonClicked;
            DeleteButtonPanel.Children.Add(button);
        }

        private void deletButtonClicked(object sender, EventArgs e)
        {
            //DELETE SYSTEM
            //RESET SYSTEM ATTRIBUTES
            db.deleteSystem(CurrentSystem.machineID);
            CurrentSystem.resetAttributes();
            loadMainView();
        }

        private void SystemsGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SystemObject systemObject = (SystemObject)SystemsGrid.SelectedItem;

            if (systemObject != null)
            {
                if (systemObject.id == CurrentSystem.initialID)
                {
                    CurrentSystem.systemSetup();
                    sendMessage(createMessage("AUDIT_OFF", "Main_Window"));
                    loadMainView();
                }
                else
                {
                    CurrentSystem.auditSystem(systemObject);
                    sendMessage(createMessage("AUDIT_ON", "Main_Window"));
                    loadMainView();
                }
            }
        }
    }
}
