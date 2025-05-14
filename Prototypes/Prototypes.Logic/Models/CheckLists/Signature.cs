using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Signature : Item, ICheckListSignature
	{
		public override string TypeName => "Signature";
		public bool AcceptsPin { get; set; }
		public string ButtonLabel { get; set; }
		public bool Cancelable { get; set; }
        public bool SendEmail { get; set; }
        public string SendEmailAddress { get; set; }
        public string SignatureLevel { get; set; }
        [JsonIgnore]
        public List<SignatureRecord> History { get; } = new List<SignatureRecord>();
        [JsonIgnore]
        public bool Signed { get; set; }
        [JsonIgnore]
        public string SignedBy { get; set; }
        [JsonIgnore]
        public DateTimeOffset SignedOn { get; set; }
        [JsonIgnore]
        public string SignerDescription { get; set; }
        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }
        public string CommandExternalSourceName { get; set; }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();
            if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Signature);

            if (!string.IsNullOrEmpty(SignatureCode) && SignatureCode == Code && !CheckList.InstanceCreation)
            {
                Errors.Add(new ConfigurationError("signature", $"A signature cannot block itself"));
                Result = false;
            }

            if (CommandExternalSourceName != null)
            {
                var CommandResult = true;
                if (!CheckList.ExternalSources.TryGetValue(CommandExternalSourceName, out var ExternalSourceNode))
                {
                    Errors.Add(new ConfigurationError("command-source", $"External source {CommandExternalSourceName} not found"));
                    CommandResult = false;
                }
                Result &= CommandResult;
            }


            return Result;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var signature = new Signature
            {
                Group = parent,
                ButtonLabel = ButtonLabel,
                Cancelable = Cancelable,
                AcceptsPin = AcceptsPin,
                CommandExternalSourceName = CommandExternalSourceName
            };

            Copy(this, signature);
            return signature;
        }
    }
}
