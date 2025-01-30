/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile.Shapes
{
    public class ShapePolygon : Shape
    {
        protected internal ShapePolygon(Dictionary<string, string> metadata, byte[] shapeData)
            : base(ShapeType.Polygon, metadata, shapeData)
        {
            BoundingBox = ParseBoundingBox(shapeData, 12);
            Parts = ParsePolyLineOrPolygon(shapeData);
        }

        /// <summary>
        /// Gets the bounding box
        /// </summary>
        public Rectangle BoundingBox { get; }

        /// <summary>
        /// Gets a list of parts (segments) for the PolyLine. Each part
        /// is an array of double precision points
        /// </summary>
        public List<Point[]> Parts { get; }
    }
}
