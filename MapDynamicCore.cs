using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Grain.Map.Dynamic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

#if NETFX_CORE
	using System.Net.Http;
	using System.Threading.Tasks;
#endif
#if UNITY
	using UnityEngine.Networking;
#endif

namespace Grain.Map.Dynamic
{
    public enum TilePropertyState
    {
        None,
        Loading,
        Loaded,
        Error
    }

    public static class MapDynamicUtils
    {
        /// <summary>
        /// Normalizes a static style URL.
        /// </summary>
        /// <returns>The static style URL.</returns>
        /// <param name="url">A url, either a Mapbox URI (mapbox://{username}/{styleid}) or a full url to a map.</param>
        public static string NormalizeStaticStyleURL(string url)
        {
            bool isMapboxUrl = url.StartsWith("mapbox://", StringComparison.Ordinal);

            // Support full Mapbox URLs by returning here if a mapbox URL is not detected.
            if (!isMapboxUrl)
            {
                return url;
            }

            string[] split = url.Split('/');
            var user = split[3];
            var style = split[4];
            var draft = string.Empty;

            if (split.Length > 5)
            {
                draft = "/draft";
            }

            return Constants.BaseAPI + "styles/v1/" + user + "/" + style + draft + "/tiles";
        }

        /// <summary>
        /// Converts a MapId to a URL.
        /// </summary>
        /// <returns>The identifier to URL.</returns>
        /// <param name="id">The style id.</param>
        public static string MapIdToUrl(string id)
        {
            // TODO: Validate that id is a real id
            const string MapBaseApi = Constants.BaseAPI + "v4/";

            return MapBaseApi + id;
        }
    }

    public static class Conversions
    {
        private const int TileSize = 256;
        private const int EarthRadius = 6378137;
        private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

        /// <summary>
        /// Converts <see cref="T:Mapbox.Utils.Vector2d"/> struct, WGS84
        /// lat/lon to Spherical Mercator EPSG:900913 xy meters.
        /// </summary>
        /// <param name="v"> The <see cref="T:Mapbox.Utils.Vector2d"/>. </param>
        /// <returns> A <see cref="T:UnityEngine.Vector2d"/> of coordinates in meters. </returns>
        private static Vector2d LatLonToMeters(Vector2d v)
        {
            return LatLonToMeters(v.x, v.y);
        }

        /// <summary>
        /// Converts WGS84 lat/lon to Spherical Mercator EPSG:900913 xy meters.
        /// SOURCE: http://stackoverflow.com/questions/12896139/geographic-coordinates-converter.
        /// </summary>
        /// <param name="lat"> The latitude. </param>
        /// <param name="lon"> The longitude. </param>
        /// <returns> A <see cref="T:UnityEngine.Vector2d"/> of xy meters. </returns>
        private static Vector2d LatLonToMeters(double lat, double lon)
        {
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new Vector2d(posx, posy);
        }

        public static float Adjustment(int zoom)
        {
            float adj = 0.0f;

            switch(zoom)
            {
                case 18:
                    adj = 20.0f;
                    break;
                case 17:
                    adj = 40.0f;
                    break;
                case 16:
                    adj = 60.0f;
                    break;
                case 15:
                    adj = 80.0f;
                    break;
                case 14:
                    adj = 100.0f;
                    break;
                case 12:
                    adj = 120.0f;
                    break;
                case 11:
                    adj = 140.0f;
                    break;
                case 10:
                    adj = 160.0f;
                    break;
                case 19:
                case 20:
                case 21:
                case 22:
                    adj = 10.0f;
                    break;

            }

            return adj;
        }

        /// <summary>
        /// Converts WGS84 lat/lon to x/y meters in reference to a center point
        /// </summary>
        /// <param name="lat"> The latitude. </param>
        /// <param name="lon"> The longitude. </param>
        /// <param name="refPoint"> A <see cref="T:UnityEngine.Vector2d"/> center point to offset resultant xy</param>
        /// <param name="scale"> Scale in meters. (default scale = 1) </param>
        /// <returns> A <see cref="T:UnityEngine.Vector2d"/> xy tile ID. </returns>
        /// <example>
        /// Converts a Lat/Lon of (37.7749, 122.4194) into Unity coordinates for a map centered at (10,10) and a scale of 2.5 meters for every 1 Unity unit 
        /// <code>
        /// var worldPosition = Conversions.GeoToWorldPosition(37.7749, 122.4194, new Vector2d(10, 10), (float)2.5);
        /// // worldPosition = ( 11369163.38585, 34069138.17805 )
        /// </code>
        /// </example>
        public static Vector2d GeoToWorldPosition(double lat, double lon, Vector2d refPoint, float scale = 1)
        {
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new Vector2d((posx - refPoint.x) * scale, (posy - refPoint.y) * scale);
        }

        public static Vector2d GeoToWorldPosition(Vector2d latLong, Vector2d refPoint, float scale = 1)
        {
            return GeoToWorldPosition(latLong.x, latLong.y, refPoint, scale);
        }

        /// <summary>
        /// Converts Spherical Mercator EPSG:900913 in xy meters to WGS84 lat/lon.
        /// Inverse of LatLonToMeters.
        /// </summary>
        /// <param name="m"> A <see cref="T:UnityEngine.Vector2d"/> of coordinates in meters.  </param>
        /// <returns> The <see cref="T:Mapbox.Utils.Vector2d"/> in lat/lon. </returns>

        /// <example>
        /// Converts EPSG:900913 xy meter coordinates to lat lon 
        /// <code>
        /// var worldPosition =  new Vector2d (4547675.35434,13627665.27122);
        /// var latlon = Conversions.MetersToLatLon(worldPosition);
        /// // latlon = ( 37.77490, 122.41940 )
        /// </code>
        /// </example>
        public static Vector2d MetersToLatLon(Vector2d m)
        {
            var vx = (m.x / OriginShift) * 180;
            var vy = (m.y / OriginShift) * 180;
            vy = 180 / Math.PI * (2 * Math.Atan(Math.Exp(vy * Math.PI / 180)) - Math.PI / 2);
            return new Vector2d(vy, vx);
        }

        /// <summary>
        /// Gets the xy tile ID from Spherical Mercator EPSG:900913 xy coords.
        /// </summary>
        /// <param name="m"> <see cref="T:UnityEngine.Vector2d"/> XY coords in meters. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> A <see cref="T:UnityEngine.Vector2d"/> xy tile ID. </returns>
        /// 
        /// <example>
        /// Converts EPSG:900913 xy meter coordinates to web mercator tile XY coordinates at zoom 12.
        /// <code>
        /// var meterXYPosition = new Vector2d (4547675.35434,13627665.27122);
        /// var tileXY = Conversions.MetersToTile (meterXYPosition, 12);
        /// // tileXY = ( 655, 2512 )
        /// </code>
        /// </example>
        public static Vector2 MetersToTile(Vector2d m, int zoom)
        {
            var p = MetersToPixels(m, zoom);
            return PixelsToTile(p);
        }

        /// <summary>
        /// Gets the tile bounds in Spherical Mercator EPSG:900913 meters from an xy tile ID.
        /// </summary>
        /// <param name="tileCoordinate"> XY tile ID. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> A <see cref="T:UnityEngine.Rect"/> in meters. </returns>
        public static RectD TileBounds(Vector2 tileCoordinate, int zoom)
        {
            var min = PixelsToMeters(new Vector2d(tileCoordinate.x * TileSize, tileCoordinate.y * TileSize), zoom);
            var max = PixelsToMeters(new Vector2d((tileCoordinate.x + 1) * TileSize, (tileCoordinate.y + 1) * TileSize), zoom);
            return new RectD(min, max - min);
        }

        public static RectD TileBounds(UnwrappedTileId unwrappedTileId)
        {
            var min = PixelsToMeters(new Vector2d(unwrappedTileId.X * TileSize, unwrappedTileId.Y * TileSize), unwrappedTileId.Z);
            var max = PixelsToMeters(new Vector2d((unwrappedTileId.X + 1) * TileSize, (unwrappedTileId.Y + 1) * TileSize), unwrappedTileId.Z);
            return new RectD(min, max - min);
        }

