namespace CodeRun
{
    public class Variable
    {
        public Type type { get; }
        public double n { get; private set; }
        public string s { get; private set; }
        public bool b { get; private set; }
        
        public Variable(string s)
        {
            this.s = s;
            this.type = Type.STRING;
        }

        public Variable(double n)
        {
            this.n = n;
            this.type = Type.NUMBER;
        }
        
        public Variable(bool b)
        {
            this.b = b;
            this.type = Type.BOOLEAN;
        }

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

        public bool Is(Type t) => type == t;

        public string String => type switch
        {
            Type.NUMBER => $"{n}",
            Type.STRING => s,
            Type.BOOLEAN => $"{b}",
            _ => null
        };
    }
}