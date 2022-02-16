using System.Collections.Generic;
using UnityEngine;

namespace CodeRun
{
    public class Parser
    {
        private enum ParserType { GROUP, ARRAY, Linear };
        private Lexer lexer;
        private Program program;
        private Token curToken, peekToken;
        private int countTab, layer;
        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
        }

        public Program ParseProgram()
        {
            program = new Program();
            curToken = lexer.NextToken();
            peekToken = lexer.NextToken();

            while (curToken.type != Type.EOP)
            {
                if (curToken.type == Type.ERROR)
                {
                    _tracer.Error(Code.Invalid, curToken.literal);
                    return null;
                }
                layer = countTab = 0;
                var statement = ParseStatement();
                if (statement != null) program.Add(statement);
                
                if (countTab != 0) _tracer.Error(Code.Invalid,"Tab");
                if (_tracer.error) return null;
                NextToken();
            }
            return program;
        }

        private void NextToken()
        {
            curToken = peekToken;
            peekToken = lexer.NextToken();
        }

        private bool ExpectPeek(Type t)
        {
            if (peekToken.type == t)
            {
                NextToken();
                return true;
            }
            return false;
        }

        private Statement ParseStatement()
        {
            return curToken.type switch
            {
                Type.IDENT => ParseIdentifier(),
                Type.IF => ParseIfStatement(),
                Type.LOOP => ParseLoopStatement(),
                _ => ParseNull()
            };
        }

        private Statement ParseNull()
        {
            switch (curToken.type)
            {
                case Type.EOL:
                case Type.SEMICOLON:
                case Type.EOP:
                    return null;
                default:
                    _tracer.Error(Code.Invalid,$"{peekToken.literal}");
                    return null;
            }
        }

        private Statement ParseIdentifier()
        {
            switch (peekToken.type)
            {
                case Type.ASSIGN:
                    return ParseAssignStatement();
                case Type.LBRACKET:
                    return ParseAssignStatement_Index();
                case Type.LPAREN:
                    return ParseCallStatement();
                default:
                    _tracer.Error(Code.Invalid,$"{peekToken.literal}");
                    return null;
            }
        }

        private AssignStatement ParseAssignStatement()
        {
            _tracer.Trace(curToken.literal);
            var token = curToken;//curToken is iden;
            NextToken();// curToken is '='

            NodeExpression value;
            if (ExpectPeek(Type.LBRACKET))
                value = PasreArray();
            else
                value = ParseExpression();

            if (_tracer.error) return null;
            NextToken();//CurToken is end
            _tracer.UnTrace();
            return new AssignStatement(token, value);
        }

        private AssignStatement ParseAssignStatement_Index()
        {
            _tracer.Trace(curToken.literal);
            var token = curToken;
            NextToken();
            var index = ParseExpression(Type.RBRACKET);

            if (!ExpectPeek(Type.RBRACKET))
                _tracer.Error(Code.Missing,"]");
            if (!ExpectPeek(Type.ASSIGN))
                _tracer.Error(Code.Invalid,$"{curToken.literal}?{peekToken.literal}");

            if (_tracer.error) return null;


            NodeExpression value;
            var array = ExpectPeek(Type.LBRACKET);
            if (array)
                value = PasreArray();
            else
                value = ParseExpression();

            if (array && !ExpectPeek(Type.RBRACKET)) _tracer.Error(Code.Missing,"]");
            if (_tracer.error) return null;

            NextToken();//CurToken is end
            _tracer.UnTrace();
            return new AssignStatement(token, value, index);
        }

        private CallStatement ParseCallStatement()
        {
            _tracer.Trace(curToken.literal);
            var token = curToken;
            NextToken();//curToken = '('

            var argument = (ExpectPeek(Type.RPAREN)) ? null : PasreArray(Type.RPAREN);

            if (curToken.type != Type.RPAREN)
                _tracer.Error(Code.Missing, ")");

            if (!ExpectPeek(Type.SEMICOLON) && !ExpectPeek(Type.EOL) && !ExpectPeek(Type.EOP))
                _tracer.Error(Code.Invalid, $"{curToken.literal}?{peekToken.literal}");

            if (_tracer.error) return null;
            //CurToken is end
            _tracer.UnTrace();
            return new CallStatement(token, argument);
        }

        private BlockStatement ParseBlockStatement(int layer)
        {
            var block = new BlockStatement();
            var limit = countTab = CountTab();
            while (countTab == limit && countTab > layer)
            {
                NextToken();
                if (curToken.type == Type.ERROR)
                {
                    _tracer.Error(Code.Invalid, curToken.literal);
                    return null;
                }

                this.layer = countTab;
                countTab = 0;
                var statement = ParseStatement();
                if (statement != null) block.Add(statement);

                if (_tracer.error) return null;

                if (curToken.type != Type.SEMICOLON)
                    countTab += CountTab(limit);
                else
                    countTab = limit;
            }

            if (block.statements.Count == 0) _tracer.Error(Code.MissingBlock);

            this.layer = layer;
            return block;
        }

        private IfStatement ParseIfStatement()
        {
            _tracer.Trace("if");
            var layer = this.layer;
            var condition = ParseExpression(Type.COLON);
            BlockStatement consequence = null;

            if (!ExpectPeek(Type.COLON))
                _tracer.Error(Code.Missing, ":");
            else if (!ExpectPeek(Type.EOL))
                _tracer.Error(Code.MissingBlock);
            else
                consequence = ParseBlockStatement(layer);

            if (_tracer.error) return null;

            Statement alternative = null;
            if (countTab == layer && ExpectPeek(Type.ELSE))
            {
                _tracer.Trace("else");
                if (!ExpectPeek(Type.COLON))
                    _tracer.Error(Code.Missing, ":");
                else if (!ExpectPeek(Type.EOL))
                    _tracer.Error(Code.MissingBlock);
                else if (ExpectPeek(Type.IF))
                    alternative = ParseIfStatement();
                else
                    alternative = ParseBlockStatement(layer);

                if (_tracer.error) return null;
                _tracer.UnTrace();
            }

            _tracer.UnTrace();
            return new IfStatement(condition, consequence, alternative);
        }