        /// <summary>
        /// Gets the xy tile ID at the requested zoom that contains the WGS84 lat/lon point.
        /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="latitude"> The latitude. </param>
        /// <param name="longitude"> The longitude. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> A <see cref="T:UnityEngine.Vector2d"/> xy tile ID. </returns>
        public static Vector2d LatitudeLongitudeToTileId(float latitude, float longitude, int zoom)
        {
            var x = (int)Math.Floor((longitude + 180.0) / 360.0 * Math.Pow(2.0, zoom));
            var y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latitude * Math.PI / 180.0)
                    + 1.0 / Math.Cos(latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom));

            return new Vector2d(x, y);
        }

        /// <summary>
        /// Gets the WGS84 longitude of the northwest corner from a tile's X position and zoom level.
        /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="x"> Tile X position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> NW Longitude. </returns>
        public static double TileXToNWLongitude(int x, int zoom)
        {
            var n = Math.Pow(2.0, zoom);
            var lon_deg = x / n * 360.0 - 180.0;
            return lon_deg;
        }

        /// <summary>
        /// Gets the WGS84 latitude of the northwest corner from a tile's Y position and zoom level.
        /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
        /// </summary>
        /// <param name="y"> Tile Y position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> NW Latitude. </returns>
        public static double TileYToNWLatitude(int y, int zoom)
        {
            var n = Math.Pow(2.0, zoom);
            var lat_rad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
            var lat_deg = lat_rad * 180.0 / Math.PI;
            return lat_deg;
        }

        /// <summary>
        /// Gets the <see cref="T:Mapbox.Utils.Vector2dBounds"/> of a tile.
        /// </summary>
        /// <param name="x"> Tile X position. </param>
        /// <param name="y"> Tile Y position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> The <see cref="T:Mapbox.Utils.Vector2dBounds"/> of the tile. </returns>
        public static Vector2dBounds TileIdToBounds(int x, int y, int zoom)
        {
            var sw = new Vector2d(TileYToNWLatitude(y, zoom), TileXToNWLongitude(x + 1, zoom));
            var ne = new Vector2d(TileYToNWLatitude(y + 1, zoom), TileXToNWLongitude(x, zoom));
            return new Vector2dBounds(sw, ne);
        }

        /// <summary>
        /// Gets the WGS84 lat/lon of the center of a tile.
        /// </summary>
        /// <param name="x"> Tile X position. </param>
        /// <param name="y"> Tile Y position. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns>A <see cref="T:UnityEngine.Vector2d"/> of lat/lon coordinates.</returns>
        public static Vector2d TileIdToCenterLatitudeLongitude(int x, int y, int zoom)
        {
            var bb = TileIdToBounds(x, y, zoom);
            var center = bb.Center;
            return new Vector2d((float)center.x, (float)center.y);
        }

        /// <summary>
        /// Gets the meters per pixels at given latitude and zoom level for a 256x256 tile.
        /// See: http://wiki.openstreetmap.org/wiki/Zoom_levels.
        /// </summary>
        /// <param name="latitude"> The latitude. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> Meters per pixel. </returns>
        public static float GetTileScaleInMeters(float latitude, int zoom)
        {
            return 40075000 * Mathf.Cos(Mathf.Deg2Rad * latitude) / Mathf.Pow(2f, zoom + 8);
        }

        /// <summary>
        /// Gets height from terrain-rgb adjusted for a given scale.
        /// </summary>
        /// <param name="color"> The <see cref="T:UnityEngine.Color"/>. </param>
        /// <param name="relativeScale"> Relative scale. </param>
        /// <returns> Adjusted height in meters. </returns>
        public static float GetRelativeHeightFromColor(Color color, float relativeScale)
        {
            return GetAbsoluteHeightFromColor(color) * relativeScale;
        }

        /// <summary>
        /// Specific formula for mapbox.terrain-rgb to decode height values from pixel values.
        /// See: https://www.mapbox.com/blog/terrain-rgb/.
        /// </summary>
        /// <param name="color"> The <see cref="T:UnityEngine.Color"/>. </param>
        /// <returns> Height in meters. </returns>
        public static float GetAbsoluteHeightFromColor(Color color)
        {
            return (float)(-10000 + ((color.r * 255 * 256 * 256 + color.g * 255 * 256 + color.b * 255) * 0.1));
        }

        public static float GetAbsoluteHeightFromColor32(Color32 color)
        {
            return (float)(-10000 + ((color.r * 256 * 256 + color.g * 256 + color.b) * 0.1));
        }

        public static float GetAbsoluteHeightFromColor(float r, float g, float b)
        {
            return (float)(-10000 + ((r * 256 * 256 + g * 256 + b) * 0.1));
        }

        private static double Resolution(int zoom)
        {
            return InitialResolution / Math.Pow(2, zoom);
        }

        private static Vector2d PixelsToMeters(Vector2d p, int zoom)
        {
            var res = Resolution(zoom);
            var met = new Vector2d();
            met.x = (float)(p.x * res - OriginShift);
            met.y = (float)-(p.y * res - OriginShift);
            return met;
        }

        private static Vector2d MetersToPixels(Vector2d m, int zoom)
        {
            var res = Resolution(zoom);
            var pix = new Vector2d((float)((m.x + OriginShift) / res), (float)((-m.y + OriginShift) / res));
            return pix;
        }

        private static Vector2 PixelsToTile(Vector2d p)
        {
            var t = new Vector2((int)Math.Ceiling(p.x / (double)TileSize) - 1, (int)Math.Ceiling(p.y / (double)TileSize) - 1);
            return t;
        }
    }

    /// <summary>
	///     Helper funtions to get a tile cover, i.e. a set of tiles needed for
	///     covering a bounding box.
	/// </summary>
	public static class TileCover
    {
        /// <summary> Get a tile cover for the specified bounds and zoom. </summary>
        /// <param name="bounds"> Geographic bounding box.</param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns> The tile cover set. </returns>
        /// <example>
        /// Build a map of Colorado using TileCover:
        /// <code>
        /// var sw = new Vector2d(36.997749, -109.0524961);
        /// var ne = new Vector2d(41.0002612, -102.0609668);
        /// var coloradoBounds = new Vector2dBounds(sw, ne);
        /// var tileCover = TileCover.Get(coloradoBounds, 8);
        /// Console.Write("Tiles Needed: " + tileCover.Count);
        /// foreach (var id in tileCover)
        /// {
        /// 	var tile = new RasterTile();
        /// 	var parameters = new Tile.Parameters();
        /// 	parameters.Id = id;
        ///		parameters.Fs = MapboxAccess.Instance;
        ///		parameters.MapId = "mapbox://styles/mapbox/outdoors-v10";
        ///		tile.Initialize(parameters, (Action)(() =&gt;
        ///		{
        ///			// Place tiles and load textures.
        ///		}));
        ///	}
        /// </code>
        /// </example>
        public static HashSet<CanonicalTileId> Get(Vector2dBounds bounds, int zoom)
        {
            var tiles = new HashSet<CanonicalTileId>();

            if (bounds.IsEmpty() ||
                bounds.South > Constants.LatitudeMax ||
                bounds.North < -Constants.LatitudeMax)
            {
                return tiles;
            }

            var hull = Vector2dBounds.FromCoordinates(
                new Vector2d(Math.Max(bounds.South, -Constants.LatitudeMax), bounds.West),
                new Vector2d(Math.Min(bounds.North, Constants.LatitudeMax), bounds.East));

            var sw = CoordinateToTileId(hull.SouthWest, zoom);
            var ne = CoordinateToTileId(hull.NorthEast, zoom);

            // Scanlines.
            for (var x = sw.X; x <= ne.X; ++x)
            {
                for (var y = ne.Y; y <= sw.Y; ++y)
                {
                    tiles.Add(new UnwrappedTileId(zoom, x, y).Canonical);
                }
            }

            return tiles;
        }

        /// <summary> Converts a coordinate to a tile identifier. </summary>
        /// <param name="coord"> Geographic coordinate. </param>
        /// <param name="zoom"> Zoom level. </param>
        /// <returns>The to tile identifier.</returns>
        /// <example>
        /// Convert a geocoordinate to a TileId:
        /// <code>
        /// var unwrappedTileId = TileCover.CoordinateToTileId(new Vector2d(40.015, -105.2705), 18);
        /// Console.Write("UnwrappedTileId: " + unwrappedTileId.ToString());
        /// </code>
        /// </example>
        public static UnwrappedTileId CoordinateToTileId(Vector2d coord, int zoom)
        {
            var lat = coord.x;
            var lng = coord.y;

            // See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            var x = (int)Math.Floor((lng + 180.0) / 360.0 * Math.Pow(2.0, zoom));
            var y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0)
                    + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom));

            return new UnwrappedTileId(zoom, x, y);
        }
    }

    /// <summary> Collection of constants used across the project. </summary>
    public static class Constants
    {
        /// <summary> Base URL for all the Mapbox APIs. </summary>
        public const string BaseAPI = "https://api.mapbox.com/";

        public const string EventsAPI = "https://events.mapbox.com/";

        /// <summary> Mercator projection max latitude limit. </summary>
        public const double LatitudeMax = 85.0511;

        /// <summary> Mercator projection max longitude limit. </summary>
        public const double LongitudeMax = 180;
    }

    /// <summary>
	///     Unwrapped tile identifier in a slippy map. Similar to <see cref="CanonicalTileId"/>,
	///     but might go around the globe.
	/// </summary>
	public struct UnwrappedTileId
    {
        /// <summary> The zoom level. </summary>
        public readonly int Z;

        /// <summary> The X coordinate in the tile grid. </summary>
        public readonly int X;

        /// <summary> The Y coordinate in the tile grid. </summary>
        public readonly int Y;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnwrappedTileId"/> struct,
        ///     representing a tile coordinate in a slippy map that might go around the
        ///     globe.
        /// </summary>
        /// <param name="z">The z coordinate.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public UnwrappedTileId(int z, int x, int y)
        {
            this.Z = z;
            this.X = x;
            this.Y = y;
        }

        /// <summary> Gets the canonical tile identifier. </summary>
        /// <value> The canonical tile identifier. </value>
        public CanonicalTileId Canonical
        {
            get
            {
                return new CanonicalTileId(this);
            }
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.UnwrappedTileId"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.UnwrappedTileId"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Z + "/" + this.X + "/" + this.Y;
        }

        public UnwrappedTileId North
        {
            get
            {
                return new UnwrappedTileId(Z, X, Y - 1);
            }
        }

        public UnwrappedTileId East
        {
            get
            {
                return new UnwrappedTileId(Z, X + 1, Y);
            }
        }

        public UnwrappedTileId South
        {
            get
            {
                return new UnwrappedTileId(Z, X, Y + 1);
            }
        }

        public UnwrappedTileId West
        {
            get
            {
                return new UnwrappedTileId(Z, X - 1, Y);
            }
        }

        public UnwrappedTileId NorthEast
        {
            get
            {
                return new UnwrappedTileId(Z, X + 1, Y - 1);
            }
        }

        public UnwrappedTileId SouthEast
        {
            get
            {
                return new UnwrappedTileId(Z, X + 1, Y + 1);
            }
        }

        public UnwrappedTileId NorthWest
        {
            get
            {
                return new UnwrappedTileId(Z, X - 1, Y - 1);
            }
        }

        public UnwrappedTileId SouthWest
        {
            get
            {
                return new UnwrappedTileId(Z, X - 1, Y + 1);
            }
        }
    }

    /// <summary>
	/// Data type to store  <see href="https://en.wikipedia.org/wiki/Web_Mercator"> Web Mercator</see> tile scheme.
	/// <see href="http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/"> See tile IDs in action. </see>
	/// </summary>
	public struct CanonicalTileId
    {
        /// <summary> The zoom level. </summary>
        public readonly int Z;

        /// <summary> The X coordinate in the tile grid. </summary>
        public readonly int X;

        /// <summary> The Y coordinate in the tile grid. </summary>
        public readonly int Y;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CanonicalTileId"/> struct,
        ///     representing a tile coordinate in a slippy map.
        /// </summary>
        /// <param name="z"> The z coordinate or the zoom level. </param>
        /// <param name="x"> The x coordinate. </param>
        /// <param name="y"> The y coordinate. </param>
        public CanonicalTileId(int z, int x, int y)
        {
            this.Z = z;
            this.X = x;
            this.Y = y;
        }

        internal CanonicalTileId(UnwrappedTileId unwrapped)
        {
            var z = unwrapped.Z;
            var x = unwrapped.X;
            var y = unwrapped.Y;

            var wrap = (x < 0 ? x - (1 << z) + 1 : x) / (1 << z);

            this.Z = z;
            this.X = x - wrap * (1 << z);
            this.Y = y < 0 ? 0 : Math.Min(y, (1 << z) - 1);
        }

        /// <summary>
        ///     Get the cordinate at the top left of corner of the tile.
        /// </summary>
        /// <returns> The coordinate. </returns>
        public Vector2d ToVector2d()
        {
            double n = Math.PI - ((2.0 * Math.PI * this.Y) / Math.Pow(2.0, this.Z));

            double lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
            double lng = (this.X / Math.Pow(2.0, this.Z) * 360.0) - 180.0;

            // FIXME: Super hack because of rounding issues.
            return new Vector2d(lat - 0.0001, lng + 0.0001);
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.CanonicalTileId"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.CanonicalTileId"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Z + "/" + this.X + "/" + this.Y;
        }
    }

    /// <summary> Represents a bounding box derived from a southwest corner and a northeast corner. </summary>
	public struct Vector2dBounds
    {
        /// <summary> Southwest corner of bounding box. </summary>
        public Vector2d SouthWest;

        /// <summary> Northeast corner of bounding box. </summary>
        public Vector2d NorthEast;

        /// <summary> Initializes a new instance of the <see cref="Vector2dBounds" /> struct. </summary>
        /// <param name="sw"> Geographic coordinate representing southwest corner of bounding box. </param>
        /// <param name="ne"> Geographic coordinate representing northeast corner of bounding box. </param>
        public Vector2dBounds(Vector2d sw, Vector2d ne)
        {
            this.SouthWest = sw;
            this.NorthEast = ne;
        }

        /// <summary> Gets the south latitude. </summary>
        /// <value> The south latitude. </value>
        public double South
        {
            get
            {
                return this.SouthWest.x;
            }
        }

        /// <summary> Gets the west longitude. </summary>
        /// <value> The west longitude. </value>
        public double West
        {
            get
            {
                return this.SouthWest.y;
            }
        }

        /// <summary> Gets the north latitude. </summary>
        /// <value> The north latitude. </value>
        public double North
        {
            get
            {
                return this.NorthEast.x;
            }
        }

        /// <summary> Gets the east longitude. </summary>
        /// <value> The east longitude. </value>
        public double East
        {
            get
            {
                return this.NorthEast.y;
            }
        }

        /// <summary>
        ///     Gets or sets the central coordinate of the bounding box. When
        ///     setting a new center, the bounding box will retain its original size.
        /// </summary>
        /// <value> The central coordinate. </value>
        public Vector2d Center
        {
            get
            {
                var lat = (this.SouthWest.x + this.NorthEast.x) / 2;
                var lng = (this.SouthWest.y + this.NorthEast.y) / 2;

                return new Vector2d(lat, lng);
            }

            set
            {
                var lat = (this.NorthEast.x - this.SouthWest.x) / 2;
                this.SouthWest.x = value.x - lat;
                this.NorthEast.x = value.x + lat;

                var lng = (this.NorthEast.y - this.SouthWest.y) / 2;
                this.SouthWest.y = value.y - lng;
                this.NorthEast.y = value.y + lng;
            }
        }

        /// <summary>
        ///     Creates a bound from two arbitrary points. Contrary to the constructor,
        ///     this method always creates a non-empty box.
        /// </summary>
        /// <param name="a"> The first point. </param>
        /// <param name="b"> The second point. </param>
        /// <returns> The convex hull. </returns>
        public static Vector2dBounds FromCoordinates(Vector2d a, Vector2d b)
        {
            var bounds = new Vector2dBounds(a, a);
            bounds.Extend(b);

            return bounds;
        }

        /// <summary> A bounding box containing the world. </summary>
        /// <returns> The world bounding box. </returns>
        public static Vector2dBounds World()
        {
            var sw = new Vector2d(-90, -180);
            var ne = new Vector2d(90, 180);

            return new Vector2dBounds(sw, ne);
        }

        /// <summary> Extend the bounding box to contain the point. </summary>
        /// <param name="point"> A geographic coordinate. </param>
        public void Extend(Vector2d point)
        {
            if (point.x < this.SouthWest.x)
            {
                this.SouthWest.x = point.x;
            }

            if (point.x > this.NorthEast.x)
            {
                this.NorthEast.x = point.x;
            }

            if (point.y < this.SouthWest.y)
            {
                this.SouthWest.y = point.y;
            }

            if (point.y > this.NorthEast.y)
            {
                this.NorthEast.y = point.y;
            }
        }

        /// <summary> Extend the bounding box to contain the bounding box. </summary>
        /// <param name="bounds"> A bounding box. </param>
        public void Extend(Vector2dBounds bounds)
        {
            this.Extend(bounds.SouthWest);
            this.Extend(bounds.NorthEast);
        }

        /// <summary> Whenever the geographic bounding box is empty. </summary>
        /// <returns> <c>true</c>, if empty, <c>false</c> otherwise. </returns>
        public bool IsEmpty()
        {
            return this.SouthWest.x > this.NorthEast.x ||
                       this.SouthWest.y > this.NorthEast.y;
        }

        /// <summary>
        /// Converts to an array of doubles.
        /// </summary>
        /// <returns>An array of coordinates.</returns>
        public double[] ToArray()
        {
            double[] array =
            {
                this.SouthWest.x,
                this.SouthWest.y,
                this.NorthEast.x,
                this.NorthEast.y
            };

            return array;
        }

        /// <summary> Converts the Bbox to a URL snippet. </summary>
        /// <returns> Returns a string for use in a Mapbox query URL. </returns>
        public override string ToString()
        {
            return string.Format("{0},{1}", this.SouthWest.ToString(), this.NorthEast.ToString());
        }
    }

    public struct RectD
    {
        public Vector2d Min { get; private set; }
        public Vector2d Max { get; private set; }
        //size is absolute width&height so Min+size != max
        public Vector2d Size { get; private set; }
        public Vector2d Center { get; private set; }

        public RectD(Vector2d min, Vector2d size)
        {
            Min = min;
            Max = min + size;
            Center = new Vector2d(Min.x + size.x / 2, Min.y + size.y / 2);
            Size = new Vector2d(Mathd.Abs(size.x), Mathd.Abs(size.y));
        }

        public bool Contains(Vector2d point)
        {
            bool flag = Size.x < 0.0 && point.x <= Min.x && point.x > (Min.x + Size.x) || Size.x >= 0.0 && point.x >= Min.x && point.x < (Min.x + Size.x);
            return flag && (Size.y < 0.0 && point.y <= Min.y && point.y > (Min.y + Size.y) || Size.y >= 0.0 && point.y >= Min.y && point.y < (Min.y + Size.y));
        }
    }

    public struct Vector2d
    {
        public const double kEpsilon = 1E-05d;
        public double x;
        public double y;

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }
        }

        public Vector2d normalized
        {
            get
            {
                Vector2d vector2d = new Vector2d(this.x, this.y);
                vector2d.Normalize();
                return vector2d;
            }
        }

        public double magnitude
        {
            get
            {
                return Mathd.Sqrt(this.x * this.x + this.y * this.y);
            }
        }

        public double sqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y;
            }
        }

        public static Vector2d zero
        {
            get
            {
                return new Vector2d(0.0d, 0.0d);
            }
        }

        public static Vector2d one
        {
            get
            {
                return new Vector2d(1d, 1d);
            }
        }

        public static Vector2d up
        {
            get
            {
                return new Vector2d(0.0d, 1d);
            }
        }

        public static Vector2d right
        {
            get
            {
                return new Vector2d(1d, 0.0d);
            }
        }

        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x - b.x, a.y - b.y);
        }

        public static Vector2d operator -(Vector2d a)
        {
            return new Vector2d(-a.x, -a.y);
        }

        public static Vector2d operator *(Vector2d a, double d)
        {
            return new Vector2d(a.x * d, a.y * d);
        }

        public static Vector2d operator *(float d, Vector2d a)
        {
            return new Vector2d(a.x * d, a.y * d);
        }

        public static Vector2d operator /(Vector2d a, double d)
        {
            return new Vector2d(a.x / d, a.y / d);
        }

        public static bool operator ==(Vector2d lhs, Vector2d rhs)
        {
            return Vector2d.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
        }

        public static bool operator !=(Vector2d lhs, Vector2d rhs)
        {
            return (double)Vector2d.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
        }

        public void Set(double new_x, double new_y)
        {
            this.x = new_x;
            this.y = new_y;
        }

        public static Vector2d Lerp(Vector2d from, Vector2d to, double t)
        {
            t = Mathd.Clamp01(t);
            return new Vector2d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
        }

        public static Vector2d MoveTowards(Vector2d current, Vector2d target, double maxDistanceDelta)
        {
            Vector2d vector2 = target - current;
            double magnitude = vector2.magnitude;
            if (magnitude <= maxDistanceDelta || magnitude == 0.0d)
                return target;
            else
                return current + vector2 / magnitude * maxDistanceDelta;
        }

        public static Vector2d Scale(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x * b.x, a.y * b.y);
        }

        public void Scale(Vector2d scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
        }

        public void Normalize()
        {
            double magnitude = this.magnitude;
            if (magnitude > 9.99999974737875E-06)
                this = this / magnitude;
            else
                this = Vector2d.zero;
        }

        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "{0:F5},{1:F5}", this.y, this.x);
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2d))
                return false;
            Vector2d vector2d = (Vector2d)other;
            if (this.x.Equals(vector2d.x))
                return this.y.Equals(vector2d.y);
            else
                return false;
        }

        public static double Dot(Vector2d lhs, Vector2d rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public static double Angle(Vector2d from, Vector2d to)
        {
            return Mathd.Acos(Mathd.Clamp(Vector2d.Dot(from.normalized, to.normalized), -1d, 1d)) * 57.29578d;
        }

        public static double Distance(Vector2d a, Vector2d b)
        {
            return (a - b).magnitude;
        }

        public static Vector2d ClampMagnitude(Vector2d vector, double maxLength)
        {
            if (vector.sqrMagnitude > maxLength * maxLength)
                return vector.normalized * maxLength;
            else
                return vector;
        }

        public static double SqrMagnitude(Vector2d a)
        {
            return (a.x * a.x + a.y * a.y);
        }

        public double SqrMagnitude()
        {
            return (this.x * this.x + this.y * this.y);
        }

        public static Vector2d Min(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y));
        }

        public static Vector2d Max(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y));
        }

        public double[] ToArray()
        {
            double[] array =
            {
                this.x,
                this.y
            };

            return array;
        }
    }

    public struct Mathd
    {
        public const double PI = 3.141593d;
        public const double Infinity = double.PositiveInfinity;
        public const double NegativeInfinity = double.NegativeInfinity;
        public const double Deg2Rad = 0.01745329d;
        public const double Rad2Deg = 57.29578d;
        public const double Epsilon = 1.401298E-45d;

        public static double Sin(double d)
        {
            return Math.Sin(d);
        }

        public static double Cos(double d)
        {
            return Math.Cos(d);
        }

        public static double Tan(double d)
        {
            return Math.Tan(d);
        }

        public static double Asin(double d)
        {
            return Math.Asin(d);
        }

        public static double Acos(double d)
        {
            return Math.Acos(d);
        }

        public static double Atan(double d)
        {
            return Math.Atan(d);
        }

        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        public static double Sqrt(double d)
        {
            return Math.Sqrt(d);
        }

        public static double Abs(double d)
        {
            return Math.Abs(d);
        }

        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        public static double Min(double a, double b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        public static double Min(params double[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0.0d;
            double num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] < num)
                    num = values[index];
            }
            return num;
        }

        public static int Min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        public static int Min(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] < num)
                    num = values[index];
            }
            return num;
        }

        public static double Max(double a, double b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        public static double Max(params double[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0d;
            double num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if ((double)values[index] > (double)num)
                    num = values[index];
            }
            return num;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        public static int Max(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] > num)
                    num = values[index];
            }
            return num;
        }

        public static double Pow(double d, double p)
        {
            return Math.Pow(d, p);
        }

        public static double Exp(double power)
        {
            return Math.Exp(power);
        }

        public static double Log(double d, double p)
        {
            return Math.Log(d, p);
        }

        public static double Log(double d)
        {
            return Math.Log(d);
        }

        public static double Log10(double d)
        {
            return Math.Log10(d);
        }

        public static double Ceil(double d)
        {
            return Math.Ceiling(d);
        }

        public static double Floor(double d)
        {
            return Math.Floor(d);
        }

        public static double Round(double d)
        {
            return Math.Round(d);
        }

        public static int CeilToInt(double d)
        {
            return (int)Math.Ceiling(d);
        }

        public static int FloorToInt(double d)
        {
            return (int)Math.Floor(d);
        }

        public static int RoundToInt(double d)
        {
            return (int)Math.Round(d);
        }

        public static double Sign(double d)
        {
            return d >= 0.0 ? 1d : -1d;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0d;
            if (value > 1.0)
                return 1d;
            else
                return value;
        }

        public static double Lerp(double from, double to, double t)
        {
            return from + (to - from) * Mathd.Clamp01(t);
        }

        public static double LerpAngle(double a, double b, double t)
        {
            double num = Mathd.Repeat(b - a, 360d);
            if (num > 180.0d)
                num -= 360d;
            return a + num * Mathd.Clamp01(t);
        }

        public static double MoveTowards(double current, double target, double maxDelta)
        {
            if (Mathd.Abs(target - current) <= maxDelta)
                return target;
            else
                return current + Mathd.Sign(target - current) * maxDelta;
        }

        public static double MoveTowardsAngle(double current, double target, double maxDelta)
        {
            target = current + Mathd.DeltaAngle(current, target);
            return Mathd.MoveTowards(current, target, maxDelta);
        }

        public static double SmoothStep(double from, double to, double t)
        {
            t = Mathd.Clamp01(t);
            t = (-2.0 * t * t * t + 3.0 * t * t);
            return to * t + from * (1.0 - t);
        }

        public static double Gamma(double value, double absmax, double gamma)
        {
            bool flag = false;
            if (value < 0.0)
                flag = true;
            double num1 = Mathd.Abs(value);
            if (num1 > absmax)
            {
                if (flag)
                    return -num1;
                else
                    return num1;
            }
            else
            {
                double num2 = Mathd.Pow(num1 / absmax, gamma) * absmax;
                if (flag)
                    return -num2;
                else
                    return num2;
            }
        }

        public static bool Approximately(double a, double b)
        {
            return Mathd.Abs(b - a) < Mathd.Max(1E-06d * Mathd.Max(Mathd.Abs(a), Mathd.Abs(b)), 1.121039E-44d);
        }

        public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            smoothTime = Mathd.Max(0.0001d, smoothTime);
            double num1 = 2d / smoothTime;
            double num2 = num1 * deltaTime;
            double num3 = (1.0d / (1.0d + num2 + 0.479999989271164d * num2 * num2 + 0.234999999403954d * num2 * num2 * num2));
            double num4 = current - target;
            double num5 = target;
            double max = maxSpeed * smoothTime;
            double num6 = Mathd.Clamp(num4, -max, max);
            target = current - num6;
            double num7 = (currentVelocity + num1 * num6) * deltaTime;
            currentVelocity = (currentVelocity - num1 * num7) * num3;
            double num8 = target + (num6 + num7) * num3;
            if (num5 - current > 0.0 == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }
            return num8;
        }

        public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            target = current + Mathd.DeltaAngle(current, target);
            return Mathd.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static double Repeat(double t, double length)
        {
            return t - Mathd.Floor(t / length) * length;
        }

        public static double PingPong(double t, double length)
        {
            t = Mathd.Repeat(t, length * 2d);
            return length - Mathd.Abs(t - length);
        }

        public static double InverseLerp(double from, double to, double value)
        {
            if (from < to)
            {
                if (value < from)
                    return 0d;
                if (value > to)
                    return 1d;
                value -= from;
                value /= to - from;
                return value;
            }
            else
            {
                if (from <= to)
                    return 0d;
                if (value < to)
                    return 1d;
                if (value > from)
                    return 0d;
                else
                    return (1.0d - (value - to) / (from - to));
            }
        }

        public static double DeltaAngle(double current, double target)
        {
            double num = Mathd.Repeat(target - current, 360d);
            if (num > 180.0d)
                num -= 360d;
            return num;
        }

        internal static bool LineIntersection(Vector2d p1, Vector2d p2, Vector2d p3, Vector2d p4, ref Vector2d result)
        {
            double num1 = p2.x - p1.x;
            double num2 = p2.y - p1.y;
            double num3 = p4.x - p3.x;
            double num4 = p4.y - p3.y;
            double num5 = num1 * num4 - num2 * num3;
            if (num5 == 0.0d)
                return false;
            double num6 = p3.x - p1.x;
            double num7 = p3.y - p1.y;
            double num8 = (num6 * num4 - num7 * num3) / num5;
            result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }

        internal static bool LineSegmentIntersection(Vector2d p1, Vector2d p2, Vector2d p3, Vector2d p4, ref Vector2d result)
        {
            double num1 = p2.x - p1.x;
            double num2 = p2.y - p1.y;
            double num3 = p4.x - p3.x;
            double num4 = p4.y - p3.y;
            double num5 = (num1 * num4 - num2 * num3);
            if (num5 == 0.0d)
                return false;
            double num6 = p3.x - p1.x;
            double num7 = p3.y - p1.y;
            double num8 = (num6 * num4 - num7 * num3) / num5;
            if (num8 < 0.0d || num8 > 1.0d)
                return false;
            double num9 = (num6 * num2 - num7 * num1) / num5;
            if (num9 < 0.0d || num9 > 1.0d)
                return false;
            result = new Vector2d(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }
    }

    public static class VectorExtensions
    {
        /// <summary>
        /// Vector2 convenience method to convert Vector2 to Vector3.
        /// </summary>
        /// <returns>Vector3 with a y value of zero.</returns>
        /// <param name="v">Vector2.</param>
        public static Vector3 ToVector3xz(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        /// <summary>
        /// Vector2d convenience method to convert Vector2d to Vector3.
        /// </summary>
        /// <returns>Vector3 with a y value of zero.</returns>
        /// <param name="v">Vector2d.</param>
        public static Vector3 ToVector3xz(this Vector2d v)
        {
            return new Vector3((float)v.x, 0, (float)v.y);
        }

        /// <summary>
        /// Vector3 convenience method to convert Vector3 to Vector2.
        /// </summary>
        /// <returns>The Vector2.</returns>
        /// <param name="v">Vector3.</param>
        public static Vector2 ToVector2xz(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        /// <summary>
        /// Vector3 convenience method to convert Vector3 to Vector2d.
        /// </summary>
        /// <returns>The Vector2d.</returns>
        /// <param name="v">Vector3.</param>
        public static Vector2d ToVector2d(this Vector3 v)
        {
            return new Vector2d(v.x, v.z);
        }

        /// <summary>
        /// Transform extension method to move a Unity transform to a specific latitude/longitude.
        /// </summary>
        /// <param name="t">Transform.</param>
        /// <param name="lat">Latitude.</param>
        /// <param name="lng">Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Place a Unity transform at 10, 10, with a map center of (0, 0) and scale 1:
        /// <code>
        /// transform.MoveToGeocoordinate(10, 10, new Vector2d(0, 0), 1f);
        /// Debug.Log(transform.position);
        /// // (1113195.0, 0.0, 1118890.0)
        /// </code>
        /// </example>
        public static void MoveToGeocoordinate(this Transform t, double lat, double lng, Vector2d refPoint, float scale = 1)
        {
            t.position = Conversions.GeoToWorldPosition(lat, lng, refPoint, scale).ToVector3xz();
        }

        /// <summary>
        /// Transform extension method to move a Unity transform to a specific Vector2d.
        /// </summary>
        /// <param name="t">Transform.</param>
        /// <param name="latLon">Latitude Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Place a Unity transform at 10, 10, with a map center of (0, 0) and scale 1:
        /// <code>
        /// transform.MoveToGeocoordinate(new Vector2d(10, 10), new Vector2d(0, 0), 1f);
        /// Debug.Log(transform.position);
        /// // (1113195.0, 0.0, 1118890.0)
        /// </code>
        /// </example>
        public static void MoveToGeocoordinate(this Transform t, Vector2d latLon, Vector2d refPoint, float scale = 1)
        {
            t.MoveToGeocoordinate(latLon.x, latLon.y, refPoint, scale);
        }

        /// <summary>
        /// Vector2 extension method to convert from a latitude/longitude to a Unity Vector3.
        /// </summary>
        /// <returns>The Vector3 Unity position.</returns>
        /// <param name="latLon">Latitude Longitude.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        public static Vector3 AsUnityPosition(this Vector2 latLon, Vector2d refPoint, float scale = 1)
        {
            return Conversions.GeoToWorldPosition(latLon.x, latLon.y, refPoint, scale).ToVector3xz();
        }

        /// <summary>
        /// Transform extension method to return the transform's position as a Vector2d latitude/longitude.
        /// </summary>
        /// <returns>Vector2d that represents latitude/longitude.</returns>
        /// <param name="t">T.</param>
        /// <param name="refPoint">Reference point.</param>
        /// <param name="scale">Scale.</param>
        /// <example>
        /// Get the latitude/longitude of a transform at (1113195, 0, 1118890), map center (0, 0) and scale 1.
        /// <code>
        /// var latLng = transform.GetGeoPosition(new Vector2d(0, 0), 1);
        /// Debug.Log(latLng);
        /// // (10.00000, 10.00000)
        /// </code>
        /// </example>
        public static Vector2d GetGeoPosition(this Transform t, Vector2d refPoint, float scale = 1)
        {
            var pos = refPoint + (t.position / scale).ToVector2d();
            return Conversions.MetersToLatLon(pos);
        }

        public static Vector2d GetGeoPosition(this Vector3 position, Vector2d refPoint, float scale = 1)
        {
            var pos = refPoint + (position / scale).ToVector2d();
            return Conversions.MetersToLatLon(pos);
        }
    }

    /// <summary> A handle to an asynchronous request. </summary>
    public interface IAsyncRequest
    {
        /// <summary> True after the request has finished. </summary>
        bool IsCompleted { get; }

        /// <summary> Cancel the ongoing request, preventing it from firing a callback. </summary>
        void Cancel();
    }

    /// <summary>
	///    A Map tile, a square with vector or raster data representing a geographic
	///    bounding box. More info <see href="https://en.wikipedia.org/wiki/Tiled_web_map">
	///    here </see>.
	/// </summary>
	public class RasterTile : IAsyncRequest
    {
        private CanonicalTileId _id;
        private List<Exception> _exceptions;
        private State _state = State.New;
        private IAsyncRequest _request;
        private Action _callback;

        private byte[] data;

        public byte[] Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary> Tile state. </summary>
        public enum State
        {
            /// <summary> New tile, not yet initialized. </summary>
            New,
            /// <summary> Loading data. </summary>
            Loading,
            /// <summary> Data loaded and parsed. </summary>
            Loaded,
            /// <summary> Data loading cancelled. </summary>
            Canceled
        }

        /// <summary> Gets the <see cref="T:Mapbox.Map.CanonicalTileId"/> identifier. </summary>
        /// <value> The canonical tile identifier. </value>
        public CanonicalTileId Id
        {
            get { return _id; }
            set { _id = value; }
        }


        /// <summary>Flag to indicate if the request was successful</summary>
        public bool HasError
        {
            get
            {
                return _exceptions == null ? false : _exceptions.Count > 0;
            }
        }


        /// <summary> Exceptions that might have occured during creation of the tile. </summary>
        public ReadOnlyCollection<Exception> Exceptions
        {
            get { return null == _exceptions ? null : _exceptions.AsReadOnly(); }
        }


        /// <summary> Messages of exceptions otherwise empty string. </summary>
        public string ExceptionsAsString
        {
            get
            {
                if (null == _exceptions || _exceptions.Count == 0) { return string.Empty; }
                return string.Join(Environment.NewLine, _exceptions.Select(e => e.Message).ToArray());
            }
        }


        /// <summary>
        /// Sets the error message.
        /// </summary>
        /// <param name="errorMessage"></param>
        internal void AddException(Exception ex)
        {
            if (null == _exceptions) { _exceptions = new List<Exception>(); }
            _exceptions.Add(ex);
        }


        /// <summary>
        ///     Gets the current state. When fully loaded, you must
        ///     check if the data actually arrived and if the tile
        ///     is accusing any error.
        /// </summary>
        /// <value> The tile state. </value>
        public State CurrentState
        {
            get
            {
                return _state;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _state == State.Loaded;
            }
        }


        /// <summary>
        ///     Initializes the <see cref="T:Mapbox.Map.Tile"/> object. It will
        ///     start a network request and fire the callback when completed.
        /// </summary>
        /// <param name="param"> Initialization parameters. </param>
        /// <param name="callback"> The completion callback. </param>
        public void Initialize(Parameters param, Action callback)
        {
            Cancel();

            _state = State.Loading;
            _id = param.Id;
            _callback = callback;
            _request = param.Fs.Request(MakeTileResource(param.MapId).GetUrl(), HandleTileResponse, tileId: _id, mapId: param.MapId);
        }

        internal void Initialize(IFileSource fileSource, CanonicalTileId canonicalTileId, string mapId, Action p)
        {
            Cancel();

            _state = State.Loading;
            _id = canonicalTileId;
            _callback = p;
            _request = fileSource.Request(MakeTileResource(mapId).GetUrl(), HandleTileResponse, tileId: _id, mapId: mapId);
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.Tile"/>.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String"/> that represents the current
        ///     <see cref="T:Mapbox.Map.Tile"/>.
        /// </returns>
        public override string ToString()
        {
            return Id.ToString();
        }


        /// <summary>
        ///     Cancels the request for the <see cref="T:Mapbox.Map.Tile"/> object.
        ///     It will stop a network request and set the tile's state to Canceled.
        /// </summary>
        /// <example>
        /// <code>
        /// // Do not request tiles that we are already requesting
        ///	// but at the same time exclude the ones we don't need
        ///	// anymore, cancelling the network request.
        ///	tiles.RemoveWhere((T tile) =>
        ///	{
        ///		if (cover.Remove(tile.Id))
        ///		{
        ///			return false;
        ///		}
        ///		else
        ///		{
        ///			tile.Cancel();
        ///			NotifyNext(tile);
        ///			return true;			
        /// 	}
        ///	});
        /// </code>
        /// </example>
        public void Cancel()
        {
            if (_request != null)
            {
                _request.Cancel();
                _request = null;
            }

            _state = State.Canceled;
        }


        // Get the tile resource (raster/vector/etc).
        internal virtual TileResource MakeTileResource(string styleUrl)
        {
            return TileResource.MakeRaster(Id, styleUrl);
        }


        // Decode the tile.
        internal virtual bool ParseTileData(byte[] data)
        {
            // We do not parse raster tiles as they are
            this.data = data;

            return true;
        }


        // TODO: Currently the tile decoding is done on the main thread. We must implement
        // a Worker class to abstract this, so on platforms that support threads (like Unity
        // on the desktop, Android, etc) we can use worker threads and when building for
        // the browser, we keep it single-threaded.
        private void HandleTileResponse(Response response)
        {

            if (response.HasError)
            {
                response.Exceptions.ToList().ForEach(e => AddException(e));
            }
            else
            {
                // only try to parse if request was successful

                // current implementation doesn't need to check if parsing is successful:
                // * Mapbox.Map.VectorTile.ParseTileData() already adds any exception to the list
                // * Mapbox.Map.RasterTile.ParseTileData() doesn't do any parsing
                ParseTileData(response.Data);
            }

            // Cancelled is not the same as loaded!
            if (_state != State.Canceled)
            {
                _state = State.Loaded;
            }
            _callback();
        }


        /// <summary>
        ///    Parameters for initializing a Tile object.
        /// </summary>
        /// <example>
        /// <code>
        /// var parameters = new Tile.Parameters();
        /// parameters.Fs = MapboxAccess.Instance;
        /// parameters.Id = new CanonicalTileId(_zoom, _tileCoorindateX, _tileCoordinateY);
        /// parameters.MapId = "mapbox.mapbox-streets-v7";
        /// </code>
        /// </example>
        public struct Parameters
        {
            /// <summary> The tile id. </summary>
            public CanonicalTileId Id;

            /// <summary>
            ///     The tileset map ID, usually in the format "user.mapid". Exceptionally,
            ///     <see cref="T:Mapbox.Map.RasterTile"/> will take the full style URL
            ///     from where the tile is composited from, like mapbox://styles/mapbox/streets-v9.
            /// </summary>
            public string MapId;

            /// <summary> The data source abstraction. </summary>
            public IFileSource Fs;
        }


    }

    /// <summary> 
    /// Interface representing a Mapbox resource URL. Used to build request strings
    /// and return full URLs to a Mapbox Web Service API resource. 
    /// </summary>
    public interface IResource
    {
        /// <summary>Builds a complete, valid URL string.</summary>
        /// <returns>URL string.</returns>
        string GetUrl();
    }

    public class RetinaRasterTile : RasterTile
    {
        internal override TileResource MakeTileResource(string mapId)
        {
            return TileResource.MakeRetinaRaster(Id, mapId);
        }
    }

    public interface IFileSource
    {
        /// <summary> Performs a request asynchronously. </summary>
        /// <param name="uri"> The resource description in the URI format. </param>
        /// <param name="callback"> Callback to be called after the request is completed. </param>
        /// <returns>
        ///     Returns a <see cref="IAsyncRequest" /> that can be used for canceling a pending
        ///     request. This handle can be completely ignored if there is no intention of ever
        ///     canceling the request.
        /// </returns>
        IAsyncRequest Request(string uri, Action<Response> callback, int timeout = 10, CanonicalTileId tileId = new CanonicalTileId(), string mapId = null);
    }

    public class TileResource : IResource
    {
        static readonly string _eventQuery = "events=true";
        readonly string _query;

        internal TileResource(string query)
        {
            _query = query;
        }

        public static TileResource MakeRaster(CanonicalTileId id, string styleUrl)
        {
            return new TileResource(string.Format("{0}/{1}", MapDynamicUtils.NormalizeStaticStyleURL(styleUrl ?? "mapbox://styles/mapbox/satellite-v9"), id));
        }

        internal static TileResource MakeRetinaRaster(CanonicalTileId id, string styleUrl)
        {
            return new TileResource(string.Format("{0}/{1}@2x", MapDynamicUtils.NormalizeStaticStyleURL(styleUrl ?? "mapbox://styles/mapbox/satellite-v9"), id));
        }

        public static TileResource MakeClassicRaster(CanonicalTileId id, string mapId)
        {
            return new TileResource(string.Format("{0}/{1}.png", MapDynamicUtils.MapIdToUrl(mapId ?? "mapbox.satellite"), id));
        }

        internal static TileResource MakeClassicRetinaRaster(CanonicalTileId id, string mapId)
        {
            return new TileResource(string.Format("{0}/{1}@2x.png", MapDynamicUtils.MapIdToUrl(mapId ?? "mapbox.satellite"), id));
        }

        public static TileResource MakeRawPngRaster(CanonicalTileId id, string mapId)
        {
            return new TileResource(string.Format("{0}/{1}.pngraw", MapDynamicUtils.MapIdToUrl(mapId ?? "mapbox.terrain-rgb"), id));
        }

        public static TileResource MakeVector(CanonicalTileId id, string mapId)
        {
            return new TileResource(string.Format("{0}/{1}.vector.pbf", MapDynamicUtils.MapIdToUrl(mapId ?? "mapbox.mapbox-streets-v7"), id));
        }

        internal static TileResource MakeStyleOptimizedVector(CanonicalTileId id, string mapId, string optimizedStyleId, string modifiedDate)
        {
            return new TileResource(string.Format("{0}/{1}.vector.pbf?style={2}@{3}", MapDynamicUtils.MapIdToUrl(mapId ?? "mapbox.mapbox-streets-v7"), id, optimizedStyleId, modifiedDate));
        }

        public string GetUrl()
        {
            var uriBuilder = new UriBuilder(_query);
            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
            {
                uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + _eventQuery;
            }
            else
            {
                uriBuilder.Query = _eventQuery;
            }

            return uriBuilder.ToString();
        }
    }

    public class Response
    {


        private Response() { }


        public IAsyncRequest Request { get; private set; }


        public bool RateLimitHit
        {
            get { return StatusCode.HasValue ? 429 == StatusCode.Value : false; }
        }


        /// <summary>Flag to indicate if the request was successful</summary>
        public bool HasError
        {
            get { return _exceptions == null ? false : _exceptions.Count > 0; }
        }

        public bool LoadedFromCache;

        public string RequestUrl;


        public int? StatusCode;


        public string ContentType;


        /// <summary>Length of rate-limiting interval in seconds. https://www.mapbox.com/api-documentation/#rate-limits </summary>
        public int? XRateLimitInterval;


        /// <summary>Maximum number of requests you may make in the current interval before reaching the limit. https://www.mapbox.com/api-documentation/#rate-limits </summary>
        public long? XRateLimitLimit;


        /// <summary>Timestamp of when the current interval will end and the ratelimit counter is reset. https://www.mapbox.com/api-documentation/#rate-limits </summary>
        public DateTime? XRateLimitReset;


        private List<Exception> _exceptions;
        /// <summary> Exceptions that might have occured during the request. </summary>
        public ReadOnlyCollection<Exception> Exceptions
        {
            get { return null == _exceptions ? null : _exceptions.AsReadOnly(); }
        }


        /// <summary> Messages of exceptions otherwise empty string. </summary>
        public string ExceptionsAsString
        {
            get
            {
                if (null == _exceptions || _exceptions.Count == 0) { return string.Empty; }
                return string.Join(Environment.NewLine, _exceptions.Select(e => e.Message).ToArray());
            }
        }


        /// <summary> Headers of the response. </summary>
        public Dictionary<string, string> Headers;


        /// <summary> Raw data fetched from the request. </summary>
        public byte[] Data;

        public void AddException(Exception ex)
        {
            if (null == _exceptions) { _exceptions = new List<Exception>(); }
            _exceptions.Add(ex);
        }

        // TODO: we should store timestamp of the cache!
        public static Response FromCache(byte[] data)
        {
            Response response = new Response();
            response.Data = data;
            response.LoadedFromCache = true;
            return response;
        }

#if !NETFX_CORE && !UNITY // full .NET Framework
        public static Response FromWebResponse(IAsyncRequest request, HttpWebResponse apiResponse, Exception apiEx)
        {

            Response response = new Response();
            response.Request = request;

            if (null != apiEx)
            {
                response.AddException(apiEx);
            }

            // timeout: API response is null
            if (null == apiResponse)
            {
                response.AddException(new Exception("No Reponse."));
            }
            else
            {
                // https://www.mapbox.com/api-documentation/#rate-limits
                if (null != apiResponse.Headers)
                {
                    response.Headers = new Dictionary<string, string>();
                    for (int i = 0; i < apiResponse.Headers.Count; i++)
                    {
                        // TODO: implement .Net Core / UWP implementation
                        string key = apiResponse.Headers.Keys[i];
                        string val = apiResponse.Headers[i];
                        response.Headers.Add(key, val);
                        if (key.Equals("X-Rate-Limit-Interval", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int limitInterval;
                            if (int.TryParse(val, out limitInterval)) { response.XRateLimitInterval = limitInterval; }
                        }
                        else if (key.Equals("X-Rate-Limit-Limit", StringComparison.InvariantCultureIgnoreCase))
                        {
                            long limitLimit;
                            if (long.TryParse(val, out limitLimit)) { response.XRateLimitLimit = limitLimit; }
                        }
                        else if (key.Equals("X-Rate-Limit-Reset", StringComparison.InvariantCultureIgnoreCase))
                        {
                            double unixTimestamp;
                            if (double.TryParse(val, out unixTimestamp))
                            {
                                response.XRateLimitReset = UnixTimestampUtils.From(unixTimestamp);
                            }
                        }
                        else if (key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
                        {
                            response.ContentType = val;
                        }
                    }
                }

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    response.AddException(new Exception(string.Format("{0}: {1}", apiResponse.StatusCode, apiResponse.StatusDescription)));
                }
                int statusCode = (int)apiResponse.StatusCode;
                response.StatusCode = statusCode;
                if (429 == statusCode)
                {
                    response.AddException(new Exception("Rate limit hit"));
                }

                if (null != apiResponse)
                {
                    using (Stream responseStream = apiResponse.GetResponseStream())
                    {
                        byte[] buffer = new byte[0x1000];
                        int bytesRead;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            while (0 != (bytesRead = responseStream.Read(buffer, 0, buffer.Length)))
                            {
                                ms.Write(buffer, 0, bytesRead);
                            }
                            response.Data = ms.ToArray();
                        }
                    }
                    apiResponse.Close();
                }
            }

            return response;
        }
#endif

#if NETFX_CORE && !UNITY //UWP but not Unity
		public static async Task<Response> FromWebResponse(IAsyncRequest request, HttpResponseMessage apiResponse, Exception apiEx) {

			Response response = new Response();
			response.Request = request;

			if (null != apiEx) {
				response.AddException(apiEx);
			}

			// timeout: API response is null
			if (null == apiResponse) {
				response.AddException(new Exception("No Reponse."));
			} else {
				// https://www.mapbox.com/api-documentation/#rate-limits
				if (null != apiResponse.Headers) {
					response.Headers = new Dictionary<string, string>();
					foreach (var hdr in apiResponse.Headers) {
						string key = hdr.Key;
						string val = hdr.Value.FirstOrDefault();
						response.Headers.Add(key, val);
						if (key.Equals("X-Rate-Limit-Interval", StringComparison.OrdinalIgnoreCase)) {
							int limitInterval;
							if (int.TryParse(val, out limitInterval)) { response.XRateLimitInterval = limitInterval; }
						} else if (key.Equals("X-Rate-Limit-Limit", StringComparison.OrdinalIgnoreCase)) {
							long limitLimit;
							if (long.TryParse(val, out limitLimit)) { response.XRateLimitLimit = limitLimit; }
						} else if (key.Equals("X-Rate-Limit-Reset", StringComparison.OrdinalIgnoreCase)) {
							double unixTimestamp;
							if (double.TryParse(val, out unixTimestamp)) {
								DateTime beginningOfTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
								response.XRateLimitReset = beginningOfTime.AddSeconds(unixTimestamp).ToLocalTime();
							}
						} else if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
							response.ContentType = val;
						}
					}
				}

				if (apiResponse.StatusCode != HttpStatusCode.OK) {
					response.AddException(new Exception(string.Format("{0}: {1}", apiResponse.StatusCode, apiResponse.ReasonPhrase)));
				}
				int statusCode = (int)apiResponse.StatusCode;
				response.StatusCode = statusCode;
				if (429 == statusCode) {
					response.AddException(new Exception("Rate limit hit"));
				}

				if (null != apiResponse) {
					response.Data = await apiResponse.Content.ReadAsByteArrayAsync();
				}
			}

			return response;
		}
#endif

#if UNITY // within Unity or UWP build from Unity
		public static Response FromWebResponse(IAsyncRequest request, UnityWebRequest apiResponse, Exception apiEx) {

			Response response = new Response();
			response.Request = request;

			if (null != apiEx) {
				response.AddException(apiEx);
			}

			if (apiResponse.isNetworkError) {
				response.AddException(new Exception(apiResponse.error));
			}

			if (null == apiResponse.downloadHandler.data) {
				response.AddException(new Exception("Response has no data."));
			}

#if NETFX_CORE
			StringComparison stringComp = StringComparison.OrdinalIgnoreCase;
#else
			StringComparison stringComp = StringComparison.InvariantCultureIgnoreCase;
#endif

			Dictionary<string, string> apiHeaders = apiResponse.GetResponseHeaders();
			if (null != apiHeaders) {
				response.Headers = new Dictionary<string, string>();
				foreach (var apiHdr in apiHeaders) {
					string key = apiHdr.Key;
					string val = apiHdr.Value;
					response.Headers.Add(key, val);
					if (key.Equals("X-Rate-Limit-Interval", stringComp)) {
						int limitInterval;
						if (int.TryParse(val, out limitInterval)) { response.XRateLimitInterval = limitInterval; }
					} else if (key.Equals("X-Rate-Limit-Limit", stringComp)) {
						long limitLimit;
						if (long.TryParse(val, out limitLimit)) { response.XRateLimitLimit = limitLimit; }
					} else if (key.Equals("X-Rate-Limit-Reset", stringComp)) {
						double unixTimestamp;
						if (double.TryParse(val, out unixTimestamp)) {
							response.XRateLimitReset = UnixTimestampUtils.From(unixTimestamp);
						}
					} else if (key.Equals("Content-Type", stringComp)) {
						response.ContentType = val;
					}
				}
			}

			int statusCode = (int)apiResponse.responseCode;
			response.StatusCode = statusCode;

			if (statusCode != 200) {
				response.AddException(new Exception(string.Format("Status Code {0}", apiResponse.responseCode)));
			}
			if (429 == statusCode) {
				response.AddException(new Exception("Rate limit hit"));
			}

			response.Data = apiResponse.downloadHandler.data;

			return response;
		}
#endif



    }

    /// <summary>
	/// A set of Unix Timestamp utils.
	/// </summary>
	public static class UnixTimestampUtils
    {

        // http://gigi.nullneuron.net/gigilabs/converting-tofrom-unix-timestamp-in-c/

        /// <summary>
        /// Convert from DateTime to Unix timestamp
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double To(DateTime date)
        {
            //return date.ToLocalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }


        /// <summary>
        /// Convert from Unitx timestamp to DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime From(double timestamp)
        {
            //return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(timestamp)).ToLocalTime();
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(timestamp));
        }


    }
}
