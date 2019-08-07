using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api.PDF.Helper;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Table : CheckListSection
    {
        public class RenderingNode
        {
            public RenderingNode Parent = null;
            public List<RenderingNode> Children = new List<RenderingNode>();

            public RenderingNode() { }

            public void AppendTo(RenderingNode NewParent)
            {
                Parent = NewParent;
                NewParent.Children.Add(this);
            }
        }

        public class RenderingTable : RenderingNode { }
        public class RenderingRow : RenderingNode { }
        public class RenderingCell : RenderingNode
        {
            public int Colspan = 1, Rowspan = 1;
            public bool SubGroup;
            public string Text = "";
        }

        private string TimeZone { get; }

        public Table(CheckListPDF PDFBase, ICheckListGroup Group, int Indentation = 1) : base(PDFBase, Group, Indentation)
        {
            TimeZone = PDFBase.TimeZone;
        }

        public override async Task RenderAsync(params object[] State)
        {
            if (Element == null)
                return;

            var GroupElement = (ICheckListGroup)Element;

            if (!string.IsNullOrEmpty(GroupElement.Description))
                DrawText(GroupElement.Description, Indentation == 1 ? PDFBase.TitleFont1 : Indentation == 2 ? PDFBase.TitleFont2 : PDFBase.TitleFont3, 10);

            PdfPTable Table = await RenderTableHeader(GroupElement);
            await RenderTableBody(Table, GroupElement);
            PDFBase.Container.Add(Table);

            DrawText("\n");

            if (Signatures.Any(x => x.History.Count > 0))
            {
                DrawText("Signatures", PDFBase.TitleFont2, 10);

                Table = RenderSignaturesTable();
                PDFBase.Container.Add(Table);
            }

            DrawText("\n");

            return;
        }


        #region Rendering Primitives

        PdfPCell NewCell(string Text, Font Font = null)
        {
            return PdfPCellFromNode(new RenderingCell { Text = Text }, Font);
        }

        PdfPCell PdfPCellFromNode(RenderingNode Node, Font Font = null)
        {
            if (Font == null)
                Font = PDFBase.SmallFont;

            var Cell = (RenderingCell)Node;

            return new PdfPCell(new Paragraph().SetText(Cell.Text).SetFont(Font))
            {
                Rowspan = Cell.Rowspan,
                Colspan = Cell.Colspan,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = ParagraphAlignment.Left,
            };
        }
        #endregion

        int VisibleSignaturesCount = 0;
        List<ICheckListSignature> Signatures = new List<ICheckListSignature>();

        #region Signatures Rendering
        void RenderSignature(PdfPTable Table, int Id, string Description, string User = null, string Date = null, string Time = null, string Signature = null)
        {
            var Padding = CheckListPDF.IsPrototype() ? 10 : 0;
            var Align = CheckListPDF.IsPrototype() ? VerticalAlignment.Bottom : VerticalAlignment.Center;
            var FillableText = "_ _ _ _ _ _ _ _ _ _";

            Table.AddCell(
                NewCell("")
                .AddNote(Id == 0 ? "" : Id.ToString())
                .SetBorder(0)
                .SetPadding(Padding)
            );

            Table.AddCell(
                NewCell(CheckListPDF.IsPrototype() ? FillableText : Description)
                .SetBorder(0)
                .SetVerticalAlign(Align)
             );

            Table.AddCell(
                NewCell(CheckListPDF.IsPrototype() ? FillableText : User)
                .SetBorder(0)
                .SetVerticalAlign(Align)
            );

            Table.AddCell(
                NewCell(CheckListPDF.IsPrototype() ? FillableText : Date)
                .SetBorder(0)
                .SetVerticalAlign(Align)
            );

            Table.AddCell(
                NewCell(CheckListPDF.IsPrototype() ? FillableText : Time)
                .SetBorder(0)
                .SetVerticalAlign(Align)
            );

            Table.AddCell(
                NewCell(CheckListPDF.IsPrototype() ? FillableText : Date == null ? " " : Signature)
                .SetBorder(0)
                .SetVerticalAlign(Align)
            );

            Table.CompleteRow();
        }

        PdfPTable RenderSignaturesTable()
        {
            PdfPTable Table = new PdfPTable(6)
            {
                HorizontalAlignment = RowAlignment.Center,
                WidthPercentage = 100
            };
            Table.SetWidths(new float[] { 10, 15, 20, 20, 15, 20 });

            /*Header*/
            Table.AddCell(
                NewCell("Reference", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Status", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Signed by", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Date", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Time", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Signature", PDFBase.SmallBoldFont)
                .SetBorder(0)
            );

            Table.CompleteRow();

            VisibleSignaturesCount = 0;
            for (int s = 0; s < Signatures.Count; ++s)
            {
                if (CheckListPDF.IsPrototype())
                    RenderSignature(Table, (++VisibleSignaturesCount), "", "");
                else
                {
                    if (Signatures[s].History.Count > 0)
                    {
                        ++VisibleSignaturesCount;
                        int i = 0;
                        foreach (var Sign in Signatures[s].History.OrderByDescending(x => x.DateTime))
                        {
                            var Date = Sign.DateTime.ToLocalTime(TimeZone);
                            RenderSignature(Table, i > 0 ? 0 : VisibleSignaturesCount, string.IsNullOrEmpty(Sign.Note) ? "Signed" : "Canceled", Sign.User, Date.ToString("yyyy-MM-dd"), Date.ToString("HH:mm"), Sign.Signature);
                            i++;
                        }
                    }
                }
            }


            return Table;
        }
        #endregion

        #region Table Rendering
        public async Task RenderTableRow(PdfPTable Table, List<ICheckListItem> Items, bool completeRow = true)
        {
            if (completeRow)
                Table.CompleteRow();

            foreach (var Item in Items)
                if (Item.TypeName == "Group")
                    await RenderTableRow(Table, ((ICheckListGroup)Item).Items, false);
                else
                {
                    PdfPCell Cell = new PdfPCell(new Paragraph());
                    switch (Item.TypeName)
                    {
                        case "Signature":
                            var Signature = (ICheckListSignature)Item;
                            Signatures.Add(Signature);

                            if (CheckListPDF.IsPrototype())
                                Cell = NewCell((++VisibleSignaturesCount).ToString());
                            else
                                Cell = Signature.History.Count > 0 ? NewCell(string.IsNullOrEmpty(Signature.History.OrderByDescending(x => x.DateTime).FirstOrDefault().Note) ? "Signed" : "Canceled").AddNote(" " + (++VisibleSignaturesCount).ToString()) : NewCell("");

                            break;
                        case "Text":
                            Cell = NewCell(string.IsNullOrWhiteSpace(Item.Description) ? "(no description)" : Item.Description);
                            break;
                        case "Field":
                            if (CheckListPDF.IsPrototype())
                                Cell = NewCell("").SetHorizontalAlign(ParagraphAlignment.Center);
                            else
                            {
                                var Value = CheckListPDF.GetFieldValue((ICheckListField)Item);
                                Cell = NewCell(string.IsNullOrWhiteSpace(Value) ? " " : Value);
                            }
                            break;
                        case "Link":
                            var Link = (ICheckListLink)Item;
                            Cell = NewCell(string.Empty);
                            var Paragraph = Cell.Content.AddHyperlink(Link.Url, HyperlinkType.Web).AddFormattedText(string.IsNullOrWhiteSpace(Item.Description) ? "(link)" : Item.Description);
                            Paragraph.Font.Underline = Underline.Single;
                            break;
                        case "Image":
                            var Image = (ICheckListImage)Item;
                            if (Image.Key == null)
                                Cell = NewCell("(# no key)");
                            else
                            {
                                var Index = await CheckListPDF.AddAttachmentAsync(CheckListPDF.ConfigurationService.PrototypesStorageBucket, Image.Key);
                                Cell = NewCell($"{(string.IsNullOrEmpty(Image.Description) ? string.Empty : Image.Description + " ")}(see attachament {Index})");
                            }
                            break;
                        case "Attachment":
                            var Attachment = (ICheckListAttachment)Item;
                            if (Attachment.Key != null)
                            {
                                var Index = await CheckListPDF.AddAttachmentAsync(CheckListPDF.ConfigurationService.InstancesStorageBucket, Attachment.Key);
                                Cell = NewCell($"{(string.IsNullOrEmpty(Attachment.Description) ? string.Empty : Attachment.Description + " ")}(see attachament {Index})");
                            }
                            break;
                        default:
                            Cell = NewCell(Item.TypeName);
                            break;
                    }

                    Table.AddCell(Cell.SetPadding(5));
                }
        }

        public async Task RenderTableBody(PdfPTable Table, ICheckListGroup Element)
        {
            if (CheckListPDF.IsPrototype())
                for (int i = 0; i < Element.PrototypeIterations; i++)
                    await RenderTableRow(Table, Element.Items);
            else
                foreach (var Iteration in Element.Iterations)
                    await RenderTableRow(Table, Iteration.Items);
        }

        public class Size
        {
            public int RowsCount, ColumnsCount;
        }

        RenderingTable Main = new RenderingTable();

        public async Task<Size> CreateTableHeader(List<RenderingRow> Rows, int RowIndex, List<ICheckListItem> Items)
        {
            int
                RowsCount = 1,
                ColumnsCount = 0;

            RenderingRow Row = null;

            if (Rows.Count < RowIndex + 1)
            {
                Row = new RenderingRow();
                Row.AppendTo(Main);
                Rows.Add(Row);
            }
            else
                Row = Rows[RowIndex];

            for (int Index = 0; Index < Items.Count; ++Index)
            {
                var Item = Items[Index];
                var Cell = new RenderingCell();
                Cell.AppendTo(Row);
                Cell.Text = string.IsNullOrEmpty(Item.Description) ? "(empty)" : Item.Description;

                if (Item.TypeName == "Group")
                {
                    Cell.SubGroup = true;
                    var Size = await CreateTableHeader(Rows, RowIndex + 1, ((ICheckListGroup)Item).Items);
                    RowsCount = Math.Max(Size.RowsCount + 1, RowsCount);
                    ColumnsCount += Math.Max(Size.ColumnsCount, 1);
                    if (Size.ColumnsCount > 1)
                        Cell.Colspan = Size.ColumnsCount;
                }
                else
                    ColumnsCount++;
            }

            if (RowsCount > 1)
                foreach (RenderingCell Cell in Row.Children.Where(x => !((RenderingCell)x).SubGroup))
                    Cell.Rowspan = RowsCount;

            return new Size { RowsCount = RowsCount, ColumnsCount = ColumnsCount };
        }

        public async Task<PdfPTable> RenderTableHeader(ICheckListGroup Group)
        {
            Size Size = await CreateTableHeader(new List<RenderingRow> { }, 0/*, null*/, Group.Items);

            PdfPTable Table = new PdfPTable(Size.ColumnsCount) { HorizontalAlignment = RowAlignment.Center, WidthPercentage = 100 };

            foreach (RenderingRow Row in Main.Children)
                foreach (RenderingCell Cell in Row.Children)
                    Table.AddCell(PdfPCellFromNode(Cell).SetPadding(5));

            return Table;
        }
        #endregion

        public override Task RenderChildrenAsync()
        {
            CheckListPDF.RenderGroup((ICheckListGroup)Element);
            return Task.CompletedTask;
        }
    }
}