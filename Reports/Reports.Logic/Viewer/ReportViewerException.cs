using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportViewerException : Exception
    {
        public ReportViewerException(string message) : base(message)
        {
        }
    }
}
