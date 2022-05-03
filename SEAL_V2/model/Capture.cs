using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using SEAL_V2.db;
using System.Windows.Media;
using System.Windows;

namespace SEAL_V2.model
{
    public class Capture
    {
        private DatabaseInterface db = DatabaseInterface.Instance;

        public int id { get; set; }
        public String name { get; set; }
        public int systemid { get; set; }
        public int sequenceid { get; set; }
        public int currentStep { get; set; }
        public int status { get; set; }
        public SolidColorBrush color { get; set; }
        public String statusText { get; set; }
        public PackIconKind icon { get; set; }
        public String ratio { get; set; }


        public PackIconKind statusIcon { get; set; }

        public Capture(int id, String name, int systemid, int sequenceid, int currentstep, int status)
        {
            this.id = id;
            this.name = name;
            this.systemid = systemid;
            this.sequenceid = sequenceid;
            this.currentStep = currentstep;
            this.status = status;
            setup();
        }

        private void setup()
        {
            switch (status)
            {
                case 0:
                    statusText = "Not Started";
                    icon = PackIconKind.NewReleases;
                    color = ((SolidColorBrush)Application.Current.Resources["OnBackground"]);
                    break;
                case 1:
                    statusText = "In Progress";
                    icon = PackIconKind.ProgressClock;
                    color = ((SolidColorBrush)Application.Current.Resources["InProgress"]);
                    break;
                case 2:
                    statusText = "Reverted";
                    icon = PackIconKind.Error;
                    color = ((SolidColorBrush)Application.Current.Resources["RemovedItem"]);
                    break;
                case 3:
                    statusText = "Complete";
                    icon = PackIconKind.Seal;
                    color = ((SolidColorBrush)Application.Current.Resources["NewItem"]);
                    break;
                default:
                    statusText = "NOT FOUND";
                    color = ((SolidColorBrush)Application.Current.Resources["OnBackground"]);
                    icon = PackIconKind.Null;
                    break;
            }

            ratio = (currentStep + 1).ToString() + "/" + (db.getSequence(sequenceid).sequenceLength).ToString();
        }

    }
}
