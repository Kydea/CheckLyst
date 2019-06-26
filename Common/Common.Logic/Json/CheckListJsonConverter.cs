using System;
using Autofac;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json.Linq;

namespace CheckListManager.Common.Logic.Json
{
	public class CheckListJsonConverter : AutofacJsonConverter<ICheckListElement>
	{
		public CheckListJsonConverter(ILifetimeScope scope) : base(scope)
		{
		}

		protected override ICheckListElement Create(Type objectType, JObject jsonObject)
		{
			var type = (jsonObject["typeName"] ?? jsonObject["TypeName"])?.ToString();
			// ReSharper disable once SwitchStatementMissingSomeCases
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
                case "Link":
                    return Scope.Resolve<ICheckListLink>();
                case "Image":
                    return Scope.Resolve<ICheckListImage>();
                case "Attachment":
                    return Scope.Resolve<ICheckListAttachment>();
            }

            if (Scope.TryResolve(objectType, out var instance))
				return (ICheckListElement)instance;

			throw new Exception($"Check List Item not supported: {type}");
		}
	}
}
