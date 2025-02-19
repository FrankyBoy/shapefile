﻿/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile.Shapes
{
    /// <summary>
    /// A Shapefile ShapePolyLineM Shape
    /// </summary>
    public class ShapePolyLineM : ShapePolyLine
    {
        /// <summary>
        /// A Shapefile PolyLine Shape
        /// </summary>
        /// <param name="recordNumber">The record number in the Shapefile</param>
        /// <param name="metadata">Metadata about the shape</param>
        /// <param name="dataRecord">IDataRecord associated with the metadata</param>
        /// <param name="shapeData">The shape record as a byte array</param>
        /// <exception cref="ArgumentNullException">Thrown if shapeData is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs parsing shapeData</exception>
        protected internal ShapePolyLineM(Dictionary<string, string> metadata, byte[] shapeData)
            : base(metadata, shapeData, false)
        {
            Type = ShapeType.PolyLineM;
            M = [];

            Parts = ParsePolyLineM(shapeData);
        }

        public double Mmin { get; protected set; }
        public double Mmax { get; protected set; }

        /// <summary>
        /// List of M values per point.
        /// 
        /// From the official documentation: The measures for each part in the 
        /// PolyLineM are stored end to end. The measures for part 2 follow the 
        /// measures for part 1, and so on. The parts array holds the array index 
        /// of the starting point for each part. There is no delimiter in the 
        /// measure array between parts.
        /// </summary>
        public List<double> M { get; protected set; }

        /// <summary>        
        /// Function is basically the same as Shape.ParsePolyLineOrPolygon, it is just
        /// extended to handle the M extreme values
        /// </summary>
        /// <param name="shapeData">The shape record as a byte array</param>
        /// <param name="boundingBox">Returns the bounding box</param>
        /// <param name="parts">Returns the list of parts</param>
        private List<Point[]> ParsePolyLineM(byte[] shapeData)
        {
            // metadata is validated by the base class
            if (shapeData == null)
            {
                throw new ArgumentNullException("shapeData");
            }

            // Note, shapeData includes an 8 byte header so positions below are +8
            // Position     Field       Value       Type        Number      Order
            // Byte 0       Shape Type  23          Integer     1           Little
            // Byte 4       Box         Box         Double      4           Little
            // Byte 36      NumParts    NumParts    Integer     1           Little
            // Byte 40      NumPoints   NumPoints   Integer     1           Little
            // Byte 44      Parts       Parts       Integer     NumParts    Little
            // Byte X       Points      Points      Point       NumPoints   Little
            // Byte Y*      Mmin        Mmin        Double      1           Little
            // Byte Y + 8*  Mmax        Mmax        Double      1           Little
            // Byte Y + 16* Marray      Marray      Double      NumPoints   Little

            //
            // *optional

            // validation step 1 - must have at least 8 + 4 + (4*8) + 4 + 4 bytes = 52
            if (shapeData.Length < 44)
            {
                throw new InvalidOperationException("Invalid shape data");
            }

            // extract bounding box, number of parts and number of points
            int numParts = EndianBitConverter.ToInt32(shapeData, 44, ByteOrder.Little);
            int numPoints = EndianBitConverter.ToInt32(shapeData, 48, ByteOrder.Little);

            // validation step 2 - we're expecting 4 * numParts + (16 + 8 * numPoints for m extremes and values) + 16 * numPoints + 52 bytes total
            if (shapeData.Length != 52 + 4 * numParts + 16 + 8 * numPoints + 16 * numPoints)
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

                    // remove bytes for M extreme block
                    numBytes -= numPoints * 8 + 16;
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

            // parse M information
            Mmin = EndianBitConverter.ToDouble(shapeData, 52 + 4 * numParts + 16 * numPoints, ByteOrder.Little);
            Mmax = EndianBitConverter.ToDouble(shapeData, 52 + 8 + 4 * numParts + 16 * numPoints, ByteOrder.Little);

            M.Clear();
            for (int i = 0; i < numPoints; i++)
            {
                double _m = EndianBitConverter.ToDouble(shapeData, 52 + 16 + 4 * numParts + 16 * numPoints + i * 8, ByteOrder.Little);
                M.Add(_m);
            }

            return parts;
        }
    }
}
