using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using SEAL_V2.db;
using MaterialDesignThemes.Wpf;

namespace SEAL_V2.model
{
    public class History
    {
        private DatabaseInterface db = DatabaseInterface.Instance;

        public int id { get; set; }
        public int groupID { get; set; }
        public String groupName { get; set; }
        public SolidColorBrush brushColor { get; set; }
        public String username { get; set; }
        public String model { get; set; }
        public String serial { get; set; }
        public int captureID { get; set; }
        public String captureName { get; set; }
        public int currentStep { get; set; }
        public String status { get; set; }
        public int color { get; set; }
        public SolidColorBrush statusColor { get; set; }
        public PackIconKind icon { get; set; }

        public History(int id, int groupid, String groupName, String username, String model, String serial, int captureID, String captureName, int currentStep, String status, int color)
        {
            this.id = id;
            this.groupID = groupid;
            this.groupName = groupName;
            this.username = username;
            this.model = model;
            this.serial = serial;
            this.captureID = captureID;
            this.captureName = captureName;
            this.currentStep = currentStep;
            this.status = status;
            this.color = color;
            setGroupColor();
            setStatusColor();
        }

        private void setGroupColor()
        {
            brushColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(db.getGroupHexColor(groupID)));
        }

        private void setStatusColor()
        {
            statusColor = ((SolidColorBrush)Application.Current.Resources["OnBackground"]);
            icon = PackIconKind.Null;
            switch (status)
            {
                case "COMPLETE":
                    statusColor = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
                    icon = PackIconKind.Seal;
                    break;
                case "SUCCESS":
                    statusColor = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
                    icon = PackIconKind.Check;
                    break;
                case "FAIL":
                    statusColor = ((SolidColorBrush)Application.Current.Resources["RemovedItem"]);
                    icon = PackIconKind.Error;
                    break;
                default:
                    statusColor = ((SolidColorBrush)Application.Current.Resources["OnBackground"]);
                    icon = PackIconKind.NewReleases;
                    break;
            }




        }

    }
}
