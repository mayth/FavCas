using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Twitterizer;

namespace FavCas
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 起動時に取得する件数
        /// </summary>
        public static readonly int FirstGetCount = 100;
        public static readonly string UserAgent = "FavCas/1.0 beta (WPF; @maytheplic)";
        static readonly Brush RelatedStatusBrush = Brushes.MediumSpringGreen;
        ObservableCollection<TwitterStatus> timeline;
        TwitterUser currentUser;
        bool isHilightEnabled = false;
        delegate void AddTimeLineDelegate(TwitterStatus status);
        OAuthTokens tokens;
        Twitterizer.Streaming.TwitterStream twitterStream;

        public static readonly RoutedCommand FavoriteCommand = new RoutedCommand();
        public static readonly RoutedCommand UnfavoriteCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartingWindow startWindow = new StartingWindow();
            startWindow.Owner = this;
            startWindow.Show();
            this.Visibility = System.Windows.Visibility.Hidden;

            var startupTask = Task.Factory.StartNew(() =>
                {
                    // 認証情報を読み込む（認証情報がなければ認証する）
                    var authTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () =>
                        {
                            if (Authenticate())
                                startWindow.Complete(StartSequenceElement.LoadCredential);
                            else
                                throw new AuthenticationFailureException();
                        });

                    try
                    {
                        authTask.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var ie in ae.InnerExceptions)
                        {
                            if (ie is AuthenticationFailureException)
                            {
                                this.Dispatcher.Invoke(new Action(() => Application.Current.Shutdown()), null);
                                return;
                            }
                        }
                    }
                    var verifyTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () =>
                        {
                            VerifyCredential();
                            startWindow.Dispatcher.BeginInvoke(new Action(() => startWindow.UserName = "@" + currentUser.ScreenName), null);
                            startWindow.Complete(StartSequenceElement.VerifyCredential);
                        });

                    var timelineTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () =>
                        {
                            GetHomeTimeline();
                            startWindow.Complete(StartSequenceElement.LoadHomeTimeline);
                        });

                    var streamingTask = System.Threading.Tasks.Task.Factory.StartNew(
                        () =>
                        {
                            StartStream();
                            startWindow.Complete(StartSequenceElement.StartStreaming);
                        });
                });
            startWindow.OnCompleted += (_sender, _e) =>
                {
                    startWindow.Dispatcher.BeginInvoke(new Action(() => startWindow.Status = "Complete!"), null);
                    System.Threading.Thread.Sleep(2000);
                    startWindow.Dispatcher.BeginInvoke(new Action(() => startWindow.Close()), null);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.Title = this.Title + " - @" + currentUser.ScreenName;
                            this.Visibility = System.Windows.Visibility.Visible;
                        }), null);
                };
        }

        private void StartStream()
        {
            twitterStream = new Twitterizer.Streaming.TwitterStream(tokens, UserAgent, null);
            // start(friend list), stop(error), statusCreated, statusDeleted, messageCreated, messageDeleted, Event
            twitterStream.StartUserStream(null, null, StatusCreatedCallback, null, null, null, null);
        }

        private void GetHomeTimeline()
        {
            var timelineResult = TwitterTimeline.HomeTimeline(tokens, new TimelineOptions() { Count = FirstGetCount });
            if (timelineResult.Result != RequestResult.Success)
            {
                MessageBox.Show("Failed getting your home timeline. This application will be shutdown." + Environment.NewLine
                    + "Result: " + timelineResult.Result.ToString() + Environment.NewLine
                    + "Error message:" + Environment.NewLine
                    + timelineResult.ErrorMessage,
                    "Getting timeline error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            timeline = new ObservableCollection<TwitterStatus>(timelineResult.ResponseObject);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                CollectionViewSource cvs = new CollectionViewSource();
                var sd = new System.ComponentModel.SortDescription();
                sd.Direction = System.ComponentModel.ListSortDirection.Descending;
                sd.PropertyName = "CreatedDate";
                cvs.SortDescriptions.Add(sd);
                cvs.Source = timeline;
                timeLineView.ItemsSource = cvs.View;
            }), null);
        }

        private void VerifyCredential()
        {
            var verifyResult = TwitterAccount.VerifyCredentials(tokens);
            if (verifyResult.Result != RequestResult.Success)
            {
                MessageBox.Show("Failed account verification. This application will be shutdown." + Environment.NewLine
                    + "Result: " + verifyResult.Result.ToString() + Environment.NewLine
                    + "Error message:" + Environment.NewLine
                    + verifyResult.ErrorMessage,
                    "Verification error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            currentUser = verifyResult.ResponseObject;
        }

        private bool Authenticate()
        {
            Authentication auth = GetAuthenticationData();
            if (auth == null)
            {
                return false;
            }
            tokens = new OAuthTokens()
            {
                ConsumerKey = Properties.Resources.ConsumerKey,
                ConsumerSecret = Properties.Resources.ConsumerSecret,
                AccessToken = auth.Token,
                AccessTokenSecret = auth.TokenSecret
            };
            return true;
        }

        void StatusCreatedCallback(TwitterStatus status)
        {
            Contract.Requires<ArgumentNullException>(status != null);

            if (timeline != null)
                Dispatcher.Invoke(new Action<TwitterStatus>(x => timeline.Add(x)), status);
        }

        void AddTimeline(TwitterStatus status)
        {
            Contract.Requires<ArgumentNullException>(status != null);

            timeline.Add(status);
        }

        void Favorite(TwitterStatus status)
        {
            TwitterFavoriteAsync.Create(tokens, status.Id, null, TimeSpan.FromSeconds(10), res => { });
        }

        void Unfavorite(TwitterStatus status)
        {
            TwitterFavoriteAsync.Delete(tokens, status.Id, null, TimeSpan.FromSeconds(10), res => { });
        }

        void Retweet(TwitterStatus status)
        {
            if (!status.Retweeted)
                TwitterStatusAsync.Retweet(tokens, status.Id, null, TimeSpan.FromSeconds(10), res => { });
        }

        void CancelRetweet(TwitterStatus status)
        {
            if (status.Retweeted)
                TwitterStatusAsync.Delete(tokens, status.Id, null, TimeSpan.FromSeconds(10), res => { });
        }

        Authentication GetAuthenticationData()
        {
            Authentication auth = new Authentication();
            if (!auth.IsAuthorized)
            {
                OAuthTokenResponse requestToken = OAuthUtility.GetRequestToken(Properties.Resources.ConsumerKey, Properties.Resources.ConsumerSecret, "oob");
                Uri authUri = OAuthUtility.BuildAuthorizationUri(requestToken.Token);
                string pinCode = null;

                var dispatcher = Application.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    pinCode = ShowAuthorizeWindow(authUri);
                }
                else
                {
                    dispatcher.Invoke(new Action(() => pinCode = ShowAuthorizeWindow(authUri)), null);
                }
                if (!string.IsNullOrEmpty(pinCode))
                {
                    OAuthTokenResponse accessToken = OAuthUtility.GetAccessToken(Properties.Resources.ConsumerKey, Properties.Resources.ConsumerSecret, requestToken.Token, pinCode);

                    // 認証情報を保存する
                    auth.Token = accessToken.Token;
                    auth.TokenSecret = accessToken.TokenSecret;
                    auth.Save();
                }
                else
                {
                    return null;
                }
            }
            return auth;
        }

        string ShowAuthorizeWindow(Uri authUri)
        {
            AuthorizeWindow authWindow = new AuthorizeWindow();
            authWindow.AuthUri = authUri;
            authWindow.Owner = this;
            if (authWindow.ShowDialog().Value)
                return authWindow.PinCode;
            else
                return null;
        }

        private void timeLineView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TwitterStatus status = timeLineView.SelectedItem as TwitterStatus;

            if (status == null)
            {
                profileImage.Source = null;
                userNameBlock.Text = string.Empty;
                dateBlock.Text = string.Empty;
                tweetFromBlock.Text = string.Empty;
                statusTextBlock.Text = string.Empty;
                return;
            }

            #region Update Status Detail view
            Uri apiUri;
            string apiName;
            var match = System.Text.RegularExpressions.Regex.Match(status.Source, "<a href=\"(?<url>.*?)\".*?>(?<text>.*?)</a>");
            if (!match.Success)
            {
                apiUri = null;
                apiName = status.Source;
            }
            else
            {
                apiUri = new Uri(match.Groups["url"].Value);
                apiName = match.Groups["text"].Value;
            }

            var inlines = tweetFromBlock.Inlines;
            inlines.Clear();
            inlines.Add("From: ");
            if (apiUri != null)
            {
                var hyperlink = new Hyperlink(new Run(apiName));
                hyperlink.Click += (c_sender, c_e) => System.Diagnostics.Process.Start(apiUri.ToString());
                hyperlink.NavigateUri = apiUri;
                inlines.Add(hyperlink);
            }
            else
                inlines.Add(apiName);

            if (status.User != null)
            {
                BitmapImage imageSource = string.IsNullOrWhiteSpace(status.User.ProfileImageLocation) ? null : new BitmapImage(new Uri(status.User.ProfileImageLocation));
                profileImage.Source = imageSource;
                userNameBlock.Text = status.User.Name;
            }
            else
            {
                profileImage.Source = null;
                userNameBlock.Text = "!ERROR!";
            }
            dateBlock.Text = status.CreatedDate.ToLocalTime().ToString("F");
            statusTextBlock.Text = status.Text;
            #endregion

            #region Highlight mentioning users' post
            if (isHilightEnabled)
            {
                for (int i = 0; i < timeLineView.Items.Count; i++)
                {
                    var item = timeLineView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (item == null || item.Background != RelatedStatusBrush)
                        continue;
                    item.Background = Brushes.White;
                }
                var relatedStatuses = GetRelatedStatuses(status);
                foreach (var related in relatedStatuses)
                {
                    var item = timeLineView.ItemContainerGenerator.ContainerFromItem(related) as ListViewItem;
                    if (item == null)
                        continue;
                    item.Background = RelatedStatusBrush;
                }
            }
            #endregion
        }

        private IEnumerable<TwitterStatus> GetRelatedStatuses(TwitterStatus status)
        {
            Contract.Requires<ArgumentNullException>(status != null);

            TwitterStatusCollection result = new TwitterStatusCollection();
            TwitterStatus temp = status;

            while (temp.InReplyToStatusId.HasValue)
            {
                var inReplyTo = TwitterStatus.Show(temp.InReplyToStatusId.Value).ResponseObject;
                result.Add(inReplyTo);
                temp = inReplyTo;
            }
            return result;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (twitterStream != null)
            {
                twitterStream.EndStream();
                twitterStream.Dispose();
                twitterStream = null;
            }
        }

        private void timeLineView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            if (item != null)
            {
                var status = (TwitterStatus)item.DataContext;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if (status.IsFavorited.HasValue && status.IsFavorited.Value)
                            Unfavorite(status);
                        else
                            Favorite(status);
                        break;
                    case MouseButton.Right:
                        if (status.Retweeted)
                            CancelRetweet(status);
                        else
                            Retweet(status);
                        break;
                    case MouseButton.Middle:
                        timeLineView.SelectedItem = item.DataContext;
                        break;
                }
            }
            e.Handled = true;
        }
    }
}
