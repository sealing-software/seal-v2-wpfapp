using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    public class StatusMessage
    {
        private long recipient;
        private object message;
        private long sender;


        public StatusMessage(long statusRecipient, object sentItem, long sender)
        {
            recipient = statusRecipient;
            message = sentItem;
            this.sender = sender;
        }

        public long getAddress()
        {
            return recipient;
        }

        public object readMessage()
        {
            return message;
        }

        public long getSender()
        {
            return sender;
        }

    }
}
