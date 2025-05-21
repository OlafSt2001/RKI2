using DataDLLInterfaces;
using GeoDataDLL;
using InzidenzDataDLL;
using RKI2.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RKI2.ViewModels
{
    public class RKIMainViewModel : INotifyPropertyChanged
    {
        private readonly GeoData GeoData;
        private readonly IDataLoader DataLoader;

        private bool DataLoaded;

        //Inzidenzen
        private readonly IKreisValues KreisData;
        private int MinKreisId;

        private int MaxKreisId;

        //Für Legende - alles verschoben nach Klasse Legend
        //private const int MAX_LEGEND_COUNT = 12;
        //private SolidColorBrush br = new (Color.FromArgb(0xFF, 0xB0, 0, 0));
        //private readonly List<SolidColorBrush> LegendColors;

        //Skalierung zum Zeichnen der Map
        private MinMaxKoord minMaxKoord;

        //Handler für unseren Button
        public DelegateCommand LoadData { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }

        //Für INotifyPropertyChanged benötigt
        public event PropertyChangedEventHandler? PropertyChanged;


        #region Data for our UserControl "Bundesland"
        private List<string> _BundeslandData;
        public List<string> BundeslandData
        {
            get => _BundeslandData;
            set => SetField(ref _BundeslandData, value);
        }

        private string SelectedBundeslandItemItem;
        public string SelectedBundeslandItem
        {
            get => SelectedBundeslandItemItem;
            set
            {
                SetField(ref SelectedBundeslandItemItem, value);
                //Hier mehr Zeug für Landkreis-Combo
            }
        }

        private int _SelectedBundeslandIndex;
        public int SelectedBundeslandIndex
        {
            get => _SelectedBundeslandIndex;
            set
            {
                SetField(ref _SelectedBundeslandIndex, value);
                //Oder hier mehr Zeuh für Landkreis-Combo
                FillLandkreisCombo(_SelectedBundeslandIndex);
                //Process Legend here !!!
                SetupBundeslandInzidenzData(value);
                OnPropertyChanged(nameof(Inzidenzen));
                DrawMap(_SelectedBundeslandIndex, -1);
            }
        }
        #endregion

        #region Data for our UserControl "Kreise"
        private int _SelectedLandkreisIndex;
        public int SelectedLandKreisIndex
        {
            get => _SelectedLandkreisIndex;
            set
            {
                SetField(ref _SelectedLandkreisIndex, value);
                //Landkreis zeichnen. Achtung: Durch das eingefügte (kein) muss vom Index
                //eins abgezogen werden, um den korrekten Index in LandkreisData-List zu
                //bekommen !
            }
        }

        private List<string> _LandkreisData;
        public List<string> LandkreisData
        {
            get => _LandkreisData;
            set => SetField(ref _LandkreisData, value);
        }
        #endregion

        #region Data for our Legend
        private IEnumerable<LegendItem> _Inzidenzen = [];
        public IEnumerable<LegendItem> Inzidenzen
        {
            get => _Inzidenzen;
            set => SetField(ref _Inzidenzen, value);
        }
        #endregion

        #region Zeichenfläche Größenproperty
        private Size _canvasSize;
        public Size CanvasSize
        {
            get => _canvasSize;
            set 
            {
                minMaxKoord?.SetRect(value);
                SetField(ref _canvasSize, value);
            }
        }
        #endregion

        #region Constructor
#pragma warning disable CS8618
        public RKIMainViewModel()
#pragma warning restore CS8618
        {
            SetupButtonHandlers();

            //Setup Map Data and Bundesland- and KreisData
            DataLoader = new GeoDataLoader();
            GeoData = new GeoData(DataLoader);
            _BundeslandData = Enumerable.Empty<string>().ToList();
            _BundeslandData.Add("(kein)");
            _SelectedBundeslandIndex = -1;
            SelectedBundeslandItemItem = string.Empty;

            _LandkreisData = Enumerable.Empty<string>().ToList();
            _SelectedLandkreisIndex = -1;

            //Setup InzidenzData, this only for testing
            KreisData = new InzidenzData();
            KreisData.VisualizeData = new();

            KreisData.LoadData(@"Z:\Temp\01122021.csv");
            MinKreisId = KreisData.VisualizeData.Min(dr => dr.Id);
            MaxKreisId = KreisData.VisualizeData.Max(dr => dr.Id);
            //Legendenfarben intialisieren. Wegen einer benutzerdefinierten Farbe können wir das nicht in der
            //Deklaration machen, der Compiler weigert sich standhaft
            //LegendColors = new List<SolidColorBrush>(MAX_LEGEND_COUNT)
            //{
            //    Brushes.Green,
            //    Brushes.GreenYellow,
            //    Brushes.BurlyWood,
            //    Brushes.Silver,
            //    Brushes.SlateGray,
            //    Brushes.PeachPuff,
            //    Brushes.Coral,
            //    Brushes.Red,
            //    br,
            //    Brushes.DarkRed,
            //    Brushes.DarkOrchid,
            //    Brushes.DarkSlateBlue
            //};
            SetupFullInzidenzData();
            //Vorbereitung für Skalierung
            minMaxKoord = new MinMaxKoord();
            //Das Rect wird über den SizeObserver gesetzt. Wir füllen jetzt die Geokoordinaten ein

            
        }


        private void SetupFullInzidenzData()
        {
            double finalMinVal = double.MaxValue;
            double finalMaxVal = double.MinValue;
            for (int i = MinKreisId; i < MaxKreisId; i++)
                (finalMinVal, finalMaxVal) = GetFinalMinMaxVal(i, finalMinVal, finalMaxVal);
            Inzidenzen = Legend.CalculateLegend(finalMinVal, finalMaxVal);
        }

        private (double minVal, double maxVal) GetFinalMinMaxVal(int KreisId, double finalMinVal, double finalMaxVal)
        {
            (double minV, double maxV) = KreisData.GetMinMaxValueForKreis(KreisId);
            if (double.IsNaN(minV))
                return (double.MaxValue, double.MinValue);
            if (minV < finalMinVal)
                finalMinVal = minV;
            if (maxV > finalMaxVal)
                finalMaxVal = maxV;
            return (finalMinVal, finalMaxVal);
        }

        private void SetupBundeslandInzidenzData(int BLID)
        {
            var p = GeoData.GetAllKreisForBundesland(BLID);
            var KreisIds = p.Select(kr => kr.KreisId).ToList();
            double finalMinVal = double.MaxValue;
            double finalMaxVal = double.MinValue;
            //Wenn wir nur einen Landkreis haben (HH z.B.) dann direkt berechnen
            if (KreisIds.Count() == 1)
                (finalMinVal, finalMaxVal) = GetFinalMinMaxVal(KreisIds[0], finalMinVal, finalMaxVal);
            else
                //Ansonsten halt über alle Kreise
                for (var i = KreisIds.Min(KRID => KRID); i < KreisIds.Max(KRID => KRID); i++)
                    (finalMinVal, finalMaxVal) = GetFinalMinMaxVal(i, finalMinVal, finalMaxVal);

            Inzidenzen = Legend.CalculateLegend(finalMinVal, finalMaxVal, KreisIds.Count == 1);
        }
        #endregion

        private void GetMinMaxKreisID()
        {
            var BL = GeoData.GetAllBundesland();
            int MaxBLID = BL.Select(br => br.BundeslandId).Max();
            int MinBLID = BL.Select(br => br.BundeslandId).Min();
            MinKreisId = GeoData.GetAllKreisForBundesland(MinBLID).Select(kr => kr.KreisId).Min();
            MaxKreisId = GeoData.GetAllKreisForBundesland(MaxBLID).Select(kr => kr.KreisId).Max();
        }

        private void FillLandkreisCombo(int bundeslandIndex)
        {
            _LandkreisData.Clear();
            SelectedLandKreisIndex = -1;

            if (bundeslandIndex == 0)
                //(kein) ausgewählt
                //Deutschlandkarte zeichnen
                return;

            _LandkreisData.Add("(kein)");
            var p = GeoData.GetAllKreisForBundesland(bundeslandIndex);
            p.ForEach(kr => _LandkreisData.Add(kr.KreisName));
            OnPropertyChanged(nameof(LandkreisData));
        }

        private void SetupButtonHandlers()
        {
            DataLoaded = false; //Haben ja noch nichts geladen
            //Events für Load-Button. Hier lösen wir erstmal das Laden der Bundesländer etc. aus
            //später müssen das natürlich Inzidenzdaten sein (historisch, neue gibts leider keine mehr)
            LoadData = new DelegateCommand(
                (o) => LoadGeoData(),
                (o) => !DataLoaded);

            ExitCommand = new DelegateCommand((o) => Application.Current.Shutdown(0));
        }

        private void LoadGeoData()
        {
            if (DataLoaded)
                return;

            GeoData.LoadGeoData(@"D:\VC#\RKIConv\bin\Debug\net8.0");
            DataLoaded = true;
            //Assign Bundesland-Stuff to our Binding property
            GeoData.GetAllBundesland().ForEach(br => _BundeslandData.Add(br.BundeslandName));
            //Get MinMaxIds for Kreise
            GetMinMaxKreisID();
            //Legende neu aufbauen
            SetupFullInzidenzData();
            //Tell everyone that this prop has changed
            OnPropertyChanged(nameof(BundeslandData));
            //Tell everyone that we have data
            LoadData.RaiseCanExecuteChanged();
        }

        #region Map zeichnen
        private void DrawMap(int BLID, int KRID)
        {
            if (BLID < 1) //Ganz Deutschland
            {
                DrawFullMap();
                return;
            }

            if (KRID < 1)
            {
                DrawBundesland(BLID);
                return;
            }

            DrawKreis(KRID);
        }

        private void DrawFullMap()
        {
            //Not yet implemented
        }

        private void DrawBundesland(int BLID)
        {
            //Not yet implemented
        }

        private void DrawKreis(int KRID)
        {
            //Not yet implemented
        }

        /*
         Was brauchen wir ?

        1. Nördlichste Koordinate
        2. Südlichste Kordinate
        3. Westlichste Koordinate
        4. Östlichste Koordinate

        5. Diese Umwandeln in Bildschirmkoordinaten (Top, Bottom, Left, Right)
        6. Skalieren, das die berechnet Box gut passt (Aspect Ratio beibehalten)
        7. Jede Koordinate nun mit Skalierung in Bildschirmkoordinate wandeln
        8. Pixel setzen
        9. Floodfill mit Inziodenzfarbe
        */
        #endregion
        #region Old drawing methods
        /*
        private Point ScaleCoordToPoint(float Lat, float Long)
        {
            Point result = new()
            {
                X = (Lat - mmc.minLat) / ScaleHorizontal,
                Y = TheCanvas.ActualHeight - ((Long - mmc.minLong) / ScaleVertical)
            };

            return result;

        }

        private void DrawElement(float[][][] data, double Inzidenz, string KreisName = "")
        {
            //Und los gehts mit Zeichnen
            //List<PathFigure> li = new();

            for (int areaCount = 0; areaCount < data.Length; areaCount++)
            {

                PathGeometry pg = new();

                //Startpunkt ermitteln
                Point startPoint = ScaleCoordToPoint(data[areaCount][0][0], data[areaCount][0][1]);
                PointsDrawn++;

                //PathGeometry zeichnen
                PathFigure path = new()
                {
                    StartPoint = startPoint
                };

                //Anpassung für Granularität
                //for (int coordCount = 1; coordCount < data[areaCount].Length; coordCount++)
                for (int coordCount = 1; coordCount < data[areaCount].Length; coordCount += Granularity)
                {
                    Point pt = ScaleCoordToPoint(data[areaCount][coordCount][0], data[areaCount][coordCount][1]);
                    PointsDrawn++;
                    path.Segments.Add(new LineSegment(pt, true));
                }
                pg.Figures.Add(path);

                System.Windows.Shapes.Path p = new()
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = GetInzidenzColor(Inzidenz),//Brushes.AliceBlue;  //FillColor hier
                    Data = pg,
                    Tag = KreisName
                };

                p.MouseEnter += AreaEnter;
                p.MouseLeave += AreaLeave;
                //Trigger hier anklemmen
                TheCanvas.Children.Add(p);
            }
        }

        private async Task DrawKreis(string KreisName, bool withRescale = true)
        {
            var t = await Rootobject2.GetCountyAreaAsync(json2, KreisName);

            if (withRescale)
            {
                Rootobject2.FindMinMaxCoords(t, mmc);
                ReScale();
            }
            double Inzidenz = await Rootobject2.GetCountyInzidenzAsync(json2, KreisName);
            DrawElement(t, Inzidenz, KreisName);

        }

        private async Task DrawBundesland(int BundeslandID, CancellationToken ct)
        {
            //Not yet implemented
            //Finde heraus, wie man an die Dimensionen des Bundeslandes kommt
            //Diese braucht man für das Rescale
            //Dann alle Landkreise zeichnen -> ergibt das komplette Bundesland
            List<double> InzList = new();
            //Landkreise zeichnen

            List<string> li = await Rootobject2.GetAllKreiseAsync(JSonAsList, BundeslandID + 1);
            mmc.Reset();

            foreach (var krName in li)
            {
                if (ct.IsCancellationRequested)
                    return;

                var t1 = Rootobject2.GetCountyAreaAsync(json2, krName);
                var t2 = Rootobject2.GetCountyInzidenzAsync(json2, krName);
                Task.WaitAll(new Task[] { t1, t2 }, cancellationToken: ct);

                float[][][] f = t1.Result;
                double Inzidenz = t2.Result;
                //float[][][] f = await Rootobject2.GetCountyAreaAsync(json2, krName);
                ////Inzidenz für den Kreis hier auch gleich ermitteln und ab in ne Liste
                //double Inzidenz = await Rootobject2.GetCountyInzidenzAsync(json2, krName);
                InzList.Add(Inzidenz);
                //Dann verteilen auf die Liste mit Inzidenzen
                mmc = Rootobject2.FindMinMaxCoords(f, mmc);
            }
            ReScale();
            InzList.Sort();
            InzList.ForEach(d => Debug.Print($"Inz = {d}"));
            //Debug.Print(InzList.ToString);
            foreach (var krName in li)
            {
                if (ct.IsCancellationRequested)
                    return;
                await DrawKreis(krName, false);
            }
        }

        private void ReScale(Size e)
        {
            double d = mmc.maxLat - mmc.minLat;
            ScaleHorizontal = d / e.Width;

            d = mmc.maxLong - mmc.minLong;
            ScaleVertical = d / e.Height;

        }

        private void ReScale()
        {
            Size e = new()
            {
                Width = TheCanvas.ActualWidth,
                Height = TheCanvas.ActualHeight
            };
            ReScale(e);

        }
        */
        #endregion

        #region  Implementation INotifyPropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            //PropertyChanged? -> NULL-Check. Wenn NULL (kein Eventhandler zugeordnet), wird nix weiter gemacht
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            //Hat sich wirklich was geändert ? Wenn nicht, dann gleich wieder raus hier
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            //Wert eintragen und PropertyChanged auslösen
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
