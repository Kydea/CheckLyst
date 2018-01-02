using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System.Linq;
using CheckListManager.Instances.Logic.Exceptions;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Signature : Item, ICheckListSignature
	{
		public override string TypeName => "Signature";
		public bool AcceptsPin { get; set; }
		public string ButtonLabel { get; set; }
		public bool Cancelable { get; set; }
		public List<SignatureRecord> History { get; } = new List<SignatureRecord>();
		public string SignatureLevel { get; set; }
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

        private void CheckItem(ICheckListItem Item)
        {
            if (Item.SignatureCode == Code && !Item.Valid) throw new InstanceException($"The item {(string.IsNullOrEmpty(Item.Description) ? Item.Id.ToString() : $"{Item.Description} (id: {Item.Id})")} is not valid");
        }

        private void FindInvalidItem(ICheckListGroup Group)
        {
            if (Group.Iterator)
                foreach (var Iteration in Group.Iterations)
                    FindInvalidItem(Iteration);
            else
                foreach (var Item in Group.Items)
                    if (Item is ICheckListGroup)
                        FindInvalidItem((ICheckListGroup)Item);
                    else
                        CheckItem(Item);
        }

        public void TrySign(String UserName)
        {
            var Instance = (Instance)CheckList;
            if (Signed) throw new Exception("The signature " + Id + " is already signed");
            try
            {
                Signed = true;
                SignedBy = UserName;
                SignedOn = DateTimeOffset.UtcNow;
                Instance.Update();
                if (!Valid) throw new InstanceException("Signature not valid. " + ValidityMessage);
                if (Code != null)
                {
                    if (IterationItem)
                        FindInvalidItem(Iteration);
                    else
                        Instance.Items.Values.ToList().ForEach(x => CheckItem(x));
                }
            }
            finally
            {
                Signed = false;
                SignedBy = null;
                Instance.Update();
            }
        }

        public void Sign(string UserName, string UserDescription, DateTimeOffset Now)
        {
            Signed = true;
            SignedBy = UserName;
            SignerDescription = UserDescription;
            SignedOn = Now;

            History.Add(new SignatureRecord { DateTime = SignedOn.ToString("dd/MM/yyyy HH:mm"), User = UserName, Signature = SignerDescription });
        }

    }
}
