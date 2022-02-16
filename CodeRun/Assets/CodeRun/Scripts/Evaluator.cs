using System.Collections.Generic;
using System;

namespace CodeRun
{
    public class Evaluator
    {

        public bool end;
        private Environment env;
        private List<StructProgram> programs;
        private int index;

        public Evaluator(Environment env)
        {
            this.env = env;
            programs = new List<StructProgram>();
        }

        public void Eval(Node node)
        {
            switch (node)
            {
                case Program _p:
                    programs.Clear();
                    index = -1;
                    EvalProgram(_p.statements);
                    break;
                case BlockStatement _block:
                    EvalProgram(_block.statements);
                    break;
                case AssignStatement _assign:
                    EvalAssign(_assign.name, _assign.value, _assign.index);
                    break;
                case IfStatement _if:
                    EvalIfStatement(_if.condition, _if.consequence, _if.alternative);
                    break;
                case CallStatement _call:
                    EvalCallStatement(_call.name, _call.argument);
                    break;
                case LoopStatement _while:
                    EvalLoopStatement(_while.condition, _while.consequence);
                    break;
                case null:
                    NextStatement();
                    break;
            }
        }

        public void NextStatement()
        {
            if (_tracer.error) UnityEngine.Debug.Log($"{_tracer.code} {_tracer.GetLog()}");
            var ok = programs[index].Next();
            if (ok)
            {
                CurrentStatement();
            }
            else
            {
                programs.RemoveAt(index);
                if (--index >= 0)
                    NextStatement();
                else
                    end = true;
            }
        }

        public void CurrentStatement()
        {
            if (_tracer.error) return;

            var statement = programs[index].GetStatement();
            Eval(statement);
        }

        private void EvalProgram(List<Statement> statements)
        {
            programs.Add(new StructProgram(statements));
            index++;
            CurrentStatement();
        }

        private void EvalAssign(string name, NodeExpression value, Expression index)
        {
            _tracer.Trace(name);
            var ok = env.HasName(name);

            if (ok && env.IsReadOnly(name))
            {
                _tracer.Error(Code.ReadOnly, name);
                return;
            }
            CodeObject _index = null;
            if (index != null && ok)
            {
                _index = Calculate(index);
                if (_index == null) return;
                if (!_index.IsNumber)
                {
                    _tracer.Error(Code.Variable, $"{_index.typeName}?index");
                    return;
                }
            }
            else if (index != null)
            {
                _tracer.Error(Code.Unknow, name);
                return;
            }


            CodeObject _value;
            if (!value.array)
                _value = Calculate(value as Expression);
            else
                _value = Calculate(value as Array);

            if (_tracer.error) return;
            if (ok && index != null)
                env.Set(name, _value, _index.i);
            else if (ok)
                env.Set(name, _value);
            else
                env.Add(name, _value);

            _tracer.UnTrace();
            NextStatement();
        }

        private void EvalCallStatement(string name, Array argument)
        {
            _tracer.Trace(name);
            var ok = env.funcs.TryGetValue(name, out Action<CodeObject, Action> func);
            if (!ok)
            {
                _tracer.Error(Code.Unknow, name);
                return;
            }

            CodeObject _array = (argument != null) ? Calculate(argument as Array) : null;
            CodeObject _argument = (_array != null && _array.Count == 1) ? _array[0] : _array;
            _tracer.UnTrace();
            func(_argument, NextStatement);
        }

        private void EvalIfStatement(Expression condition, BlockStatement consequence, Statement alternative)
        {
            _tracer.Trace("if");
            var _condition = Calculate(condition);
            if (_tracer.error) return;
            if (!_condition.IsBoolean)
            {
                _tracer.Error(Code.Variable, $"{_condition.typeName}?index");
                return;
            }
            _tracer.UnTrace();

            if (_condition.b)
                EvalProgram(consequence.statements);
            else if (alternative == null)
                NextStatement();
            else if (alternative is BlockStatement _block)
                EvalProgram(_block.statements);
            else if (alternative is IfStatement _if)
                EvalIfStatement(_if.condition, _if.consequence, _if.alternative);
        }

