using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RKI2.UserControls
{
    /// <summary>
    /// Interaction logic for UC_RKISelector.xaml
    /// </summary>
    public partial class UC_RKISelector : UserControl, INotifyPropertyChanged
    {
        #region LabelCaption
        private string labelCaption = string.Empty;
        public string LabelCap
        {
            get => labelCaption;
            set => SetField(ref labelCaption, value);
            //Auf keinen Fall so (Also das Control direkt ansprechen) machen !
            //RKISelectorLabel.Content = value;
        }
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(LabelCap), 
            typeof(string), typeof(UC_RKISelector));
        #endregion

        #region ComboBoxData

        //Liste mit Combobox-Items
        private List<string> _ItemList;
        public List<string> ItemList
        {
            get => ItemList;
            set => SetField(ref _ItemList, value);
        }

        public static readonly DependencyProperty ItemListProperty = DependencyProperty.Register(nameof(ItemList), 
            typeof(IEnumerable<string>), typeof(UC_RKISelector));

        //SelectedItem
        private object _SelItem;
        public object cbSelectedItem
        {
            get => _SelItem;
            set => SetField(ref _SelItem, value);
        }
        public static readonly DependencyProperty SelectedItem = DependencyProperty.Register(nameof(cbSelectedItem),
            typeof(object), typeof(UC_RKISelector));

        //SelectedIndex
        private int _SelectedIndex;

        public int cbSelectedIndex
        {
            get => _SelectedIndex;
            set => SetField(ref _SelectedIndex, value);
        }
        public static readonly DependencyProperty SelectedIndex = DependencyProperty.Register(nameof(cbSelectedIndex),
            typeof(int), typeof(UC_RKISelector));

        //SelectionChanged
        /* This shit doesn't work
        public object SelectionChanged
        {
            get => (object)GetValue(SelectionChangedProperty);
            set => SetValue(SelectionChangedProperty, value);
        }


        public static readonly DependencyProperty SelectionChangedProperty = DependencyProperty.Register(nameof(SelectionChanged),
            typeof(int), typeof(UC_RKISelector),
            new PropertyMetadata(-1, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cb = d as ComboBox;
            cb?.SetValue(e.Property, (int)e.NewValue);
        }
        */
        #endregion

        public UC_RKISelector()
        {
            InitializeComponent();
            _ItemList = Enumerable.Empty<string>().ToList();
            _SelItem = new object();
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
