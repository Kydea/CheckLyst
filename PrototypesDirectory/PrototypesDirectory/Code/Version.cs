using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matematici
{
    public sealed class Version
    {
        public static readonly int Major = 1;
        public static readonly int Minor = 4;
        public static readonly int Release = 0;
        public static readonly string Stage = ""; //empty, .beta, .alpha

        public static string Get()
        {
            return $"{Major}.{Minor}.{Release}{Stage}";
        }
    }
}
