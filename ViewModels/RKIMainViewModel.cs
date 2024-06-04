using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using DataDLLInterfaces;
using GeoDataDLL;
using InzidenzDataDLL;
using RKI2.Models;
using BundeslandRecord = DataDLLInterfaces.BundeslandRecord;

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

        //Für Legende
        private const int MAX_LEGEND_COUNT = 12;
        private SolidColorBrush br = new (Color.FromArgb(0xFF, 0xB0, 0, 0));
        private readonly List<SolidColorBrush> LegendColors;

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
        private List<LegendItem> _Inzidenzen = [];
        public List<LegendItem> Inzidenzen
        {
            get => _Inzidenzen;
            set => SetField(ref _Inzidenzen, value);
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

            KreisData.LoadData(@"Z:\Temp\01062023.csv");
            MinKreisId = KreisData.VisualizeData.Min(dr => dr.Id);
            MaxKreisId = KreisData.VisualizeData.Max(dr => dr.Id);
            //Legendenfarben intialisieren. Wegen einer benutzerdefinierten Farbe können wir das nicht in der
            //Deklaration machen, der Compiler weigert sich standhaft
            LegendColors = new List<SolidColorBrush>(MAX_LEGEND_COUNT)
            {
                Brushes.Green,
                Brushes.GreenYellow,
                Brushes.BurlyWood,
                Brushes.Silver,
                Brushes.SlateGray,
                Brushes.PeachPuff,
                Brushes.Coral,
                Brushes.Red,
                br,
                Brushes.DarkRed,
                Brushes.DarkOrchid,
                Brushes.DarkSlateBlue
            };
            SetupInzidenzData();
        }

        private void SetupInzidenzData()
        {
            _Inzidenzen.Clear();
            double minVal, finalMinVal = double.MaxValue;
            double maxVal, finalMaxVal = double.MinValue;
            for (int i = MinKreisId; i < MaxKreisId; i++)
            {
                (minVal, maxVal) = KreisData.GetMinMaxValueForKreis(i);
                if (double.IsNaN(minVal))
                    continue;
                if (minVal < finalMinVal)
                    finalMinVal = minVal;
                if (maxVal > finalMaxVal)
                    finalMaxVal = maxVal;
            }

            double step = (finalMaxVal - finalMinVal) / (double)MAX_LEGEND_COUNT;
            for (int i = 0; i < MAX_LEGEND_COUNT; i++)
            {
                LegendItem li = new()
                {
                    InzidenzMin = i * step,
                    InzidenzMax = (i + 1) * step,
                    InzidenzColor = LegendColors[i]
                };
                li.InzidenzRangeText = $"{li.InzidenzMin:F2}...{li.InzidenzMax:F2}";
                _Inzidenzen.Add(li);
            }
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
            if (bundeslandIndex == 0)
            {
                //(kein) ausgewählt
                //Deutschlandkarte zeichnen
                return;
            }
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
