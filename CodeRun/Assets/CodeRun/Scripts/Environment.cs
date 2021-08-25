using System;
using System.Collections.Generic;

namespace CodeRun
{
    using Store = Dictionary<string, CodeObject>;
    using Function = Dictionary<string, Action<CodeObject>>;
    public enum EnvironmentStatus { ReadOnly }

    public class Environment
    {
        public Store readOnly;
        public Store store;
        public Function func;

        public Environment()
        {
            readOnly = new Store();
            store = new Store();
            func = new Function();
        }

        //---------- Add ----------

        public void Add(string name, CodeObject obj)
        {
            if (func.ContainsKey(name) || readOnly.ContainsKey(name)) return;

            if (!store.TryGetValue(name, out var _obj))
                store.Add(name, obj);
            else if (obj.type == _obj.type)
                store[name] = obj;
            else
                _trace.Error($"Can't convert {obj.typeName} to {_obj.typeName}.");
        }

        public void Add(string name, CodeObject obj, EnvironmentStatus status)
        {
            if (status != EnvironmentStatus.ReadOnly) return;
            if (store.ContainsKey(name) || func.ContainsKey(name)) return;

            if (!readOnly.TryGetValue(name, out var _obj))
                readOnly.Add(name, obj);
            else if (obj.type == _obj.type)
                readOnly[name] = obj;
            else
                _trace.Error($"Can't convert {obj.typeName} to {_obj.typeName}.");
        }

        public void Add(string name, Action<CodeObject> action)
        {
            if (store.ContainsKey(name) || func.ContainsKey(name) || readOnly.ContainsKey(name)) return;

            func.Add(name, action);
        }

        //---------- Get ----------

        public bool GetStore(string name, out CodeObject obj)
        {
            obj = null;

            if (store.ContainsKey(name))
                obj = store[name];
            else if (readOnly.ContainsKey(name))
                obj = readOnly[name];
            else
                return false;

            return true;
        }

        //---------- Set ----------

        public void Set(string name, CodeObject obj)
        {
            if (!store.TryGetValue(name, out var _obj))
                if (obj.type == _obj.type)
                    store[name] = obj;
                else
                    _trace.Error($"Can't convert {obj.typeName} to {_obj.typeName}.");
        }

        public void Set(string name, CodeObject obj, EnvironmentStatus status)
        {
            if (status != EnvironmentStatus.ReadOnly) return;

            if (readOnly.TryGetValue(name, out var _obj))
                if (obj.type == _obj.type)
                    readOnly[name] = obj;
                else
                    _trace.Error($"Can't convert {obj.typeName} to {_obj.typeName}.");
        }

        public void Remove(string name)
        {
            if (func.ContainsKey(name))
                func.Remove(name);
            else if (store.ContainsKey(name))
                store.Remove(name);
            else if (readOnly.ContainsKey(name))
                readOnly.Remove(name);
        }

        public void ClearStore()
        {
            store.Clear();
        }

        public void ClearStore(EnvironmentStatus status)
        {
            if (status != EnvironmentStatus.ReadOnly) return;
            store.Clear();
            readOnly.Clear();
        }

    }
}
