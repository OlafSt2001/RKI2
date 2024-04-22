﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GeoDataDLL;

namespace RKI2.ViewModels
{
    public class RKIMainViewModel : INotifyPropertyChanged
    {
        private readonly GeoData GeoData;
        private readonly IDataLoader DataLoader;
        private bool DataLoaded;

        public LoadDataCommand LoadData { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RKIMainViewModel()
        {
            DataLoaded = false; //Haben ja noch nichts geladen
            //Events für Load-Button. Hier lösen wir erstmal das Laden der Bundesländer etc. aus
            //später müssen das natürlich Inzidenzdaten sein (historisch, neue gibts leider keine mehr)
            LoadData = new LoadDataCommand(
                (o) => LoadGeoData(),
                (o) => !DataLoaded);

            DataLoader = new GeoDataLoader();
            GeoData = new GeoData(DataLoader);
        }

        private void LoadGeoData()
        {
            if (DataLoaded) 
                return;

            GeoData.LoadGeoData(GeoDataDatasource.CSVFile, @"D:\VC#\RKIConv\bin\Debug\net8.0");
            DataLoaded = true;
            LoadData?.RaiseCanExecuteChanged();
        }
        #region  Implementation INotifyPropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            //PropertyChanged? -> NULL-Check. Wenn NULL (kein Eventhandler zugeordnet), wird nix weiter gemacht
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Deas hier hat sich mir noch nicht so recht erschlossen, habe aber so eine Ahnung
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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