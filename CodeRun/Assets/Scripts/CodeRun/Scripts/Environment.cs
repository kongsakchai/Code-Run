using System;
using System.Collections.Generic;

namespace CodeRun
{
    using Store = Dictionary<string, CodeObject>;
    using Function = Dictionary<string, Action<CodeObject>>;
    public enum EnvironmentStatus { Root }

    public class Environment
    {
        public Store root;
        public Store store;
        public Function func;

        public Environment()
        {
            root = new Store();
            store = new Store();
            func = new Function();
        }

        //---------- Add ----------

        public void Add(string name, CodeObject var)
        {
            if (!store.ContainsKey(name) && !func.ContainsKey(name) && !root.ContainsKey(name))
                store.Add(name, var);
            else if (!func.ContainsKey(name) && !root.ContainsKey(name))
                store[name] = var;
        }

        public void Add(string name, CodeObject var, EnvironmentStatus status)
        {
            if (status != EnvironmentStatus.Root) return;
            if (!store.ContainsKey(name) && !func.ContainsKey(name) && !root.ContainsKey(name))
                root.Add(name, var);
            else if (!store.ContainsKey(name) && !func.ContainsKey(name))
                root[name] = var;
        }

        public void Add(string name, Action<CodeObject> action)
        {
            if (!store.ContainsKey(name) && !func.ContainsKey(name) && !root.ContainsKey(name))
                func.Add(name, action);
        }

        //---------- Set ----------

        public void Set(string name, CodeObject var)
        {
            if (store.ContainsKey(name))
                store[name] = var;
        }

        public void Set(string name, CodeObject var, EnvironmentStatus status)
        {
            if (status != EnvironmentStatus.Root) return;
            if (root.ContainsKey(name))
                root[name] = var;
        }

        //---------- Get ----------

        public bool GetStore(string name, out CodeObject var)
        {
            if (store.ContainsKey(name))
            {
                var = store[name];
                return true;
            }
            if (root.ContainsKey(name))
            {
                var = root[name];
                return true;
            }
            var = null;
            return false;
        }

        public void Remove(string name)
        {
            if (func.ContainsKey(name))
                func.Remove(name);
            else if (store.ContainsKey(name))
                store.Remove(name);
            else if (root.ContainsKey(name))
                root.Remove(name);
        }

        public void ClearStore()
        {
            store.Clear();
        }

    }
}
