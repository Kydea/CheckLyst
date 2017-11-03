using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListSignature : ICheckListItem
	{
		bool AcceptsPin { get; set; }
		string ButtonLabel { get; set; }
		bool Cancelable { get; set; }
		List<SignatureRecord> History { get; }
		int? SignatureLevel { get; set; }
		bool Signed { get; set; }
		string SignedBy { get; set; }
		DateTime SignedOn { get; set; }
		string SignerDescription { get; set; }
	}
}