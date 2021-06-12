using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CodeRun
{
    public class CodeComponent
    {
        private Environment env;
        private Evaluation evaluation;
        private Lexer lexer;
        private Parser parser;
        private Builtin builtin;
        public CodeComponent()
        {
            lexer = new Lexer();
            parser = new Parser(lexer);
            env = new Environment();
            evaluation = new Evaluation(env);
            builtin = new Builtin(env);
        }

        public void Compiler(string source)
        {
            lexer.Read(source);
            var program = parser.ParseProgram();
            evaluation.Eval(program);
        }

        public void Add(string name, Variable var) => env.Add(name, var);
        public void Set(string name, Variable var) => env.Set(name, var);
        public void Add(string name, Action<Variable> func) => env.Add(name, func);

    }
}
