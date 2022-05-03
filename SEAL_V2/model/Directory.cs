using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    class Directory
    {
        public int id { get; set; }
        public String name { get; set; }
        public int parentID { get; set; }
        public int allowSequence { get; set; }
        public int sequenceID { get; set; }

        public Directory(int id, String name, int parentID, int allowSequence, int sequenceID)
        {
            this.id = id;
            this.name = name;
            this.parentID = parentID;
            this.allowSequence = allowSequence;
            this.sequenceID = sequenceID;
        }

    }
}
