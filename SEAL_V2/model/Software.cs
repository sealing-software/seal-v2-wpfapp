using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SEAL_V2.model
{
    class Software : INotifyPropertyChanged, IComparable
    {
        private int id;
        private String softwarename;
        private String softwareversion;
        private String softwarevendor;
        private String softwaretype;
        private int captureid;
        private int added;
        private String location;
        private int regadd;
        private String regkey;
        private int visible;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public String SoftwareVersion
        {
            get
            {
                return softwareversion;
            }
            set
            {
                softwareversion = value;
            }
        }

        public String SoftwareVendor
        {
            get
            {
                return softwarevendor;
            }
            set
            {
                softwarevendor = value;
            }
        }

        public String SoftwareType
        {
            get
            {
                return softwaretype;
            }
            set
            {
                softwaretype = value;
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

        public int Added
        {
            get
            {
                return added;
            }
            set
            {
                added = value;
            }
        }

        public String Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        public int RegAdd
        {
            get
            {
                return regadd;
            }
            set
            {
                regadd = value;
            }
        }

        public String RegKey
        {
            get
            {
                return regkey;
            }
            set
            {
                regkey = value;
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

        public void toggle()
        {
            if (isVisible())
            {
                visible = 0;
            }
            else
            {
                visible = 1;
            }
            NotifyPropertyChanged("Visible");
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

            Software otherSoftware = obj as Software;
            if (otherSoftware != null)
                return this.SoftwareName.CompareTo(otherSoftware.SoftwareName);
            else
                throw new ArgumentException("Object is not software");
        }


    }
}
