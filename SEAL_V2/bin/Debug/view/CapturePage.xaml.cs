using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
using SEAL_V2.view.usercontrolobjects;
using WpfAnimatedGif;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using Microsoft.Win32;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for CapturePage.xaml
    /// </summary>
    public partial class CapturePage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Capture_Page";
        private DatabaseInterface db = DatabaseInterface.Instance;
        public string name = "Capture";
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;
        public String loadingGifLoc = "images/WaitCover.gif";
        private String selectedFileName;
        private int total = 0;
        private int totalVisible;
        private int totalApplication;
        private int totalRegistry;
        private int totalDriver;
        private int totalSecurity;

        public CapturePage()
        {
            InitializeComponent();

            loadObjectID();

            initialPageLoad();
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

        public String getName()
        {
            return name;
        }

        public void refreshPage()
        {
            initialPageLoad();
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
            SelectionView.Visibility = Visibility.Hidden;
            ResetView.Visibility = Visibility.Hidden;
            loadingView.Visibility = Visibility.Hidden;
            SoftwareView.Visibility = Visibility.Hidden;
            AdditionaEntriesCard.Visibility = Visibility.Hidden;
            infoCard.Visibility = Visibility.Hidden;
        }
        private void hideAllButtons()
        {

        }

        private void resetPage()
        {
            ButtonPanel.Children.Clear();
            MainCard.Visibility = Visibility.Hidden;
        }

        private void initialPageLoad()
        {
            resetPage();
            hideAllViews();
            SelectionView.Visibility = Visibility.Visible;

            loadCaptureComboBox();
        }

        private void showStatusView()
        {

        }

        private void loadCaptureComboBox()
        {
            if (!CurrentSystem.audit)
            {
                List<Capture> temp = new List<Capture>();
                List<Capture> list = db.getCaptures(CurrentSystem.machineID);

                foreach (Capture capture in list)
                {
                    //Make sure capture is not sealed AND the capture is in state for current group of user to proceed
                    if (capture.status != 3 && db.getSequence(capture.sequenceid).getGroupAtIndex(capture.currentStep).ID == User.getAssignedGroupID())
                    {
                        temp.Add(capture);
                    }
                }

                CaptureValues.Visibility = Visibility.Visible;

                CaptureValues.ItemsSource = temp;
            }
        }

        private void loadCaptureButton()
        {
            StandardButton button = new StandardButton("Begin_Button", "Begin", MaterialDesignThemes.Wpf.PackIconKind.Play, MaterialDesignThemes.Wpf.PackIconKind.PlayOutline, (Brush)Application.Current.Resources["NewItem"], "Begin");
            button.clicked += beginButtonClicked;
            ButtonPanel.Children.Add(button);
        }

        private void beginButtonClicked(object sender, EventArgs e)
        {
            MainCard.Visibility = Visibility.Visible;

            loadGif();

            beginCapture();
        }

        private void selectionViewInCapture()
        {
            SelectionView.Visibility = Visibility.Hidden;
            ResetView.Visibility = Visibility.Visible;
        }

        private async void beginCapture()
        {
            hideAllViews();

            selectionViewInCapture();

            ButtonPanel.Children.Clear();

            loadingView.Visibility = Visibility.Visible;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int captureid = (CaptureValues.SelectedItem as Capture).id;

            StatusText.Text = "Cleaning Table";

            await Task.Run(() =>
            {
                db.cleanSoftwareTableWithCapture(captureid);
            });

            StatusText.Text = "Getting last capture";

            int systemID = CurrentSystem.machineID;

            Capture lastCapture = await Task.Run(() =>
            {
                return db.getPreviousCapture(systemID);
            });

            StatusText.Text = "Searching WMI";

            await Task.Run(() =>
            {
                db.findWMISoftware(captureid);
            });

            StatusText.Text = "Searching Registry";

            await Task.Run(() =>
            {
                db.findRegistrySoftware(captureid);
            });

            if (lastCapture != null)
            {
                StatusText.Text = "Getting Additions";

                int lastCaptureID = lastCapture.id;

                List<Software> additions = await Task.Run(() =>
                {
                    return db.getPreviousAddedSoftware(lastCaptureID);
                });

                getAdditionInfo(captureid, additions);
            }

            Console.WriteLine(lastCapture.id);

            StatusText.Text = "Fetching From Database";

            List<Software> list = await Task.Run(() =>
            {
                List<Software> list = db.getSoftware(captureid);
                return list;
            });

            StatusText.Text = "Returning Settings";

            foreach (Software software in list)
            {
                if (db.isVisible(software, lastCapture.id))
                {
                    software.Visible = 1;
                    db.updateSoftware(software);
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            showSoftwareGrid(captureid, list, elapsedTime);

        }

        //Updates properties for software that was added from previous capture
        private void getAdditionInfo(int captureID, List<Software> softwareList)
        {
            foreach(Software software in softwareList)
            {
                if (software.RegAdd == 1)
                {
                    try
                    {
                        using (var rootKey = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                        {
                            using (var key = rootKey.OpenSubKey(software.RegKey, false))
                            {
                                if (key != null)
                                {
                                    software.SoftwareName = Convert.ToString(key.GetValue("DisplayName"));
                                    software.SoftwareVersion = Convert.ToString(key.GetValue("DisplayVersion"));
                                    software.SoftwareVendor = Convert.ToString(key.GetValue("Publisher"));
                                    software.CaptureID = captureID;
                                    db.addSoftware(software);
                                }
                            }
                        }
                    }
                    catch (Exception ex) 
                    {

                    }
                }
                else
                {
                    FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(software.Location);

                    if (fileInfo != null)
                    {
                        if (fileInfo.ProductName.Equals(""))
                        {
                            software.SoftwareName = "NOT FOUND";
                        }
                        else
                        {
                            software.SoftwareName = fileInfo.ProductName;
                        }

                        if (fileInfo.ProductVersion.Equals(""))
                        {
                            software.SoftwareVersion = "NOT FOUND";
                        }
                        else
                        {

                            software.SoftwareVersion = fileInfo.ProductVersion;
                        }

                        if (fileInfo.CompanyName.Equals(""))
                        {
                            software.SoftwareVendor = "NOT FOUND";
                        }
                        else
                        {
                            software.SoftwareVendor = fileInfo.CompanyName;
                        }

                        software.CaptureID = captureID;
                        db.addSoftware(software);
                    }



                }
            }
        }

        private void showSoftwareGrid(int captureID, List<Software> passedList, String time)
        {
            hideAllViews();
            selectionViewInCapture();
            AdditionaEntriesCard.Visibility = Visibility.Visible;
            SoftwareView.Visibility = Visibility.Visible;
            TimeElapsedText.Text = time;
            SoftwareGrid.ItemsSource = passedList;
            VisibilityToggle.Children.Clear();
            ToggleItem visibilityToggle = new ToggleItem("Only Visible", false);
            VisibilityToggle.Children.Add(visibilityToggle);

            refreshCountFull();
            loadSaveCaptureButton();
        }

        private void loadSaveCaptureButton()
        {
            StandardButton button = new StandardButton("Save_Button", "Delete", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Save");
            button.clicked += saveCaptureClicked;
            MainButtonGrid.Children.Add(button);
        }

        private void saveCaptureClicked(object sender, EventArgs e)
        {
            initialPageLoad();
        }

        private void refreshCountFull()
        {
            resetCounts();
            setupCount(SoftwareGrid.ItemsSource as List<Software>);

            totalitems.Text = total.ToString();
            visibleitems.Text = totalVisible.ToString();
            applicationitems.Text = totalApplication.ToString();
            registryitems.Text = totalRegistry.ToString();
            driveritems.Text = totalDriver.ToString();
            securityitems.Text = totalSecurity.ToString();
        }

        private void loadGif()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(loadingGifLoc, UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(loadingGif, image);
        }

        private void CaptureValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Capture item = (Capture)CaptureValues.SelectedItem;

            if (item != null)
            {
                ButtonPanel.Children.Clear();

                loadCaptureButton();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            resetCounts();
            filter(sender, e);
            if (total == 0 && SearchBox.Text.Equals(""))
            {
                setupCount(SoftwareGrid.ItemsSource as List<Software>);
            }

            totalitems.Text = total.ToString();
            visibleitems.Text = totalVisible.ToString();
            applicationitems.Text = totalApplication.ToString();
            registryitems.Text = totalRegistry.ToString();
            driveritems.Text = totalDriver.ToString();
            securityitems.Text = totalSecurity.ToString();
        }

        private void resetCounts()
        {
            total = 0;
            totalVisible = 0;
            totalApplication = 0;
            totalRegistry = 0;
            totalDriver = 0;
            totalSecurity = 0;
        }

        private void setupCount(List<Software> softwareList)
        {
            foreach (Software software in softwareList)
            {
                total++;

                if (software.isVisible())
                {
                    totalVisible++;
                }

                switch (software.SoftwareType)
                {
                    case "APPLICATION":
                        totalApplication++;
                        break;
                    case "REGISTRY":
                        totalRegistry++;
                        break;
                    case "DRIVER":
                        totalDriver++;
                        break;
                    case "SECURITY":
                        totalSecurity++;
                        break;
                }
            }
        }

        private void filter(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            string filter = t.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(SoftwareGrid.ItemsSource);
            if (filter == "")
                filterVisibility();
            else
            {
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((VisibilityToggle.Children[0] as ToggleItem).isOn)
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1);
                    }
                    else
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()))
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()));
                    }


                };
            }
        }

        private void SoftwareGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Software item = (Software)(SoftwareGrid.SelectedItem);

            if (item != null)
            {
                item.toggle();
                db.updateSoftware(item);
                refreshCountFull();
            }
        }

        private void AddFile_MouseEnter(object sender, MouseEventArgs e)
        {
            AddFile.Opacity = 0.5;
        }

        private void AddFile_MouseLeave(object sender, MouseEventArgs e)
        {
            AddFile.Opacity = 1;
        }

        private void AddFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".exe";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                selectedFileName = dlg.FileName;
                infoButtonPanel.Children.Clear();
                loadInfoCardAddExe();
            }
        }

        private void AddReg_MouseEnter(object sender, MouseEventArgs e)
        {
            AddReg.Opacity = 0.5;
        }

        private void AddReg_MouseLeave(object sender, MouseEventArgs e)
        {
            AddReg.Opacity = 1;
        }

        private void AddReg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetinfoCardView();
            infoCard.Visibility = Visibility.Visible;
            RegAddView.Visibility = Visibility.Visible;
            RegButtonPanel.Children.Clear();

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void loadInfoCardAddExe()
        {
            resetinfoCardView();
            infoCard.Visibility = Visibility.Visible;
            AddExeView.Visibility = Visibility.Visible;

            FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(selectedFileName);

            if (fileInfo.ProductName.Equals(""))
            {
                fileNameText.Text = "NOT FOUND";
            }
            else
            {
                fileNameText.Text = fileInfo.ProductName;
            }

            if (fileInfo.ProductVersion.Equals(""))
            {
                fileVersionText.Text = "NOT FOUND";
            }
            else
            {

                fileVersionText.Text = fileInfo.ProductVersion;
            }

            if (fileInfo.CompanyName.Equals(""))
            {
                fileVendorText.Text = "NOT FOUND";
            }
            else
            {
                fileVendorText.Text = fileInfo.CompanyName;
            }

            TypeValues.Items.Clear();

            TypeValues.Items.Add("APPLICATION");
            TypeValues.Items.Add("DRIVER");
            TypeValues.Items.Add("SECURITY");

            ToggleItem toggle = new ToggleItem("Visible", true);

            ToggleGrid.Children.Clear();

            ToggleGrid.Children.Add(toggle);

            infoButtonPanel.Children.Clear();
        }

        private void resetinfoCardView()
        {
            AddExeView.Visibility = Visibility.Hidden;
            RegAddView.Visibility = Visibility.Hidden;
            infoButtonPanel.Children.Clear();
            regKeyText.Text = "";
            ToggleRegGrid.Children.Clear();
        }

        private void loadSaveAdditionButton()
        {
            StandardButton button = new StandardButton("Save_Button", "Delete", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Add");
            button.clicked += addSoftwareClicked;
            infoButtonPanel.Children.Add(button);
        }

        private void addSoftwareClicked(object sender, EventArgs e)
        {
            Software software = new Software();
            software.SoftwareName = fileNameText.Text;
            software.SoftwareVersion = fileVersionText.Text;
            software.SoftwareVendor = fileVendorText.Text;
            software.SoftwareType = TypeValues.Text;
            software.CaptureID = (CaptureValues.SelectedItem as Capture).id;
            software.Added = 1;
            software.Location = selectedFileName;

            if ((ToggleGrid.Children[0] as ToggleItem).isOn)
            {
                software.Visible = 1;
            }
            else
            {
                software.Visible = 0;
            }

            db.addSoftware(software);
            infoCard.Visibility = Visibility.Hidden;
            refreshList();
            refreshCountFull();
        }

        private void TypeValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            infoButtonPanel.Children.Clear();
            loadSaveAdditionButton();
        }

        private void refreshList()
        {
            SearchBox.Text = "";
            List<Software> list = db.getSoftware((CaptureValues.SelectedItem as Capture).id);
            int lastCaptureID = db.getPreviousCapture(CurrentSystem.machineID).id;

            foreach (Software software in list)
            {
                if (db.isVisible(software, lastCaptureID))
                {
                    software.Visible = 1;
                }
            }

            SoftwareGrid.ItemsSource = list;

        }

        private void ResetCapture_MouseEnter(object sender, MouseEventArgs e)
        {
            ResetCapture.Opacity = 0.5;
        }

        private void ResetCapture_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetCapture.Opacity = 1;
        }

        private void ResetCapture_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            initialPageLoad();
        }

        private void regKeyText_TextChanged(object sender, TextChangedEventArgs e)
        {
            regSaveViewChange();
        }

        public void regSaveViewChange()
        {
            if (!regKeyText.Text.Equals(""))
            {
                try
                {
                    using (var rootKey = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        using (var key = rootKey.OpenSubKey(regKeyText.Text, false))
                        {
                            if (key != null)
                            {
                                regNameText.Text = Convert.ToString(key.GetValue("DisplayName"));
                                regVersionText.Text = Convert.ToString(key.GetValue("DisplayVersion"));
                                regVendorText.Text = Convert.ToString(key.GetValue("Publisher"));

                                ToggleItem toggle = new ToggleItem("Visible", true);
                                ToggleRegGrid.Children.Add(toggle);
                                FoundRegistryItemView.Visibility = Visibility.Visible;
                                NoRegItemFound.Visibility = Visibility.Hidden;

                                RegButtonPanel.Children.Clear();
                                loadSaveRegButton();
                            }
                            else
                            {
                                FoundRegistryItemView.Visibility = Visibility.Hidden;
                                NoRegItemFound.Visibility = Visibility.Visible;
                                RegButtonPanel.Children.Clear();
                                ToggleRegGrid.Children.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
                {
                    FoundRegistryItemView.Visibility = Visibility.Hidden;
                    NoRegItemFound.Visibility = Visibility.Visible;
                    RegButtonPanel.Children.Clear();
                    ToggleRegGrid.Children.Clear();
                }
            }
            else
            {
                FoundRegistryItemView.Visibility = Visibility.Hidden;
                NoRegItemFound.Visibility = Visibility.Hidden;
                RegButtonPanel.Children.Clear();
                ToggleRegGrid.Children.Clear();
            }
        }

        private void loadSaveRegButton()
        {
            StandardButton button = new StandardButton("Save_Button", "Delete", MaterialDesignThemes.Wpf.PackIconKind.ContentSave, MaterialDesignThemes.Wpf.PackIconKind.ContentSaveOutline, (Brush)Application.Current.Resources["NewItem"], "Add");
            button.clicked += addRegClicked;
            RegButtonPanel.Children.Add(button);
        }

        private void addRegClicked(object sender, EventArgs e)
        {
            Software software = new Software();
            software.SoftwareName = regNameText.Text;
            software.SoftwareVersion = regVersionText.Text;
            software.SoftwareVendor = regVendorText.Text;
            software.SoftwareType = "REGISTRY";
            software.CaptureID = (CaptureValues.SelectedItem as Capture).id;
            software.Added = 1;
            software.RegAdd = 1;
            software.RegKey = regKeyText.Text;

            if ((ToggleRegGrid.Children[0] as ToggleItem).isOn)
            {
                software.Visible = 1;
            }
            else
            {
                software.Visible = 0;
            }

            db.addSoftware(software);
            infoCard.Visibility = Visibility.Hidden;
            refreshList();
            refreshCountFull();
        }

        private void VisibilityToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetCounts();
            filterVisibility();
            if (total == 0 && SearchBox.Text.Equals(""))
            {
                setupCount(SoftwareGrid.ItemsSource as List<Software>);
            }

            totalitems.Text = total.ToString();
            visibleitems.Text = totalVisible.ToString();
            applicationitems.Text = totalApplication.ToString();
            registryitems.Text = totalRegistry.ToString();
            driveritems.Text = totalDriver.ToString();
            securityitems.Text = totalSecurity.ToString();
        }

        private void filterVisibility()
        {
            string filter = SearchBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(SoftwareGrid.ItemsSource);
            if (filter == "")
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((VisibilityToggle.Children[0] as ToggleItem).isOn)
                    {
                        if (p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.Visible == 1);
                    }
                    else
                    {
                        if (p.Visible == 0 || p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.Visible == 0 || p.Visible == 1);
                    }


                };
            else
            {
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((VisibilityToggle.Children[0] as ToggleItem).isOn)
                    {

                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1);
                    }
                    else
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()))
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.SoftwareType)
                            {
                                case "APPLICATION":
                                    totalApplication++;
                                    break;
                                case "REGISTRY":
                                    totalRegistry++;
                                    break;
                                case "DRIVER":
                                    totalDriver++;
                                    break;
                                case "SECURITY":
                                    totalSecurity++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()));
                    }

                };
            }
        }
    }
}
