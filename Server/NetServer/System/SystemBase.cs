using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetServer.Action;

namespace NetServer.System
{
    /// <summary>
    /// 系统基类
    /// </summary>
    public abstract class SystemBase
    {
        /// <summary>
        /// 处理模块委托
        /// </summary>
        public delegate void HandleModule(ActionParameter parameter);

        /// <summary>
        /// 调用模块键名
        /// </summary>
        public const string MODULENAME = "ModuleName";

        /// <summary>
        /// 系统编号
        /// </summary>
        public abstract int SystemID { get; }

        // 调用队列
        private ConcurrentQueue<ActionParameter> InvokeQueue = new ConcurrentQueue<ActionParameter>();

        // 线程控制对象
        private AutoResetEvent autoEvent = new AutoResetEvent(false);

        // 执行处理委托
        private Dictionary<string, HandleModule> handles = new Dictionary<string, HandleModule>();

        public SystemBase()
        {
            //自动添加处理委托
            Type methodParameterType = typeof(ActionParameter);
            Type delegateType = typeof(HandleModule);
            Type type = GetType();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var parameterInfo = method.GetParameters();
                if (parameterInfo.Length == 1 && parameterInfo[0].ParameterType.Equals(methodParameterType))
                {
                    HandleModule handleDelegate = method.CreateDelegate(delegateType, this) as HandleModule;
                    handles.Add(method.Name, handleDelegate);
                }
            }

            //启动更新线程
            Thread updateThread = new Thread(Update);
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        /// <summary>
        /// 添加处理参数队列
        /// </summary>
        public void Add(ActionParameter parameter)
        {
            InvokeQueue.Enqueue(parameter);
        }

        /// <summary>
        /// 释放等待更新
        /// </summary>
        public void Set()
        {
            autoEvent.Set();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void Update()
        {
            while (true)
            {
                if (InvokeQueue.Count == 0)
                    autoEvent.WaitOne();

                ActionParameter parameter;
                while (InvokeQueue.TryDequeue(out parameter))
                {
                    string invokeModuleName = parameter.GetValue<string>(MODULENAME);
                    if (!string.IsNullOrEmpty(invokeModuleName) && handles.ContainsKey(invokeModuleName))
                    {
                        handles[invokeModuleName](parameter);
                    }
                }

                LateUpdate();
            }
        }

        /// <summary>
        /// 更新后
        /// </summary>
        public abstract void LateUpdate();
    }
}
