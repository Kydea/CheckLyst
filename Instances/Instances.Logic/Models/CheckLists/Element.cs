using System.Collections.Generic;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public abstract class Element : ICheckListElement
	{
		public ICheckList CheckList { get; set; }
		public string Code { get; set; }

		public Expression CountDownTo => CountDownToCode == null ? null : CheckList.Expressions[CountDownToCode];

		public string CountDownToCode { get; set; }
		public bool Hidden
		{
			get
			{
				var expressionValue = HiddenExpressionCode != null && CheckList != null && CheckList.Expressions.ContainsKey(HiddenExpressionCode) && CheckList.Expressions[HiddenExpressionCode].Value.Type == DataType.Boolean && CheckList.Expressions[HiddenExpressionCode].Value.BooleanValue;
				return
					HiddenCondition == Conditions.Expression && expressionValue ||
					HiddenCondition == Conditions.True;
			}
		}
		public Conditions HiddenCondition { get; set; }
		public string HiddenExpressionCode { get; set; }
		public Conditions ReadOnlyCondition { get; set; }
		public string ReadOnlyExpressionCode { get; set; }

		public ICheckListSignature Signature
			=> SignatureCode == null ? null : (ICheckListSignature)GetBranchElements()[SignatureCode];
		public string SignatureCode { get; set; }

		public bool Valid
		{
			get
			{
				if (Hidden) return true;

				var expressionValue = ValidExpressionCode != null && CheckList != null && CheckList.Expressions.ContainsKey(ValidExpressionCode) && CheckList.Expressions[ValidExpressionCode].Value.Type == DataType.Boolean && CheckList.Expressions[ValidExpressionCode].Value.BooleanValue;
				return
					ValidCondition == Conditions.Default && IsValidByDefault() ||
					ValidCondition == Conditions.Expression && expressionValue ||
					ValidCondition == Conditions.ExpressionOrDefault && (expressionValue || IsValidByDefault()) ||
					ValidCondition == Conditions.ExpressionAndDefault && expressionValue && IsValidByDefault() ||
					ValidCondition == Conditions.True;
			}
		}

		public Conditions ValidCondition { get; set; }
		public string ValidExpressionCode { get; set; }
		public string ValidityMessage { get; set; }

		public virtual bool IsValidByDefault()
		{
			return true;
		}

		protected virtual Dictionary<string, ICheckListElement> GetBranchElements()
		{
			return CheckList.Elements;
		}

		public virtual void Setup()
		{
			if (Code != null)
				CheckList.Elements[Code] = this;
		}

		public abstract void Update();

		public bool IsReadOnly { get; protected set; }

		protected virtual bool IsReadOnlyByDefault() => false;

		public virtual void UpdateReadOnly(string userCode, bool canEdit)
		{
			if (!canEdit)
			{
				IsReadOnly = true;
				return;
			}

			var expressionValue = ReadOnlyExpressionCode != null && CheckList != null &&
			                      CheckList.Expressions.ContainsKey(ReadOnlyExpressionCode) &&
			                      CheckList.Expressions[ReadOnlyExpressionCode].Value.Type == DataType.Boolean &&
			                      CheckList.Expressions[ReadOnlyExpressionCode].Value.BooleanValue;

			IsReadOnly = (Signature?.Signed ?? false) ||
				ReadOnlyCondition == Conditions.Default && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.Expression && expressionValue ||
				ReadOnlyCondition == Conditions.ExpressionOrDefault && (expressionValue || IsReadOnlyByDefault()) ||
				ReadOnlyCondition == Conditions.ExpressionAndDefault && expressionValue && IsReadOnlyByDefault() ||
				ReadOnlyCondition == Conditions.True;
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
