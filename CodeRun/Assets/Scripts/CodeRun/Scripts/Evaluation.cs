using System.Collections.Generic;
using System;

namespace CodeRun
{
    public class Evaluation
    {
        private Environment env;
        private bool pause;
        private Stack<int> position;
        private List<Statement> statements;

        public void Pause()
        {
            pause = true;
        }

        public void Play()
        {
            pause = false;
            var i = (position.Count > 0) ? position.Pop() : 0;
            EvalProgram(statements, i);
        }

        public Evaluation(Environment env)
        {
            this.env = env;
            pause = false;
            position = new Stack<int>();
        }

        public void Eval(INode node)
        {
            switch (node)
            {
                case Program _p:
                    pause = false;
                    position.Clear();
                    statements = _p.statements;
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
                case WhileStatement _while:
                    EvalWhileStatement(_while.condition, _while.consequence);
                    break;
            }
        }

        private void EvalProgram(List<Statement> statements, int start = 0)
        {
            var count = statements.Count;
            var i = start;
            while (i < count && !pause)
            {
                Eval(statements[i]);
                if (_trace.error)
                {
                    return;
                }
                if (!pause) i++;
            }

            if (i == count)
            {
                return;
            }

            if (position.Count > 0 && position.Peek() < 0)
            {
                position.Pop();
                position.Push(i);
                return;
            }
            else
                position.Push(i + 1);

        }

        private void EvalAssign(string name, List<Token> value)
        {
            var _value = Calculate(value);
            if (_value == null) return;
            env.Add(name, _value);
        }

        private void EvalIfStatement(List<Token> condition, BlockStatement consequence, Statement alternative)
        {
            if (position.Count > 0)
            {
                var p = position.Pop();
                if (p != -1)
                    EvalProgram(consequence.statements, p);
                else if (alternative is BlockStatement _block)
                    EvalProgram(_block.statements, position.Pop());
                else if (alternative is IfStatement _if)
                    EvalIfStatement(_if.condition, _if.consequence, _if.alternative);

                if (pause)
                {
                    if (p == -1 && alternative is IfStatement) position.Pop();
                    if (p == -1) position.Push(-1);
                    position.Push(-1);
                }
                return;
            }

            var _condition = Calculate(condition);
            if (_condition == null) return;
            if (!_condition.IsBoolean)
            {
                _trace.Error($"Can't convert {_condition.typeName} to boolean.");
                return;
            }

            if (_condition.b)
                EvalProgram(consequence.statements);
            else if (alternative == null)
                return;
            else if (alternative is BlockStatement _block)
                EvalProgram(_block.statements);
            else if (alternative is IfStatement _if)
                EvalIfStatement(_if.condition, _if.consequence, _if.alternative);

            if (pause)
            {
                if (!_condition.b && alternative is IfStatement) position.Pop();
                if (!_condition.b) position.Push(-1);
                position.Push(-1);
            }
        }

        private void EvalWhileStatement(List<Token> condition, BlockStatement consequence)
        {
            if (position.Count > 0)
                EvalProgram(consequence.statements, position.Pop());

            if (pause)
            {
                position.Push(-1);
                return;
            }

            var _count = 0;
            var _condition = Calculate(condition);
            if (_condition == null) return;
            if (!_condition.IsBoolean)
            {
                _trace.Error($"Can't convert {_condition.typeName} to boolean.");
                return;
            }

            while (_condition.b && _count < 1000)
            {
                EvalProgram(consequence.statements);
                if (pause)
                {
                    position.Push(-1);
                    return;
                }
                _condition = Calculate(condition);
                _count++;
            }
        }

        private void EvalCallStatement(string name, List<Token> argument)
        {
            var ok = env.func.TryGetValue(name, out Action<CodeObject> func);
            if (!ok)
            {
                _trace.Error($"Unknown '{name}'.");
                return;
            }
            var _argument = Calculate(argument);
            func(_argument);
        }

