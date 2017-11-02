using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public class Signature : Item
	{
		// ReSharper disable once CollectionNeverQueried.Local
		private readonly List<Element> _elements = new List<Element>();

		public override string TypeName => "Signature";

		public override bool NewLine => true;

		public string ButtonLabel { get; set; }

		public bool Cancelable { get; set; }

		public bool AcceptsPin { get; set; }

		public bool Signed { get; set; }

		public string SignedBy { get; set; }

		public string SignerDescription { get; set; }

		public DateTime SignedOn { get; set; }

		public int? SignatureLevel { get; set; }

		public override bool IsValidByDefault()
		{
			return Signed;
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		//public void Sign(EntitiesContainer db, string userName, string passord, string pin, bool simple)
		//{
		//	TrySign(userName);

		//	if (simple)
		//		Security.VerifyPassword(db, userName, passord);
		//	else
		//		Security.VerifyPIN(db, userName, pin);

		//	var prototype = Prototypes.Get(db, ((Instance)CheckList).Key.Prototype);
		//	if (!UserFunctions.Contains(db, userName, prototype.Company, prototype.Department, prototype.PrototypeType, UserFunctions.INSTANCEOPERATOR, UserFunctions.INSTANCEAPPROVER, UserFunctions.INSTANCECREATOR))
		//		throw new Exception("The user doesn't have the grant to apply this signature");

		//	if (SignatureLevel.HasValue)
		//	{
		//		var userSignatureLevel = UserFunctions.GetSignatureLevel(db, userName, prototype.Company, prototype.Department, prototype.PrototypeType);

		//		if (SignatureLevel.Value != userSignatureLevel)
		//			throw new Exception($"The signature must be applied by an user with level {SignatureLevel.Value}");
		//	}

		//	Signed = true;
		//	SignedBy = userName;
		//	SignerDescription = Users.Get(db, userName).Description;
		//	SignedOn = DateTime.Now;

		//	History.Add(new Record { DateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), User = userName, Signature = SignerDescription });
		//}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		//public void Unsign(Client client, EntitiesContainer db, string userName, string passord, string note)
		//{
		//	if (string.IsNullOrWhiteSpace(note)) throw new Exception("Specify the note for the signature cancelation.");
		//	if (!Signed) throw new Exception("The item " + Id + " is not signed");

		//	try
		//	{
		//		Signed = false;
		//		((Instance)CheckList).Update();
		//		if (IsReadOnly(client.Id))
		//			throw new Exception("This signature is read-only.");

		//		Security.VerifyPassword(db, userName, passord);

		//		var instance = ((Instance)CheckList);
		//		var prototype = Prototypes.Get(db, instance.Key.Prototype);
		//		if (!UserFunctions.Contains(db, userName, prototype.Company, prototype.Department, prototype.PrototypeType, UserFunctions.SIGNATUREMANAGER))
		//			throw new Exception("User doesn't have grants to cancel the signature.");

		//		History.Add(new Record { DateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"), Note = note, User = userName, Signature = Users.Get(db, userName).Description });
		//	}
		//	catch
		//	{
		//		Signed = true;
		//		((Instance)CheckList).Update();
		//		throw;
		//	}
		//}

		private void CheckItem(Item item)
		{
			if (item.SignatureCode == Code && !item.Valid) throw new ElementException("Invalid Item", item);
		}

		private void FindInvalidItem(Group group)
		{
			if (group.Iterator)
				foreach (var iteration in group.Iterations)
					FindInvalidItem(iteration);
			else
				foreach (var item in group.Items)
					if (item is Group subgroup)
						FindInvalidItem(subgroup);
					else
						CheckItem(item);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void TrySign(string userName)
		{
			try
			{
				if (Signed) throw new Exception("The item " + Id + " is already signed");
				Signed = true;
				SignedBy = userName;
				SignedOn = DateTime.Now;
				((Instance)CheckList).Update();
				if (!Valid) throw new Exception("Signature not valid. " + ValidityMessage);
				if (Code != null)
				{
					if (IterationItem)
						FindInvalidItem(Iteration);
					else
						((Instance)CheckList).Items.Values.ToList().ForEach(CheckItem);
				}
			}
			finally
			{
				Signed = false;
				SignedBy = null;
				((Instance)CheckList).Update();
			}
		}

		public void Add(Element element)
		{
			_elements.Add(element);
		}

		public override Item Clone(Group parent)
		{
			var signature = new Signature()
			{
				Group = parent,
				ButtonLabel = ButtonLabel,
				Cancelable = Cancelable,
				AcceptsPin = AcceptsPin
			};
			Copy(this, signature);
			return signature;
		}

		public override dynamic GetModel(uint clientId)
		{
			var result = base.GetModel(clientId);
			result.ButtonLabel = ButtonLabel;
			result.Signed = Signed;
			if (Signed)
			{
				result.SignedBy = SignedBy;
				result.SignerDescription = SignerDescription;
				result.SignedOn = SignedOn;
			}
			result.ReadOnly |= Signed;
			result.Cancelable = Cancelable;
			result.AcceptsPIN = AcceptsPin;
			result.History = History;
			result.SignatureLevel = SignatureLevel;
			return result;
		}

		public List<Record> History { get; } = new List<Record>();

		public struct Record
		{
			public string DateTime { get; set; }

			public string User { get; set; }

			public string Signature { get; set; }

			public string Note { get; set; }

			public string Action => "<center><i class=\"fa fa-fw fa-" + (Note == null ? "check" : "remove") + "\"></i></center>";
		}
	}
}
