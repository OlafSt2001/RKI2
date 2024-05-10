using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using GeoDataDLL;
using RKI2.Models;

namespace RKI2.ViewModels
{
    public class RKIMainViewModel : INotifyPropertyChanged
    {
        private readonly GeoData GeoData;
        private readonly IDataLoader DataLoader;
        private bool DataLoaded;
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

            DataLoader = new GeoDataLoader();
            GeoData = new GeoData(DataLoader);
            _BundeslandData = Enumerable.Empty<string>().ToList();
            _BundeslandData.Add("(kein)");
            _SelectedBundeslandIndex = -1;
            SelectedBundeslandItemItem = string.Empty;

            _LandkreisData = Enumerable.Empty<string>().ToList();
            _SelectedLandkreisIndex = -1;

            var li = new LegendItem
            {
                InzidenzColor = Brushes.GreenYellow,
                InzidenzMin = 0,
                InzidenzMax = 5,
                InzidenzRangeText = "bis 5"
            };
            _Inzidenzen.Add(li);

        }
        #endregion
        
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
            //Tell everyone that this prop has changed
            OnPropertyChanged(nameof(BundeslandData));
            //Tell everyone that we have data
            LoadData.RaiseCanExecuteChanged();
        }
       
        #region  Implementation INotifyPropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            //PropertyChanged? -> NULL-Check. Wenn NULL (kein Eventhandler zugeordnet), wird nix weiter gemacht
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Deas hier hat sich mir noch nicht so recht erschlossen, habe aber so eine Ahnung
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) 
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

    }
}
