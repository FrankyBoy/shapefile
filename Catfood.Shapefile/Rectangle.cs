/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile
{
    /// <summary>
    /// A simple double precision rectangle
    /// </summary>
    public readonly struct Rectangle(double left, double top, double right, double bottom)
    {
        public double Left { get; } = left;
        public double Top { get; } = top;
        public double Right { get; } = right;
        public double Bottom { get; } = bottom;
    }
}
