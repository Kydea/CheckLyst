using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public class MigraDocBuilder
    {
        protected Document Document { get; }

        protected PdfDocument PdfDocument { get; }

        protected List<int> ExcludeList { get; }

        protected int PdfPage { get; set; }

        public MigraDocBuilder()
        {
            Document = new Document();
            ExcludeList = new List<int>();
            PdfDocument = new PdfDocument();
        }

        public Section AddSection()
        {
            return Document.AddSection();
        }

        protected PdfPage AddPage(PdfPage PdfPage)
        {
            ExcludeList.Add(this.PdfPage++);
            Document.LastSection.AddPageBreak();
            return PdfDocument.AddPage(PdfPage);
        }

        public PdfPage Add(PdfPage Page)
        {
            Render();
            return AddPage(Page);
        }

        public void AddRange(IEnumerable<PdfPage> Pages)
        {
            Render();

            foreach (var Page in Pages)
                AddPage(Page);
        }

        protected DocumentRenderer GetDocumentRenderer()
        {
            var documentRenderer = new DocumentRenderer(Document);
            documentRenderer.PrepareDocument();
            return documentRenderer;
        }

        protected void Render()
        {
            var DocumentRenderer = GetDocumentRenderer();
            var StartIndex = 1 + PdfPage;

            for (int i = StartIndex; i <= DocumentRenderer.FormattedDocument.PageCount; i++)
            {
                PageInfo pageInfo = DocumentRenderer.FormattedDocument.GetPageInfo(i);
                var page = PdfDocument.AddPage();
                page.Width = pageInfo.Width;
                page.Height = pageInfo.Height;
                page.Orientation = pageInfo.Orientation;
                PdfPage++;
            }
        }

        public void Save(Stream stream)
        {
            Render();

            var DocumentRenderer = GetDocumentRenderer();

            Enumerable
                .Range(0, DocumentRenderer.FormattedDocument.PageCount)
                .Where(index => !ExcludeList.Contains(index))
                .ToList().ForEach(index => {
                    var page = PdfDocument.Pages[index];

                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                        DocumentRenderer.RenderPage(gfx, index + 1);
                });

            PdfDocument.Save(stream, false);
            PdfDocument.Close();
        }

        public MigraDocBuilder SetSubject(string Subject)
        {
            PdfDocument.Info.Subject = Subject;
            return this;
        }

        public MigraDocBuilder SetCreator(string Creator)
        {
            PdfDocument.Info.Creator = Creator;
            return this;
        }

        public MigraDocBuilder SetAuthor(string Author)
        {
            PdfDocument.Info.Author = Author;
            return this;
        }

        public MigraDocBuilder SetKeywords(string Keywords)
        {
            PdfDocument.Info.Keywords = Keywords;
            return this;
        }
    }
}
