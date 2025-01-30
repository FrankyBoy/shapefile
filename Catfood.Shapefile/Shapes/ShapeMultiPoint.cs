/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile.Shapes
{
    public class ShapeMultiPoint : Shape
    {
        protected internal ShapeMultiPoint(Dictionary<string, string> metadata, byte[] shapeData)
            : base(ShapeType.MultiPoint, metadata, shapeData)
        {
            ArgumentNullException.ThrowIfNull(shapeData);

            // Note, shapeData includes an 8 byte header so positions below are +8
            // Position     Field       Value       Type        Number      Order
            // Byte 0       Shape Type  8           Integer     1           Little
            // Byte 4       Box         Box         Double      4           Little
            // Byte 36      NumPoints   Num Points  Integer     1           Little
            // Byte 40      Points      Points      Point       NumPoints   Little

            // validation step 1 - must have at least 8 + 4 + (4*8) + 4 bytes = 48
            if (shapeData.Length < 48)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            // extract bounding box and points
            BoundingBox = ParseBoundingBox(shapeData, 12);
            int numPoints = EndianBitConverter.ToInt32(shapeData, 44, ByteOrder.Little);

            // validation step 2 - we're expecting 16 * numPoints + 48 bytes total
            if (shapeData.Length != 48 + 16 * numPoints)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            // now extract the points
            Points = new Point[numPoints];
            for (int pointNum = 0; pointNum < numPoints; pointNum++)
            {
                Points[pointNum] = new Point(EndianBitConverter.ToDouble(shapeData, 48 + 16 * pointNum, ByteOrder.Little),
                    EndianBitConverter.ToDouble(shapeData, 56 + 16 * pointNum, ByteOrder.Little));
            }
        }

        /// <summary>
        /// Gets the bounding box
        /// </summary>
        public Rectangle BoundingBox { get; }

        /// <summary>
        /// Gets the array of points
        /// </summary>
        public Point[] Points { get; }
    }
}
