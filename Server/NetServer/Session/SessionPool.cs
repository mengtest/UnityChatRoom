using System.Collections.Generic;
using System.Collections.Concurrent;
using NetServer.Session;

namespace NetServer.Session
{
    /// <summary>
    /// 会话客户端池
    /// </summary>
    public class SessionClientPool
    {
        // 会话池
        private static ConcurrentBag<SessionClient> Sessions = new ConcurrentBag<SessionClient>();

        /// <summary>
        /// 设置最大会话数量
        /// </summary>
        public static void SetMaxSessionClient(int count)
        {
            Sessions = new ConcurrentBag<SessionClient>();
            for (int i = 0; i < count; i++)
            {
                Sessions.Add(new SessionClient());
            }
        }

        /// <summary>
        /// 获取空会话
        /// </summary>
        public static SessionClient GetSessionClient()
        {
            foreach (var session in Sessions)
            {
                if (!session.isUse)
                    return session;
            }
            return null;
        }

        public static IEnumerable<SessionClient> GetOnlineSession()
        {
            List<SessionClient> sessions = new List<SessionClient>();
            foreach (var session in GetEnumerable())
            {
                if (session.isUse)
                    sessions.Add(session);
            }
            return sessions;
        }

        /// <summary>
        /// 获取会话池迭代器
        /// </summary>
        public static IEnumerable<SessionClient> GetEnumerable()
        {
            return Sessions;
        }
    }
}
