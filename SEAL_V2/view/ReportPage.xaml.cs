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
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using regx = System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SEAL_V2.view
{
    /// <summary>
    /// Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page, Pages, MessageProtocol, objectIDRequirements
    {
        private long objectID;
        private String objectName = "Reports_Page";
        private DatabaseInterface db = DatabaseInterface.Instance;
        private string name = "Reports";
        private Dictionary<long, object> objects = new Dictionary<long, object>();
        public event EventHandler<StatusMessage> message;
        private int total;
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
        private List<Software> currentFilteredCaptureList = new List<Software>();
        private List<SoftwareCompare> currentFilteredVerifyList = new List<SoftwareCompare>();


        public ReportPage()
        {
            InitializeComponent();

            loadObjectID();
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

        }

        public void initialPageLoad()
        {
            resetButtons();
            hideAllCards();
        }

        private void resetButtons()
        {
            CameraIcon.Kind = PackIconKind.Camera;
            CameraIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["EditItem"]);
            CheckIcon.Kind = PackIconKind.CheckBold;
            CheckIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["EditItem"]);
            CompareIcon.Kind = PackIconKind.FileCompare;
            CompareIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["EditItem"]);
        }

        private void CapturePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            CapturePanel.Opacity = 0.5;
        }

        private void CapturePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            CapturePanel.Opacity = 1;
        }

        private void CapturePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetButtons();
            hideAllCards();
            CameraIcon.Kind = PackIconKind.CameraOutline;
            CameraIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
            loadCaptureSelection();
        }

        private void VerifyPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            VerifyPanel.Opacity = 0.5;
        }

        private void VerifyPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            VerifyPanel.Opacity = 1;
        }

        private void VerifyPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetButtons();
            hideAllCards();
            CheckIcon.Kind = PackIconKind.CheckOutline;
            CheckIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
            loadVerifySelection();
        }

        private void ComparisonPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            ComparisonPanel.Opacity = 0.5;
        }

        private void ComparisonPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            ComparisonPanel.Opacity = 1;
        }

        private void ComparisonPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetButtons();
            hideAllCards();
            CompareIcon.Foreground = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
            CompareIcon.Kind = PackIconKind.SelectCompare;
        }

        public void hideAllCards()
        {
            SelectionCard.Visibility = Visibility.Hidden;
            MainCard.Visibility = Visibility.Hidden;
            infoCard.Visibility = Visibility.Hidden;
        }

        public void hideAllSelectionCardViews()
        {
            CaptureSelectView.Visibility = Visibility.Hidden;
            VerifySelectView.Visibility = Visibility.Hidden;
            CaptureExcelPanel.Visibility = Visibility.Hidden;
            VerifyExcelPanel.Visibility = Visibility.Hidden;
            CaptureValues.SelectedItem = null;
            VerifyValues.SelectedItem = null;
        }

        public void hideAllMainCardViews()
        {
            SoftwareCaptureView.Visibility = Visibility.Hidden;
            VerifySoftwareView.Visibility = Visibility.Hidden;
        }

        public void loadCaptureSelection()
        {
            hideAllSelectionCardViews();
            SelectionCard.Visibility = Visibility.Visible;
            CaptureSelectView.Visibility = Visibility.Visible;
            List<Capture> list = new List<Capture>();
            List<Capture> temp = db.getCaptures(CurrentSystem.machineID);

            foreach(Capture capture in temp)
            {
                if (db.doesSoftwareExistInCapture(capture.id))
                {
                    list.Add(capture);
                }
            }

            CaptureValues.ItemsSource = list;
        }

        public void loadVerifySelection()
        {
            hideAllSelectionCardViews();
            SelectionCard.Visibility = Visibility.Visible;
            VerifySelectView.Visibility = Visibility.Visible;
            List<Capture> templist = new List<Capture>();
            List<Capture> tempdbpull = db.getCaptures(CurrentSystem.machineID);

            foreach (Capture capture in tempdbpull)
            {
                if (db.doesSoftwareCompareExistInCapture(capture.id))
                {
                    templist.Add(capture);
                }
            }

            VerifyValues.ItemsSource = templist;
        }

        private void CaptureValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Capture item = (Capture)CaptureValues.SelectedItem;

            if (item != null)
            {
                loadCaptureView();
            }
        }

        private void VerifyValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Capture item = (Capture)VerifyValues.SelectedItem;

            if (item != null)
            {
                loadVerifyView();
            }
        }


        private void loadCaptureView()
        {
            hideAllMainCardViews();
            MainCard.Visibility = Visibility.Visible;
            SoftwareCaptureView.Visibility = Visibility.Visible;
            List<Software> temp = db.getSoftware((CaptureValues.SelectedItem as Capture).id);
            CapturedSoftwareGrid.ItemsSource = temp;
            currentFilteredCaptureList = temp;

            CaptureVisibilityToggle.Children.Clear();
            ToggleItem visibilityToggle = new ToggleItem("Only Visible", false);
            CaptureVisibilityToggle.Children.Add(visibilityToggle);
            if (doesExcelExists())
            {
                CaptureExcelPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CaptureExcelPanel.Visibility = Visibility.Hidden;
            }

            refreshCaptureCount();
        }

        private void loadVerifyView()
        {
            hideAllMainCardViews();
            MainCard.Visibility = Visibility.Visible;
            VerifySoftwareView.Visibility = Visibility.Visible;
            infoCard.Visibility = Visibility.Visible;
            List<SoftwareCompare> temp = db.getSoftwareCompare((VerifyValues.SelectedItem as Capture).id);
            CompareSoftwareGrid.ItemsSource = temp;
            currentFilteredVerifyList = temp;

            VerifyVisibilityToggle.Children.Clear();
            ToggleItem visibilityToggle = new ToggleItem("Only Visible", false);
            VerifyVisibilityToggle.Children.Add(visibilityToggle);

            if (doesExcelExists())
            {
                VerifyExcelPanel.Visibility = Visibility.Visible;
            }
            else
            {
                VerifyExcelPanel.Visibility = Visibility.Hidden;
            }

            refreshVerifyCount();
        }

        private void CaptureVisibilityToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetCounts();
            filterCaptureVisibility();

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
        }

        private void CaptureSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            resetCounts();
            filterCapture(sender, e);
            if (total == 0 && CaptureSearchBox.Text.Equals(""))
            {
                setupCount(CapturedSoftwareGrid.ItemsSource as List<Software>);
            }

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
        }

        private void filterCapture(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            string filter = t.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CapturedSoftwareGrid.ItemsSource);
            currentFilteredCaptureList = new List<Software>();
            if (filter == "")
                filterCaptureVisibility();
            else
            {
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((CaptureVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredCaptureList.Add(p);
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

                            currentFilteredCaptureList.Add(p);
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()));
                    }


                };
            }
        }

        private void filterCaptureVisibility()
        {
            string filter = CaptureSearchBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CapturedSoftwareGrid.ItemsSource);
            currentFilteredCaptureList = new List<Software>();
            if (filter == "")
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((CaptureVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredCaptureList.Add(p);
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

                            currentFilteredCaptureList.Add(p);
                        }

                        return (p.Visible == 0 || p.Visible == 1);
                    }


                };
            else
            {
                cv.Filter = o =>
                {
                    Software p = o as Software;

                    if ((CaptureVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredCaptureList.Add(p);
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

                            currentFilteredCaptureList.Add(p);
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.SoftwareVersion.ToUpper().StartsWith(filter.ToUpper()));
                    }

                };
            }
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

        private void refreshCaptureCount()
        {
            resetCounts();
            setupCount(CapturedSoftwareGrid.ItemsSource as List<Software>);

            comparetotalitems.Text = total.ToString();
            comparevisibleitems.Text = totalVisible.ToString();
            compareapplicationitems.Text = totalApplication.ToString();
            compareregistryitems.Text = totalRegistry.ToString();
            comparedriveritems.Text = totalDriver.ToString();
            comparesecurityitems.Text = totalSecurity.ToString();
        }

        private void refreshVerifyCount()
        {
            resetCounts();
            setupCompareCount(CompareSoftwareGrid.ItemsSource as List<SoftwareCompare>);

            verifycomparetotalitems.Text = total.ToString();
            verifycomparevisibleitems.Text = totalVisible.ToString();
            verifycompareapplicationitems.Text = totalApplication.ToString();
            verifycompareregistryitems.Text = totalRegistry.ToString();
            verifycomparedriveritems.Text = totalDriver.ToString();
            verifysecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();
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

        private bool doesExcelExists()
        {
            bool found = false;

            regx.Regex excelrgx = new regx.Regex("excel", regx.RegexOptions.IgnoreCase);

            String path = "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths";

            RegistryKey key = Registry.LocalMachine.OpenSubKey(path);

            foreach (var v in key.GetSubKeyNames())
            {
                regx.MatchCollection matches = excelrgx.Matches(v.ToString());

                if (matches.Count > 0)
                {
                    found = true;

                    return found;
                }
            }

            return found;
        }

        private void CaptureExcelPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            CaptureExcelPanel.Opacity = 0.5;
        }

        private void CaptureExcelPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            CaptureExcelPanel.Opacity = 1;
        }

        private async void CaptureExcelPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Capture currentCapture = db.getCapture((CaptureValues.SelectedItem as Capture).id);

            await Task.Run(() =>
            {
                try
                {
                    ExcelGenerator.createCaptureDocument(currentFilteredCaptureList, currentCapture);
                }
                catch (Exception e)
                {

                }
            });

        }

        private void VerifyVisibilityToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetCounts();
            filterVerifyVisibility();

            verifycomparetotalitems.Text = total.ToString();
            verifycomparevisibleitems.Text = totalVisible.ToString();
            verifycompareapplicationitems.Text = totalApplication.ToString();
            verifycompareregistryitems.Text = totalRegistry.ToString();
            verifycomparedriveritems.Text = totalDriver.ToString();
            verifysecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();
        }

        private void filterVerifyVisibility()
        {
            string filter = VerifySearchBox.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CompareSoftwareGrid.ItemsSource);
            currentFilteredVerifyList = new List<SoftwareCompare>();
            if (filter == "")
                cv.Filter = o =>
                {
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((VerifyVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredVerifyList.Add(p);
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

                            currentFilteredVerifyList.Add(p);
                        }

                        return (p.Visible == 0 || p.Visible == 1);
                    }


                };
            else
            {
                cv.Filter = o =>
                {
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((VerifyVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredVerifyList.Add(p);
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


                            currentFilteredVerifyList.Add(p);
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()));
                    }

                };
            }
        }

        private void VerifySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            resetCounts();
            filterVerify(sender, e);
            if (total == 0 && VerifySearchBox.Text.Equals(""))
            {
                setupCount(CompareSoftwareGrid.ItemsSource as List<Software>);
            }


            verifycomparetotalitems.Text = total.ToString();
            verifycomparevisibleitems.Text = totalVisible.ToString();
            verifycompareapplicationitems.Text = totalApplication.ToString();
            verifycompareregistryitems.Text = totalRegistry.ToString();
            verifycomparedriveritems.Text = totalDriver.ToString();
            verifysecurityitems.Text = totalSecurity.ToString();
            nochangetotal.Text = totalNoChange.ToString();
            newtotal.Text = totalNew.ToString();
            upgradetotal.Text = totalUpgrade.ToString();
            downgradetotal.Text = totalDowngrade.ToString();
            removetotal.Text = totalRemoved.ToString();
            duplicatetotal.Text = totalDuplicate.ToString();
        }

        private void filterVerify(object sender, TextChangedEventArgs e)
        {
            TextBox t = (TextBox)sender;
            string filter = t.Text;
            ICollectionView cv = CollectionViewSource.GetDefaultView(CompareSoftwareGrid.ItemsSource);
            currentFilteredVerifyList = new List<SoftwareCompare>();
            if (filter == "")
                filterVerifyVisibility();
            else
            {
                cv.Filter = o =>
                {
                    SoftwareCompare p = o as SoftwareCompare;

                    if ((VerifyVisibilityToggle.Children[0] as ToggleItem).isOn)
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

                            currentFilteredVerifyList.Add(p);
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

                            currentFilteredVerifyList.Add(p);
                        }

                        return (p.SoftwareName.ToUpper().StartsWith(filter.ToUpper()) || p.Version.ToUpper().StartsWith(filter.ToUpper()));
                    }

                };
            }
        }

        private void VerifyExcelPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Capture currentCapture = db.getCapture((VerifyValues.SelectedItem as Capture).id);
            try
            {
                ExcelGenerator.createVerifyDocument(currentFilteredVerifyList, currentCapture);
            }
            catch (Exception ex)
            {

            }
        }

        private void VerifyExcelPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            VerifyExcelPanel.Opacity = 0.5;
        }

        private void VerifyExcelPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            VerifyExcelPanel.Opacity = 1;
        }
    }
}
