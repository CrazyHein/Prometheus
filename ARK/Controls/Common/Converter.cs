using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    public class InvertBooleanValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class ComponentStateVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool modified = (bool)values[0];
            bool validated = (bool)values[1];
            if (modified == false && validated == false)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ComponentStateImage : IMultiValueConverter
    {
        public static BitmapImage BlankIcon { get; } = new BitmapImage(new Uri("pack://application:,,,/imgs/blank.png"));
        public static BitmapImage ModifiedIcon { get; } = new BitmapImage(new Uri("pack://application:,,,/imgs/modified.png"));
        public static BitmapImage ValidatedIcon { get; } = new BitmapImage(new Uri("pack://application:,,,/imgs/validated.png"));
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool modified = (bool)values[0];
            bool validated = (bool)values[1];
            if (modified == false && validated == false)
                return BlankIcon;
            else
            {
                if (validated == true)
                    return ValidatedIcon;
                else
                    return ModifiedIcon;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanValueBrush : IValueConverter
    {
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Red);
        public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.Transparent);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value ? TrueBrush : FalseBrush;
            else
                return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ShaderRecordBackgroundBrush : IMultiValueConverter
    {
        public Brush Default { get; set; } = new SolidColorBrush(Colors.Transparent);
        public Brush Invalid { get; set; } = new SolidColorBrush(Colors.Red);
        public Brush CanBeOmitted { get; set; } = new SolidColorBrush(Colors.BlanchedAlmond);
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isValid = (bool)values[0];
            bool canBeOmitted = (bool)values[1];
            if (isValid == false)
                return Invalid;
            else if (canBeOmitted == true)
                return CanBeOmitted;
            else
                return Default;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionControlIsDirtyBrush : IValueConverter
    {
        public Brush DirtyBrush { get; set; } = new SolidColorBrush(Colors.Red);
        public Brush CleanBrush { get; set; } = new SolidColorBrush(Colors.Transparent);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data = (value as UserControl)?.DataContext;
            if (data is ComponentCollection<Component>)
                return (data as ComponentCollection<Component>).IsDirty ? DirtyBrush : CleanBrush;
            else
                return CleanBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ByteArrayToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] && targetType == typeof(string) && parameter is string)
            {
                byte[] array = value as byte[];
                string sep = parameter as string;
                return string.Join(sep, array.Select(x => x.ToString("X2")));
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
