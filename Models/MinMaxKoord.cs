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

        public GPSKoord(GPSKoord other)
        {
            Latitude = other.Latitude;
            Longitude = other.Longitude;
        }
    }

    internal class MinMaxKoord
    {
        private Rect Rect;
        private List<GPSKoord> KoordList;

        public MinMaxKoord()
        {
            Rect = new Rect(0, 0, 0, 0);
            KoordList = [];
        }

        #region Methods for Rect
        public void SetRect(Rect r) => Rect = new(r.TopLeft, r.BottomRight);

        public void SetRect(double Top, double Bottom, double Left, double Right)
            => Rect = new(Top, Bottom, Left, Right);

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
            //Not implemented yet
        }

        public void ScaleGPSCoord()
        {
            //Not implemented yet
        }

        public void ScaleScreenCoord()
        {
            //Not implemented yet
        }
        #endregion

    }
}
