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
using SEAL_V2.view.usercontrolobjects;
using System.ComponentModel;
using WpfAnimatedGif;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for VerifyPage.xaml
    /// </summary>
    public partial class VerifyPage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        public long objectID;
        private String objectName = "Verify_Page";
        private DatabaseInterface db = DatabaseInterface.Instance;
        public String name = "Verify";
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;
        public String loadingGifLoc = "images/WaitCover.gif";
        private int total = 0;
        private int totalVisible;
        private int totalApplication;
        private int totalRegistry;
        private int totalDriver;
        private int totalSecurity;
        private int totalNoChange;
        private int totalUpgrade;
        private int totalDowngrade;
        private int totalNew;
        private int totalRemoved;
        private int totalDuplicate;


        public VerifyPage()
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
            hideAllViews();
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
        }

        private void hideSelectionCardViews()
        {
            SelectionView.Visibility = Visibility.Hidden;
            ResetView.Visibility = Visibility.Hidden;
        }

        private void hideAllCardsButSelection()
        {
            MainCard.Visibility = Visibility.Hidden;
            SaveCard.Visibility = Visibility.Hidden;
            infoCard.Visibility = Visibility.Hidden;
        }

        private void initialPageLoad()
        {
            hideAllCardsButSelection();
            hideSelectionCardViews();
            SelectionView.Visibility = Visibility.Visible;
            loadComboBox();
        }

        private void loadComboBox()
        {
            if (!CurrentSystem.audit)
            {
                List<Capture> temp = new List<Capture>();
                List<Capture> list = db.getCaptures(CurrentSystem.machineID);

                foreach (Capture capture in list)
                {
                    //Make sure capture is not sealed AND the capture is in state for current group of user to proceed AND there exists a software capture to validate against
                    if (capture.status != 3 && db.getSequence(capture.sequenceid).getGroupAtIndex(capture.currentStep).ID == User.getAssignedGroupID() && db.doesSoftwareExistInCapture(capture.id))
                    {
                        temp.Add(capture);
                    }
                }

                CaptureValues.Visibility = Visibility.Visible;

                CaptureValues.ItemsSource = temp;
            }
        }

        private void CaptureValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Capture item = (Capture)CaptureValues.SelectedItem;

            if (item != null)
            {
                SelectionButtonPanel.Children.Clear();

                showCapture();
            }
        }

        private void clearMainCard()
        {
            loadingView.Visibility = Visibility.Hidden;
            SoftwareView.Visibility = Visibility.Hidden;
            CompareSoftwareView.Visibility = Visibility.Hidden;
        }

        private void showCapture()
        {
            clearMainCard();
            MainCard.Visibility = Visibility.Visible;
            SoftwareView.Visibility = Visibility.Visible;
            VisibilityToggle.Children.Clear();
            ToggleItem visibilityToggle = new ToggleItem("Only Visible", false);
            VisibilityToggle.Children.Add(visibilityToggle);

            loadCaptureDetails();
        }

        private void loadCaptureDetails()
        {
            SoftwareGrid.Visibility = Visibility.Visible;
            SoftwareGrid.ItemsSource = db.getSoftware((CaptureValues.SelectedItem as Capture).id);

            setupCount(SoftwareGrid.ItemsSource as List<Software>);

            totalitems.Text = total.ToString();
            visibleitems.Text = totalVisible.ToString();
            applicationitems.Text = totalApplication.ToString();
            registryitems.Text = totalRegistry.ToString();
            driveritems.Text = totalDriver.ToString();
            securityitems.Text = totalSecurity.ToString();

            loadVerifyButton();
        }

        public void loadVerifyButton()
        {
            StandardButton button = new StandardButton("Verify_Button", "Verify", MaterialDesignThemes.Wpf.PackIconKind.Check, MaterialDesignThemes.Wpf.PackIconKind.CheckOutline, (Brush)Application.Current.Resources["NewItem"], "Verify");
            button.clicked += beginVerifyClicked;
            SelectionButtonPanel.Children.Add(button);
        }

        public void beginVerifyClicked(object sender, EventArgs e)
        {
            hideAllViews();

            SelectionButtonPanel.Children.Clear();

            ResetView.Visibility = Visibility.Visible;

            clearMainCard();

            loadGif();

            beginVerify();
        }

        private async void beginVerify()
        {
            loadingView.Visibility = Visibility.Visible;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            StatusText.Text = "Cleaning Table";

            int curCaptureID = (CaptureValues.SelectedItem as Capture).id;

            await Task.Run(() =>
            {
                db.cleanSoftwareTableWithCapture(0);
                db.cleanSoftwareCaptureTable(curCaptureID);
            });

            StatusText.Text = "Searching WMI";

            await Task.Run(() =>
            {
                db.findWMISoftware(0);
            });

            StatusText.Text = "Searching Registry";

            await Task.Run(() =>
            {
                db.findRegistrySoftware(0);
            });

            StatusText.Text = "Getting Additions";

            List<Software> additions = await Task.Run(() =>
            {
                return db.getPreviousAddedSoftware(curCaptureID);
            });

            getAdditionInfo(0, additions);

            StatusText.Text = "Fetching From Database";

            List<Software> alist = await Task.Run(() =>
            {
                List<Software> list = db.getSoftware(0);
                return list;
            });

            List<Software> blist = await Task.Run(() =>
            {
                List<Software> list = db.getSoftware(curCaptureID);
                return list;
            });

            StatusText.Text = "Comparing Captures";

            List<SoftwareCompare> compareList = await Task.Run(() =>
            {
                SoftwareComparison compare = new SoftwareComparison(alist, blist);
                compare.loadDictionaries();
                compare.softwareComparison();
                return compare.getCompareList();
            });

            StatusText.Text = "Complete";

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            showCaptureSoftwareGrid(compareList, elapsedTime);
        }

        private void getAdditionInfo(int captureID, List<Software> softwareList)
        {
            foreach (Software software in softwareList)
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

        private void showCaptureSoftwareGrid(List<SoftwareCompare> compareList, String timeText)
        {
            clearMainCard();
            CompareSoftwareView.Visibility = Visibility.Visible;
            SaveCard.Visibility = Visibility.Visible;
            infoCard.Visibility = Visibility.Visible;

            VerifyResults.Text = "";
            CompareVisibilityToggle.Children.Clear();
            ToggleItem visibilityToggle = new ToggleItem("Only Visible", (VisibilityToggle.Children[0] as ToggleItem).isOn);
            CompareVisibilityToggle.Children.Add(visibilityToggle);

            TimeElapsedText.Text = timeText;

            CompareSoftwareGrid.ItemsSource = compareList;

            if ((CompareVisibilityToggle.Children[0] as ToggleItem).isOn)
            {
                resetCounts();
                compareFilterVisibility();
            }
            else
            {
                setupCompareCount(compareList);
            }
                    

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();

            allSoftwareMatch();
        }

        private void loadGif()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(loadingGifLoc, UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(loadingGif, image);
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

        private void resetCounts()
        {
            total = 0;
            totalVisible = 0;
            totalApplication = 0;
            totalRegistry = 0;
            totalDriver = 0;
            totalSecurity = 0;
            totalNoChange = 0;
            totalUpgrade = 0;
            totalDowngrade = 0;
            totalNew = 0;
            totalRemoved = 0;
            totalDuplicate = 0;
        }

        private void setupCount(List<Software> softwareList)
        {
            resetCounts();

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

        private void setupCompareCount(List<SoftwareCompare> softwareList)
        {
            resetCounts();

            foreach (SoftwareCompare software in softwareList)
            {
                total++;

                if (software.isVisible())
                {
                    totalVisible++;
                }

                switch (software.Type)
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

                switch (software.Comparison)
                {
                    case 0:
                        totalNoChange++;
                        break;
                    case 1:
                        totalUpgrade++;
                        break;
                    case 2:
                        totalDowngrade++;
                        break;
                    case 3:
                        totalNew++;
                        break;
                    case 4:
                        totalRemoved++;
                        break;
                    case 5:
                        totalDuplicate++;
                        break;
                }
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

        private void CompareVisibilityToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetCounts();
            compareFilterVisibility();

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();

            allSoftwareMatch();
        }

        private void CompareSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            resetCounts();
            compareFilter(sender, e);

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();

            allSoftwareMatch();
        }

        private void compareFilter(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            string filter = t.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CompareSoftwareGrid.ItemsSource);
            if (filter == "")
                compareFilterVisibility();
            else
            {
                cv.Filter = o =>
                {
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((CompareVisibilityToggle.Children[0] as ToggleItem).isOn)
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.Version.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.Version.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1);
                    }
                    else
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()))
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()));
                    }
                };
            }
        }

        private void compareFilterVisibility()
        {
            string filter = CompareSearchBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CompareSoftwareGrid.ItemsSource);

            if (filter == "")
                cv.Filter = o =>
                {
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((CompareVisibilityToggle.Children[0] as ToggleItem).isOn)
                    {
                        if (p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
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

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
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
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((CompareVisibilityToggle.Children[0] as ToggleItem).isOn)
                    {

                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.Version.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1)
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1 || p.Version.ToUpper().StartsWith(filter.ToUpper()) && p.Visible == 1);
                    }
                    else
                    {
                        if (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()))
                        {
                            total++;
                            if (p.isVisible())
                            {
                                totalVisible++;
                            }

                            switch (p.Type)
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

                            switch (p.Comparison)
                            {
                                case 0:
                                    totalNoChange++;
                                    break;
                                case 1:
                                    totalUpgrade++;
                                    break;
                                case 2:
                                    totalDowngrade++;
                                    break;
                                case 3:
                                    totalNew++;
                                    break;
                                case 4:
                                    totalRemoved++;
                                    break;
                                case 5:
                                    totalDuplicate++;
                                    break;
                            }
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()));
                    }

                };
            }
        }

        private void ResetCapture_MouseEnter(object sender, MouseEventArgs e)
        {
            ResetCapture.Opacity = 0.5;
        }

        private void ResetCapture_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetCapture.Opacity = 1;
        }

        private void ResetCapture_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            initialPageLoad();
        }

        private void allSoftwareMatch()
        {
            int nonChanges = totalUpgrade + totalDowngrade + totalNew + totalRemoved + totalDuplicate;

            if (nonChanges > 0)
            {
                VerifyResults.Text = "FAILED";
                VerifyResults.Foreground = ((SolidColorBrush)Application.Current.Resources["RemovedItem"]);
            }
            else
            {
                VerifyResults.Text = "PASSED";
                VerifyResults.Foreground = ((SolidColorBrush)Application.Current.Resources["NewItem"]);

            }
        }

        private void SaveVerify_MouseEnter(object sender, MouseEventArgs e)
        {
            SaveVerify.Opacity = 0.5;
        }

        private void SaveVerify_MouseLeave(object sender, MouseEventArgs e)
        {
            SaveVerify.Opacity = 1;
        }

        private async void SaveVerify_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            List<SoftwareCompare> list = CompareSoftwareGrid.ItemsSource as List<SoftwareCompare>;

            foreach (SoftwareCompare item in list)
            {
                item.CaptureID = (CaptureValues.SelectedItem as Capture).id;
            }

            Saveviewbox.Visibility = Visibility.Hidden;
            Savingviewbox.Visibility = Visibility.Visible;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(loadingGifLoc, UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(SavingIcon, image);

            await Task.Run(() =>
            {
                db.saveSoftwareCompare(list);
            });

            Savingviewbox.Visibility = Visibility.Hidden;
            Saveviewbox.Visibility = Visibility.Visible;

            initialPageLoad();
        }
    }
}
