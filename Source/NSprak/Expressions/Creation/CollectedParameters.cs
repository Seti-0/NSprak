using System.Collections.Generic;
using NSprak.Language;

namespace NSprak.Expressions.Creation
{
    public class CollectedParameters
    {
        public List<SprakType> Types;
        public List<string> Names;

        public CollectedParameters()
        {
            Types = new List<SprakType>();
            Names = new List<string>();
        }

        public CollectedParameters(List<SprakType> types, List<string> names)
        {
            Types = types;
            Names = names;
        }

        public override string ToString()
        {
            List<string> items = new List<string>();

            for (int i = 0; i < Types.Count; i++)
            {
                string item = Types[i].Text;
                item += " " + Names[i];
                items.Add(item);
            }

            string result = string.Join(", ", items);
            return result;
        }
    }
}
