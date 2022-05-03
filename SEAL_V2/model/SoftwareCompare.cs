using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SEAL_V2.model
{
    class SoftwareCompare : INotifyPropertyChanged, IComparable
    {
        private int id;
        private String softwarename;
        private String version;
        private String foundversion;
        private String vendor;
        private String type;
        private int captureid;
        private int visible;
        private int comparison;
        private String comparisonstring;
        public event PropertyChangedEventHandler PropertyChanged;

        public SoftwareCompare()
        {

        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public String SoftwareName
        {
            get
            {
                return softwarename;
            }
            set
            {
                softwarename = value;
            }
        }

        public String Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }

        public String FoundVersion
        {
            get
            {
                return foundversion;
            }
            set
            {
                foundversion = value;
            }
        }

        public String Vendor
        {
            get
            {
                return vendor;
            }
            set
            {
                vendor = value;
            }
        }

        public String Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public int CaptureID
        {
            get
            {
                return captureid;
            }
            set
            {
                captureid = value;
            }
        }

        public int Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        public int Comparison
        {
            get
            {
                return comparison;
            }
            set
            {
                comparison = value;
                setStringValue();
                NotifyPropertyChanged("Comparison");
            }
        }

        public String ComparisonString
        {
            get
            {
                return comparisonstring;
            }
            set
            {
                comparisonstring = value;
                NotifyPropertyChanged("comparisonstring");
            }
        }

        private void setStringValue()
        {
            switch (comparison)
            {
                case 0:
                    ComparisonString = "NO CHANGE";
                    break;
                case 1:
                    ComparisonString = "UPGRADED";
                    break;
                case 2:
                    ComparisonString = "DOWNGRADED";
                    break;
                case 3:
                    ComparisonString = "NEW";
                    break;
                case 4:
                    ComparisonString = "REMOVED";
                    break;
                case 5:
                    ComparisonString = "DUPLICATE";
                    break;
            }

        }

        public bool isVisible()
        {
            if (visible > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            SoftwareCompare otherSoftware = obj as SoftwareCompare;
            if (otherSoftware != null)
                return this.SoftwareName.CompareTo(otherSoftware.SoftwareName);
            else
                throw new ArgumentException("Object is not software");
        }
    }
}
