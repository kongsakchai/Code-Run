using UnityEngine;
using System;
using System.Collections.Generic;

namespace CodeRun
{
    public class CodeComponent : MonoBehaviour
    {
        private Environment env;
        private Evaluator evaluation;
        private Lexer lexer;
        private Parser parser;
        private Builtin builtin;
        public CodeComponent()
        {
            lexer = new Lexer();
            parser = new Parser(lexer);
            env = new Environment();
            evaluation = new Evaluator(env);
            builtin = new Builtin(env, this);
        }

        public void Compiler(string source)
        {
            _tracer.Clear();
            env.ClearStore();
            evaluation.end = false;
            
            lexer.Read(source);
            var program = parser.ParseProgram();
            if (!_tracer.error)
                evaluation.Eval(program);

            if (_tracer.error) Debug.Log($"{_tracer.code} {_tracer.GetLog()}");
        }
        public bool end => _tracer.error || evaluation.end;
        public void Add(string name, CodeObject var, int layer) => env.Add(name, var);
        public void Add(string name, CodeObject var, int layer, Permission per) => env.Add(name, var, per);
        public void Add(string name, Action<CodeObject, Action> func) => env.Add(name, func);
        public void GetStore(string name, out CodeObject var) => env.GetStore(name, out var);
    }
    public enum Code { Null, Missing, Invalid, MissingBlock, Unknow, Variable, ReadOnly, OutofArray }
    public class _tracer
    {
        private static List<string> trace = new List<string>(0);
        private static string log = "";
        public static bool error => log != "" || code != Code.Null;
        public static Code code = Code.Null;
        public static void Trace(string message)
        {
            trace.Add(message);
        }
        public static void UnTrace()
        {
            trace.RemoveAt(trace.Count - 1);
        }
        public static void Error(Code _code)
        {
            if (code == Code.Null)
                code = _code;
        }

        public static void Error(Code _code, string message)
        {
            if (code == Code.Null || code == _code && log == "")
            {
                code = _code;
                log = message;
            }
        }

        public static void Clear()
        {
            trace.Clear();
            log = "";
            code = Code.Null;
        }

        public static string GetTrace() => trace[trace.Count - 1];
        public static string GetLog() => log;
    }

}
