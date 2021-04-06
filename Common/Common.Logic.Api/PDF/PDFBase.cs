using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Fonts;
using System.IO;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class PDFBase
    {

        public static readonly Color Red = Colors.Red;
        public static readonly Color LightGray = Color.FromRgb(235, 235, 235);
        public static readonly Color Gray = Color.FromRgb(204, 204, 204);
        public static readonly Color DarkGray = Color.FromRgb(153, 153, 153);
        public static readonly Color Green = Color.FromRgb(0, 135, 122);
        public static readonly Color Yellow = Color.FromRgb(255, 186, 0);

        public static Border DefaultBorder => new Border { Color = Gray, Style = BorderStyle.Single };
        public static Border RedBorder => new Border { Color = Red, Style = BorderStyle.Single };
       
        public static Font TitleFont1 => FontHelper.GetInstance("Arial Unicode MS", 20, FontStyle.Bold, Yellow).Clone();
        public static Font TitleFont2 => FontHelper.GetInstance("Arial Unicode MS", 16, FontStyle.Bold, Green).Clone();
        public static Font TitleFont3 => FontHelper.GetInstance("Arial Unicode MS", 12, FontStyle.Bold, Green).Clone();
        public static Font RedTitleFont => FontHelper.GetInstance("Arial Unicode MS", 12, FontStyle.Bold, Red).Clone();
        public static Font GenericFont => FontHelper.GetInstance("Arial Unicode MS", 10, FontStyle.Regular, Colors.Black).Clone();
        public static Font BoldFont => FontHelper.GetInstance("Arial Unicode MS", 10, FontStyle.Bold, DarkGray).Clone();
        public static Font GrayFont => FontHelper.GetInstance("Arial Unicode MS", 10, FontStyle.Regular, DarkGray).Clone();
        public static Font SmallFont => FontHelper.GetInstance("Arial Unicode MS", 8, FontStyle.Regular, Colors.Black).Clone();
        public static Font SmallBoldFont => FontHelper.GetInstance("Arial Unicode MS", 8, FontStyle.Bold, Colors.Black).Clone();

        protected string Subject { get; private set; }
        public Section Container { get; private set; }
        protected MigraDocBuilder DocumentBuilder { get; set; }

        public PDFBase(string Subject)
        {
            this.Subject = Subject;
        }

        public async Task<Section> AddSectionAsync()
        {
            Container = DocumentBuilder.AddSection();
            await OnNewSectionAsync();
            return Container;
        }

        public async Task GenerateAsync(Stream Stream)
        {
            try
            {
                if (!(GlobalFontSettings.FontResolver is FontResolver))
                    GlobalFontSettings.FontResolver = new FontResolver();

                DocumentBuilder = new MigraDocBuilder()
                {
                    Unicode = true // allow unicode charset
                };
                
                await AddSectionAsync();
                await OnGenerateAsync();
                await OnRenderAttachmentsAsync();

                DocumentBuilder
                    .SetSubject(Subject)
                    .SetCreator("www.matematici.com")
                    .SetAuthor("Matematici")
                    .Save(Stream);

                Stream.Flush();
            }
            catch(System.Exception ex)
            {
                throw ex;
            }
        }

        protected abstract Task OnGenerateAsync();

        protected abstract Task OnRenderAttachmentsAsync();

        protected abstract Task OnNewSectionAsync();
    }
}
