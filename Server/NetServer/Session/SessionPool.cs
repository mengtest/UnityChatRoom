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
        private ConcurrentBag<SessionClient> sessions;

        // 单例
        private static SessionClientPool instance;

        private SessionClientPool()
        {

        }

        /// <summary>
        /// 单例全局接口
        /// </summary>
        public static SessionClientPool Instance
        {
            get
            {
                if (instance == null)
                    instance = new SessionClientPool();
                return instance;
            }
        }

        /// <summary>
        /// 设置最大会话数量
        /// </summary>
        public void SetMaxSessionClient(int count)
        {
            sessions = new ConcurrentBag<SessionClient>();
            for (int i = 0; i < count; i++)
            {
                sessions.Add(new SessionClient());
            }
        }

        /// <summary>
        /// 获取空会话
        /// </summary>
        public SessionClient GetSessionClient()
        {
            foreach (var session in sessions)
            {
                if (!session.isUse)
                    return session;
            }
            return null;
        }

        public IEnumerable<SessionClient> GetOnlineSession()
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
        public IEnumerable<SessionClient> GetEnumerable()
        {
            return sessions;
        }
    }
}
