using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Matematici.DataSource
{
    public class PrototypeFile
    {
        public PrototypeFile(string Folder, string FileName)
        {
            if (FileName == null)
                throw new ArgumentNullException("FileName");

            if (!FileName.EndsWith(".pdf"))
                throw new Exception($"Invalid file extension: {FileName}.");

            this.FileName = Path.Combine(Folder, FileName);

            FileName = FileName.Substring(0, FileName.Length - 4);

            var Tokens = FileName.Split('#');
            if (Tokens.Length < 6 || Tokens.Length > 7)
                throw new Exception($"Invalid file name: {FileName}, invalid number of tokens.");

            Company = Tokens[0];
            if (string.IsNullOrWhiteSpace(Company))
                throw new Exception($"Invalid file name: {FileName}, invalid company.");

            Department = Tokens[1];
            if (string.IsNullOrWhiteSpace(Department))
                throw new Exception($"Invalid file name: {FileName}, invalid department.");

            PrototypeType = Tokens[2];
            if (string.IsNullOrWhiteSpace(PrototypeType))
                throw new Exception($"Invalid file name: {FileName}, invalid prototype type.");

            Name = Tokens[3];
            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception($"Invalid file name: {FileName}, invalid code.");

            if (!int.TryParse(Tokens[4], out int Version))
                throw new Exception($"Invalid file name: {FileName}, invalid version.");
            this.Version = Version;

            Description = Tokens[5];

            if (Tokens.Length == 7)
            {
                if (Tokens[6] != "O")
                    throw new Exception($"Invalid file name: {FileName}, invalid suffix, only O (for obsolete prototypes) can be used.");
                Obsolete = true;
            }
        }

        public void GeneratePDF(string User, Stream Stream)
        {
            XFont Font = new XFont("Sans Serif", 12);
            PdfDocument Document = PdfReader.Open(FileName);
            var Watermark = $"Printed on {DateTime.Now.ToUniversalTime():yyyy-MM-dd HH:mm UTC} by {User}";

            for (int Index = 0; Index < Document.Pages.Count; Index++)
            {
                PdfPage Page = Document.Pages[Index];
                var Graphics = XGraphics.FromPdfPage(Page, XGraphicsPdfPageOptions.Prepend);
                var Size = Graphics.MeasureString(Watermark, Font);
                var Brush = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));
                Graphics.DrawString(Watermark, Font, XBrushes.Black, new XPoint(40, Page.Height - 35));
            }

            using (MemoryStream MemoryStream = new MemoryStream())
            {
                Document.Save(MemoryStream, false);
                byte[] Buffer = new byte[MemoryStream.Length];
                MemoryStream.Seek(0, SeekOrigin.Begin);
                MemoryStream.Flush();
                MemoryStream.Read(Buffer, 0, (int)MemoryStream.Length);
                Stream.Write(Buffer, 0, Buffer.Length);
            }

            Document.Close();
            Stream.Flush();
        }

        public string FileName { get; }
        public string Company { get; }
        public string Department { get; }
        public string PrototypeType { get; }
        public string Name { get; }
        public int Version { get; }
        public string Description { get; }
        public bool Obsolete { get; }

    }

}