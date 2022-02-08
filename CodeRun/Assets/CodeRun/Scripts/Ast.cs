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
        LBRACKET, // [
        RBRACKET, // ]
        COMMA, // ,
        TAB,
        COLON,// :

        // Keywords
        TRUE,
        FALSE,
        IF,
        ELSE,
        FUNC,
        LOOP,

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

    public abstract class Node
    {
    }

    public abstract class NodeExpression
    {
        public virtual bool array => false;
    }

    public class Program : Node
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

    public class Expression : NodeExpression
    {
        public Token token;
        public Expression left { get; private set; }
        public Expression right { get; private set; }
        public int p { get; private set; }
        public Expression(Token token)
        {
            this.token = token;
            this.left = null;
            this.right = null;
        }
        public void SetPriority(int priority) => this.p = priority;
        public void SetLeft(Expression left) => this.left = left;
        public void SetRight(Expression right) => this.right = right;

        public override string ToString() => $"Token : {token.literal} | Priority : {p}";
    }

    public class Array : NodeExpression
    {
        public List<Expression> list;
        public int count;

        public Array()
        {
            this.list = new List<Expression>();
            this.count = 0;
        }

        public void Add(Expression expression)
        {
            this.list.Add(expression);
            this.count += 1;
        }

        public Expression this[int index]
        {
            get
            {
                if (count > index) return list[index];
                return null;
            }
        }

        public override bool array => true;
    }

    public abstract class Statement : Node
    {
        public Token token { get; protected set; }
        public virtual string String { get => token.literal; }
    }

    public class BlockStatement : Statement
    {
        public List<Statement> statements;
        public BlockStatement()
        {
            statements = new List<Statement>();
        }
        public BlockStatement(List<Statement> statements)
        {
            //this.token = new Token(Type.LBRACE, "{");
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
        public NodeExpression value { get; }
        public Expression index { get;}
        public AssignStatement(Token token, NodeExpression value,Expression index = null)
        {
            this.token = token;
            this.name = token.literal;
            this.value = value;
            this.index = index;
        }
        //public override string String => $"{base.String} = {string.Join(" ", value.Select(s => s.literal))}";
    }

    public class IfStatement : Statement
    {
        public Expression condition { get; }
        public BlockStatement consequence { get; }
        public Statement alternative { get; }
        public IfStatement(Expression condition, BlockStatement consequence, Statement alternative)
        {
            this.condition = condition;
            this.consequence = consequence;
            this.alternative = alternative;
        }
        /*public override string String
        {
            get
            {
                var s = $"if {string.Join(" ", condition.Select(s => s.literal))} {{{consequence.String}}}";
                if (alternative is null)
                    return s;
                else
                    return $"{s} else {{{alternative.String}}}";
            }
        }*/
    }

    public class LoopStatement : Statement
    {
        public Expression condition { get; }
        public BlockStatement consequence { get; }
        public LoopStatement(Expression condition, BlockStatement consequence)
        {
            this.condition = condition;
            this.consequence = consequence;
        }
        //public override string String => $"while {string.Join(" ", condition.Select(s => s.literal))} {{{consequence.String}}}";
    }

    public class CallStatement : Statement
    {
        public string name { get; }
        public Array argument { get; }
        public CallStatement(Token token, Array argument)
        {
            this.token = token;
            this.name = token.literal;
            this.argument = argument;
        }
        //public override string String => $"{base.String} ({string.Join(" ", argument.Select(s => s.literal))})";
    }

}