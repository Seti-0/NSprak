using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSprak.Tokens;

namespace NSprak.Expressions.Types
{
    public class MainHeader : Header
    {
        public override string FriendlyBlockName => "main function";

        public override Token StartToken => null;

        public override Token EndToken => null;

        public MainHeader() {}

        public override IEnumerable<Expression> GetSubExpressions()
        {
            return Enumerable.Empty<Expression>();
        }

        public override string ToString()
        {
            return "Main";
        }

        public override string GetTraceString()
        {
            return "Main Header";
        }
    }
}
