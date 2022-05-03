using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    public static class Action
    {
        private static DatabaseInterface db = DatabaseInterface.Instance;

        public static void save(String status)
        {
            int color = 0;

            db.saveAction(User.getAssignedGroupID(), db.getGroup(User.getAssignedGroupID()).name, User.getUserName(), CurrentSystem.model, CurrentSystem.serial, StaticCapture.id, StaticCapture.name, StaticCapture.currentStep, status, color);
        }

        public static List<History> getHistory()
        {
            return db.getHistory();
        }

        public static List<History> getHistory(int captureID)
        {
            return db.getHistory(captureID);
        }
    }
}
