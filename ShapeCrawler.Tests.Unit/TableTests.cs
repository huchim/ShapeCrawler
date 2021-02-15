using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using ShapeCrawler.Collections;
using ShapeCrawler.Models.SlideComponents;
using ShapeCrawler.Tables;
using ShapeCrawler.Tests.Unit.Helpers;
using ShapeCrawler.Tests.Unit.Properties;
using Xunit;

namespace ShapeCrawler.Tests.Unit
{
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    [SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
    public class TableTests : IClassFixture<PresentationFixture>
    {
        private readonly PresentationFixture _fixture;

        public TableTests(PresentationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void RowsAndRowCellsCounters_ReturnNumberOfRowsInTheTableAndNumberOfCellsInTheTableRow()
        {
            // Arrange
            TableSc table = _fixture.Pre009.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;

            // Act
            RowCollection tableRows = table.Rows;
            IEnumerable<CellSc> rowCells = tableRows.First().Cells;

            // Assert
            tableRows.Should().HaveCount(3);
            rowCells.Should().HaveCount(3);
        }

        [Fact]
        public void RowRemoveAt_RemovesTableRowWithSpecifiedIndex()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._009, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            int originRowsCount = table.Rows.Count;
            var mStream = new MemoryStream();

            // Act
            table.Rows.RemoveAt(0);

            // Assert
            table.Rows.Should().HaveCountLessThan(originRowsCount);

            presentation.SaveAs(mStream);
            table = PresentationSc.Open(mStream, false).Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            table.Rows.Should().HaveCountLessThan(originRowsCount);
        }

        [Fact]
        public void RowHeightGetter_ReturnsHeightOfTableRow()
        {
            // Arrange
            TableSc table = _fixture.Pre001.Slides[1].Shapes.First(sp => sp.Id == 3) as TableSc;

            // Act-Assert
            table.Rows[0].Height.Should().Be(370840);
        }

        [Fact]
        public void CellIsMergedCell_ReturnsTrue_WhenCellMergedWithOtherHorizontally()
        {
            // Arrange
            RowSc row = ((ITable)_fixture.Pre001.Slides[1].Shapes.First(sp => sp.Id == 4)).Rows[1];
            CellSc cell1x0 = row.Cells[0];
            CellSc cell1x1 = row.Cells[1];

            // Act-Assert
            cell1x0.IsMergedCell.Should().BeTrue();
            cell1x1.IsMergedCell.Should().BeTrue();
            cell1x0.Should().BeSameAs(cell1x1);
        }

        [Theory]
        [MemberData(nameof(TestCasesCellIsMergedCell))]
        public void CellIsMergedCell_ReturnsTrue_WhenCellMergedWithOtherVertically(CellSc cell1, CellSc cell2)
        {
            cell1.IsMergedCell.Should().BeTrue();
            cell2.IsMergedCell.Should().BeTrue();
            cell1.Should().Be(cell2);
        }

        public static IEnumerable<object[]> TestCasesCellIsMergedCell()
        {
            TableSc table = PresentationSc.Open(Resources._001, false).Slides[1].Shapes.First(sp => sp.Id == 3) as TableSc;
            yield return new object[] {table[0, 0], table[1, 0]};

            table = PresentationSc.Open(Resources._001, false).Slides[1].Shapes.First(sp => sp.Id == 5) as TableSc;
            yield return new object[] { table[1, 1], table[2, 1] };

            table = PresentationSc.Open(Resources._001, false).Slides[3].Shapes.First(sp => sp.Id == 4) as TableSc;
            yield return new object[] { table[0, 1], table[1, 1] };
        }

        [Fact]
        public void ColumnsCount_ReturnsNumberOfColumnsInTheTable()
        {
            // Arrange
            TableSc table = _fixture.Pre001.Slides[1].Shapes.First(sp => sp.Id == 4) as TableSc;

            // Act
            int columnsCount = table.Columns.Count;

            // Assert
            columnsCount.Should().Be(3);
        }

        [Fact]
        public void ColumnWidthGetter_ReturnsTableColumnWidthInEMU()
        {
            // Arrange
            TableSc table = _fixture.Pre001.Slides[1].Shapes.First(sp => sp.Id == 4) as TableSc;
            Column column = table.Columns[0];

            // Act
            long columnWidth = column.Width;

            // Assert
            columnWidth.Should().Be(3505199);
        }

        [Fact]
        public void ColumnWidthSetter_ChangeTableColumnWidth()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[1].Shapes.First(sp => sp.Id == 3) as TableSc;
            const long newColumnWidth = 4074000;
            var mStream = new MemoryStream();

            // Act
            table.Columns[0].Width = newColumnWidth;

            // Assert
            table.Columns[0].Width.Should().Be(newColumnWidth);

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[1].Shapes.First(sp => sp.Id == 3) as TableSc;
            table.Columns[0].Width.Should().Be(newColumnWidth);
        }

#if DEBUG

        [Theory]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 1, 0, 0)]
        public void MergeCells_MergesSpecifiedCellsRange(int rowIdx1, int colIdx1, int rowIdx2, int colIdx2)
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[1].Shapes.First(sp => sp.Id == 4) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[rowIdx1, colIdx1], table[rowIdx2, colIdx2]);

            // Assert
            table[rowIdx1, colIdx1].IsMergedCell.Should().BeTrue();
            table[rowIdx2, colIdx2].IsMergedCell.Should().BeTrue();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[1].Shapes.First(sp => sp.Id == 4) as TableSc;
            table[rowIdx1, colIdx1].IsMergedCell.Should().BeTrue();
            table[rowIdx2, colIdx2].IsMergedCell.Should().BeTrue();
        }

        [Fact(DisplayName = "MergeCells #1")]
        public void MergeCells_Merges0x0And0x1CellsOf2x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[0, 1]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
        }

        [Fact(DisplayName = "MergeCells #2")]
        public void MergeCells_Merges0x1And0x2CellsOf3x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 1], table[0, 2]);

            // Assert
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();
        }

        [Fact(DisplayName = "MergeCells #3")]
        public void MergeCells_Merges0x0And0x1And0x2CellsOf3x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[0, 2]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();
        }

        [Fact(DisplayName = "MergeCells #4")]
        public void MergeCells_Merges0x0And0x1MergedCellsWith0x2CellIn3x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 7) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[0, 2]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 7) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeTrue();
        }

        [Fact(DisplayName = "MergeCells #5")]
        public void MergeCells_Merges0x0And1x0CellsOf2x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[1, 0]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeFalse();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeFalse();
        }

        [Fact(DisplayName = "MergeCells #6")]
        public void MergeCells_Merges0x1And1x1CellsOf3x2Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 1], table[1, 1]);

            // Assert
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 0].IsMergedCell.Should().BeFalse();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 3) as TableSc;
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 0].IsMergedCell.Should().BeFalse();
        }

        [Fact(DisplayName = "MergeCells #7")]
        public void MergeCells_Merges0x0To1x1RangeOf3x3Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 10) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[1, 1]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeFalse();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 10) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 2].IsMergedCell.Should().BeFalse();
        }

        [Fact]
        public void MergeCells_MergesMergedCellWithNonMergedCell()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[1].Shapes.First(sp => sp.Id == 5) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[1, 1], table[1, 2]);

            // Assert
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[1, 2].IsMergedCell.Should().BeTrue();
            table[1, 1].Should().Be(table[1, 2]);
            table[3, 2].IsMergedCell.Should().BeFalse();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[1].Shapes.First(sp => sp.Id == 5) as TableSc;
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[1, 2].IsMergedCell.Should().BeTrue();
            table[1, 1].Should().Be(table[1, 2]);
            table[3, 2].IsMergedCell.Should().BeFalse();
        }

        [Fact]
        public void MergeCells_MergesTwoMergedCells()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[3].Shapes.First(sp => sp.Id == 2) as TableSc;
            var mStream = new MemoryStream();

            // Act
            table.MergeCells(table[0, 0], table[0, 1]);

            // Assert
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 0].Should().Be(table[1, 1]);
            table[0, 2].IsMergedCell.Should().BeFalse();

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[3].Shapes.First(sp => sp.Id == 2) as TableSc;
            table[0, 0].IsMergedCell.Should().BeTrue();
            table[0, 1].IsMergedCell.Should().BeTrue();
            table[1, 0].IsMergedCell.Should().BeTrue();
            table[1, 1].IsMergedCell.Should().BeTrue();
            table[0, 0].Should().Be(table[1, 1]);
            table[0, 2].IsMergedCell.Should().BeFalse();
        }

        [Fact(DisplayName = "MergeCells #8")]
        public void MergeCells_Converts2X1TableInto1X1_WhenAllCellsAreMerged()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[3].Shapes.First(sp => sp.Id == 3) as TableSc;
            var mStream = new MemoryStream();
            long totalColWidth = table.Columns[0].Width + table.Columns[1].Width;

            // Act
            table.MergeCells(table[0, 0], table[0, 1]);

            // Assert
            table.Columns.Should().HaveCount(1);
            table.Columns[0].Width.Should().Be(totalColWidth);
            table.Rows.Should().HaveCount(1);
            table.Rows[0].Cells.Should().HaveCount(1);

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[3].Shapes.First(sp => sp.Id == 3) as TableSc;
            table.Columns.Should().HaveCount(1);
            table.Columns[0].Width.Should().Be(totalColWidth);
            table.Rows.Should().HaveCount(1);
            table.Rows[0].Cells.Should().HaveCount(1);
        }

        [Fact(DisplayName = "MergeCells #9")]
        public void MergeCells_Converts2X2TableInto1X1_WhenAllCellsAreMerged()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            var mStream = new MemoryStream();
            long mergedColumnWidth = table.Columns[0].Width + table.Columns[1].Width;
            long mergedRowHeight = table.Rows[0].Height + table.Rows[1].Height;

            // Act
            table.MergeCells(table[0, 0], table[1,1]);

            // Assert
            AssertTable(table, mergedColumnWidth, mergedRowHeight);

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[2].Shapes.First(sp => sp.Id == 5) as TableSc;
            AssertTable(table, mergedColumnWidth, mergedRowHeight);

            static void AssertTable(TableSc tableSc, long expectedMergedColumnWidth, long expectedMergedRowHeight)
            {
                tableSc.Columns.Should().HaveCount(1);
                tableSc.Columns[0].Width.Should().Be(expectedMergedColumnWidth);
                tableSc.Rows.Should().HaveCount(1);
                tableSc.Rows[0].Cells.Should().HaveCount(1);
                tableSc.Rows[0].Height.Should().Be(expectedMergedRowHeight);
            }
        }

        [Fact(DisplayName = "MergeCells #10")]
        public void MergeCells_Merges0x0and0x1CellsIn3x1Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[3].Shapes.First(sp => sp.Id == 6) as TableSc;
            var mStream = new MemoryStream();
            long mergedColumnWidth = table.Columns[0].Width + table.Columns[1].Width;

            // Act
            table.MergeCells(table[0, 0], table[0, 1]);

            // Assert
            AssertTable(table, mergedColumnWidth);

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[3].Shapes.First(sp => sp.Id == 6) as TableSc;
            AssertTable(table, mergedColumnWidth);

            static void AssertTable(TableSc tableSc, long expectedMergedColumnWidth)
            {
                tableSc.Columns.Should().HaveCount(2);
                tableSc.Columns[0].Width.Should().Be(expectedMergedColumnWidth);
                tableSc.Rows.Should().HaveCount(1);
                tableSc.Rows[0].Cells.Should().HaveCount(2);
            }
        }

        [Fact(DisplayName = "MergeCells #11")]
        public void MergeCells_Merges0x1and0x2CellsIn3x1Table()
        {
            // Arrange
            PresentationSc presentation = PresentationSc.Open(Resources._001, true);
            TableSc table = presentation.Slides[3].Shapes.First(sp => sp.Id == 6) as TableSc;
            var mStream = new MemoryStream();
            long mergedColumnWidth = table.Columns[1].Width + table.Columns[2].Width;

            // Act
            table.MergeCells(table[0, 1], table[0, 2]);

            // Assert
            AssertTable(table, mergedColumnWidth);

            presentation.SaveAs(mStream);
            presentation = PresentationSc.Open(mStream, false);
            table = presentation.Slides[3].Shapes.First(sp => sp.Id == 6) as TableSc;
            AssertTable(table, mergedColumnWidth);

            static void AssertTable(TableSc tableSc, long expectedMergedColumnWidth)
            {
                tableSc.Columns.Should().HaveCount(2);
                tableSc.Columns[1].Width.Should().Be(expectedMergedColumnWidth);
                tableSc.Rows.Should().HaveCount(1);
                tableSc.Rows[0].Cells.Should().HaveCount(2);
            }
        }
#endif

        [Fact]
        public void Indexer_ReturnsCellByRowAndColumnIndexes()
        {
            // Arrange
            TableSc tableCase1 = _fixture.Pre001.Slides[1].Shapes.First(sp => sp.Id == 4) as TableSc;
            TableSc tableCase2 = _fixture.Pre001.Slides[3].Shapes.First(sp => sp.Id == 4) as TableSc;

            // Act
            CellSc cellCase1 = tableCase1[0, 0];
            CellSc cellCase2 = tableCase2[1, 1];

            // Assert
            cellCase1.Should().NotBeNull();
            cellCase2.Should().NotBeNull();
        }
    }
}
