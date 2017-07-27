using System;
using System.Collections.Generic;

namespace NetServer.Action
{
    /// <summary>
    /// 处理参数
    /// </summary>
    public class ActionParameter
    {
        Dictionary<string, object> container = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                if (container.ContainsKey(key))
                    return container[key];
                else
                    return null;
            }
            set
            {
                if (container.ContainsKey(key))
                    container[key] = value;
                else
                    container.Add(key, value);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public T GetValue<T>(string key)
        {
            return (T)this[key];
        }
    }
}

