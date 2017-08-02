using NetServer.Data;
using NetServer.Session;

namespace NetServer.Action
{
    public class InformAction : ActionBase
    {
        private const int ACTIONTYPE = 99;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override bool Check(ActionParameter parameter)
        {
            string message = null;
            if (Packet.Data.TryReadString(ref message))
            {
                parameter["message"] = message;
                return true;
            }
            return false;
        }

        public override object Clone()
        {
            return new InformAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            string message = "【系统通知】 " + parameter.GetValue<string>("message");
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(message);
            DataPackage packet = new DataPackage(buffer, 100);
            foreach (var session in SessionClientPool.GetOnlineSession())
            {
                session.Send(packet);
            }
            return true;
        }
    }
}
