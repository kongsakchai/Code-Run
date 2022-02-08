namespace CodeRun
{
    public class Lexer
    {
        private string source;
        private char ch;
        private int readPos, index, countSpace;
        private Token tok;

        public Lexer() { }

        public void Read(string source)
        {
            this.source = source;
            readPos = 0;
            countSpace = 0;
            tok = new Token(Type.NULL, "");
            ReadChar();
        }

        public Token NextToken()
        {

            //4 space is 1 tab
            countSpace = 0;
            while(ch=='\r')ReadChar();
            while (ch == ' ')
            {
                countSpace += 1;
                if (countSpace == 4)
                    ch = '\t';
                else
                    ReadChar();
            }

            switch (ch)
            {
                case '\0':
                    tok = new Token(Type.EOP, "End of program");
                    break;
                case '\n':
                    tok = new Token(Type.EOL, "End of line");
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
                case '%':
                    tok = new Token(Type.MOD, "%");
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
                    if (PeekChar() == '|') { tok = new Token(Type.OR, "||"); ReadChar(); }
                    else tok = new Token(Type.ERROR, "|");
                    break;
                case '\t':
                    tok = new Token(Type.TAB, "Tab");
                    break;
                case ':':
                    tok = new Token(Type.COLON, ":");
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
                case '[':
                    tok = new Token(Type.LBRACKET, "[");
                    break;
                case ']':
                    tok = new Token(Type.RBRACKET, "]");
                    break;
                case ',':
                    tok = new Token(Type.COMMA, ",");
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
            if (readPos < source.Length)
            {
                ch = source[readPos];
                index = readPos;
                readPos++;
            }
            else
            {
                index = readPos;
                ch = '\0';
            }
        }

        private char PeekChar() => readPos < source.Length ? source[readPos] : '\0';

        private string ReadIdentifier()
        {
            int i = index;
            while (IsLetter(ch) || IsNumber(ch))
                ReadChar();
            return source.Substring(i, index - i);
        }

        private string ReadNumber()
        {
            int i = index;
            while (IsNumber(ch) || ch == '.' && IsNumber(PeekChar()))
                ReadChar();
            return source.Substring(i, index - i);
        }

        private Token ReadString()
        {
            int i = index + 1;
            do
            {
                ReadChar();
            } while (ch != '"' && readPos < source.Length);
            if (ch == '"')
                return new Token(Type.STRING, source.Substring(i, index - i));
            else
                return new Token(Type.ERROR, $"Missing '\"'.");
        }

        private Type Keywords(string str)
        {
            return str switch
            {
                "loop" => Type.LOOP,
                "true" => Type.TRUE,
                "false" => Type.FALSE,
                "if" => Type.IF,
                "else" => Type.ELSE,
                //"let"=>Type.LET,
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
                case Type.COMMA:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

    }
}