/* ------------------------------------------------------------------------
 * (c)copyright 2009-2019 Robert Ellison and contributors - https://github.com/abfo/shapefile
 * Provided under the ms-PL license, see LICENSE.txt
 * ------------------------------------------------------------------------ */

namespace Catfood.Shapefile
{
    /// <summary>
    /// A simple double precision point
    /// </summary>
    public readonly struct Point(double x, double y)
    {
        public double X { get; } = x;
        public double Y { get; } = y;
    }
}
