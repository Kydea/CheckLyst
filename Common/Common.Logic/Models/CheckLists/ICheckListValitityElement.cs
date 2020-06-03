using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListValidityElement : ICheckListElement
    {
        bool Valid { get; set; }
        Conditions ValidCondition { get; set; }
        string ValidExpressionCode { get; set; }
        string ValidityMessage { get; set; }
    }
}