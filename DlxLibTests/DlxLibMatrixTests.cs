﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using DlxLib;

namespace DlxLibTests
{
    [TestFixture]
    public partial class DlxLibMatrixTests
    {
        #region basic object creation and adding to rows/columns
        [Test]
        public void RootProperties()
        {
            var sutRoot = new RootObject();
            Assert.That(sutRoot.Root, Is.EqualTo(sutRoot));
            Assert.That(sutRoot.Kind, Is.EqualTo("Root"));
            Assert.That(sutRoot.ColumnHeader, Is.EqualTo(sutRoot));
            Assert.Throws<NotImplementedException>(() => { var _ = sutRoot.ColumnCover; });
            Assert.That(sutRoot.RowIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x0]"));

            Assert.DoesNotThrow(() => sutRoot.ValidateRowIndexAvailable(sutRoot, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRoot.ValidateRowIndexAvailable(sutRoot, 0));
            Assert.DoesNotThrow(() => sutRoot.ValidateColumnIndexAvailable(sutRoot, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRoot.ValidateColumnIndexAvailable(sutRoot, 0));
        }

        [Test]
        public void RowProperties()
        {
            var root = new RootObject();
            var sutRow = new RowObject(root, 0);
            (root as IColumn).Append(sutRow);

            Assert.That(sutRow.Root, Is.EqualTo(root));
            Assert.That(sutRow.ColumnHeader, Is.EqualTo(root));
            Assert.That(sutRow.Kind, Is.EqualTo("Row"));
            Assert.That(sutRow.RowIndex, Is.EqualTo(0));
            Assert.That(sutRow.ColumnIndex, Is.EqualTo(-1));
            Assert.That(sutRow.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRow.ToString(), Is.EqualTo("Row[0]"));

            Assert.DoesNotThrow(() => sutRow.ValidateRowIndexAvailable(root, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRow.ValidateRowIndexAvailable(root, 0));
            Assert.DoesNotThrow(() => sutRow.ValidateColumnIndexAvailable(root, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sutRow.ValidateColumnIndexAvailable(root, 0));
        }

        [Test]
        public void PrimaryColumnProperties()
        {
            var root = new RootObject();
            var sutColumn = new ColumnObject(root, 0, ColumnCover.Primary); // TODO: Test Secondary
            (root as IRow).Append(sutColumn);

            Assert.That(sutColumn.Root, Is.EqualTo(root));
            Assert.That(sutColumn.Kind, Is.EqualTo("Column"));
            Assert.That(sutColumn.ColumnHeader, Is.EqualTo(sutColumn));
            Assert.That(sutColumn.RowIndex, Is.EqualTo(-1));
            Assert.That(sutColumn.ColumnIndex, Is.EqualTo(0));
            Assert.That(sutColumn.ColumnCover, Is.EqualTo(ColumnCover.Primary));
            Assert.That(sutColumn.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutColumn.ToString(), Is.EqualTo("Column[0,Primary]"));
        }

        [Test]
        public void ElementProperties()
        {
            const int colIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutColumn5 = sutRoot.GetColumn(colIndex);

            var sutElement = new ElementObject(sutRoot, sutColumn5, 0, colIndex);
            Assert.That(sutElement, Is.Not.Null);
            Assert.That(sutElement.Root, Is.EqualTo(sutRoot));
            Assert.That(sutElement.ColumnHeader, Is.EqualTo(sutColumn5));
            Assert.That(sutElement.RowIndex, Is.EqualTo(0));
            Assert.That(sutElement.ColumnIndex, Is.EqualTo(5));
            Assert.That(sutElement.Kind, Is.EqualTo("Element"));
            Assert.That(sutElement.ToString(), Is.EqualTo("Element[0,5]"));

        }

        [Test]
        public void BareColumnAndRowCreation()
        {
            var sutRoot = new RootObject();
            var sutColumn = new ColumnObject(sutRoot, 0, ColumnCover.Primary);
            var sutRow = new RowObject(sutRoot, 0);

            ValidateColumn(sutColumn, 0, ColumnCover.Primary);
            ValidateRow(sutRow, 0);
        }


        [Test]
        public void CreateEmpty0x0Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(0, 0);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x0]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(0)); // no rows
            Assert.That(sut.Item3.Length, Is.EqualTo(0)); // no columns

            Assert.That(sutRoot.Elements, Is.Empty);

            Assert.Throws<IndexOutOfRangeException>(() => { (sutRoot as IRoot).GetRow(0); });
            Assert.Throws<IndexOutOfRangeException>(() => { (sutRoot as IRoot).GetColumn(0); });
        }

        [Test]
        public void CreateEmpty0x3Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(0, 3);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(0));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(3));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[0x3]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(0)); // no rows
            Assert.That(sut.Item3.Length, Is.EqualTo(3)); // 3 columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, ColumnObject, object>(sut.Item3, (n, col) => { ValidateColumn(col, n, ColumnCover.Primary); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(0); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(3); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetColumn(i), Is.EqualTo(sut.Item3[i]));
            }

        }

        [Test]
        public void CreateEmpty3x0Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 0);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(3));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(0));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[3x0]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(3)); // 3 rows
            Assert.That(sut.Item3.Length, Is.EqualTo(0)); // no columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, RowObject, object>(sut.Item2, (n, row) => { ValidateRow(row, n); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(3); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(0); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetRow(i), Is.EqualTo(sut.Item2[i]));
            }
        }

        [Test]
        public void CreateEmpty3x3Matrix()
        {
            var sut = RootObject.CreateEmptyMatrix(3, 3);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Item1, Is.Not.Null);
            Assert.That(sut.Item2, Is.Not.Null);
            Assert.That(sut.Item3, Is.Not.Null);

            var sutRoot = sut.Item1;
            Assert.That(sutRoot.NumberOfRows, Is.EqualTo(3));
            Assert.That(sutRoot.NumberOfColumns, Is.EqualTo(3));
            Assert.That(sutRoot.ToString(), Is.EqualTo("Root[3x3]"));

            Assert.That(sut.Item2.Length, Is.EqualTo(3)); // 3 rows
            Assert.That(sut.Item3.Length, Is.EqualTo(3)); // 3 columns

            Assert.That(sutRoot.Elements, Is.EquivalentTo(sut.Item2.Cast<DataObject>().Concat(sut.Item3.Cast<DataObject>())));

            Enumerable.Range(0, 3).Zip<int, RowObject, object>(sut.Item2, (n, row) => { ValidateRow(row, n); return null; }).Last();
            Enumerable.Range(0, 3).Zip<int, ColumnObject, object>(sut.Item3, (n, col) => { ValidateColumn(col, n, ColumnCover.Primary); return null; }).Last();

            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetRow(3); });
            Assert.Throws<IndexOutOfRangeException>(() => { sutRoot.GetColumn(3); });

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetRow(i), Is.EqualTo(sut.Item2[i]));
            }

            for (int i = 0; i < 3; i++)
            {
                Assert.That(sutRoot.GetColumn(i), Is.EqualTo(sut.Item3[i]));
            }
        }

        [Test]
        public void BareColumnCreation()
        {
            const int colIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutColumn5 = sutRoot.GetColumn(colIndex);

            Assert.That(sutColumn5, Is.Not.Null);
            Assert.That(sutColumn5.ColumnIndex, Is.EqualTo(colIndex));

            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary);

            var elt1 = new ElementObject(sutRoot, sutColumn5, 0, colIndex);
            sutColumn5.Append(elt1);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1);

            var elt2 = new ElementObject(sutRoot, sutColumn5, 2, colIndex);
            sutColumn5.Append(elt2);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2);

            var elt3 = new ElementObject(sutRoot, sutColumn5, 4, colIndex);
            sutColumn5.Append(elt3);
            ValidateColumn(sutColumn5, colIndex, ColumnCover.Primary, elt1, elt2, elt3);
        }

        [Test]
        public void BareRowCreation()
        {
            const int rowIndex = 5;

            var sut = RootObject.CreateEmptyMatrix(7, 7);
            var sutRoot = sut.Item1;
            var sutRow5 = sutRoot.GetRow(rowIndex);
            var sutColumns = sut.Item3;

            Assert.That(sutRow5, Is.Not.Null);
            Assert.That(sutRow5.RowIndex, Is.EqualTo(rowIndex));

            ValidateRow(sutRow5, rowIndex);

            var elt1 = new ElementObject(sutRoot, sutColumns[0], rowIndex, 0);
            sutRow5.Append(elt1);
            ValidateRow(sutRow5, rowIndex, elt1);
            var elt2 = new ElementObject(sutRoot, sutColumns[2], rowIndex, 2);
            sutRow5.Append(elt2);
            ValidateRow(sutRow5, rowIndex, elt1, elt2);
            var elt3 = new ElementObject(sutRoot, sutColumns[4], rowIndex, 4);
            sutRow5.Append(elt3);
            ValidateRow(sutRow5, rowIndex, elt1, elt2, elt3);
        }

        #endregion
    }
}
