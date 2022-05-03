using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    //Singleton class
    public sealed class User
    {
        private static String userName;
        private static String displayName;
        private static int groupID;
        private static int id;
        private static String groupString;
        private static DatabaseInterface db = DatabaseInterface.Instance;
        private static bool userLoggedIn = false;

        public static void userInfo(String userName, String displayName, int assignedGroup, int userID)
        {
            User.userLoggedIn = true;
            User.userName = userName;
            User.displayName = displayName;
            User.id = userID;
            User.groupID = assignedGroup;
            User.groupString = db.getAccountGroupText(assignedGroup);          
        }

        public static String getUserName()
        {
            return userName;
        }

        public static String getDisplayName()
        {
            return displayName;
        }

        public static int getAssignedGroupID()
        {
            return groupID;
        }

        public static int getUserId()
        {
            return id;
        }

        public static String getAssignedGroupText()
        {
            return groupString;
        }

        public static bool isUserLoggedIn()
        {
            return userLoggedIn;
        }

        public static bool userAuthorized(String objName)
        {
            bool authorized = false;

            if (isUserLoggedIn())
            {
                if (db.checkGroupObjectPermission(User.groupID, objName))
                {
                    authorized = true;
                }
            }

            return authorized;
        }

        public static bool isAdmin()
        {
            bool result = false;

            if (User.groupID == 1)
            {
                result = true;
            }

            return result;
        }

        public static void userLogOut()
        {
            User.userLoggedIn = false;
            User.userName = null;
            User.displayName = null;
            User.id = 0;
            User.groupID = 0;
            User.groupString = null;

        }
    }
}
