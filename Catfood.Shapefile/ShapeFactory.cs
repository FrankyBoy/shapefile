/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

using Catfood.Shapefile.Shapes;

namespace Catfood.Shapefile
{
    /// <summary>
    /// Static factory class to create shape objects from a shape record
    /// </summary>
    static class ShapeFactory
    {
        /// <summary>
        /// Creates a Shape object (or derived object) from a shape record
        /// </summary>
        /// <param name="shapeData">The shape record as a byte array</param>
        /// <param name="metadata">Metadata associated with this shape (optional)</param>
        /// <param name="dataRecord">IDataRecord associated with the metadata</param>
        /// <returns>A Shape, or derived class</returns>
        /// <exception cref="ArgumentNullException">Thrown if shapeData or metadata are null</exception>
        /// <exception cref="ArgumentException">Thrown if shapeData is less than 12 bytes long</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs parsing shapeData</exception>
        public static Shape ParseShape(byte[] shapeData, Dictionary<string, string> metadata)
        {
            if (shapeData == null)
            {
                throw new ArgumentNullException("shapeData");
            }

            if (shapeData.Length < 12)
            {
                throw new ArgumentException("shapeData must be at least 12 bytes long");
            }

            // shape data contains a header (shape number and content length)
            // the first field in each shape is the shape type

            //Position  Field           Value                   Type        Order
            //Byte 0    Record          Number Record Number    Integer     Big
            //Byte 4    Content Length  Content Length          Integer     Big

            //Position  Field       Value                   Type        Number      Order
            //Byte 0    Shape Type  Shape Type              Integer     1           Little

            int contentLengthInWords = EndianBitConverter.ToInt32(shapeData, 4, ByteOrder.Big);
            ShapeType shapeType = (ShapeType)EndianBitConverter.ToInt32(shapeData, 8, ByteOrder.Little);

            // test that we have the expected amount of data - need to take the 8 byte header into account
            if (shapeData.Length != (contentLengthInWords * 2) + 8)
            {
                throw new InvalidOperationException("Shape data length does not match shape header length");
            }

            Shape shape = null;

            switch (shapeType)
            {
                case ShapeType.Null:
                    shape = new Shape(shapeType, metadata, shapeData);
                    break;

                case ShapeType.Point:
                    shape = new ShapePoint(metadata, shapeData);
                    break;

                case ShapeType.MultiPoint:
                    shape = new ShapeMultiPoint(metadata, shapeData);
                    break;

                case ShapeType.PolyLine:
                    shape = new ShapePolyLine(metadata, shapeData);
                    break;

                case ShapeType.PolyLineM:
                    shape = new ShapePolyLineM(metadata, shapeData);
                    break;

                case ShapeType.Polygon:
                    shape = new ShapePolygon(metadata, shapeData);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Shapetype {0} is not implemented", shapeType));
            }

            return shape;
        }
    }
}
