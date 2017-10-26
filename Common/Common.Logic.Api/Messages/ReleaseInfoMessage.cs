using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CheckListManager.Common.Logic.Api.Messages
{
	internal class ReleaseInfoMessage
	{
		public ReleaseInfoMessage()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			var fileInfo = new FileInfo(assembly.Location);

			Version = assembly.GetName().Version.ToString();
			FileVersion = versionInfo.FileVersion;

			BuildDate = fileInfo.CreationTimeUtc;
			ReleaseDate = fileInfo.LastWriteTimeUtc;
		}

		public string Version { get; set; }
		public string FileVersion { get; set; }
		public DateTimeOffset BuildDate { get; set; }
		public DateTimeOffset ReleaseDate { get; set; }
	}
}
