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
        protected float[] _widths;
        protected List<Row> _rows;
        protected List<PdfPCell> _cells;

        public int WidthPercentage { get; set; } = 100;

        public int Columns { get; protected set; }

        public RowAlignment HorizontalAlignment { get; set; }

        public PdfPTable()
        {
            _rows = new List<Row>();
            _cells = new List<PdfPCell>();
        }

        public PdfPTable(int columns) : this()
        {
            if (columns == 0)
                throw new InvalidOperationException("Cannot create table, almost one column needed.");

            Columns = columns;
        }

        public void AddCell(PdfPCell cell)
        {
            _cells.Add(cell);
        }

        public void SetWidths(float[] widths)
        {
            if (widths.Length != Columns)
                throw new Exception();

            _widths = widths;
        }

        public void CompleteRow()
        {
            // Check for rowspan & add "missing" cells.
            #region Rowspan check
            Row row;
            int idx = _rows.Count;

            for (int rowIdx = 0; rowIdx < idx; rowIdx++)
            {
                row = _rows[rowIdx];

                for (int j = 0; j < row.Cells.Count; j++)
                {
                    Cell cell = row.Cells[j];

                    if (cell.MergeDown > 0 && (cell.MergeDown + rowIdx >= idx) && _cells.Count < Columns)
                        _cells.Insert(j, new PdfPCell(new Paragraph()));
                }
            }

            int colspanCount = _cells.Sum(s => s.Colspan);

            // Add missing cells
            if (colspanCount != Columns)
            {
                _cells.Add(new PdfPCell(new Paragraph()) {
                    Colspan = Columns - colspanCount
                });
            }
            #endregion
                
            row = new Row();

            for (int i = 0; i < _cells.Count; i++)
            {
                PdfPCell cell = _cells[i];

                row.Cells[i].Add(cell.Content.Clone());
                row.Cells[i].VerticalAlignment = cell.VerticalAlignment;

                if ((cell.Border & PdfPCell.NO_BORDER) == PdfPCell.NO_BORDER)
                    row.Cells[i].Borders.Visible = false;
                else
                {
                    row.Cells[i].Borders.Top.Visible = (cell.Border & PdfPCell.TOP) == PdfPCell.TOP;
                    row.Cells[i].Borders.Left.Visible = (cell.Border & PdfPCell.LEFT) == PdfPCell.LEFT;
                    row.Cells[i].Borders.Right.Visible = (cell.Border & PdfPCell.RIGHT) == PdfPCell.RIGHT;
                    row.Cells[i].Borders.Bottom.Visible = (cell.Border & PdfPCell.BOTTOM) == PdfPCell.BOTTOM;
                }

                if (cell.Colspan > 1)
                {
                    row.Cells[i].MergeRight = cell.Colspan - 1;
                    i += cell.Colspan - 1;
                }

                if (cell.Rowspan > 1)
                    row.Cells[i].MergeDown = cell.Rowspan - 1;
            }

            _rows.Add(row);
            _cells.Clear();
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

            _rows.ForEach(row => table.Rows.Add(row));

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
