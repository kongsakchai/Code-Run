using UnityEngine;

namespace CodeRun
{
    public class Builtin
    {
        public Builtin(Environment env)
        {
            env.Add("print", Print);
        }

        public void Print(Variable var) => Debug.Log(var.String);
    }
}