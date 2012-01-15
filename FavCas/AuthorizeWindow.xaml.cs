using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FavCas
{
    /// <summary>
    /// AuthorizeWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthorizeWindow : Window
    {
        public Uri AuthUri { get; set; }
        public string PinCode { get; set; }

        public AuthorizeWindow()
        {
            InitializeComponent();
            AuthUri = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AuthUri == null)
            {
                DialogResult = false;
                return;
            }
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AuthUri.ToString());
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            string pinCode = pincodeBox.Text;
            if (string.IsNullOrWhiteSpace(pinCode) || !System.Text.RegularExpressions.Regex.IsMatch(pinCode, "[0-9]+"))
            {
                MessageBox.Show("PINコードが入力されていないか、無効な値です。認証後に表示されたPINコードを正しく入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            PinCode = pinCode;
            DialogResult = true;
        }
    }
}
