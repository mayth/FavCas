/*
 * Copyright (c) 2012 mayth
 * FavCas is released under the MIT license.
 * The Full-text of the license is included in License.txt.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Twitterizer;

namespace FavCas
{
    /// <summary>
    /// <see cref="TwitterUser"/>クラスから文字列への変換を行います。
    /// </summary>
    class UserNameConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TwitterUser user = value as TwitterUser;
            if (user == null)
                return "";
            return user.ScreenName + Environment.NewLine + user.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
