
using System.Windows.Media;

namespace RKI2.Models
{
    public class LegendItem
    {
        public double InzidenzMin { get; set; }
        public double InzidenzMax { get; set; }
        public string InzidenzRangeText { get; set; }
        public Brush InzidenzColor { get; set; }
    }
}
