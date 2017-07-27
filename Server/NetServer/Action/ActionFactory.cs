using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NetServer.Session;

namespace NetServer.Action
{
    public static class ActionFactory
    {
        private static Dictionary<int, ActionBase> ActionTemplates = new Dictionary<int, ActionBase>();

        static ActionFactory()
        {
            Type actionBase = typeof(ActionBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(actionBase))
                {
                    ActionBase _action = type.Assembly.CreateInstance(type.FullName) as ActionBase;
                    ActionTemplates.Add(_action.ActionType, _action);
                }
            }
        }

        public static ActionBase CreateAction(int actionType, SessionClient session)
        {
            if (!ActionTemplates.ContainsKey(actionType))
                return null;

            ActionBase _action = ActionTemplates[actionType].Clone() as ActionBase;
            _action.Session = session;
            return _action;
        }

        public static Dictionary<int, ActionBase> CreateAllAction(SessionClient session)
        {
            Dictionary<int, ActionBase> actions = new Dictionary<int, ActionBase>();
            foreach (var actionType in ActionTemplates.Keys)
            {
                actions.Add(actionType, CreateAction(actionType, session));
            }
            return actions;
        }
    }
}
