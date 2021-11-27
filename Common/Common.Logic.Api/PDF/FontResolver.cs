using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF
{
    class FontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var name = familyName.ToLower();

            switch (name)
            {
                case "arial unicode ms":
                    if (isBold || isItalic)
                        return new FontResolverInfo("ArialUnicodeMS#", isBold && isItalic ? PdfSharp.Drawing.XStyleSimulations.BoldItalicSimulation : isBold ? PdfSharp.Drawing.XStyleSimulations.BoldSimulation : PdfSharp.Drawing.XStyleSimulations.ItalicSimulation);

                    return new FontResolverInfo("ArialUnicodeMS#");
            }

            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "ArialUnicodeMS#":
                    return FontResolverHelper.ArialUnicodeMS;
            }

            return null;
        }
    }

    public static class FontResolverHelper
    {
        public static byte[] ArialUnicodeMS => LoadFont("CheckListManager.Common.Logic.Api.PDF.Fonts.ARIALUNI.TTF");

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        static byte[] LoadFont(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }
}
