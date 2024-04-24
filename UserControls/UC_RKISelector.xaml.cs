using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace RKI2.UserControls
{
    /// <summary>
    /// Interaction logic for UC_RKISelector.xaml
    /// </summary>
    public partial class UC_RKISelector : UserControl, INotifyPropertyChanged
    {
        #region LabelCaption
        private string labelCaption = string.Empty;
        public string LabelCaption
        {
            get => labelCaption;
            set => SetField(ref labelCaption, value);
            //Auf keinen Fall so (Also das Control direkt ansprechen) machen !
            //RKISelectorLabel.Content = value;
        }
        #endregion

        #region ComboBoxData
        private IEnumerable comboBoxData;

        public IEnumerable ComboBoxData
        {
            get => comboBoxData;
            set => SetField(ref comboBoxData, value);
        }
        #endregion

        public UC_RKISelector()
        {
            InitializeComponent();
            //labelCaption = string.Empty;
        }
        
        //Inhalt der Combobox muss setzbar sein
        //Wenn Combo-Selektion verändert, muss ein Command dranhängbar sein
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
