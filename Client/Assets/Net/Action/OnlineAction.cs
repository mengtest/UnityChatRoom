using System.Collections.Generic;

namespace NetServer.Action
{
    /// <summary>
    /// 消息转发
    /// </summary>
    public class OnlineAction : ActionBase
    {
        private const int ACTIONTYPE = 100;

        public override int ActionType { get { return ACTIONTYPE; } }

        public override void Clean()
        {
            base.Clean();
        }

        public override bool ReceiveProcess(ActionParameter parameter)
        {
            List<string> onlineList = null;
            if (Packet.Data.TryObject(ref onlineList))
            {
                parameter["onlineList"] = onlineList;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool SendProcess(ActionParameter parameter)
        {
            return true;
        }
    }
}
