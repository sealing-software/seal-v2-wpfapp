using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    class StaticCapture
    {
        public static int id { get; set; }
        public static String name { get; set; }
        public static int systemid { get; set; }
        public static int sequenceid { get; set; }
        public static int currentStep { get; set; }
        public static int status { get; set; }
        private static DatabaseInterface db = DatabaseInterface.Instance;

        public static void applyCapture(Capture passed)
        {
            StaticCapture.id = passed.id;
            StaticCapture.name = passed.name;
            StaticCapture.systemid = passed.systemid;
            StaticCapture.sequenceid = passed.sequenceid;
            StaticCapture.currentStep = passed.currentStep;
            StaticCapture.status = passed.status;
            SEAL_V2.model.Action.save("NEW CAPTURE");
        }

        public static void changeCapture(Capture passed)
        {
            StaticCapture.id = passed.id;
            StaticCapture.name = passed.name;
            StaticCapture.systemid = passed.systemid;
            StaticCapture.sequenceid = passed.sequenceid;
            StaticCapture.currentStep = passed.currentStep;
            StaticCapture.status = passed.status;
        }

        public static void passed(int length)
        {
            SEAL_V2.model.Action.save("SUCCESS");
            if (currentStep == length - 1)
            {
                status = 3;
                SEAL_V2.model.Action.save("COMPLETE");
            }
            else
            {
                status = 1;
                currentStep++;
            }

            StaticCapture.updateCapture();
        }

        public static void failed()
        {
            SEAL_V2.model.Action.save("FAIL");
            if (currentStep != 0)
            {
                currentStep--;
            }

            status = 2;

            StaticCapture.updateCapture();
        }

        private static void updateCapture()
        {
            db.updateCapture(StaticCapture.id, StaticCapture.currentStep, StaticCapture.status);
        }
    }
}
