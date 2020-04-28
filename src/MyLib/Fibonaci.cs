using System;
using System.Collections.Generic;

namespace MyLib
{
    public class Fibonaci
    {
        private int SeriesLength { get; set; }
        private Type OutputFormat { get; set; }

        public void SetFibonaciLength(int length)
        {
            SeriesLength = length;
        }

        public void SetOutputFormat(Type type)
        {
            OutputFormat = type;
        }

        public dynamic Generate()
        {
            int n1 = 0, n2 = 1, n3;
            var output = new List<int> { n1, n2 };
            for (int i = 2; i < SeriesLength; ++i)   
            {
                n3 = n1 + n2;
                n1 = n2;
                n2 = n3;
                output.Add(n3);
            }

            if (OutputFormat == typeof(string))
            {
                return string.Join(",", output);
            }
            return output;
        }
    }
}
