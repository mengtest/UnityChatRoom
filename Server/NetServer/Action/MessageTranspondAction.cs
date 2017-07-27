using NetServer.Data;
using NetServer.Session;

namespace NetServer.Action
{
    public class MessageTranspondAction : ActionBase
    {
        private const int ACTIONTYPE = 1001;

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
            return new MessageTranspondAction();
        }

        public override bool Process(ActionParameter parameter)
        {
            string message = Session.GetRemoteAddress() + ": " + parameter.GetValue<string>("message");
            DynamicBuffer buffer = new DynamicBuffer(0);
            buffer.WriteValue(message);
            DataPackage packet = new DataPackage(buffer, ActionType);
            foreach (var session in SessionClientPool.Instance.GetOnlineSession())
            {
                session.Send(packet);
            }
            return true;
        }
    }
}
