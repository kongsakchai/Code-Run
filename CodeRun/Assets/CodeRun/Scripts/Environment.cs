using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRun
{
    using Store = Dictionary<string, StructStore>;
    using Function = Dictionary<string, Action<CodeObject, Action>>;
    public enum Permission { ReadWrite, ReadOnly }

    public class Environment
    {
        public Store stores;
        public Function funcs;

        public Environment()
        {
            stores = new Store();
            funcs = new Function();
        }

        public bool IsReadOnly(string name)
        {
            return stores[name].IsReadOnly();
        }

        public bool HasName(string name)
        {
            return stores.ContainsKey(name) || funcs.ContainsKey(name);
        }

        public void Add(string name, CodeObject obj)
        {
            var ok = HasName(name);
            if (!ok)
                this.stores.Add(name, new StructStore(obj));
        }

        public void Add(string name, CodeObject obj, Permission permission)
        {
            var ok = HasName(name);
            if (!ok)
                this.stores.Add(name, new StructStore(permission, obj));
        }

        public void Add(string name, Action<CodeObject, Action> action)
        {
            var ok = HasName(name);
            if (!ok)
                funcs.Add(name, action);
        }

        //---------- Get ----------

        public bool GetStore(string name, out CodeObject obj)
        {
            obj = null;

            if (stores.ContainsKey(name))
                obj = stores[name].obj;
            else
                return false;

            return true;
        }

        public bool GetFunction(string name, out Action<CodeObject, Action> obj)
        {
            obj = null;

            if (funcs.ContainsKey(name))
                obj = funcs[name];
            else
                return false;

            return true;
        }

        //---------- Set ----------
        public void Set(string name, CodeObject obj)
        {
            var ok = this.stores.TryGetValue(name, out var store);

            if (!ok)
                _tracer.Error(Code.Unknow, name);
            else if (!store.Type(obj.type))
                _tracer.Error(Code.Variable, $"{obj.typeName}?{store.obj.typeName}");
            else
                store.obj = obj;
        }

        public void Set(string name, CodeObject obj, int index)
        {
            var ok = this.stores.TryGetValue(name, out var store);

            if (!ok)
                _tracer.Error(Code.Unknow, name);
            else if (!store.Type(Type.ARRAY))
                _tracer.Error(Code.Variable, $"{store.obj.typeName}?Array");
            else if (index >= store.obj.Count)
                _tracer.Error(Code.OutofArray);
            else if (store.obj[index].type != obj.type)
                _tracer.Error(Code.Variable, $"{obj.typeName}?{store.obj[index].typeName}");
            else
                store.obj[index] = obj;
        }


        //-------- Delete --------
        public void Remove(string name)
        {
            if (funcs.ContainsKey(name))
                funcs.Remove(name);
            else if (stores.ContainsKey(name))
                stores.Remove(name);
        }

        public void ClearStore()
        {
            stores.Clear();
        }

    }

    public class StructStore
    {
        //public int layer;
        public Permission permission;
        public CodeObject obj;

        //public StructStore(int layer, Permission permission, CodeObject obj) => (this.layer, this.permission, this.obj) = (layer, permission, obj);
        public StructStore(Permission permission, CodeObject obj) => (this.permission, this.obj) = (permission, obj);
        public StructStore(CodeObject obj) => (this.permission, this.obj) = (Permission.ReadWrite, obj);

        public bool IsReadOnly() => permission == Permission.ReadOnly;
        public bool Type(Type type) => obj.type == type;

    }
}
