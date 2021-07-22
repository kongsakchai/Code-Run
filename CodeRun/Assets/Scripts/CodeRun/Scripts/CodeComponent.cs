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

        public void Pause() => evaluation.Pause();
        public void Play() => evaluation.Play();

        public void Add(string name, CodeObject var) => env.Add(name, var);
        public void Add(string name, CodeObject var, EnvironmentStatus status) => env.Add(name, var, status);
        public void Add(string name, Action<CodeObject> func) => env.Add(name, func);
        public void GetStore(string name, out CodeObject var) => env.GetStore(name, out var);
    }

    public class _trace
    {
        private static string _error = "";
        public static bool error => _error != "";

        public static void Clear()
        {
            if (_error == null) return;
            _error = "";
        }

        public static void Error(string _message)
        {
            _error = _message;
            Debug.Log(_message);
        }
    }

}
