using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    interface objectIDRequirements
    {
        public void loadObjectID();

        public long getObjectID();

        public String getObjectName();
    }
}
