using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Email
{
	public class EmailMessage
	{
		public EmailMessage()
		{
			
		}

		public EmailMessage(long company, string subject, string content, string to, List<Tuple<string, string>> parameters = null)
		{
			Company = company;
            Subject = subject;
            Content = content;

            if (parameters != null)
                foreach (var parameter in parameters)
                    Content = Content.Replace($"[{parameter.Item1}]", parameter.Item2);

            if (!string.IsNullOrEmpty(to)) To = new[] { to };
		}

		public long Company { get; set; }

		public string Subject { get; set; }
		public string Content { get; set; }
		public string[] To { get; set; } = new string [0];
		public string[] Cc { get; set; } = new string [0];

		public EmailTrailMessage TrailMessage { get; set; }
	}
}
