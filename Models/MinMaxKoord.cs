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
        private double MinLAT = double.MaxValue;
        private double MaxLAT = double.MinValue;

        private double MinLONG = double.MaxValue;
        private double MaxLONG = double.MinValue;

        private double HorizontalGPSSize;
        private double VerticalGPSSize;
        private double OneHorizontalGpsPixel;
        private double OneVerticalGpsPixel;

        /// <summary>
        /// Internes Flag, das Zeichenflächenbereich gesetzt wurde
        /// </summary>
        private bool IsScreenRectSet;
        /// <summary>
        /// TRect der Zeichenfläche, auf die skaliert werden soll
        /// </summary>
        private Size CanvasRect;
        /// <summary>
        /// Liste der zu skalierenden GPS-Koordinaten
        /// </summary>
        private List<GPSKoord> KoordList;
        private Dictionary<GPSKoord, Point> ScreenCoordList;

        public MinMaxKoord()
        {
            CanvasRect = new Size(0, 0);
            KoordList = [];
            ScreenCoordList = [];
        }

        #region Methods for Rect
        public void SetRect(Size s)
        {
            CanvasRect = s;
            IsScreenRectSet = true;
            //Cache ist nun ungültig, also leeren
            ScreenCoordList.Clear();
        }

        #endregion

        #region Methods for GPS-Coordinates
        public void AddGPS(double Latitude, double Longitude)
        {
            KoordList.Add(new GPSKoord(Latitude, Longitude));
            UpdateMinMaxGPS();
        }

        public void AddGPS(GPSKoord gk)
        {
            KoordList.Add(gk);
            UpdateMinMaxGPS();
        }

        public void ClearGPS() => KoordList.Clear();

        private void UpdateMinMaxGPS()
        {
            if (!IsScreenRectSet)
                throw new InvalidOperationException("ScreenRect not set !");

            MinLAT = MinLONG = double.MaxValue;
            MaxLAT = MaxLONG = double.MinValue;

            foreach(GPSKoord gk in KoordList)
            {
                //Die aufeinanderfolgenden IFs sind tatsächlich die schnellstmögliche Version,
                //sogar minimal schneller als die "else if"-Lösung
                //Compare Actual LAT against Min/MaxLAT 
                if (gk.Latitude > MaxLAT)
                    MaxLAT = gk.Latitude;
                if (gk.Latitude < MinLAT)
                    MinLAT = gk.Latitude;

                //Compare actual LONG against Min/MaxLONG
                if (gk.Longitude > MaxLONG) 
                    MaxLONG = gk.Longitude;
                if (gk.Longitude < MinLONG) 
                    MinLONG = gk.Longitude;
            }
            //Höhe und Breite des gesamten GPS-Rects ermitteln
            HorizontalGPSSize = MaxLAT - MinLAT;
            VerticalGPSSize = MaxLONG - MinLONG;
            OneHorizontalGpsPixel = HorizontalGPSSize / CanvasRect.Width;
            OneVerticalGpsPixel = VerticalGPSSize / CanvasRect.Height;
        }

        #endregion

        #region Scaling
        private Point GetScreenCoordsForGPS(GPSKoord gk)
        {
            if (!IsScreenRectSet)
                throw new InvalidOperationException("Screen Rect is not set");

            //Berechnen wieviele Pixel ein GPS-Step sind
            double OneHorizontalGpsPixel = HorizontalGPSSize / CanvasRect.Width;
            double OneVerticalGpsPixel = VerticalGPSSize / CanvasRect.Height;

            //Nun können wir skalieren
            Point screenCoord = new Point();
            screenCoord.X = OneHorizontalGpsPixel * gk.Latitude;
            screenCoord.Y = OneVerticalGpsPixel * gk.Longitude; 
            return screenCoord;
        }

        public void Scale()
        {
            if (!IsScreenRectSet)
                return;
            
            UpdateMinMaxGPS();

            foreach (GPSKoord gk in KoordList)
            {
                //Check for already in cache
                if (ScreenCoordList.ContainsKey(gk))
                    continue;
                //Get Screen-Coords
                Point p = GetScreenCoordsForGPS(gk);                
                //Put Screencoord + GPSKoord in Dictionary
                ScreenCoordList.Add(gk, p );
            }
        }

        public Point ScaleGPSCoord(double Lat, double Long)
        {
            GPSKoord gk = new(Lat, Long);

            if (ScreenCoordList.TryGetValue(gk, out Point point))
                return point;
            //Calculate Screencoords
            point = GetScreenCoordsForGPS(gk);
            //Add to Dictionary
            ScreenCoordList.Add(gk, point);
            return point;
        }

        #endregion

    }
}
