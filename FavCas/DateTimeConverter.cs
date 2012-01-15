using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FavCas
{
    /// <summary>
    /// <see cref="DateTime"/>から<see cref="string"/>へ変換します。
    /// </summary>
    class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            dt = dt.ToLocalTime();
            if (dt.Date == DateTime.Now.Date)
                return dt.ToString("T", culture.DateTimeFormat);
            else
                return dt.ToString("T", culture.DateTimeFormat) + Environment.NewLine + dt.ToString("D", culture.DateTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
