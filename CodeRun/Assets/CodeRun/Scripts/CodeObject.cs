using System.Collections.Generic;
using System.Linq;

namespace CodeRun
{
    public class CodeObject
    {
        public Type type { get; }
        public string typeName { get; }
        public double n { get; private set; }
        public int i => (int)n;
        public string s { get; private set; }
        public bool b { get; private set; }
        public List<CodeObject> list { get; private set; }
        public int Count => (list == null) ? -1 : list.Count;

        //---------- new CodeObject ----------
        public CodeObject(Type type)
        {
            if (type == Type.ARRAY)
            {
                this.type = type;
                list = new List<CodeObject>();
                typeName = "array";
            }
        }

        public CodeObject(string s)
        {
            this.s = s;
            type = Type.STRING;
            typeName = "string";
        }

        public CodeObject(double n)
        {
            this.n = n;
            type = Type.NUMBER;
            typeName = "number";
        }

        public CodeObject(bool b)
        {
            this.b = b;
            type = Type.BOOLEAN;
            typeName = "boolean";
        }

        //---------- Set value ----------
        public void Set(string s)
        {
            if (type == Type.STRING)
                this.s = s;
        }

        public void Set(double n)
        {
            if (type == Type.NUMBER)
                this.n = n;
        }

        public void Set(bool b)
        {
            if (type == Type.BOOLEAN)
                this.b = b;
        }

        //---------- Array ----------

        public CodeObject this[int index]
        {
            get
            {
                if (type == Type.ARRAY && list.Count > index) return list[index];
                return null;
            }
            set
            {
                if (type == Type.ARRAY && list.Count > index) list[index] = value;
            }
        }

        public void Add(CodeObject obj)
        {
            if (type == Type.ARRAY)
            {
                list.Add(obj);
            }
        }

        public void Add(double n)
        {
            Add(new CodeObject(n));
        }

        public void Add(string s)
        {
            Add(new CodeObject(s));
        }

        public void Add(bool b)
        {
            Add(new CodeObject(b));
        }

        //---------- Check is ...

        public bool IsNumber => type == Type.NUMBER;
        public bool IsString => type == Type.STRING;
        public bool IsBoolean => type == Type.BOOLEAN;
        public bool IsArray => type == Type.ARRAY;

        public string String => type switch
        {
            Type.NUMBER => $"{n}",
            Type.STRING => s,
            Type.BOOLEAN => $"{b}",
            Type.ARRAY => string.Join(" , ", list.Select(v => v.String)),
            _ => null
        };

        public static CodeObject Convert(Token tok) => tok.type switch
        {
            Type.NUMBER => new CodeObject(double.Parse(tok.literal)),
            Type.STRING => new CodeObject(tok.literal),
            Type.TRUE => new CodeObject(true),
            Type.FALSE => new CodeObject(false),
            _ => null
        };
    }
}