using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using SEAL_V2.db;
using Microsoft.Win32;

namespace SEAL_V2.model
{
    class CurrentSystem
    {
        public static int initialID {get;set;}
        public static int machineID { get; set; }
        public static String machineName { get; set; }
        public static String model { get; set; }
        public static String manufacturer { get; set; }
        public static String serial { get; set; }

        public static int nicknameAdded { get; set; }

        public static String nickname { get; set; }
        public static int dirAdded { get; set; }
        public static int dirID { get; set; }
        public static int regadded { get; set; }
        public static String regKey { get; set; }
        public static String regValue { get; set; }

        public static bool audit { get; set; }
        public static DatabaseInterface db = DatabaseInterface.Instance;

        public static void systemSetup()
        {
            audit = false;
            setupSystemInfo();
        }

        private static void setupSystemInfo()
        {
            if (audit)
            {

            }
            else
            {
                localPropertySetup();
                getAssignValues();
            }

        }

        private static void localPropertySetup()
        {
            getMachineName();
            getModelandSerial();
        }

        private static void getMachineName()
        {
            machineName = Environment.MachineName;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_BaseBoard");

            ManagementObjectCollection information = searcher.Get();
            foreach (ManagementObject obj in information)
            {
                foreach (PropertyData data in obj.Properties)
                {
                    manufacturer = data.Value.ToString();
                }
            }
        }

        private static void getModelandSerial()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, IdentifyingNumber FROM Win32_ComputerSystemProduct");

            ManagementObjectCollection information = searcher.Get();
            foreach (ManagementObject obj in information)
            {
                foreach (PropertyData data in obj.Properties)
                {
                    if (data.Name.ToString().Equals("Name"))
                    {
                        model = data.Value.ToString();
                    }
                    else
                    {
                        serial = data.Value.ToString();
                    }

                }
            }
        }

        private static void getAssignValues()
        {
            if (db.doesSystemExist(CurrentSystem.model))
            {
                db.loadLocalSystem(CurrentSystem.model);
                CurrentSystem.initialID = CurrentSystem.machineID;
            }
            else
            {
                noModelFound();
            }

        }

        private static void noModelFound()
        {
            nicknameAdded = 0;
            dirAdded = 0;
            regadded = 0;
            regKey = "";
            regValue = "";
        }

        public static bool nicknameExists()
        {
            bool result = false;

            if (nicknameAdded > 0)
            {
                result = true;
            }

            return result;
        }

        public static bool customVersionExists()
        {
            bool result = false;

            if (regadded > 0)
            {
                result = true;
            }

            return result;
        }

        public static String getRegVersion()
        {
            String result = "ERROR FETCHING";

            try
            {
                using (var rootKey = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = rootKey.OpenSubKey(CurrentSystem.regKey, false))
                    {
                        if (key != null)
                        {
                            string found = Convert.ToString(key.GetValue(CurrentSystem.regValue));

                            if (found.Equals(""))
                            {
                                result = "NO VERSION";
                            }
                            else
                            {
                                result = found;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                
            }

            return result;
        }

        public static void resetAttributes()
        {
            machineID = 0;
            machineName = "";
            model = "";
            manufacturer = "";
            serial = "";
            nicknameAdded = 0;
            nickname = "";
            dirAdded = 0;
            dirID = 0;
            regadded = 0;
            regKey = "";
            regValue = "";

            setupSystemInfo();
        }

        public static void auditSystem(SystemObject passedSystem)
        {
            audit = true;
            machineID = passedSystem.id;
            machineName = "N/A";
            model = passedSystem.modelname;
            manufacturer = "N/A";
            serial = "N/A";
            nicknameAdded = passedSystem.addnickname;
            nickname = passedSystem.nickname;
            dirAdded = passedSystem.assigneddir;
            dirID = passedSystem.DirID;
            regadded = 0;
            regKey = "";
            regValue = "";
        }
    }
}
