using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckListSignature : ICheckListItem
    {
        bool AcceptsPin { get; set; }
        string ButtonLabel { get; set; }
        bool Cancelable { get; set; }
        bool SendEmail { get; set; }
        string SendEmailAddress { get; set; }
        List<SignatureRecord> History { get; }
		string SignatureLevel { get; set; }
		bool Signed { get; set; }
		string SignedBy { get; set; }
		DateTimeOffset SignedOn { get; set; }
		string SignerDescription { get; set; }
    }
}