        private void EvalLoopStatement(Expression condition, BlockStatement consequence)
        {
            _tracer.Trace("loop");
            var _condition = Calculate(condition);
            if (_tracer.error) return;
            if (!_condition.IsBoolean)
            {
                _tracer.Error(Code.Variable, $"{_condition.typeName}?index");
                return;
            }
            try
            {
                if (_condition.b)
                {
                    programs[index].Previous();
                    EvalProgram(consequence.statements);
                }
                else
                    NextStatement();
            }
            catch
            {

            }
            _tracer.UnTrace();
        }

        private CodeObject Calculate(Array value)
        {
            CodeObject output = new CodeObject(Type.ARRAY);
            CodeObject temp = null;

            for (int i = 0; i < value.count; i++)
            {
                temp = Calculate(value[i]);
                if (_tracer.error) return null;
                output.Add(temp);
            }

            return output;
        }

        private CodeObject Calculate(Expression value)
        {
            if (value == null) return null;

            if (value.token.type == Type.IDENT)
            {
                var _ok = env.GetStore(value.token.literal, out var _var);
                if (!_ok)
                {
                    _tracer.Error(Code.Unknow, value.token.literal);
                    return null;
                }

                if (_var.IsArray && value.right != null)
                {
                    var index = Calculate(value.right);
                    if (index == null) return null;
                    if (!index.IsNumber)
                    {
                        _tracer.Error(Code.Variable, $"{index.typeName}?index");
                        return null;
                    }
                    return _var[index.i];
                }
                return _var;
            }
            else if (value.left == null && value.right == null)
                return CodeObject.Convert(value.token);
            else
            {
                var _left = Calculate(value.left);
                var _right = Calculate(value.right);
                if (_tracer.error) return null;

                if (value.token.type == Type.NOT || value.token.type == Type.SIGN)
                {
                    if (_right != null)
                        return CalculatePrefix(value.token.type, _right);

                    _tracer.Error(Code.Missing, "value");
                    return null;
                }
                else
                    return CalculateInfix(value.token.type, _left, _right);
            }
        }

        private CodeObject CalculateInfix(Type opr, CodeObject var1, CodeObject var2)
        {
            if (var1.IsNumber && var2.IsNumber)
            {
                return CalculateInfixNumber(opr, var1.n, var2.n);
            }
            else if (var1.IsBoolean && var1.IsBoolean)
            {
                return CalculateInfixBoolean(opr, var1.b, var2.b);
            }
            else if (var1.IsString || var2.IsString)
            {
                if (opr == Type.PLUS)
                {
                    return new CodeObject($"{var1.String}{var2.String}");
                }
                else if (opr == Type.EQUAL && var1.type == var2.type)
                {
                    return new CodeObject(var1.s == var2.s);
                }
                else if (opr == Type.NOTEQUAL && var1.type == var2.type)
                {
                    return new CodeObject(var1.s != var2.s);
                }
            }

            _tracer.Error(Code.Invalid, $"{var1.typeName}?{operatorSymbol(opr)}?{var2.typeName}");
            return null;
        }

        private CodeObject CalculateInfixNumber(Type opr, double value1, double value2)
        {
            return opr switch
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

        private CodeObject CalculateInfixBoolean(Type opr, bool value1, bool value2)
        {
            return opr switch
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
                return new CodeObject(!var.b);
            else if (opr == Type.SIGN && var.IsNumber)
                return new CodeObject(-var.n);
            else
                _tracer.Error(Code.Invalid, $"{operatorSymbol(opr)}?{var.typeName}");
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

    class StructProgram
    {
        private int count { get; }
        private int index;
        private List<Statement> statements;
        public StructProgram(List<Statement> statements)
        {
            this.statements = statements;
            this.count = statements.Count;
            this.index = 0;
        }

        public Statement GetStatement() => (index < count) ? statements[index] : null;
        public bool Next() => (++index < count);
        public void Previous() => index--;
    }

}