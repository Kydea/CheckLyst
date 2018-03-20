using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF.Helper
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
    }

    public static class FontHelper
    {
        public static MigraDoc.DocumentObjectModel.Font GetInstance(string fontFamily, MigraDoc.DocumentObjectModel.Unit size, FontStyle fontStyle, MigraDoc.DocumentObjectModel.Color color)
        {
            var Font = new MigraDoc.DocumentObjectModel.Font(fontFamily, size)
            {
                Color = color,
                Bold = fontStyle.HasFlag(FontStyle.Bold),
                Italic = fontStyle.HasFlag(FontStyle.Italic)
            };

            return Font;
        }
    }
}
