using System.Threading;
using Common.com.serialization;

namespace Common.com.networking.Messages
{
    public abstract class Message:JsonSerializable
    {
        public int Tick { get; set; }

        protected Message(int tick)
        {
            Tick = tick;
        }
        public void Send(Connection connection,bool tcp)
        {
            var toSend=Serialize();
            if (tcp)
            {
                connection.SendTcp(toSend);
            }
            else
            {
                connection.SendUdp(toSend);
            }
            Thread.Sleep(1);
        }

        
    }
}