        private LoopStatement ParseLoopStatement()
        {
            _tracer.Trace("loop");
            var layer = this.layer;
            var condition = ParseExpression(Type.COLON);
            BlockStatement consequence = null;

            if (!ExpectPeek(Type.COLON))
                _tracer.Error(Code.Missing, ":");
            else if (!ExpectPeek(Type.EOL))
                _tracer.Error(Code.MissingBlock);
            else
                consequence = ParseBlockStatement(layer);

            if (_tracer.error) return null;

            _tracer.UnTrace();
            return new LoopStatement(condition, consequence);
        }

        private Array PasreArray(Type end = Type.RBRACKET)
        {
            Array output = new Array();
            Expression value = null;

            do
            {
                value = ParseExpression(end, true);
                output.Add(value);
                NextToken();
                if (_tracer.error) return null;
            } while (curToken.type == Type.COMMA);

            return output;
        }

        private Expression ParseExpression(Type end = Type.EOL, bool onComma = false)
        {

            Expression output = null;
            Expression temp = null;
            var p1 = 2;
            var p2 = Priority(peekToken.type);//peekToken is next token after '=', '(' and '['

            while (peekToken.type != end && peekToken.type != Type.EOL && peekToken.type != Type.EOP)
            {
                if (VerifyState(p1, p2))
                {
                    if (p2 == 8)
                    {
                        NextToken();// curToken is '('
                        temp = ParseExpression(Type.RPAREN);
                        temp.SetPriority(10);
                        p2 = 10;
                    }
                    else if (p2 == 9)
                    {
                        if (curToken.type != Type.IDENT)
                        {
                            _tracer.Error(Code.Invalid, $"{curToken.literal}?{peekToken.literal}");
                            break;
                        }
                        NextToken();// curToken is '('
                        temp = ParseExpression(Type.RBRACKET);
                        temp.SetPriority(0);
                        p2 = 10;
                    }
                    else
                    {
                        temp = new Expression(peekToken);
                        temp.SetPriority(p2);
                    }

                    output = InsertExpression(output, temp);

                    NextToken();
                    // Emergency break
                    if (peekToken.type == Type.SEMICOLON) return output;
                    if (onComma && peekToken.type == Type.COMMA) return output;

                    p1 = p2;
                    p2 = Priority(peekToken.type);
                }
                else
                {
                    _tracer.Error(Code.Invalid, $"{curToken.literal}?{peekToken.literal}");
                    break;
                }
            }

            if (end != Type.EOL && peekToken.type != end)
                _tracer.Error(Code.Missing);

            if (output == null)
                _tracer.Error(Code.Invalid, $"{curToken.literal}");

            if (_tracer.error) return null;

            return output;
        }

        private Expression InsertExpression(Expression node, Expression newNode)
        {

            if (node == null)
                return newNode;
            else if (newNode.p == 0)
            {
                if (node.p == 1)
                    node.SetRight(newNode);
                else
                {
                    InsertExpression(node.right, newNode);
                }
            }
            else if (node.p == 1 || newNode.p != 1 && node.p >= newNode.p)
            {
                newNode.SetLeft(node);
                return newNode;
            }
            else if (newNode.p == 1 || node.p != 1 && node.p < newNode.p)
            {
                if (node.right == null)
                    node.SetRight(newNode);
                else
                {
                    var temp = InsertExpression(node.right, newNode);
                    node.SetRight(temp);
                }
            }
            return node;
        }

        private int Priority(Type type)
        {
            switch (type)
            {
                case Type.NUMBER:
                case Type.STRING:
                case Type.IDENT:
                case Type.TRUE:
                case Type.FALSE:
                    return 1;
                case Type.ASSIGN:
                    return 2;
                case Type.AND:
                case Type.OR:
                    return 3;
                case Type.EQUAL:
                case Type.NOTEQUAL:
                case Type.GREATER:
                case Type.LESSTHAN:
                case Type.GREATEROR:
                case Type.LESSTHANOR:
                    return 4;
                case Type.PLUS:
                case Type.MINUS:
                    return 5;
                case Type.MULTIPLY:
                case Type.DIVIDE:
                case Type.MOD:
                    return 6;
                case Type.NOT:
                case Type.SIGN:
                    return 7;
                case Type.LPAREN:
                    return 8;
                case Type.LBRACKET:
                    return 9;
                case Type.RPAREN:
                case Type.RBRACKET:
                    return 10;
                default:
                    return 0;
            }
        }

        private bool VerifyState(int p1, int p2)
        {
            //4 5 6 = 3
            if (p1 >= 4 && p1 <= 6) p1 = 3;
            if (p2 >= 4 && p2 <= 6) p2 = 3;

            switch (p1)
            {
                case 1:
                case 10:
                    if (p2 == 3 || p2 == 9 || p2 == 10) return true;
                    return false;
                case 2:
                case 3:
                case 7:
                case 8:
                case 9:
                    if (p2 == 1 || p2 == 7 || p2 == 8) return true;
                    return false;
                case 0:
                    return false;
            }
            return false;
        }

        private int CountTab(int tab = 0)
        {
            var i = 0;
            while ((tab == 0 || i < tab) && ExpectPeek(Type.TAB)) i++;
            return i;
        }
    }
}