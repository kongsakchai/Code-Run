using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRun
{
    public class Parser
    {
        private Lexer lexer;
        private Program program;
        private Token curToken, peekToken;
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
                var statement = ParseStatement();
                if (statement != null)
                {
                    program.Add(statement);
                }
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
                Type.WHILE => ParseWhileStatement(),
                _ => null
            };
        }

        private Statement ParseIdentifier()
        {
            var token = curToken;
            if (ExpectPeek(Type.ASSIGN))
            {
                var value = ParseValue();
                if (value == null) return null;
                return new AssignStatement(token, value);
            }
            else if (ExpectPeek(Type.LPAREN))
            {
                var argument = ParseGroup();
                if (argument == null) return null;
                NextToken();
                if (!ExpectPeek(Type.SEMICOLON)) return null;
                return new CallStatement(token, argument);
            }
            return null;
        }

        private IfStatement ParseIfStatement()
        {
            var token = curToken;

            if (!ExpectPeek(Type.LPAREN)) return null;
            var condition = ParseGroup();
            if (condition == null) return null;
            NextToken();

            if (!ExpectPeek(Type.LBRACE)) return null;
            var consequence = ParseBlockStatement();
            if (consequence == null) return null;

            Statement alternative = null;
            if (ExpectPeek(Type.ELSE))
            {
                if (ExpectPeek(Type.LBRACE))
                    alternative = ParseBlockStatement();
                else if (ExpectPeek(Type.IF))
                    alternative = ParseIfStatement();

                if (alternative == null) return null;
            }

            return new IfStatement(curToken, condition, consequence, alternative);
        }

        private WhileStatement ParseWhileStatement()
        {
            var token = curToken;
            if (!ExpectPeek(Type.LPAREN)) return null;
            var condition = ParseGroup();
            if (condition == null) return null;
            NextToken();

            if (!ExpectPeek(Type.LBRACE)) return null;
            var consequence = ParseBlockStatement();
            if (consequence == null) return null;
            return new WhileStatement(token, condition, consequence);
        }

        private BlockStatement ParseBlockStatement()
        {
            var block = new BlockStatement(curToken);
            NextToken();
            while (curToken.type != Type.RBRACE && curToken.type != Type.EOP)
            {
                var statement = ParseStatement();
                if (statement == null) return null;
                block.Add(statement);
                NextToken();
            }
            if (curToken.type != Type.RBRACE) return null;

            return block;
        }

        private List<Token> ParseValue()
        {
            var output = new List<Token>();
            var stack = new Stack<Token>();

            var p1 = Priority(curToken.type);
            var p2 = Priority(peekToken.type);
            while (peekToken.type != Type.SEMICOLON && peekToken.type != Type.EOP)
            {
                if (VerifyState(p1, p2))
                {
                    if (p2 == 1)
                        output.Add(peekToken);
                    else if (p2 >= 3 && p2 <= 7)
                    {
                        while (stack.Count > 0 && p2 < Priority(stack.Peek().type))
                            output.Add(stack.Pop());
                        stack.Push(peekToken);
                    }
                    else if (p2 == 8)
                    {
                        NextToken();
                        var list = ParseGroup();
                        if (list == null) return null;
                        output.AddRange(list);
                        p2 = 9;
                    }
                    NextToken();
                    p1 = p2;
                    p2 = Priority(peekToken.type);
                }
                else
                {
                    return null;
                    //Error
                }
            }
            if (!ExpectPeek(Type.SEMICOLON)) return null;

            while (stack.Count > 0)
                output.Add(stack.Pop());

            if (output.Count > 0) return output;
            return null;
        }

        private List<Token> ParseGroup()
        {
            var output = new List<Token>();
            var stack = new Stack<Token>();

            var p1 = Priority(curToken.type);
            var p2 = Priority(peekToken.type);

            while (peekToken.type != Type.RPAREN && peekToken.type != Type.EOP)
            {
                if (VerifyState(p1, p2))
                {
                    if (p2 == 1)
                        output.Add(peekToken);
                    else if (p2 >= 3 && p2 <= 7)
                    {
                        while (stack.Count > 0 && p2 < Priority(stack.Peek().type))
                            output.Add(stack.Pop());
                        stack.Push(peekToken);
                    }
                    else if (p2 == 8)
                    {
                        NextToken();
                        var list = ParseGroup();
                        if (list == null) return null;
                        output.AddRange(list);
                    }
                    NextToken();
                    p1 = p2;
                    p2 = Priority(peekToken.type);
                }
                else
                {
                    return null;
                    //Error
                }
            }
            if (ExpectPeek(Type.EOP)) return null;

            while (stack.Count > 0)
                output.Add(stack.Pop());

            if (output.Count > 0) return output;
            return null;
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
                    return 6;
                case Type.NOT:
                case Type.SIGN:
                    return 7;
                case Type.LPAREN:
                    return 8;
                case Type.RPAREN:
                    return 9;
                default:
                    return 0;
            }
        }

        private bool VerifyState(int p1, int p2)
        {
            if (p1 >= 4 && p1 <= 6) p1 = 3;
            if (p2 >= 4 && p2 <= 6) p2 = 3;

            switch (p1)
            {
                case 1:
                case 9:
                    if (p2 == 3 || p2 == 9) return true;
                    break;
                case 2:
                case 3:
                case 7:
                case 8:
                    if (p2 == 1 || p2 == 7 || p2 == 8) return true;
                    break;
                case 0:
                    return false;
            }
            return false;
        }

    }
}