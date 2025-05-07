using System.Linq;
using System.Windows;

namespace RKI2.Models
{
    public struct GPSKoord
    {
        public double Latitude { get; set; }  //Breitengrad  (Nord, Süd)
        public double Longitude { get; set; } //Längengrad  (Ost, West)

        public GPSKoord() => Latitude = Longitude = 0;

        public GPSKoord(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        //Diese beiden brauichen wir für ein sauberes Contains im Dictionary
        public GPSKoord(GPSKoord other)
        {
            Latitude = other.Latitude;
            Longitude = other.Longitude;
        }

        public override readonly int GetHashCode()
            => Latitude.GetHashCode() ^ Longitude.GetHashCode();

        public override readonly bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            GPSKoord gk = (GPSKoord)obj;
            return (gk.Latitude == Latitude && gk.Longitude == Longitude);
        }
    }

    internal class MinMaxKoord
    {
        /// <summary>
        /// Internes Flag, das Zeichenflächenbereich gesetzt wurde
        /// </summary>
        private bool IsScreenRectSet;
        /// <summary>
        /// TRect der Zeichenfläche, auf die skaliert werden soll
        /// </summary>
        private Rect Rect;
        /// <summary>
        /// Liste der zu skalierenden GPS-Koordinaten
        /// </summary>
        private List<GPSKoord> KoordList;
        private Dictionary<GPSKoord, Point> ScreenCoordList;

        public MinMaxKoord()
        {
            Rect = new Rect(0, 0, 0, 0);
            KoordList = [];
            ScreenCoordList = [];
        }

        #region Methods for Rect
        public void SetRect(Rect r)
        {
            Rect = new(r.TopLeft, r.BottomRight);
            IsScreenRectSet = true;
            //Cache ist nun ungültig, also leeren
            ScreenCoordList.Clear();
        }

        public void SetRect(double Top, double Bottom, double Left, double Right)
        {
            Rect = new(Top, Bottom, Left, Right);
            IsScreenRectSet = true;
            //Cache ist nun ungültig, also leeren
            ScreenCoordList.Clear();
        }
        #endregion

        #region Methods for GPS-Coordinates
        public void AddGPS(double Latitude, double Longitude)
            => KoordList.Add(new GPSKoord(Latitude, Longitude));

        public void AddGPS(GPSKoord gk) => KoordList.Add(gk);

        public void ClearGPS() => KoordList.Clear();

        #endregion

        #region Scaling
        public void Scale()
        {
            if (!IsScreenRectSet)
                return;

            foreach (GPSKoord gk in KoordList)
            {
                //Check for already in cache
                if (ScreenCoordList.ContainsKey(gk))
                    continue;
                //Calculate Screenkoords
                
                //Put Screencoord + GPSKoord in Dictionary
            }
        }

        public Point ScaleGPSCoord(double Lat, double Long)
        {
            GPSKoord gk = new(Lat, Long);
            Point point = new Point();

            if (ScreenCoordList.ContainsKey(gk))
                return ScreenCoordList[gk];
            //Calculate Screencoords

            //Add to Dictionary
            return point;
        }

        #endregion

    }
}
