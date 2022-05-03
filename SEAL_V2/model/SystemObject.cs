using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    class SystemObject
    {
        DatabaseInterface db = DatabaseInterface.Instance;

        public int id { get; set; }
        public String modelname { get; set; }
        public int addnickname { get; set; }
        public String nickname { get; set; }
        public int assigneddir { get; set; }
        private int dirid;
        public int regadded { get; set; }
        public String regkey { get; set; }
        public String regvalue { get; set; }

        public string fulldirloca { get; set; }

        public SystemObject()
        {

        }

        public int DirID
        {
            get
            {
                return dirid;
            }
            set
            {
                dirid = value;

                if (assigneddir > 0 && dirid > 0)
                {
                    loadDirString();
                }
            }
        }

        private void loadDirString()
        {
            String fullDir = "";

            SEAL_V2.model.Directory found = db.getDirectory(dirid);

            fullDir = found.name + "/";

            while (found.parentID > 0)
            {
                found = db.getDirectory(found.parentID);
                fullDir = fullDir + found.name + "/";
            }

            fulldirloca = "/" + fullDir;
        }

    }
}
