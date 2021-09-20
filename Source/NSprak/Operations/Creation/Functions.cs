using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Operations.Types;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Language;

namespace NSprak.Operations.Creation
{
    public static class Functions
    {
        public static void GenerateCode(OperatorCall call, GeneratorContext builder)
        {
            if (call.LeftInput != null) builder.AddCode(call.LeftInput);
            if (call.RightInput != null) builder.AddCode(call.RightInput);

            builder.AddOp(new CallBuiltIn(call.BuiltInFunctionHint), call.OperatorToken);
        }

        public static void GenerateCode(FunctionCall call, GeneratorContext builder)
        {
            foreach (Expression arg in call.Arguments)
                builder.AddCode(arg);

            if (call.BuiltInFunctionHint != null)
                builder.AddOp(new CallBuiltIn(call.BuiltInFunctionHint), call.NameToken);

            else builder.AddOp(new Call(call.UserFunctionHint), call.NameToken);
        }

        public static void GenerateCode(FunctionHeader header, GeneratorContext builder)
        {
            foreach (string name in header.ParameterNames.Reverse())
                builder.AddOp(new VariableCreate(name), header.NameToken);

            foreach (Expression statement in header.ParentBlockHint.Statements)
                builder.AddCode(statement);

            // This is hacky, and might be cleaned up someday
            List<Op> sheet = builder.Operations;
            bool returnMissing = sheet.Count == 0 || !(sheet[^1] is Types.Return);
            if (returnMissing) builder.AddOp(new Types.Return(), header.EndToken);
        }
    }
}
