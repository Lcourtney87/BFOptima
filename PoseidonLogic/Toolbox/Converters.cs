using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic
{
    public static class Converters
    {
        public static int ConvertToInt(this string str)
        {
            decimal dec;
            int value;

            if (decimal.TryParse(str, out dec))
            {
                return (int)dec;
            }

            return 0;
        }
    }
}
