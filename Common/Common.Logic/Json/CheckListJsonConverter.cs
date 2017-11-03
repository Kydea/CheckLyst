using System;
using Autofac;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
	public class CheckListJsonConverter : AutofacJsonConverter<ICheckListItem>
	{
		public CheckListJsonConverter(ILifetimeScope scope) : base(scope)
		{
		}

		protected override ICheckListItem Create(Type objectType, JObject jObject)
		{
			var type = jObject["typeName"]?.ToString();
			switch (type)
			{
				case "Field":
					return Scope.Resolve<ICheckListField>();
				case "Group":
					return Scope.Resolve<ICheckListGroup>();
				case "Iteration":
					return Scope.Resolve<ICheckListIteration>();
				case "Signature":
					return Scope.Resolve<ICheckListSignature>();
				case "Text":
					return Scope.Resolve<ICheckListText>();
				default:
					throw new Exception($"Check List Item not supported: {type}");
			}
		}
	}
}
