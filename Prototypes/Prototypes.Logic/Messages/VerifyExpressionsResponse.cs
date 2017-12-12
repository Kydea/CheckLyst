using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class VerifyExpressionsResponse
    {
        public bool Valid { get; set; }

        public Dictionary<string, Expression> Expressions { get; set; }
    }
}
