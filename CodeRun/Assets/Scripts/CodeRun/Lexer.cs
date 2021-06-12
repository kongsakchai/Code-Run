using System.Collections;
using System.Collections.Generic;

namespace CodeRun
{
    public class Lexer
    {
        private string code;
        private char ch;
        private int readPos, index;
        private Token tok;

        public Lexer() { }

        public void Read(string source)
        {
            code = source;
            readPos = 0;
            tok = new Token(Type.NULL, "");
            ReadChar();
        }

        public Token NextToken()
        {
            while (ch == ' ' || ch == '\n')
                ReadChar();

            switch (ch)
            {
                case '\0':
                    tok = new Token(Type.EOP, "\0");
                    break;
                case '+':
                    tok = new Token(Type.PLUS, "+");
                    break;
                case '-':
                    if (IsSign()) tok = new Token(Type.SIGN, "-");
                    else tok = new Token(Type.MINUS, "-");
                    break;
                case '*':
                    tok = new Token(Type.MULTIPLY, "*");
                    break;
                case '/':
                    tok = new Token(Type.DIVIDE, "/");
                    break;
                case '=':
                    if (PeekChar() == '=') { tok = new Token(Type.EQUAL, "=="); ReadChar(); }
                    else tok = new Token(Type.ASSIGN, "=");
                    break;
                case '!':
                    if (PeekChar() == '=') { tok = new Token(Type.NOTEQUAL, "!="); ReadChar(); }
                    else tok = new Token(Type.NOT, "!");
                    break;
                case '<':
                    if (PeekChar() == '=') { tok = new Token(Type.LESSTHANOR, "<="); ReadChar(); }
                    else tok = new Token(Type.LESSTHAN, "<");
                    break;
                case '>':
                    if (PeekChar() == '=') { tok = new Token(Type.GREATEROR, ">="); ReadChar(); }
                    else tok = new Token(Type.GREATER, ">");
                    break;
                case '&':
                    if (PeekChar() == '&') { tok = new Token(Type.AND, "&&"); ReadChar(); }
                    else tok = new Token(Type.ERROR, "&");
                    break;
                case '|':
                    if (PeekChar() == '|') tok = new Token(Type.OR, "||");
                    else tok = new Token(Type.ERROR, "|");
                    break;
                case ';':
                    tok = new Token(Type.SEMICOLON, ";");
                    break;
                case '(':
                    tok = new Token(Type.LPAREN, "(");
                    break;
                case ')':
                    tok = new Token(Type.RPAREN, ")");
                    break;
                case '{':
                    tok = new Token(Type.LBRACE, "{");
                    break;
                case '}':
                    tok = new Token(Type.RBRACE, "}");
                    break;
                case '"':
                    tok = ReadString();
                    break;
                default:
                    if (IsNumber(ch))
                    {
                        tok = new Token(Type.NUMBER, ReadNumber());
                        return tok;
                    }
                    else if (IsLetter(ch))
                    {
                        var ident = ReadIdentifier();
                        var type = Keywords(ident);
                        tok = new Token(type, ident);
                        return tok;
                    }
                    else
                        tok = new Token(Type.ERROR, $"Unknown '{ch}'.");
                    break;
            }

            ReadChar();
            return tok;
        }

        private void ReadChar()
        {
            if (readPos < code.Length)
            {
                ch = code[readPos];
                index = readPos;
                readPos++;
            }
            else
            {
                index = readPos;
                ch = '\0';
            }
        }

        private char PeekChar() => readPos < code.Length ? code[readPos] : '\0';

        private string ReadIdentifier()
        {
            int i = index;
            while (IsLetter(ch) || IsNumber(ch))
                ReadChar();
            return code.Substring(i, index - i);
        }

        private string ReadNumber()
        {
            int i = index;
            while (IsNumber(ch) || ch == '.' && IsNumber(PeekChar()))
                ReadChar();
            return code.Substring(i, index - i);
        }

        private Token ReadString()
        {
            int i = index + 1;
            do
            {
                ReadChar();
            } while (ch != '"' && readPos < code.Length);
            if (readPos < code.Length)
                return new Token(Type.STRING, code.Substring(i, index - i));
            else
                return new Token(Type.ERROR, $"Missing '\"'.");
        }

        private Type Keywords(string str)
        {
            return str switch
            {
                "true" => Type.TRUE,
                "false" => Type.FALSE,
                "if" => Type.IF,
                "else" => Type.ELSE,
                _ => Type.IDENT,
            };
        }

        #region Boolean
        private bool IsNumber(char ch) => ch >= '0' && ch <= '9';
        private bool IsLetter(char ch) => ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch == '_';
        private bool IsSign()
        {
            switch (tok.type)
            {
                case Type.ASSIGN:
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
                case Type.LPAREN:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

    }
}