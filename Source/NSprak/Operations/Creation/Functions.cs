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
            switch (call.Operator.Inputs)
            {
                case OperatorSide.Both:
                    if (call.LeftInput != null) builder.AddCode(call.LeftInput);
                    if (call.RightInput != null) builder.AddCode(call.RightInput);
                    break;

                case OperatorSide.Left:
                    if (call.LeftInput != null) builder.AddCode(call.LeftInput);
                    break;

                case OperatorSide.Right:
                    if (call.RightInput != null) builder.AddCode(call.RightInput);
                    break;
            }

            builder.AddOp(new CallBuiltIn(call.BuiltInFunctionHint));
        }

        public static void GenerateCode(FunctionCall call, GeneratorContext builder)
        {
            foreach (Expression arg in call.Arguments)
                builder.AddCode(arg);

            if (call.BuiltInFunctionHint != null)
                builder.AddOp(new CallBuiltIn(call.BuiltInFunctionHint), call.StartToken);

            else builder.AddOp(new Call(call.UserFunctionHint), call.StartToken);
        }

        public static void GenerateCode(FunctionHeader header, GeneratorContext builder)
        {
            foreach (string name in header.ParameterNames.Reverse())
                builder.AddOp(new VariableCreate(name));

            foreach (Expression statement in header.ParentHint.Statements)
                builder.AddCode(statement);

            // This is hacky, and might be cleaned up someday
            List<Op> sheet = builder.Operations;
            bool returnMissing = sheet.Count == 0 || !(sheet[sheet.Count - 1] is Types.Return);
            if (returnMissing) builder.AddOp(new Types.Return());
        }
    }
}
