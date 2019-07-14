using System;
using MultipartDataMediaFormatter.Infrastructure;

namespace CheckListManager.Prototypes.Logic.Messages
{
	public class UploadAttachmentRequest
	{
		public long Id { get; set; }
		public int Version { get; set; }

		public long? Department { get; set; }
		public long? PrototypeType { get; set; }
		
		public Guid? AttachmentTemporaryKey { get; set; }
		public string Code { get; set; }
		public HttpFile Attachment { get; set; }
	}
}
