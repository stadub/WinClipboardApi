using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClipboardViewer.MvvmBase
{
    [ValueConversion(typeof(bool), typeof(Visibility),ParameterType = typeof(bool))]
    public class VisibilityConverter:IValueConverter
    {
        public VisibilityConverter()
        {
            InvisibleValue = Visibility.Collapsed;
        }

        public Visibility InvisibleValue { get; set; }

        public object Convert(object visible, Type targetType, object reverse, CultureInfo culture)
        {
            if (!(visible is bool))
                throw new ArgumentException("Only Boolean value is allowed", "visible");
            if (targetType != typeof(Visibility))
                throw new ArgumentException("Only Visibility value can be produced", "targetType");

            var isVisible = (bool) visible;

            var isReverse = TryGetBoolValue(reverse).GetValueOrDefault();

            return isVisible ^ isReverse ? Visibility.Visible : InvisibleValue;
        }

        private bool? TryGetBoolValue(object value)
        {
            if (value is bool)
                return (bool)value;
            if (value is int)
            {
                return ((int) value != 0);
            }
            if (value is string)
            {
                var valueString = value.ToString().ToLower();
                bool boolValue;
                if (bool.TryParse(valueString, out boolValue))
                    return boolValue;
                if (valueString == "yes" || valueString == "ok") return true;
                if (valueString == "no" || valueString == "cancel") return false;
                return null;
            }
            if (value != null)
                return System.Convert.ToBoolean(value);
            return null;
        }

        public object ConvertBack(object visible, Type targetType, object reverse, CultureInfo culture)
        {
            if (!(visible is Visibility))
                throw new ArgumentException("Only Visibility value is allowed", "visible");
            if (targetType != typeof(bool))
                throw new ArgumentException("Only Boolean value can be produced", "targetType");

            var visibility= (Visibility)visible;
            var isReverse = TryGetBoolValue(reverse);

            return visibility == Visibility.Visible | isReverse;
        }
    }
}
