/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile.Shapes
{
    public class ShapePoint : Shape
    {
        protected internal ShapePoint(Dictionary<string, string> metadata, byte[] shapeData)
            : base(ShapeType.Point, metadata, shapeData)
        {
            ArgumentNullException.ThrowIfNull(shapeData);

            // Note, shapeData includes an 8 byte header so positions below are +8
            // Position     Field       Value   Type        Number  Order
            // Byte 0       Shape Type  1       Integer     1       Little
            // Byte 4       X           X       Double      1       Little
            // Byte 12      Y           Y       Double      1       Little

            // validation - shapedata should be 8 + 4 + 8 + 8 = 28 bytes long
            if (shapeData.Length != 28)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            Point = new Point(EndianBitConverter.ToDouble(shapeData, 12, ByteOrder.Little),
                EndianBitConverter.ToDouble(shapeData, 20, ByteOrder.Little));
        }

        /// <summary>
        /// Gets the point
        /// </summary>
        public Point Point { get; }
    }
}