        private CodeObject Calculate(List<Token> value)
        {
            if (value == null) return null;
            CodeObject array = null;

            Stack<CodeObject> output = new Stack<CodeObject>();
            foreach (var tok in value)
            {
                switch (tok.type)
                {
                    case Type.IDENT:
                        var ok = env.GetStore(tok.literal, out var _var);
                        if (ok)
                            output.Push(_var);
                        else
                            _trace.Error($"Unknown '{tok.literal}'.");
                        break;
                    case Type.NUMBER:
                        output.Push(new CodeObject(double.Parse(tok.literal)));
                        break;
                    case Type.STRING:
                        output.Push(new CodeObject(tok.literal));
                        break;
                    case Type.TRUE:
                        output.Push(new CodeObject(true));
                        break;
                    case Type.FALSE:
                        output.Push(new CodeObject(false));
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
                    case Type.MOD:
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
                    case Type.COMMA:
                        if (array == null) array = new CodeObject(Type.ARRAY);
                        array.Add(output.Pop());
                        break;
                }
            }
            if (array != null)
            {
                array.Add(output.Pop());
                return array;
            }
            else if (output.Count == 1)
                return output.Pop();
            else
                return null;
        }

        private CodeObject CalculateInfix(Type type, CodeObject var1, CodeObject var2)
        {
            if (var1.IsNumber && var2.IsNumber)
            {
                return CalculateInfixNumber(type, var1.n, var2.n);
            }
            else if (var1.IsBoolean && var1.IsBoolean)
            {
                return CalculateInfixBoolean(type, var1.b, var2.b);
            }
            else if (var1.IsString || var2.IsString)
            {
                if (type == Type.PLUS)
                {
                    return new CodeObject($"{var1.String}{var2.String}");
                }
                else if (type == Type.EQUAL && var1.type == var2.type)
                {
                    return new CodeObject(var1.s == var2.s);
                }
                else if (type == Type.EQUAL && var1.type == var2.type)
                {
                    return new CodeObject(var1.s != var2.s);
                }
            }
            else
            {
                _trace.Error($"Can't {var1.typeName} {operatorSymbol(var1.type)} {var2.typeName}.");
            }
            return null;
        }

        private CodeObject CalculateInfixNumber(Type type, double value1, double value2)
        {
            return type switch
            {
                Type.PLUS => new CodeObject(value1 + value2),
                Type.MINUS => new CodeObject(value1 - value2),
                Type.MULTIPLY => new CodeObject(value1 * value2),
                Type.DIVIDE => new CodeObject(value1 / value2),
                Type.EQUAL => new CodeObject(value1 == value2),
                Type.NOTEQUAL => new CodeObject(value1 != value2),
                Type.GREATER => new CodeObject(value1 > value2),
                Type.GREATEROR => new CodeObject(value1 >= value2),
                Type.LESSTHAN => new CodeObject(value1 < value2),
                Type.LESSTHANOR => new CodeObject(value1 <= value2),
                Type.MOD => new CodeObject(value1 % value2),
                _ => null,
            };
        }

        private CodeObject CalculateInfixBoolean(Type type, bool value1, bool value2)
        {
            return type switch
            {
                Type.AND => new CodeObject(value1 && value2),
                Type.OR => new CodeObject(value1 || value2),
                Type.EQUAL => new CodeObject(value1 == value2),
                Type.NOTEQUAL => new CodeObject(value1 != value2),
                _ => null,
            };
        }

        private CodeObject CalculatePrefix(Type opr, CodeObject var)
        {
            if (opr == Type.NOT && var.IsBoolean)
            {
                var.Set(!var.b);
                return var;
            }
            else if (opr == Type.SIGN && var.IsNumber)
            {
                var.Set(-var.n);
                return var;
            }
            else
                _trace.Error($"Can't {operatorSymbol(opr)} {var.typeName}.");
            return null;
        }

        private string operatorSymbol(Type type)
        {
            return type switch
            {
                Type.PLUS => "+",
                Type.MINUS => "-",
                Type.MULTIPLY => "*",
                Type.DIVIDE => "/",
                Type.EQUAL => "==",
                Type.NOTEQUAL => "!=",
                Type.GREATER => ">",
                Type.GREATEROR => ">=",
                Type.LESSTHAN => "<",
                Type.LESSTHANOR => "<=",
                Type.MOD => "%",
                Type.AND => "&&",
                Type.OR => "||",
                Type.NOT => "!",
                Type.SIGN => "-",
                _ => null,
            };
        }

    }
}