using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace RKI2.UserControls
{
    /// <summary>
    /// Interaction logic for UC_RKISelector.xaml
    /// </summary>
    public partial class UC_RKISelector : UserControl
    {
        public UC_RKISelector()
        {
            InitializeComponent();
        }
        
        //Caption des Labels muss setzbar sein
        //Inhalt der Combobox muss setzbar sein
        //Wenn Combo-Selektion verändert, muss ein Command dranhängbar sein
    }
}
