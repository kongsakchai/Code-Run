using System.Collections.Generic;
using System.Linq;

namespace CodeRun
{
    public enum Type
    {
        // End
        EOL,// End of line
        EOP,// End of program

        // Varible & Identifer
        NUMBER,
        STRING,
        BOOLEAN,
        ARRAY,
        OBJECT,
        IDENT,

        // Operator
        ASSIGN, // =
        MOD, // %
        PLUS, // +
        MINUS, // -
        MULTIPLY, // *
        DIVIDE, // /
        SIGN,// -n

        // Logic
        AND, // &&
        OR, // ||
        EQUAL, // ==
        NOTEQUAL, // !=
        NOT, // !
        GREATER, // >
        LESSTHAN, // <
        GREATEROR, // >=
        LESSTHANOR, // <=

        // Delimiters
        SEMICOLON, // ;
        LPAREN, // ( 
        RPAREN, // )
        LBRACE, // {
        RBRACE, // }
        COMMA, // ,

        // Keywords
        TRUE,
        FALSE,
        IF,
        ELSE,
        FUNC,
        WHILE,

        NULL,
        ERROR
    }

    public struct Token
    {
        public Type type { get; }
        public string literal { get; }

        public Token(Type type, string literal)
        {
            this.type = type;
            this.literal = literal;
        }

        public override string ToString() => $"{type} : {literal}";
    }

    public interface INode
    {
        string String { get; }
    }

    public class Program : INode
    {
        public List<Statement> statements;
        public Program()
        {
            statements = new List<Statement>();
        }
        public void Add(Statement item)
        {
            statements.Add(item);
        }
        public string String => string.Join(" ", statements.Select(s => s.String));
    }

    public class Statement : INode
    {
        public Token token { get; protected set; }
        public virtual string String { get => token.literal; }
    }

    public class BlockStatement : Statement
    {
        public List<Statement> statements;
        public BlockStatement(Token token)
        {
            this.token = token;
            statements = new List<Statement>();
        }
        public BlockStatement(List<Statement> statements)
        {
            this.token = new Token(Type.LBRACE,"{");
            this.statements = statements;
        }
        public void Add(Statement item)
        {
            statements.Add(item);
        }
        public override string String => string.Join(" ", statements.Select(s => s.String));
    }

    public class AssignStatement : Statement
    {
        public string name { get; }
        public List<Token> value { get; }
        public AssignStatement(Token token, List<Token> value)
        {
            this.token = token;
            this.name = token.literal;
            this.value = value;
        }
        public override string String => $"{base.String} = {string.Join(" ", value.Select(s => s.literal))}";
    }

    public class IfStatement : Statement
    {
        public List<Token> condition { get; }
        public BlockStatement consequence { get; }
        public Statement alternative { get; }
        public IfStatement(Token token, List<Token> condition, BlockStatement consequence, Statement alternative)
        {
            this.token = token;
            this.condition = condition;
            this.consequence = consequence;
            this.alternative = alternative;
        }
        public override string String
        {
            get
            {
                var s = $"if {string.Join(" ", condition.Select(s => s.literal))} {{{consequence.String}}}";
                if (alternative is null)
                    return s;
                else
                    return $"{s} else {{{alternative.String}}}";
            }
        }
    }

    public class WhileStatement : Statement
    {
        public List<Token> condition { get; }
        public BlockStatement consequence { get; }
        public WhileStatement(Token token, List<Token> condition, BlockStatement consequence)
        {
            this.token = token;
            this.condition = condition;
            this.consequence = consequence;
        }
        public override string String => $"while {string.Join(" ", condition.Select(s => s.literal))} {{{consequence.String}}}";
    }

    public class CallStatement : Statement
    {
        public string name { get; }
        public List<Token> argument { get; }
        public CallStatement(Token token, List<Token> argument)
        {
            this.token = token;
            this.name = token.literal;
            this.argument = argument;
        }
        public override string String => $"{base.String} ({string.Join(" ", argument.Select(s => s.literal))})";
    }

}