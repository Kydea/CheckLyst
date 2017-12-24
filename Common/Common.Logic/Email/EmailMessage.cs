namespace CheckListManager.Common.Logic.Email
{
	public class EmailMessage
	{
		public EmailMessage()
		{
			
		}

		public EmailMessage(long company, string subject, string content, string to)
		{
			Company = company;
			Subject = subject;
			Content = content;

			if (!string.IsNullOrEmpty(to)) To = new[] { to };
		}

		public long Company { get; set; }

		public string Subject { get; set; }
		public string Content { get; set; }
		public string[] To { get; set; }
		public string[] Cc { get; set; }

		public string Name { get; set; }
		public string Link { get; set; }
	}
}
