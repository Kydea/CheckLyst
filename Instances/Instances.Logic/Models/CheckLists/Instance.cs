using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System.Linq;
using CheckListManager.Common.Data;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Instance : ICheckList
    {
        public bool ReadOnly { get; set; }
        public Dictionary<string, ICheckListElement> Elements { get; set; } = new Dictionary<string, ICheckListElement>();
        public Dictionary<string, Expression> Expressions { get; set; }
        public ICheckListGroup Header { get; set; }
        public List<ICheckListPage> Pages { get; set; }

        [JsonIgnore]
        public Formula DescriptionFormula { get; set; }
        public string DescriptionFormulaString
        {
            get => DescriptionFormula?.RawValue;
            set => DescriptionFormula = value == null ? null : new Formula(value);
        }
        public string Description
        {
            get
            {
                return DescriptionFormula?.Value;
            }
        }

        [JsonIgnore]
        public Formula FileNameFormula { get; set; }
        public string FileNameFormulaString
        {
            get => FileNameFormula?.RawValue;
            set => FileNameFormula = value == null ? null : new Formula(value);
        }
        public string FileName
        {
            get
            {
                return DescriptionFormula?.Value;
            }
        }

        public bool FTPExport { get; set; }

        [JsonIgnore]
        public bool InCreation { get; private set; }

        public ICheckList CheckList
        {
            get => this;
            set => throw new NotImplementedException("Just for inheritance purpose");
        }


        public void AddItem(ICheckListItem item)
        {
            if (item.Id == 0 || InCreation)
                item.Id = ++_sequence;
            else
                _sequence = Math.Max(_sequence, item.Id);

            Items.Add(item.Id, item);
        }

        private int _sequence;

        [JsonIgnore]
        public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>(10);

        public void Setup(bool Creation)
        {
            InCreation = Creation;

            Header.CheckList = this;
            Header.Setup();

            foreach (var page in Pages)
            {
                page.CheckList = this;
                page.Setup();
            }

            foreach (var expression in Expressions.Values)
                expression.Parse(this);

            InCreation = false;
        }

        public void Update()
        {
            foreach (var expression in Expressions.Values)
            {
                expression.Parse(this);
                expression.Evaluate(this);
            }

            DescriptionFormula?.Update(Expressions, null);
            FileNameFormula?.Update(Expressions, null);

            Header.Update();
            foreach (var page in Pages)
                page.Update();
        }

        public void UpdateReadOnly(string userCode, string state)
        {
            Header.UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Draft);
            foreach (var page in Pages)
                page.UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Progress);
        }

    }
}
