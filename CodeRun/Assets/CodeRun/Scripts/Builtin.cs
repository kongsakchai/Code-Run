using UnityEngine;
using System;
namespace CodeRun
{
    public class Builtin
    {
        CodeComponent code;
        public Builtin(Environment env, CodeComponent code)
        {
            this.code = code;
            env.Add("print", Print);
        }

        public void Print(CodeObject var,Action next){
            Debug.Log(var.String);
            next();
        }
    }
}