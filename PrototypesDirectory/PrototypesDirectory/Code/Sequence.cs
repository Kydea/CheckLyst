using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matematici
{
    public static class Sequence
    {
        private static uint Value = 0;

        public static uint Get()
        {
            return ++Value;
        }
    }
}
