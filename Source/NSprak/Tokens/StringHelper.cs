using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Tokens
{
    public static class StringHelper
    {
        public static void Split(string source, char delimiter, out IList<string> elements, out IList<int> indices)
        {
            // Does c# already have a split function which also returns the indices?

            int index = 0;

            elements = new List<string>();
            indices = new List<int>();

            while (true)
            {
                if (index >= source.Length)
                    break;

                indices.Add(index);
                int nextIndex = source.IndexOf(delimiter, index);

                if (nextIndex == -1)
                {
                    elements.Add(source.Substring(index));
                    break;
                }

                // The +1 here means that the /n character is placed at the end of the line before it,
                // as opposed to the beginning of the next line
                elements.Add(source[index..(1 + nextIndex)]);
                index = nextIndex + 1;
            }
        }
    }
}
