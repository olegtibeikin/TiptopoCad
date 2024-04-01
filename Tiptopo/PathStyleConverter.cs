using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tiptopo.Model;

namespace Tiptopo
{
    public class PathStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is LineType && parameter is Window)
            {
                Window window = (Window)parameter;
                LineType type = (LineType)value;
                switch (type)
                {
                    case LineType.SmallMetalFence: return window.FindResource("SmallMetalFencePathStyle");
                    case LineType.BigStoneFence: return window.FindResource("BigStoneFencePathStyle");
                    case LineType.BigMetalFence: return window.FindResource("BigMetalFencePathStyle");
                    case LineType.Wall: return window.FindResource("WallPathStyle");
                    case LineType.Continuous: return window.FindResource("ContinuousPathStyle");
                    case LineType.Dotted: return window.FindResource("DottedPathStyle");
                    case LineType.Dashed: return window.FindResource("DashedPathStyle");
                    case LineType.DashDotted: return window.FindResource("DashDottedPathStyle");

                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
