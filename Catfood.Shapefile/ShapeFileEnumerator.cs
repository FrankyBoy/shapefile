/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using DbfDataReader;

namespace Catfood.Shapefile
{
    class ShapeFileEnumerator : IEnumerator<Shape>
    {
        private readonly FileStream _mainStream;
        private int _currentIndex = -1;
        private readonly FileStream _indexStream;
        private readonly DbfTable _dbfTable;
        private DbfRecord _dbfRecord;

        public ShapeFileEnumerator(DbfTable dbfTable, FileStream mainStream,
                                   FileStream indexStream)
        {
            _mainStream = mainStream;
            _indexStream = indexStream;
            _dbfTable = dbfTable;
            _dbfRecord = new DbfRecord(_dbfTable);
        }


        #region IEnumerator<Shape> Members

        /// <summary>
        /// Gets the current shape in the collection
        /// </summary>
        public Shape Current
        {
            get
            {
                // get the metadata
                Dictionary<string, string> metadata = new Dictionary<string, string>();
                metadata = _dbfTable.Columns.ToDictionary(
                    x => x.ColumnName.ToLowerInvariant(),
                    x => _dbfRecord.GetStringValue(x.ColumnOrdinal ?? 0));

                // get the index record
                byte[] indexHeaderBytes = new byte[8];
                _indexStream.Seek(Header.HeaderLength + _currentIndex * 8, SeekOrigin.Begin);
                _indexStream.Read(indexHeaderBytes, 0, indexHeaderBytes.Length);
                int contentOffsetInWords = EndianBitConverter.ToInt32(indexHeaderBytes, 0, ProvidedOrder.Big);
                int contentLengthInWords = EndianBitConverter.ToInt32(indexHeaderBytes, 4, ProvidedOrder.Big);

                // get the data chunk from the main file - need to factor in 8 byte record header
                int bytesToRead = (contentLengthInWords * 2) + 8;
                byte[] shapeData = new byte[bytesToRead];
                _mainStream.Seek(contentOffsetInWords * 2, SeekOrigin.Begin);
                _mainStream.Read(shapeData, 0, bytesToRead);

                return ShapeFactory.ParseShape(shapeData, metadata);
            }
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Gets the current item in the collection
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            _dbfTable.Dispose();
        }

        /// <summary>
        /// Move to the next item in the collection (returns false if at the end)
        /// </summary>
        /// <returns>false if there are no more items in the collection</returns>
        public bool MoveNext()
        {
            _currentIndex++;
            return _dbfTable.Read(_dbfRecord);
        }

        public void Reset()
        {
        }

        #endregion
    }
}
