using UnityEngine;
using System;
using System.Collections.Generic;

namespace CodeRun
{
    public class CodeComponent : MonoBehaviour
    {
        private Environment env;
        private Evaluation evaluation;
        private Lexer lexer;
        private Parser parser;
        private Builtin builtin;
        public void Awake()
        {
            lexer = new Lexer();
            parser = new Parser(lexer);
            env = new Environment();
            evaluation = new Evaluation(env);
            builtin = new Builtin(env, this);
        }

        public void Compiler(string source)
        {
            _trace.Clear();
            env.ClearStore();

            lexer.Read(source);
            var program = parser.ParseProgram();
            if (!_trace.error) evaluation.Eval(program);
        }

        public bool finish => evaluation.finish;
        public bool active => evaluation.active;
        public void Pause() => evaluation.Pause();
        public void Play() => evaluation.Play();

        public void Add(string name, CodeObject var) => env.Add(name, var);
        public void Set(string name, CodeObject var) => env.Set(name, var);
        public void Add(string name, Action<CodeObject> func) => env.Add(name, func);
    }

    public class _trace
    {
        private static int _count = 0;
        private static List<string> _error = new List<string>();
        public static bool error => (_error == null) ? false : _count != 0;
        public static int Count => _count;

        public static void Clear()
        {
            if (_error == null) return;
            _count = 0;
            _error.Clear();
        }

        public static void Error(string _message)
        {
            if (_error == null) _error = new List<string>();
            Debug.Log(_message);
            _error.Add(_message);
            _count++;
        }

        public static string GetErrorLog(int index)
        {
            return _error[index];
        }
    }

}
