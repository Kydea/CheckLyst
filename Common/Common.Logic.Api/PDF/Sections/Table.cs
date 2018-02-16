using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using System.Drawing;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Common.Logic.Api.PDF.Sections
{
    public class Table : CheckListSection
    {
        class RenderingNode
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

        class RenderingTable : RenderingNode { }
        class RenderingRow : RenderingNode { }
        class RenderingCell : RenderingNode
        {
            public int Colspan = 1, Rowspan = 1;
            public bool SubGroup;
            public String Text = "";
        }

        public Table(CheckListPDF PDFBase, ICheckListGroup Group, int Indentation = 1) : base(PDFBase, Group, Indentation)
        { }

        public override void Render(params Object[] State)
        {
            if (Element == null)
                return;

            var GroupElement = (ICheckListGroup)Element;
            PDFBase.CurrentRenderingX = Indentation * 20;
            DrawText("\n");

            /*Render Table Header*/
            PdfPTable Table = RenderTableHeader(GroupElement);
            Table.CompleteRow();
            /*Render Table Body*/
            RenderTableBody(Table, GroupElement);
            Table.CompleteRow();
            PDFBase.Document.Add(Table);

            DrawText("\n");

            /*Render Signatures Table*/
            if (CheckListPDF.IsPrototype() || Signatures.Any(x => x.History.Count > 0))
            {
                DrawText("Signatures", SecondHeaderFont);
                DrawText("\n");

                Table = RenderSignaturesTable();
                PDFBase.Document.Add(Table);
            }

            DrawText("\n");
        }


        #region Rendering Primitives

        PdfPCell NewCell(String Text, iTextSharp.text.Font Font = null)
        {
            return PdfPCellFromNode(new RenderingCell { Text = Text }, Font);
        }

        PdfPCell PdfPCellFromNode(RenderingNode Node, iTextSharp.text.Font Font = null)
        {
            if (Font == null)
                Font = SmallFont;

            var Cell = (RenderingCell)Node;
            var Content = new iTextSharp.text.Phrase(Cell.Text, Font);
            return new PdfPCell(Content) { Rowspan = Cell.Rowspan, Colspan = Cell.Colspan, HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT, VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE };
        }
        #endregion

        int VisibleSignaturesCount = 0;
        List<ICheckListSignature> Signatures = new List<ICheckListSignature>();

        #region Signatures Rendering
        void RenderSignature(PdfPTable Table, int Id, string Description, string User = null, string Date = null, string Time = null, string Signature = null)
        {
            var Padding = CheckListPDF.IsPrototype() ? 10 : 0;
            var Align = CheckListPDF.IsPrototype() ? iTextSharp.text.Element.ALIGN_BOTTOM : iTextSharp.text.Element.ALIGN_MIDDLE;
            var FillableText = "_ _ _ _ _ _ _ _ _ _ _";

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
        }

        PdfPTable RenderSignaturesTable()
        {
            PdfPTable Table = new PdfPTable(6) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, WidthPercentage = 100 };
            Table.SetWidths(new float[] { 1, 2, 2, 2, 2, 2 });

            /*Header*/
            Table.AddCell(
                NewCell("Reference", SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Status", SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Signed by", SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Date", SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Time", SmallBoldFont)
                .SetBorder(0)
            );

            Table.AddCell(
                NewCell("Signature", SmallBoldFont)
                .SetBorder(0)
            );

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
                        foreach(var Sign in Signatures[s].History.OrderByDescending(x => x.DateTime))
                        {
                            string[] Date = Sign.DateTime.Split(' ');
                            RenderSignature(Table, i>0 ? 0 : VisibleSignaturesCount, String.IsNullOrEmpty(Sign.Note) ? "Signed" : "Canceled", Sign.User, Date[0], Date[1], Sign.Signature);
                            i++;
                        }
                    }
                }
            }


            return Table;
        }
        #endregion

        #region Table Rendering
        void RenderTableRow(PdfPTable Table, List<ICheckListItem> Items)
        {
            foreach (var Item in Items)
                if (Item.TypeName == "Group")
                    RenderTableRow(Table, ((ICheckListGroup)Item).Items);
                else
                {
                    var Cell = new PdfPCell();
                    switch (Item.TypeName)
                    {
                        case "Signature":
                            var Signature = (ICheckListSignature)Item;
                            Signatures.Add(Signature);

                            if (CheckListPDF.IsPrototype())
                                Cell = NewCell((++VisibleSignaturesCount).ToString());
                            else
                                Cell = Signature.History.Count > 0 ? NewCell(String.IsNullOrEmpty(Signature.History.OrderByDescending(x => x.DateTime).FirstOrDefault().Note) ? "Signed" : "Canceled").AddNote(" " + (++VisibleSignaturesCount).ToString()) : NewCell("");

                            break;
                        case "Text":
                            Cell = NewCell(String.IsNullOrWhiteSpace(Item.Description) ? "(no description)" : Item.Description);
                            break;
                        case "Field":
                            if (CheckListPDF.IsPrototype())
                                Cell = NewCell(" ").SetHorizontalAlign(iTextSharp.text.Element.ALIGN_CENTER);
                            else
                            {
                                var Value = CheckListPDF.GetFieldValue((ICheckListField)Item);
                                Cell = NewCell(String.IsNullOrWhiteSpace(Value) ? " " : Value);
                            }
                            break;
                        default:
                            Cell = NewCell(Item.TypeName);
                            break;
                    }

                    Table.AddCell(Cell.SetPadding(5));
                }
        }

        void RenderTableBody(PdfPTable Table, ICheckListGroup Element)
        {
            if(CheckListPDF.IsPrototype())
                for (int i = 0; i < Element.PrototypeIterations; i++)
                    RenderTableRow(Table, Element.Items);
            else
                foreach (var Iteration in Element.Iterations)
                    RenderTableRow(Table, Iteration.Items);
        }

        class Size
        {
            public int RowsCount, ColumnsCount;
        }

        RenderingTable Main = new RenderingTable();

        Size CreateTableHeader(List<RenderingRow> Rows, int RowIndex, List<ICheckListItem> Items)
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
                Cell.Text = String.IsNullOrEmpty(Item.Description) ? "(empty)" : Item.Description;

                if (Item.TypeName == "Group")
                {
                    Cell.SubGroup = true;
                    var Size = CreateTableHeader(Rows, RowIndex + 1, ((ICheckListGroup)Item).Items);
                    RowsCount = Math.Max(Size.RowsCount + 1, RowsCount);
                    ColumnsCount += Size.ColumnsCount;
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

        PdfPTable RenderTableHeader(ICheckListGroup Group)
        {
            Size Size = CreateTableHeader(new List<RenderingRow> { }, 0/*, null*/, Group.Items);

            PdfPTable Table = new PdfPTable(Size.ColumnsCount) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, WidthPercentage = 100 };

            /*Render DOM-like data*/
            foreach (RenderingRow Row in Main.Children)
                foreach (RenderingCell Cell in Row.Children)
                    Table.AddCell(PdfPCellFromNode(Cell).SetPadding(5));

            return Table;
        }
        #endregion

        public override void RenderChildren()
        {
            CheckListPDF.RenderGroup((ICheckListGroup)Element);
        }
    }
}