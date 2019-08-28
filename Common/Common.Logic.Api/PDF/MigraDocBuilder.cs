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
        protected class PageMap
        {
            public int DocumentPage { get; set; }
            public int PdfPage { get; set; }
        }

        protected Document Document { get; }

        protected PdfDocument PdfDocument { get; }
        
        protected List<PageMap> PageMapping { get; }

        protected int PdfPage { get; set; }

        protected int DocumentPage { get; set; }

        public MigraDocBuilder()
        {
            Document = new Document();
            PdfDocument = new PdfDocument();
            PageMapping = new List<PageMap>();
        }

        public Section AddSection()
        {
            return Document.AddSection();
        }

        protected PdfPage AddPage(PdfPage Page)
        {
            PdfPage++;
            return PdfDocument.AddPage(Page);
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
            int PageCount = DocumentRenderer.FormattedDocument.PageCount;
            var StartIndex = 1 + DocumentPage;

            for (int i = StartIndex; i <= PageCount; i++)
            {
                PageInfo pageInfo = DocumentRenderer.FormattedDocument.GetPageInfo(i);

                var page = PdfDocument.AddPage();
                page.Width = pageInfo.Width;
                page.Height = pageInfo.Height;
                page.Orientation = pageInfo.Orientation;

                PageMapping.Add(new PageMap()
                {
                    DocumentPage = DocumentPage + 1,
                    PdfPage = PdfPage,
                });

                DocumentPage++;
                PdfPage++;
            }
        }

        public void Save(Stream stream)
        {
            Render();

            var DocumentRenderer = GetDocumentRenderer();

            PageMapping.ForEach(map => {
                var page = PdfDocument.Pages[map.PdfPage];

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    DocumentRenderer.RenderPage(gfx, map.DocumentPage);
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
