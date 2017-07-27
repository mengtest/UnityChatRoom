using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;
using NetServer.Session;
using NetServer.Data;

namespace NetServer.Action
{
    /// <summary>
    /// 处理分发类
    /// </summary>
    public class ActionHandler
    {
        // 处理对象列表
        private Dictionary<int, ActionBase> actions;

        public ActionHandler(SessionClient session)
        {
            actions = ActionFactory.CreateAllAction(session);
        }

        /// <summary>
        /// 数据包处理过程
        /// </summary>
        public void Process(DataPackage packet)
        {
            if (!actions.ContainsKey(packet.PacketType))
                return;

            ActionParameter parameter = new ActionParameter();
            ActionBase handler = actions[packet.PacketType];
            handler.Packet = packet;
            if (handler.Check(parameter))
                if (!handler.Process(parameter))
                    Console.WriteLine("{0}请求处理失败", packet.PacketType);
            handler.Clean();
        }
    }
}
