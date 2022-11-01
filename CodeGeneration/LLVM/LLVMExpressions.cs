using Simp.AST;
using Simp.Common;

namespace Simp.CodeGeneration.LLVM
{
    public partial class Builder
    {
        string BuildExpression(ExpressionNode expr)
        {
            return expr switch
            {
                IntLiteral i => BuildIntLiteral(i),
                Binary b => BuildBinary(b),
                Variable v => BuildVariable(v),
                Assign a => BuildAssign(a),
                Call c => BuildCall(c),
                _ => throw new NotImplementedException()
            };
        }

        string BuildIntLiteral(IntLiteral i)
        {
            return i.Value.ToString();
        }

        string BuildBinaryComparison(
            Binary b,
            string reg1,
            string reg2,
            string reg3)
        {
            var expr = b.Operator.Type switch
            {
                TokenType.EqualEqual => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.eq),
                TokenType.BangEqual => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.ne),
                TokenType.Less => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.slt),
                TokenType.LessEqual => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.sle),
                TokenType.Greater => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.sgt),
                TokenType.GreaterEqual => new Comp(reg1, reg2, reg3, Int64.Instance, Comp.Condition.sge),
                _ => throw new NotImplementedException()
            };

            CurrentLabel.Add(expr);

            // Cast the result from a boolean to an int
            var cast = NextTemp();
            CurrentLabel.Add(new Extension(
                reg3,
                cast,
                I1.Instance,
                Int64.Instance,
                Extension.ExtensionType.zext
            ));

            return cast;
        }

        string BuildBinary(Binary b)
        {
            var reg1 = BuildExpression(b.Left);
            var reg2 = BuildExpression(b.Right);
            var reg3 = NextTemp();

            LLVMOp expr;
            switch (b.Operator.Type)
            {
                case TokenType.Plus:
                    expr = new MathOp(reg1, reg2, reg3, Int64.Instance, MathOp.Operator.add);
                    CurrentLabel.Add(expr);
                    break;
                case TokenType.Minus:
                    expr = new MathOp(reg1, reg2, reg3, Int64.Instance, MathOp.Operator.sub);
                    CurrentLabel.Add(expr);
                    break;
                case TokenType.Star:
                    expr = new MathOp(reg1, reg2, reg3, Int64.Instance, MathOp.Operator.mul);
                    CurrentLabel.Add(expr);
                    break;
                case TokenType.Slash:
                    expr = new MathOp(reg1, reg2, reg3, Int64.Instance, MathOp.Operator.sdiv);
                    CurrentLabel.Add(expr);
                    break;
                case TokenType.EqualEqual:
                case TokenType.BangEqual:
                case TokenType.Less:
                case TokenType.LessEqual:
                case TokenType.Greater:
                case TokenType.GreaterEqual:
                    reg3 = BuildBinaryComparison(b, reg1, reg2, reg3);
                    break;
                default:
                    throw new TokenError("Unexpected operator.", b.Operator);
            };

            return reg3;
        }

        string BuildVariable(Variable v)
        {
            var variable = LookupVariable(v);

            var value = NextTemp();
            CurrentLabel.Add(
                new Load(
                    Pointer.Int64Ptr,
                    variable,
                    Int64.Instance,
                    value)
            );

            return value;
        }

        string BuildAssign(Assign a)
        {
            var v = a.Target as Variable;
            var variable = LookupVariable(v);

            var expr = BuildExpression(a.Value);
            CurrentLabel.Add(new Store(
                Int64.Instance,
                expr,
                Pointer.Int64Ptr,
                variable
            ));

            return expr;
        }

        string BuildCall(Call c)
        {
            string callSite;
            if (c.Callee is Variable v && GlobalVariables.ContainsKey(v.Name.QualifiedName))
            {
                callSite = GlobalVariables[v.Name.QualifiedName];
            }
            else
            {
                throw new NotImplementedException();
                // callSite = BuildExpression(c.Callee);
            }

            var result = NextTemp();
            var argList = c
                .ArgumentList
                .Select(arg => (Int64.Instance as DataType, BuildExpression(arg)))
                .ToList();

            CurrentLabel.Add(new CallFunction(
                Int64.Instance,
                callSite,
                argList,
                result
            ));

            return result;

        }
    }
}