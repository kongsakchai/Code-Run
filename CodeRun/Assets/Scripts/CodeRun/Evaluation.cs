using System.Collections.Generic;
using UnityEngine;
using System;

namespace CodeRun
{
    public class Evaluation
    {
        private Environment env;
        int i, count;
        public Evaluation(Environment env)
        {
            this.env = env;
        }

        public void Eval(INode node)
        {
            switch (node)
            {
                case Program _p:
                    EvalProgram(_p.statements);
                    break;
                case BlockStatement _block:
                    EvalProgram(_block.statements);
                    break;
                case AssignStatement _assign:
                    EvalAssign(_assign.name, _assign.value);
                    break;
                case IfStatement _if:
                    EvalIfStatement(_if.condition, _if.consequence, _if.alternative);
                    break;
                case CallStatement _call:
                    EvalCallStatement(_call.name, _call.argument);
                    break;
            }
        }

        private void EvalProgram(List<Statement> statements)
        {
            foreach (var s in statements)
            {
                Eval(s);
            }
        }

        private void EvalAssign(string name, List<Token> value)
        {
            var _value = Calculate(value);
            if (_value is null) return;
            //Debug.Log($"{name} = {_value._string}");

            env.Add(name, _value);
        }

        private void EvalIfStatement(List<Token> condition, BlockStatement consequence, Statement alternative)
        {
            var _condition = Calculate(condition);
            if (_condition != null && _condition.Is(Type.BOOLEAN))
            {
                if (_condition.b)
                    EvalProgram(consequence.statements);
                else if (alternative is null)
                    return;
                else if (alternative is BlockStatement _block)
                    EvalProgram(_block.statements);
                else if (alternative is IfStatement _if)
                    EvalIfStatement(_if.condition, _if.consequence, _if.alternative);
            }
            else
            {

            }
        }

        private void EvalCallStatement(string name, List<Token> argument)
        {
            var ok = env.func.TryGetValue(name, out Action<Variable> func);
            if (!ok) return;
            var _argument = Calculate(argument);
            if (_argument is null) return;
            func(_argument);
        }

        private Variable Calculate(List<Token> value)
        {
            Stack<Variable> output = new Stack<Variable>();
            foreach (var tok in value)
            {
                switch (tok.type)
                {
                    case Type.IDENT:
                        var ok = env.store.TryGetValue(tok.literal, out var _var);
                        if (ok)
                            output.Push(_var);
                        break;
                    case Type.NUMBER:
                        output.Push(new Variable(double.Parse(tok.literal)));
                        break;
                    case Type.STRING:
                        output.Push(new Variable(tok.literal));
                        break;
                    case Type.TRUE:
                        output.Push(new Variable(true));
                        break;
                    case Type.FALSE:
                        output.Push(new Variable(false));
                        break;
                    case Type.AND:
                    case Type.OR:
                    case Type.EQUAL:
                    case Type.NOTEQUAL:
                    case Type.GREATER:
                    case Type.LESSTHAN:
                    case Type.GREATEROR:
                    case Type.LESSTHANOR:
                    case Type.PLUS:
                    case Type.MINUS:
                    case Type.MULTIPLY:
                    case Type.DIVIDE:
                        var _var2 = output.Pop();
                        var _var1 = output.Pop();
                        var _cal1 = CalculateInfix(tok.type, _var1, _var2);
                        if (_cal1 != null) output.Push(_cal1);
                        break;
                    case Type.NOT:
                    case Type.SIGN:
                        var _var3 = output.Pop();
                        var _cal2 = CalculatePrefix(tok.type, _var3);
                        if (_cal2 != null) output.Push(_cal2);
                        break;
                }
            }
            if (output.Count == 1)
                return output.Pop();
            else
                return null;
        }

        private Variable CalculateInfix(Type type, Variable var1, Variable var2)
        {
            if (var1.Is(Type.NUMBER) && var2.Is(Type.NUMBER))
            {
                return CalculateInfixNumber(type, var1.n, var2.n);
            }
            else if (var1.Is(Type.BOOLEAN) && var1.Is(Type.BOOLEAN))
            {
                return CalculateInfixBoolean(type, var1.b, var2.b);
            }
            else if (var1.Is(Type.STRING) || var2.Is(Type.STRING))
            {
                if (type == Type.PLUS)
                {
                    return new Variable($"{var1.String}{var2.String}");
                }
                else if (type == Type.EQUAL && var1.type==var2.type)
                {
                    return new Variable(var1.s == var2.s);
                }
                else if (type == Type.EQUAL && var1.type==var2.type)
                {
                    return new Variable(var1.s != var2.s);
                }
            }
            return null;
        }

        private Variable CalculateInfixNumber(Type type, double value1, double value2)
        {
            return type switch
            {
                Type.PLUS => new Variable(value1 + value2),
                Type.MINUS => new Variable(value1 - value2),
                Type.MULTIPLY => new Variable(value1 * value2),
                Type.DIVIDE => new Variable(value1 / value2),
                Type.EQUAL => new Variable(value1 == value2),
                Type.NOTEQUAL => new Variable(value1 != value2),
                Type.GREATER => new Variable(value1 > value2),
                Type.GREATEROR => new Variable(value1 >= value2),
                Type.LESSTHAN => new Variable(value1 < value2),
                Type.LESSTHANOR => new Variable(value1 <= value2),
                _ => null,
            };
        }

        private Variable CalculateInfixBoolean(Type type, bool value1, bool value2)
        {
            return type switch
            {
                Type.AND => new Variable(value1 && value2),
                Type.OR => new Variable(value1 || value2),
                Type.EQUAL => new Variable(value1 == value2),
                Type.NOTEQUAL => new Variable(value1 != value2),
                _ => null,
            };
        }

        private Variable CalculatePrefix(Type opr, Variable var)
        {
            if (opr == Type.NOT && var.Is(Type.BOOLEAN))
            {
                var.Set(!var.b);
                return var;
            }
            else if (opr == Type.SIGN && var.Is(Type.NUMBER))
            {
                var.Set(-var.n);
                return var;
            }
            else
                return null;
        }

    }
}