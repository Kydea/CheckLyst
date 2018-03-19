using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public abstract class Element : ICheckListElement
	{
		public ICheckList CheckList { get; set; }
		public string Code { get; set; }
		public string CountDownToCode { get; set; }
		public Conditions HiddenCondition { get; set; }
		public string HiddenExpressionCode { get; set; }
		public Conditions ReadOnlyCondition { get; set; }
		public string ReadOnlyExpressionCode { get; set; }
		public string SignatureCode { get; set; }
		public bool ReadOnly { get; set; }
		public Conditions ValidCondition { get; set; }
		public string ValidExpressionCode { get; set; }
		public string ValidityMessage { get; set; }

        [JsonIgnore]
        public bool Hidden => false;
        [JsonIgnore]
        public Expression CountDownTo => null;
        [JsonIgnore]
        public ICheckListSignature Signature => null;
        [JsonIgnore]
        public bool Valid => true;

        public virtual void Setup()
		{
            if (!string.IsNullOrEmpty(Code))
            {
                if (CheckList.Elements.ContainsKey(Code))
                    throw new Exception($"Duplicated code {Code}");
                CheckList.Elements[Code] = this;
            }
        }

        public void Update()
		{
			throw new NotImplementedException();
		}

		public void UpdateReadOnly(string userCode, bool canEdit)
		{
			throw new NotImplementedException();
		}

		public virtual bool IsValidByDefault()
		{
			return true;
		}

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
		{
			return CheckList.Elements;
		}

        public string GetSignatureInformation()
        {
            throw new NotImplementedException();
        }

        public virtual void Check()
        {
            var BranchElements = GetBranchElements();
            if (!string.IsNullOrEmpty(SignatureCode) && (!BranchElements.ContainsKey(SignatureCode) || !(BranchElements[SignatureCode] is ICheckListSignature)))
                SignatureCode = null;
        }

        public static void Copy(Element source, Element destination)
        {
            destination.CheckList = source.CheckList;
            destination.Code = source.Code;
            destination.ReadOnlyExpressionCode = source.ReadOnlyExpressionCode;
            destination.ReadOnlyCondition = source.ReadOnlyCondition;
            destination.HiddenExpressionCode = source.HiddenExpressionCode;
            destination.HiddenCondition = source.HiddenCondition;
            destination.ValidExpressionCode = source.ValidExpressionCode;
            destination.ValidCondition = source.ValidCondition;
            destination.ValidityMessage = source.ValidityMessage;
            destination.SignatureCode = source.SignatureCode;
        }
    }
}
