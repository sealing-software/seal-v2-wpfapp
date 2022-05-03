using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    interface MessageProtocol
    {
        public event EventHandler<StatusMessage> message;
        public StatusMessage createMessage(object message, String objectName);
        public void sendMessage(StatusMessage message);

        public void receiveMessage(object sender, StatusMessage sentMessage);
    }
}
