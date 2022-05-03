using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    public class UserInfo
    {
        public int ID { get; set; }
        public String userName { get; set; }
        public String name { get; set; }
        public int groupID { get; set; }
        public String groupName { get; set; }
        public SolidColorBrush brushColor { get; set; }
        private DatabaseInterface db = DatabaseInterface.Instance;

        public UserInfo(int accountID, String accountUserName, String accountName, int groupID)
        {
            this.ID = accountID;
            this.userName = accountUserName;
            this.name = accountName;
            this.groupID = groupID;
            getGroupColor();
            getGroupName();
        }

        private void getGroupColor()
        {
            brushColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(db.getGroupHexColor(groupID)));
        }

        private void getGroupName()
        {
            groupName = db.getAccountGroupText(groupID);
        }

        public bool isAdmin()
        {
            if (groupID == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
