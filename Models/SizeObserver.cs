using System.Windows;

namespace RKI2.Models
{
    public static class SizeObserver
    {
        public static readonly DependencyProperty ObserveProperty =
            DependencyProperty.RegisterAttached(
                "Observe", typeof(bool), typeof(SizeObserver),
                new PropertyMetadata(false, OnObserveChanged));

        public static void SetObserve(FrameworkElement element, bool value) => element.SetValue(ObserveProperty, value);
        public static bool GetObserve(FrameworkElement element) => (bool)element.GetValue(ObserveProperty);

        public static readonly DependencyProperty ObservedSizeProperty =
            DependencyProperty.RegisterAttached(
                "ObservedSize", typeof(Size), typeof(SizeObserver));

        public static void SetObservedSize(FrameworkElement element, Size value) => element.SetValue(ObservedSizeProperty, value);
        public static Size GetObservedSize(FrameworkElement element) => (Size)element.GetValue(ObservedSizeProperty);

        private static void OnObserveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue)
                    fe.SizeChanged += OnSizeChanged;
                else
                    fe.SizeChanged -= OnSizeChanged;
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement fe)
                SetObservedSize(fe, e.NewSize);
        }
    }
}
