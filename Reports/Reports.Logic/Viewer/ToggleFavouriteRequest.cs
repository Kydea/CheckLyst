using CheckListManager.Common.Logic.DataEngine;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ToggleFavouriteRequest : Request
    {
        [Required]
        public string ReportCode { get; set; }
        [Required]
        public bool? Value { get; set; }
    }
}
