using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Prototype : ICheckList
    {
        public Dictionary<string, ICheckListElement> Elements { get; } = new Dictionary<string, ICheckListElement>();
        public Dictionary<string, Expression> Expressions { get; set; } = new Dictionary<string, Expression>();
        public List<String> ExpressionsInUse { get; private set; }
        public Dictionary<string, string> ExternalSources { get; set; } = new Dictionary<string, string>();
        public ICheckListGroup Header { get; set; }
        public List<ICheckListPage> Pages { get; } = new List<ICheckListPage>();
        public string DescriptionFormulaString { get; set; }
        public string FileNameFormulaString { get; set; }
        public bool FTPExport { get; set; }

        public void AddItem(ICheckListItem item)
        {
            throw new NotImplementedException();
        }

        public ICheckList CheckList { get { return this; } set { throw new NotImplementedException("Just for inheritance purpose"); } }

        public void Setup()
        {
            ExpressionsInUse = new List<string>(Expressions.Count);

            Header.CheckList = this;
            Header.Setup();

            foreach (var Page in Pages)
            {
                Page.CheckList = this;
                Page.Setup();
            }

            foreach (var Expression in Expressions.Values)
            {
                try
                {
                    Expression.Parse(this);
                    Expression.Check(this);
                }
                catch (ParseException Exception)
                {
                    Expression.Error = $"{Exception.Message} (position {Exception.Position})";
                }
                catch (Exception Exception)
                {
                    Expression.Error = Exception.Message;
                }
            }
        }
    }
}
