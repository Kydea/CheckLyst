using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Signature : Item, ICheckListSignature
	{
		public override string TypeName => "Signature";
		public bool AcceptsPin { get; set; }
		public string ButtonLabel { get; set; }
		public bool Cancelable { get; set; }
		public List<SignatureRecord> History { get; } = new List<SignatureRecord>();
		public int? SignatureLevel { get; set; }
		public bool Signed { get; set; }
		public string SignedBy { get; set; }
		public DateTimeOffset SignedOn { get; set; }
		public string SignerDescription { get; set; }

        public override bool IsValidByDefault()
        {
            return Signed;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
		{
			var signature = new Signature
			{
				Group = parent,
				ButtonLabel = ButtonLabel,
				Cancelable = Cancelable,
				AcceptsPin = AcceptsPin
			};

			Copy(this, signature);
			return signature;
		}
	}
}
