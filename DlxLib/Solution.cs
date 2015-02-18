﻿using System.Collections.Generic;
using System.Linq;

namespace DlxLib
{
    /// <summary>
    /// Represents a single solution to an exact cover problem.
    /// </summary>
    public class Solution
    {
        internal Solution(IEnumerable<int> rowIndexes)
        {
            _rowIndexes = rowIndexes.OrderBy(rowIndex => rowIndex);
        }

        /// <summary>
        /// The indexes of the set of rows, in the original matrix, that form a solution.
        /// The indexes are always sorted in ascending order.
        /// </summary>
        public IEnumerable<int> RowIndexes
        {
            get { return _rowIndexes; }
        }

        private readonly IEnumerable<int> _rowIndexes;
    }
}