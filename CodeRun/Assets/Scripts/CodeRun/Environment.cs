using System;
using System.Collections.Generic;

namespace CodeRun
{
    using Store = Dictionary<string, Variable>;
    using Func = Dictionary<string, Action<Variable>>;

    public class Environment
    {
        public Store store;
        public Func func;
        public Environment()
        {
            store = new Store();
            func = new Func();
        }
        public void Add(string name, Variable var)
        {
            if (!store.ContainsKey(name) && !func.ContainsKey(name))
                store.Add(name, var);
            else if (!func.ContainsKey(name))
                store[name] = var;
        }

        public void Set(string name, Variable var)
        {
            if (store.ContainsKey(name))
                store[name] = var;
        }

        public void Add(string name, Action<Variable> action)
        {
            if (!func.ContainsKey(name) && !store.ContainsKey(name))
                func.Add(name, action);
        }

        public void Remove(string name)
        {
            if (func.ContainsKey(name))
                func.Remove(name);
            else if (store.ContainsKey(name))
                store.Remove(name);
        }

    }
}