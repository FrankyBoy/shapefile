/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using Catfood.Shapefile.Shapes;
using DbfDataReader;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;

namespace Catfood.Shapefile
{
    /// <summary>
    /// Provides a readonly IEnumerable interface to an ERSI Shapefile.
    /// NOTE - has not been designed to be thread safe
    /// </summary>
    /// <remarks>
    /// See the ESRI Shapefile specification at http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
    /// </remarks>
    public class Shapefile : IDisposable, IEnumerable<Shape>
    {
        private const string MainPathExtension = "shp";
        private const string IndexPathExtension = "shx";
        private const string DbasePathExtension = "dbf";

        private bool _disposed;
        private bool _opened;
        private string _shapefileMainPath;
        private string _shapefileIndexPath;
        private string _shapefileDbasePath;
        private FileStream _mainStream;
        private FileStream _indexStream;
        private Header _mainHeader;
        private Header _indexHeader;
        private DbfTable _dbfTable;

        public Shapefile(string path)
        {
            if (path != null)
            {
                Open(path);
            }
        }

        /// <summary>
        /// Create a new Shapefile object and open a Shapefile. Note that three files are required - 
        /// the main file (.shp), the index file (.shx) and the dBASE table (.dbf). The three files 
        /// must all have the same filename (i.e. shapes.shp, shapes.shx and shapes.dbf). Set path
        /// to any one of these three files to open the Shapefile.
        /// </summary>
        /// <param name="path">Path to the .shp, .shx or .dbf file for this Shapefile</param>
        /// <exception cref="ObjectDisposedException">Thrown if the Shapefile has been disposed</exception>
        /// <exception cref="ArgumentNullException">Thrown if the path parameter is null</exception>
        /// <exception cref="ArgumentException">Thrown if the path parameter is empty</exception>
        /// <exception cref="FileNotFoundException">Thrown if one of the three required files is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs parsing file headers</exception>
        public void Open(string path)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Shapefile");
            }

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length <= 0)
            {
                throw new ArgumentException("path parameter is empty", "path");
            }

            _shapefileMainPath = Path.ChangeExtension(path, MainPathExtension);
            _shapefileIndexPath = Path.ChangeExtension(path, IndexPathExtension);
            _shapefileDbasePath = Path.ChangeExtension(path, DbasePathExtension);

            if (!File.Exists(_shapefileMainPath))
            {
                throw new FileNotFoundException("Shapefile main file not found", _shapefileMainPath);
            }
            if (!File.Exists(_shapefileIndexPath))
            {
                throw new FileNotFoundException("Shapefile index file not found", _shapefileIndexPath);
            }
            if (!File.Exists(_shapefileDbasePath))
            {
                throw new FileNotFoundException("Shapefile dBase file not found", _shapefileDbasePath);
            }

            _mainStream = File.Open(_shapefileMainPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _indexStream = File.Open(_shapefileIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (_mainStream.Length < Header.HeaderLength)
            {
                throw new InvalidOperationException("Shapefile main file does not contain a valid header");
            }

            if (_indexStream.Length < Header.HeaderLength)
            {
                throw new InvalidOperationException("Shapefile index file does not contain a valid header");
            }

            // read in and parse the headers
            byte[] headerBytes = new byte[Header.HeaderLength];
            _mainStream.Read(headerBytes, 0, Header.HeaderLength);
            _mainHeader = new Header(headerBytes);
            _indexStream.Read(headerBytes, 0, Header.HeaderLength);
            _indexHeader = new Header(headerBytes);

            // set properties from the main header
            Type = _mainHeader.ShapeType;
            BoundingBox = new Rectangle(_mainHeader.XMin, _mainHeader.YMin, _mainHeader.XMax, _mainHeader.YMax);

            // index header length is in 16-bit words, including the header - number of 
            // shapes is the number of records (each 4 workds long) after subtracting the header bytes
            Count = (_indexHeader.FileLength - (Header.HeaderLength / 2)) / 4;

            // open the metadata database
            OpenDb();

            _opened = true;
        }

        /// <summary>
        /// Close the Shapefile. Equivalent to calling Dispose().
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the number of shapes in the Shapefile
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the bounding box for the Shapefile
        /// </summary>
        public Rectangle BoundingBox { get; private set; }

        /// <summary>
        /// Gets the ShapeType of the Shapefile
        /// </summary>
        public ShapeType Type { get; private set; }

        private void OpenDb()
        {
            _dbfTable = new DbfTable(_shapefileDbasePath);
        }

        private void CloseDb()
        {
            _dbfTable.Close();
        }

        #region IDisposable Members

        /// <summary />
        ~Shapefile()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose the Shapefile and free all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool canDisposeManagedResources)
        {
            if (!_disposed)
            {
                if (canDisposeManagedResources)
                {
                    if (_mainStream != null)
                    {
                        _mainStream.Close();
                        _mainStream = null;
                    }

                    if (_indexStream != null)
                    {
                        _indexStream.Close();
                        _indexStream = null;
                    }

                    CloseDb();
                }

                _disposed = true;
                _opened = false;
            }
        }


        /// <summary>
        /// Get the IEnumerator for this Shapefile
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator<Shape> GetEnumerator()
        {
            return new ShapeFileEnumerator(_dbfTable, _mainStream, _indexStream);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
