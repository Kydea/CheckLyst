using System.Web;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class UploadRequest
	{
		public long Id { get; set; }
		public int Version { get; set; }
		public string Code { get; set; }
		public HttpPostedFile Attachment { get; set; }
	}
}
