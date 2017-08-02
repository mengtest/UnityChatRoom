using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetServer.Action;
using NetServer.System;

namespace NetServer
{
    public static class SystemCommandCenter
    {
        private static Dictionary<int, SystemBase> systems = new Dictionary<int, SystemBase>();

        static SystemCommandCenter()
        {
            Type systemBase = typeof(SystemBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(systemBase))
                {
                    SystemBase _system = type.Assembly.CreateInstance(type.FullName) as SystemBase;
                    systems.Add(_system.SystemID, _system);
                }
            }
        }

        public static void SendSystemMessage(int systemID, ActionParameter parameter)
        {
            if (!systems.ContainsKey(systemID))
                return;

            SystemBase _system = systems[systemID];
            _system.Add(parameter);
            _system.Set();
        }
    }
}
