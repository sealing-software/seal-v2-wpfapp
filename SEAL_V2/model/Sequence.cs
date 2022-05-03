using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;

namespace SEAL_V2.model
{
    public class Sequence
    {
        public String sequenceName { get; set; }
        private String description;
        public String groupSequenceString { get; set; }
        public List<Group> groupSequence = new List<Group>();
        public List<String> groupDescription = new List<string>();
        public int sequenceid;
        public int systemLock { get; set; }
        DatabaseInterface db = DatabaseInterface.Instance;
        public int sequenceLength { get; set; }

        public Sequence(int sequenceid, String sequenceName, String description, String groupSequence, int systemLock)
        {
            this.sequenceid = sequenceid;
            this.sequenceName = sequenceName;
            this.description = description;
            this.groupSequenceString = groupSequence;
            this.systemLock = systemLock;
            convertStringToGroup(groupSequenceString);
            convertStringToDescription(groupSequenceString);
        }

        private void convertStringToGroup(String sequenceIDString)
        {
            //Send alert message if group id no longer exists
            String[] split = sequenceIDString.Split(',');

            for (int i = 0; i < split.Length; i+=2)
            {
                groupSequence.Add(db.getGroup(Int32.Parse(split[i])));
            }

            sequenceLength = groupSequence.Count;
        }

        private void convertStringToDescription(String sequenceString)
        {
            String[] split = sequenceString.Split(',');

            for (int i = 1; i < split.Length; i += 2)
            {
                groupDescription.Add(split[i]);
            }

            //sequenceLength = groupSequence.Count;
        }

        public List<Group> getGroupSequence()
        {
            return groupSequence;
        }

        public String getGroupSequenceString()
        {
            return groupSequenceString;
        }

        public String getDescription()
        {
            return description;
        }

        public int getID()
        {
            return sequenceid;
        }

        public bool isSystemLock()
        {
            if (systemLock == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Group getGroupAtIndex(int passedIndex)
        {
            return groupSequence[passedIndex];
        }

        public String getGroupDescriptionAtIndex(int passedIndex)
        {
            return groupDescription[passedIndex];
        }

    }
}
