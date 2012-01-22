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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Twitterizer;

namespace FavCas
{
    [ValueConversion(typeof(TwitterUser), typeof(ImageSource))]
    class UserProfileImageConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var user = value as TwitterUser;
            //return user == null ? string.Empty : user.ProfileImageUrl;
            if (user != null && !string.IsNullOrWhiteSpace(user.ProfileImageLocation))
            {
                return new BitmapImage(new Uri(user.ProfileImageLocation));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
