/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile.Shapes
{
    /// <summary>
    /// Base Shapefile shape - contains only the shape type and metadata plus helper methods.
    /// An instance of Shape is the Null ShapeType. If the Type field is not ShapeType.Null then
    /// cast to the more specific shape (i.e. ShapePolygon).
    /// </summary>
    public class Shape
    {
        private Dictionary<string, string> _metadata;

        public int RecordNumber { get; }
        public ShapeType Type { get; protected set; }

        protected internal Shape(ShapeType shapeType, Dictionary<string, string> metadata, byte[] shapeData)
        {
            _metadata = metadata;
            Type = shapeType;
            RecordNumber = EndianBitConverter.ToInt32(shapeData, 0, ByteOrder.Big);
        }

        /// <summary>
        /// Gets the metadata (as a string) for a given name (key). Valid names
        /// for this shape can be retrieved by calling GetMetadataNames().
        /// </summary>
        /// <param name="name">The name to retreieve</param>
        /// <returns>The metadata string, or null if the requested name does not exist</returns>
        public string? GetMetadata(string name)
        {
            return _metadata?.GetValueOrDefault(name);
        }

        /// <summary>
        /// Gets an array of valid metadata names (keys) for this shape. Returns
        /// empty if not metadata exists.
        /// </summary>
        /// <returns>Array of metadata names, or empty of no metadata exists</returns>
        public string[] GetMetadataNames()
        {
            return _metadata?.Keys.ToArray() ?? [];
        }

        /// <summary>
        /// Extracts a double precision rectangle (RectangleD) from a byte array - assumes that
        /// the called has validated that there is enough space in the byte array for four
        /// doubles (32 bytes)
        /// </summary>
        /// <param name="value">byte array</param>
        /// <param name="startIndex">start index in the array</param>
        /// <param name="order">byte order of the doubles to be extracted</param>
        /// <returns>The RectangleD</returns>
        protected Rectangle ParseBoundingBox(byte[] value, int startIndex)
        {
            return new Rectangle(EndianBitConverter.ToDouble(value, startIndex, ByteOrder.Little),
                EndianBitConverter.ToDouble(value, startIndex + 8, ByteOrder.Little),
                EndianBitConverter.ToDouble(value, startIndex + 16, ByteOrder.Little),
                EndianBitConverter.ToDouble(value, startIndex + 24, ByteOrder.Little));
        }

        /// <summary>
        /// The PolyLine and Polygon shapes share the same structure, this method parses the bounding box
        /// and list of parts for both
        /// </summary>
        /// <param name="shapeData">The shape record as a byte array</param>
        /// <param name="boundingBox">Returns the bounding box</param>
        /// <param name="parts">Returns the list of parts</param>
        protected List<Point[]> ParsePolyLineOrPolygon(byte[] shapeData)
        {
            // metadata is validated by the base class
            if (shapeData == null)
            {
                throw new ArgumentNullException("shapeData");
            }

            // Note, shapeData includes an 8 byte header so positions below are +8
            // Position     Field       Value       Type        Number      Order
            // Byte 0       Shape Type  3 or 5      Integer     1           Little
            // Byte 4       Box         Box         Double      4           Little
            // Byte 36      NumParts    NumParts    Integer     1           Little
            // Byte 40      NumPoints   NumPoints   Integer     1           Little
            // Byte 44      Parts       Parts       Integer     NumParts    Little
            // Byte X       Points      Points      Point       NumPoints   Little
            //
            // Note: X = 44 + 4 * NumParts

            // validation step 1 - must have at least 8 + 4 + (4*8) + 4 + 4 bytes = 52
            if (shapeData.Length < 44)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            // extract bounding box, number of parts and number of points
            int numParts = EndianBitConverter.ToInt32(shapeData, 44, ByteOrder.Little);
            int numPoints = EndianBitConverter.ToInt32(shapeData, 48, ByteOrder.Little);

            // validation step 2 - we're expecting 4 * numParts + 16 * numPoints + 52 bytes total
            if (shapeData.Length != 52 + 4 * numParts + 16 * numPoints)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            // now extract the parts
            int partsOffset = 52 + 4 * numParts;
            var parts = new List<Point[]>(numParts);
            for (int part = 0; part < numParts; part++)
            {
                // this is the index of the start of the part in the points array
                int startPart = EndianBitConverter.ToInt32(shapeData, 52 + 4 * part, ByteOrder.Little) * 16 + partsOffset;

                int numBytes;
                if (part == numParts - 1)
                {
                    // it's the last part so we go to the end of the point array
                    numBytes = shapeData.Length - startPart;
                }
                else
                {
                    // we need to get the next part
                    int nextPart = EndianBitConverter.ToInt32(shapeData, 52 + 4 * (part + 1), ByteOrder.Little) * 16 + partsOffset;
                    numBytes = nextPart - startPart;
                }

                // the number of 16-byte points to read for this segment
                int numPointsInPart = numBytes / 16;

                Point[] points = new Point[numPointsInPart];
                for (int point = 0; point < numPointsInPart; point++)
                {
                    points[point] = new Point(EndianBitConverter.ToDouble(shapeData, startPart + 16 * point, ByteOrder.Little),
                        EndianBitConverter.ToDouble(shapeData, startPart + 8 + 16 * point, ByteOrder.Little));
                }

                parts.Add(points);
            }
            return parts;
        }
    }
}
