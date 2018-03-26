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
            if (Item.SignatureCode == Code && !Item.Removed && !Item.Valid) throw new InstanceException($"The item {(string.IsNullOrEmpty(Item.Description) ? Item.Id.ToString() : $"{Item.Description} (id: {Item.Id})")} is not valid");
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

        public void TrySign(string DecimalSeparator, string UserCode)
        {
            var Instance = (Instance)CheckList;
            try
            {
                Signed = true;
                SignedBy = UserCode;
                SignedOn = DateTimeOffset.UtcNow;
                Instance.Update(DecimalSeparator);
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
                Instance.Update(DecimalSeparator);
            }
        }

        public void Sign(string UserCode, string UserDescription, DateTimeOffset Now)
        {
            Signed = true;
            SignedBy = UserCode;
            SignerDescription = UserDescription;
            SignedOn = Now;

            History.Add(new SignatureRecord { DateTime = SignedOn, User = UserCode, Signature = SignerDescription });
        }

        public void Unsign(string UserCode, string UserDescription, DateTimeOffset Now, string Note, string InstanceState)
        {
            try
            {
                Signed = false;
                var Instance = ((Instance)CheckList);
                Instance.UpdateReadOnly(UserCode, InstanceState);
                if (ReadOnly)
                    throw new Exception("This signature not editable"); //Signature is still readonly after unsigned, may be locked by another signature or a special condition

                SignedOn = Now;

                History.Add(new SignatureRecord { DateTime = SignedOn, User = UserCode, Signature = SignerDescription, Note = Note });
            }
            catch
            {
                Signed = true;
                throw;
            }
        }

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            base.UpdateReadOnly(userCode, readOnlyInstance);
            ReadOnly |= Signed && !Cancelable;
        }
    }
}
