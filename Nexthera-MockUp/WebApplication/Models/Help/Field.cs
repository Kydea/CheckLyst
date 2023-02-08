using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Field
    {
        public const string ID = "The unique identifier.";
        public const string READ_ONLY = "Not editable";
        public const string MANDATORY = "Mandatory";
        public const string FINAL = "Not editable for existing records, mandatory for new records";
        public const string DETAILS_PAGE = "Details page";
        public const string POPUP_MENU = "Result grid popup menu";

        public Field(string Name, string Description, params string[] Notes)
        {
            this.Name = Name;
            this.Description = Description;
            this.Notes = Notes;
        }

        public string Name { get; }
        public string Description { get; }
        public string[] Notes { get; }
    }
}