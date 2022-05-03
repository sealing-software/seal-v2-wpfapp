using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    //Used to determing whether to send message up or down
    class MessageRelay
    {
        public MessageRelay()
        {

        }

        public static bool sendUp(long currentID, long receipientID)
        {
            bool send = false;

            if (getDigitPlaces(currentID - receipientID) > getZeros(currentID))
            {
                send = true;
            }

            return send;
        }

        private static long getZeros(long currentID)
        {
            String id = currentID.ToString();
            int zeroes = 0;

            for (int i = id.Length - 1; i >= 0; i--)
            {
                if (id[i].Equals('0'))
                {
                    zeroes++;
                }
                else
                {
                    i = -1;
                }
            }

            return zeroes;
        }

        private static long getDigitPlaces(long difference)
        {
            String differenceString = Math.Abs(difference).ToString();

            return differenceString.Length;
        }

        public static long getSendDirection(Dictionary<long, object> passedDict, long destination)
        {
            long found = 0;


            return found;
        }

        //Returns sub ID for message relay
        public static long sendDown(long recepient, Dictionary<long, object> passedDict)
        {
            int similar = 0;
            long selectedID = 0;

            foreach (var dictItem in passedDict)
            {
                String val1 = recepient.ToString();
                String val2 = dictItem.Key.ToString();
                int localSimilar = 0;

                for (int i = 0; i < val1.Length; i++)
                {
                    if (val1[i] == val2[i])
                    {
                        localSimilar++;
                    }
                }

                if (localSimilar > similar)
                {
                    similar = localSimilar;
                    selectedID = dictItem.Key;
                }
            }

            return selectedID;
        }


    }
}
