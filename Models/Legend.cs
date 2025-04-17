using System.Windows.Media;

namespace RKI2.Models
{
    public class Legend
    {
        public const int MAX_LEGEND_COUNT = 12;
        private static readonly SolidColorBrush br = new(Color.FromArgb(0xFF, 0xB0, 0, 0));

        public static List<SolidColorBrush> LegendColors =
        [
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
        ];

        private static LegendItem CreateLegendItem(double minVal, double maxVal, int colorIndex)
        {
            return new LegendItem
            {
                InzidenzMin = minVal,
                InzidenzMax = maxVal,
                InzidenzColor = LegendColors[colorIndex],
                InzidenzRangeText = $"{minVal:F0}...{maxVal:F0}"
            };
        }
        
        public static List<LegendItem> CalculateLegend(double minVal, double maxVal, bool isSingleLegendItem = false)
        {
            //Lineare Verteilung
            var liList = new List<LegendItem>();

            if (isSingleLegendItem)
                liList.Add(CreateLegendItem(minVal, maxVal, 0));
            else
            {
                var step = (maxVal - minVal) / (double)MAX_LEGEND_COUNT;
                for (var i = 0; i < MAX_LEGEND_COUNT; i++)
                {

                    var li = CreateLegendItem(i * step + minVal, (i + 1) * step + minVal, i);
                    liList.Add(li);
                }
            }

            return liList;
        }

    }
}
