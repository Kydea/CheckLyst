using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF.Helper
{
    public class PdfPTable
    {
        public class PdfPRows
        {
            protected Dictionary<int, List<PdfPCell>> Rows;
            protected int startingRow;
            protected int Columns;

            public PdfPRows(int Columns)
            {
                this.Rows = new Dictionary<int, List<PdfPCell>>();
                this.Columns = Columns;
                this.startingRow = 0;
            }

            private int GetFilledColumns(int rowIndex)
            {
                if (rowIndex >= Rows.Count)
                    return Columns;

                return Rows[rowIndex].Where(x => x != null).Sum(x => x.Colspan);
            }

            private int GetAvailableRow(PdfPCell cell)
            {
                for (int i = startingRow; i < Rows.Count; ++i)
                    // Search the one with a free slot
                    if (GetFilledColumns(i) < Columns)
                        return i;

                // No free rows - create one
                int index = Rows.Count;
                Rows.Add(index, new List<PdfPCell>(Enumerable.Repeat<PdfPCell>(null, Columns)));
                return index; // get the last one
            }

            public void AddCell(PdfPCell cell)
            {
                int rowIndex = GetAvailableRow(cell);
                int filledColumns = GetFilledColumns(rowIndex);

                if (filledColumns + cell.Colspan > Columns)
                    throw new InvalidOperationException("Invalid operation: table colspan overflow.");

                int cellIndex = Rows[rowIndex].IndexOf(null);

                // check validity
                for (int i = cellIndex + 1; i < cellIndex + cell.Colspan; ++i)
                    if (i >= Columns || Rows[rowIndex][i] != null)
                        throw new InvalidOperationException("Invalid operation: table colspan conflict.");

                Rows[rowIndex][cellIndex] = cell;

                if (cell.Colspan > 1)
                    for (int i = cellIndex + 1; i < cellIndex + cell.Colspan; ++i)
                        Rows[rowIndex][i] = new PdfPCell() { Empty = true, Colspan = 0 }; // Colspan = 0: ignore it,just

                if (cell.Rowspan > 1)
                    for (int i = rowIndex + 1; i < rowIndex + cell.Rowspan; ++i)
                    {
                        if (!Rows.ContainsKey(i))
                            Rows.Add(i, new List<PdfPCell>(Enumerable.Repeat<PdfPCell>(null, Columns)));

                        for (int j = cellIndex; j < cellIndex + cell.Colspan; ++j)
                            Rows[i][j] = new PdfPCell() { Empty = true };
                    }
            }

            public void Render(Table table)
            {
                Row row;
                PdfPCell cell;

                for (int rowIndex = 0; rowIndex < Rows.Count; ++rowIndex)
                {
                    row = new Row();

                    for (int cellIndex = 0; cellIndex < Columns; ++cellIndex)
                    {
                        cell = Rows[rowIndex][cellIndex];

                        if (cell == null || cell.Empty)
                            continue;

                        row.Cells[cellIndex].Add(cell.Content.Clone());
                        row.Cells[cellIndex].VerticalAlignment = cell.VerticalAlignment;

                        if ((cell.Border & PdfPCell.NO_BORDER) == PdfPCell.NO_BORDER)
                            row.Cells[cellIndex].Borders.Visible = false;
                        else
                        {
                            row.Cells[cellIndex].Borders.Top.Visible = (cell.Border & PdfPCell.TOP) == PdfPCell.TOP;
                            row.Cells[cellIndex].Borders.Left.Visible = (cell.Border & PdfPCell.LEFT) == PdfPCell.LEFT;
                            row.Cells[cellIndex].Borders.Right.Visible = (cell.Border & PdfPCell.RIGHT) == PdfPCell.RIGHT;
                            row.Cells[cellIndex].Borders.Bottom.Visible = (cell.Border & PdfPCell.BOTTOM) == PdfPCell.BOTTOM;
                        }

                        row.Cells[cellIndex].MergeRight = cell.Colspan - 1;
                        row.Cells[cellIndex].MergeDown = cell.Rowspan - 1;
                        row.Cells[cellIndex].Borders.Color = CheckListPDF.BorderColor;
                    }

                    Rows[rowIndex] = null;
                    table.Rows.Add(row);
                }

                Rows = null;
            }

            internal void CompleteRow() => startingRow = Rows.Count; // start from the last
        }

        protected float[] _widths;
        protected PdfPRows Rows;

        public int WidthPercentage { get; set; } = 100;

        public int Columns { get; protected set; }

        public RowAlignment HorizontalAlignment { get; set; }

        public PdfPTable(int columns)
        {
            if (columns == 0)
                throw new InvalidOperationException("Cannot create table, almost one column needed.");

            Columns = columns;
            Rows = new PdfPRows(Columns);
        }

        public void AddCell(PdfPCell cell) => Rows.AddCell(cell);
        public void CompleteRow() => Rows.CompleteRow();

        public void SetWidths(float[] widths)
        {
            if (widths.Length != Columns)
                throw new Exception();

            _widths = widths;
        }

        public MigraDoc.DocumentObjectModel.Tables.Table Render(Section Section)
        {
            Table table = PDFExtension.CreateTable();
            table.Rows.Alignment = HorizontalAlignment;

            float width = (Section.PageSetup.PageWidth - Section.PageSetup.LeftMargin - Section.PageSetup.RightMargin) * (WidthPercentage / 100f);

            for (int i = 0; i < Columns; i++)
            {
                if (_widths != null)
                    table.AddColumn(Unit.FromPoint(width * (_widths[i] / 100f)));
                else
                    table.AddColumn(Unit.FromPoint(width / Columns));
            }

            Rows.Render(table);

            return table;
        }
    }

    public class PdfPCell
    {
        public const int TOP = 1 << 0;
        public const int LEFT = 1 << 1;
        public const int RIGHT = 1 << 2;
        public const int BOTTOM = 1 << 3;
        public const int NO_BORDER = 1 << 4;

        public Paragraph Content { get; set; }
        public int Rowspan { get; set; } = 1;
        public int Colspan { get; set; } = 1;
        public int Border { get; set; } = TOP | BOTTOM | LEFT | RIGHT;
        public bool Empty { get; set; } = false;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
        public ParagraphAlignment HorizontalAlignment
        {
            set => Content.Format.Alignment = value;
            get => Content.Format.Alignment;
        }

        public PdfPCell(Paragraph paragraph)
        {
            Content = paragraph;
        }

        public PdfPCell()
        {
            Content = new Paragraph();
        }

        public PdfPCell SetVerticalAlign(VerticalAlignment verticalAlignment)
        {
            VerticalAlignment = verticalAlignment;
            return this;
        }

        public PdfPCell SetHorizontalAlign(ParagraphAlignment horizontalAlignment)
        {
            Content.Format.Alignment = horizontalAlignment;
            return this;
        }

        public PdfPCell AddNote(string value)
        {
            var TextFormat = Content.AddFormattedText(value);
            TextFormat.Superscript = true;
            return this;
        }

        public PdfPCell SetBorder(int border)
        {
            Border = border;
            return this;
        }

        public PdfPCell SetPadding(int padding)
        {
            Content.Padding(padding);
            return this;
        }
    }
}
