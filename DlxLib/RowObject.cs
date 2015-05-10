﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// PLACEHOLDER
    /// </summary>
    internal class RowObject : HeaderObject, IRow
    {
        public RowObject(RootObject root, int rowIndex)
            : base(root, root, rowIndex, -1)
        {
        }

    #region IRow Members

        public int NumberOfColumns
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<DataObject> Elements
        {
            get
            {
                for (var element = Right; this != element; element = element.Right)
                    yield return element;
            }
        }

        /// <summary>
        /// Returns largest columnIndex of row elements (-1 if row has no elements)
        /// </summary>
        public int HighestColumnInRow
        {
            get
            {
                return Left.ColumnIndex;
            }
        }

        public void Append(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        #endregion

        protected internal override void ValidateRowIndexAvailable(RootObject root, int rowIndex)
        {
            var maxRow = root.HighestRow;
            if (maxRow >= rowIndex)
                throw new ArgumentOutOfRangeException("rowIndex", "Row index too low");
        }

        protected internal override void ValidateColumnIndexAvailable(RootObject root, int columnIndex)
        {
            if (-1 != columnIndex)
                throw new ArgumentOutOfRangeException("columnIndex", "Must be -1");
        }

        public override string Kind
        {
            get
            {
                return "Row";
            }
        }
    }
}

