using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Matematici.Security
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException()
        {
        }
    }
}