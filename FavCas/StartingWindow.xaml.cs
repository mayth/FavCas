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
using System.Diagnostics.Contracts;

namespace FavCas
{
    public enum StartSequenceElement
    {
        LoadCredential,
        VerifyCredential,
        LoadHomeTimeline,
        StartStreaming
    }

    /// <summary>
    /// StartingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class StartingWindow : Window
    {
        Dictionary<StartSequenceElement, bool> isCompleted = new Dictionary<StartSequenceElement, bool>();
        public event EventHandler OnCompleted;

        public StartingWindow()
        {
            InitializeComponent();

            foreach (var item in Enum.GetNames(typeof(StartSequenceElement)))
                isCompleted.Add((StartSequenceElement)Enum.Parse(typeof(StartSequenceElement), item), false);
        }

        public string UserName
        {
            set
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(value));

                System.Diagnostics.Debug.WriteLine("[StartingWindow]UserName changed: " + value);
                greetingText.Text = "Hello, " + value;
            }
        }

        public string Status
        {
            set
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(value));

                System.Diagnostics.Debug.WriteLine("[StartingWindow]Status changed: " + value);
                statusText.Text = value;
            }
        }

        public void Complete(StartSequenceElement sequence)
        {
            isCompleted[sequence] = true;
            switch (sequence)
            {
                case StartSequenceElement.LoadCredential:
                    this.Dispatcher.Invoke(new Action(() => indLoadCredential.Visibility = System.Windows.Visibility.Visible));
                    break;
                case StartSequenceElement.VerifyCredential:
                    this.Dispatcher.Invoke(new Action(() => indVerifyCredential.Visibility = System.Windows.Visibility.Visible));
                    break;
                case StartSequenceElement.LoadHomeTimeline:
                    this.Dispatcher.Invoke(new Action(() => indLoadHomeTimeline.Visibility = System.Windows.Visibility.Visible));
                    break;
                case StartSequenceElement.StartStreaming:
                    this.Dispatcher.Invoke(new Action(() => indStartStreaming.Visibility = System.Windows.Visibility.Visible));
                    break;
                default:
                    throw new InvalidOperationException();
            }
            if (isCompleted.All(pair => pair.Value))
                OnCompleted(this, EventArgs.Empty);
        }
    }
}
