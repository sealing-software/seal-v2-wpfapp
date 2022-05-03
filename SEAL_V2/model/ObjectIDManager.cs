using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    class ObjectIDManager
    {
        public static Dictionary<String, long> objectIDs = new Dictionary<string, long>();
        
        public static void LoadList()
        {
            objectIDs.Add("Main_Window", 1000000000000);
            objectIDs.Add("Menu_Bar", 1500000000000);
            objectIDs.Add("Menu_Bar_Home_Button", 1501000000000);
            objectIDs.Add("Menu_Bar_Settings_Button", 1502000000000);
            objectIDs.Add("Menu_Bar_Current_Button", 1503000000000);
            objectIDs.Add("Menu_Bar_Separator_Left", 1504000000000);
            objectIDs.Add("Menu_Bar_Separator_Right", 1505000000000);
            objectIDs.Add("Menu_Bar_Capture_Button", 1506000000000);
            objectIDs.Add("Menu_Bar_Verify_Button", 1507000000000);
            objectIDs.Add("Menu_Bar_Automate_Button", 1508000000000);
            objectIDs.Add("Home_Page", 1110000000000);
            objectIDs.Add("Full_System_Page", 1110100000000);
            objectIDs.Add("Home_System_View", 1110200000000);
            objectIDs.Add("Home_Sequence_View", 1110300000000);
            objectIDs.Add("Home_Capture_View", 1110400000000);
            objectIDs.Add("Home_History_View", 1110500000000);
            objectIDs.Add("Settings_Page", 1120000000000);
            objectIDs.Add("Settings_Page_List_Users", 1120100000000);
            objectIDs.Add("Settings_Page_List_Users_Page", 1120101000000);
            objectIDs.Add("Settings_Page_List_Users_Page_Remove_User", 1120101010000);
            objectIDs.Add("Settings_Page_List_Groups", 1120200000000);
            objectIDs.Add("Settings_Page_List_Groups_Page", 1120201000000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Accounts", 1120201010000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Accounts_Page", 1120201020000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Name", 1120201030000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Name_Page", 1120201040000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Color", 1120201050000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Color_Page", 1120201060000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Permissions", 1120201070000);
            objectIDs.Add("Settings_Page_List_Groups_Page_List_Permissions_Page", 1120201080000);
            objectIDs.Add("Settings_Page_List_Groups_Page_Remove_Group", 1120201090000);
            objectIDs.Add("Settings_Page_List_Data_Structure", 1120300000000);
            objectIDs.Add("Settings_Page_List_Data_Structure_Page", 1120301000000);
            objectIDs.Add("Settings_Page_List_Themes", 1120400000000);
            objectIDs.Add("Settings_Page_List_Themes_Page", 1120401000000);
            objectIDs.Add("Settings_Page_List_Sequence", 1120500000000);
            objectIDs.Add("Settings_Page_List_Sequence_Page", 1120501000000);
            objectIDs.Add("Settings_Page_Sequence_Page_Remove_Sequence", 1120501010000);
            objectIDs.Add("Capture_Page", 1130000000000);
            objectIDs.Add("Verify_Page", 1140000000000);
            objectIDs.Add("Reports_Page", 1150000000000);
            objectIDs.Add("Account_Popup", 1200000000000);
            objectIDs.Add("Account_Popup_Account_Button", 1201000000000);
            objectIDs.Add("Account_Popup_Security_Button", 1202000000000);
            objectIDs.Add("Account_Popup_License_Button", 1203000000000);
            objectIDs.Add("Account_Popup_Account_Page", 1204000000000);
            objectIDs.Add("Account_Popup_Security_Page", 1205000000000);
            objectIDs.Add("Account_Popup_License_Page", 1206000000000);
            objectIDs.Add("Status_Bar", 1300000000000);
            objectIDs.Add("Status_Bar_Database", 1301000000000);
            objectIDs.Add("Title_Bar", 1400000000000);
            objectIDs.Add("Title_Bar_Account", 1401000000000);
        }

        public static Dictionary<long, String> getIDToName()
        {
            Dictionary<long, String> dict = new Dictionary<long, string>();

            foreach (var value in objectIDs)
            {
                dict[value.Value] = value.Key;
            }

            return dict;
        }

        public static List<long> getSortedIDs()
        {
            List<long> tempList = new List<long>();

            foreach (long value in objectIDs.Values)
            {
                if (value != objectIDs["Main_Window"])
                {
                    tempList.Add(value);
                }
            }

            tempList.Sort();

            return tempList;
        }

    }
}
