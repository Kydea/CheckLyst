using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using System.Linq;
using CheckListManager.Instances.Logic.Exceptions;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Signature : Item, ICheckListSignature
	{
		public override string TypeName => "Signature";
		public bool AcceptsPin { get; set; }
		public string ButtonLabel { get; set; }
		public bool Cancelable { get; set; }
		public bool SendEmail { get; set; }
		public string SendEmailAddress { get; set; }
        public List<SignatureRecord> History { get; } = new List<SignatureRecord>();
		public string SignatureLevel { get; set; }
		public bool Signed { get; set; }
		public string SignedBy { get; set; }
		public DateTimeOffset SignedOn { get; set; }
        public string ModifiedBy => SignedBy;
        public DateTimeOffset? ModifiedOn { get; private set; } //being nullable can't just inherit SignedOn
        public string SignerDescription { get; set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            if (Cancelable)
                ItemModel.Cancelable = Cancelable;
            if (AcceptsPin)
                ItemModel.AcceptsPin = AcceptsPin;
            if (SendEmail)
            {
                ItemModel.SendEmail = SendEmail;
                ItemModel.SendEmailAddress = SendEmailAddress;
            }
            if (!string.IsNullOrEmpty(ButtonLabel))
                ItemModel.ButtonLabel = ButtonLabel;
            ItemModel.Signed = Signed;
            if (Signed)
            {
                ItemModel.SignedBy = SignedBy;
                ItemModel.SignedOn = SignedOn;
                ItemModel.SignerDescription = SignerDescription;
            }

            if (History.Any())
                ItemModel.History = History.Select(x => new {
                    x.DateTime,
                    x.User,
                    x.Signature,
                    x.Note,
                    x.Action
                });
            return ItemModel;
        }

        public override bool IsValidByDefault() => Signed;

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Signature);
        }

        protected override void OnUpdate(DependencyRelation.Properties Property)
        {
            if (Property == DependencyRelation.Properties.Signature)
                return;
            else
                base.OnUpdate(Property);
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
            if (Item.SignatureCode == Code && !Item.Removed && Item is ICheckListValidityElement checkListValidityElement && !checkListValidityElement.Valid) throw new InstanceException($"The item {(string.IsNullOrEmpty(Item.Description) ? Item.Id.ToString() : $"{Item.Description} (id: {Item.Id})")} is not valid");
        }

        private void FindInvalidItem(ICheckListGroup Group)
        {
            if (Group.Iterator)
                foreach (var Iteration in Group.Iterations.Where(i => !i.Removed))
                    FindInvalidItem(Iteration);
            else
                foreach (var Item in Group.Items)
                    if (Item is ICheckListGroup)
                        FindInvalidItem((ICheckListGroup)Item);
                    else
                        CheckItem(Item);
        }

        public void TrySign(string UserCode)
        {
            try
            {
                Signed = true;
                SignedBy = UserCode;
                ModifiedOn = SignedOn = DateTimeOffset.UtcNow;

                UpdateChain(Instance, DependencyRelation.Properties.Signature);

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
            }
        }

        public void Sign(string UserCode, string UserDescription, DateTimeOffset Now)
        {
            Signed = true;

            SignedBy = UserCode;
            SignerDescription = UserDescription;
            ModifiedOn = SignedOn = Now;

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

                SignedBy = UserCode;
                SignerDescription = UserDescription;
                ModifiedOn = SignedOn = Now;

                History.Add(new SignatureRecord { DateTime = SignedOn, User = UserCode, Signature = UserDescription , Note = Note });
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
