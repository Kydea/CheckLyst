using MultipartDataMediaFormatter.Infrastructure;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class UploadAttachmentRequest : AttachmentRequestBase
	{
		public string Code { get; set; }
		public HttpFile Attachment { get; set; }
	}
}
