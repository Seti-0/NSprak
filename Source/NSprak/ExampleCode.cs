using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSprak
{
    public class ExampleCode
    {
        public const string HelloWorld =

            "Print(\"Hello World!\")\n";

        public const string Fibonnacci =
        
            "\n" +
            "void Fib(number n)\n" +
            "   \n" +
            "   # Calculate the numbers\n" +
            "   \n" +
            "   if n == 1\n" +
            "       return 1\n" +
            "   end\n" +
            "   \n" +
            "   if n == 2\n" +
            "       return 1\n" +
            "   end\n" +
            "   \n" +
            "   number a = 1\n" +
            "   number b = 1\n" +
            "   number c = 2\n" +
            "   \n" +
            "   loop i from 1 to (n-2)\n" +
            "       c = a + b\n" +
            "       a = b\n" +
            "       b = c\n" +
            "   end\n" +
            "\n" +
            "   return c\n" +
            "end\n" +
            "\n" +
            "# Test it thoroughly\n" +
            "loop n in [1,2,3,4,5,6,7,8,9]\n" +
            "   Print(n+': '+Fib(n))\n" +
            "end\n" +
            "\n";

        public const string Stats =

            "# Taking a look at a very simple stats on arrays of numbers\n" +
            "# We'll cover the Mean (a type of average), the Standard Deviation (a\n" +
            "# measure of spread), and a count for uniqueness.\n" +
            "\n" +
            "number Mean(array X)\n" +
            "   number total = 0\n" +
            "   loop x in X\n" +
            "       total += x\n" +
            "   end\n" +
            "   return (total / Count(X))\n" +
            "end\n" +
            "\n" +
            "number StdDev(array X)\n" +
            "   number total = 0\n" +
            "   number mean = Mean(X)\n" +
            "   loop x in X\n" +
            "       total += Pow(x - mean, 2)\n" +
            "   end\n" +
            "   return Sqrt(total / Count(X))\n" +
            "end\n" +
            "\n" +
            "number CountUnique(array X)\n" +
            "   array set = []\n" +
            "   number total = 0\n" +
            "   \n" +
            "   loop x in X\n" +
            "       \n" +
            "       boolean found = false\n" +
            "       loop y in set\n" +
            "           if x == y\n" +
            "               found = true\n" +
            "           end\n" +
            "       end\n" +
            "       \n" +
            "       if found\n" +
            "           continue\n" +
            "       end\n" +
            "       \n" +
            "       Append(set, x)\n" +
            "       total++\n" +
            "   end\n" +
            "   return total\n" +
            "end\n" +
            "\n" +
            "# To test these, then, a function\n" +
            "void Test(number n)\n" +
            "   \n" +
            "   array X = []\n" +
            "   loop i from 0 to n\n" +
            "       Append(X, Round(Random()*10))\n" +
            "   end\n" +
            "   \n" +
            "   Print(X)\n" +
            "   Print('Mean: '+Mean(X))\n" +
            "   Print('Standard Deviation: '+StdDev(X)\n" +
            "   Print('Count Unique: '+CountUnique(X))\n" +
            "end\n" +
            "\n" +
            "number count = 0\n" +
            "loop\n" +
            "   if count > 5\n" +
            "       break\n" +
            "   end\n" +
            "\n" +
            "   count++\n" +
            "   Test((count+1)*3)\n" +
            "end\n" +
            "\n";
    }
}
