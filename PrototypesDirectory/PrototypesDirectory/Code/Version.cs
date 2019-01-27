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
        public static readonly int Minor = 3;
        public static readonly int Release = 0;

        public static string Get()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Release);
        }
    }
}
