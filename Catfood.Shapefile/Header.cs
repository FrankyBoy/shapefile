/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Catfood.Shapefile.Tests")]
namespace Catfood.Shapefile
{
    public enum ShapeType
    {
        Null = 0,
        Point = 1,
        PolyLine = 3,
        Polygon = 5,
        MultiPoint = 8,
        PointZ = 11,
        PolyLineZ = 13,
        PolygonZ = 15,
        MultiPointZ = 18,
        PointM = 21,
        PolyLineM = 23,
        PolygonM = 25,
        MultiPointM = 28,
        MultiPatch = 31
    }

    /// <summary>
    /// The header data for a Shapefile main file or Index file
    /// </summary>
    internal class Header
    {
        public const int HeaderLength = 100;

        private const int ExpectedFileCode = 9994;
        private const int ExpectedVersion = 1000;

        /// <summary>
        /// The header data for a Shapefile main file or Index file
        /// </summary>
        /// <param name="headerBytes">The first 100 bytes of the Shapefile main file or Index file</param>
        /// <exception cref="ArgumentNullException">Thrown if headerBytes is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs parsing the header</exception>
        public Header(byte[] headerBytes)
        {
            if (headerBytes == null)
            {
                throw new ArgumentNullException("headerBytes");
            }

            if (headerBytes.Length != HeaderLength)
            {
                throw new InvalidOperationException(string.Format("headerBytes must be {0} bytes long",
                    HeaderLength));
            }

            //Position  Field           Value       Type        Order
            //Byte 0    File Code       9994        Integer     Big
            //Byte 4    Unused          0           Integer     Big
            //Byte 8    Unused          0           Integer     Big
            //Byte 12   Unused          0           Integer     Big
            //Byte 16   Unused          0           Integer     Big
            //Byte 20   Unused          0           Integer     Big
            //Byte 24   File Length     File Length Integer     Big
            //Byte 28   Version         1000        Integer     Little
            //Byte 32   Shape Type      Shape Type  Integer     Little
            //Byte 36   Bounding Box    Xmin        Double      Little
            //Byte 44   Bounding Box    Ymin        Double      Little
            //Byte 52   Bounding Box    Xmax        Double      Little
            //Byte 60   Bounding Box    Ymax        Double      Little
            //Byte 68*  Bounding Box    Zmin        Double      Little
            //Byte 76*  Bounding Box    Zmax        Double      Little
            //Byte 84*  Bounding Box    Mmin        Double      Little
            //Byte 92*  Bounding Box    Mmax        Double      Little

            FileCode = EndianBitConverter.ToInt32(headerBytes, 0, ByteOrder.Big);
            if (FileCode != ExpectedFileCode)
            {
                throw new InvalidOperationException(string.Format("Header File code is {0}, expected {1}",
                    FileCode,
                    ExpectedFileCode));
            }

            Version = EndianBitConverter.ToInt32(headerBytes, 28, ByteOrder.Little);
            if (Version != ExpectedVersion)
            {
                throw new InvalidOperationException(string.Format("Header version is {0}, expected {1}",
                    Version,
                    ExpectedVersion));
            }

            FileLength = EndianBitConverter.ToInt32(headerBytes, 24, ByteOrder.Big);
            ShapeType = (ShapeType)EndianBitConverter.ToInt32(headerBytes, 32, ByteOrder.Little);
            XMin = EndianBitConverter.ToDouble(headerBytes, 36, ByteOrder.Little);
            YMin = EndianBitConverter.ToDouble(headerBytes, 44, ByteOrder.Little);
            XMax = EndianBitConverter.ToDouble(headerBytes, 52, ByteOrder.Little);
            YMax = EndianBitConverter.ToDouble(headerBytes, 60, ByteOrder.Little);
            ZMin = EndianBitConverter.ToDouble(headerBytes, 68, ByteOrder.Little);
            ZMax = EndianBitConverter.ToDouble(headerBytes, 76, ByteOrder.Little);
            MMin = EndianBitConverter.ToDouble(headerBytes, 84, ByteOrder.Little);
            MMax = EndianBitConverter.ToDouble(headerBytes, 92, ByteOrder.Little);
        }

        public int FileCode { get; }
        public int FileLength { get; }
        public int Version { get; }
        public ShapeType ShapeType { get; }
        public double XMin { get; }
        public double YMin { get; }
        public double XMax { get; }
        public double YMax { get; }
        public double ZMin { get; }
        public double ZMax { get; }
        public double MMin { get; }
        public double MMax { get; }
    }
}
