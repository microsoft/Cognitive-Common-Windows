//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Common-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SampleUserControlLibrary
{
    /// <summary>
    /// Interaction logic for SubscriptionKeyPage.xaml
    /// </summary>
    public partial class SubscriptionKeyPage : Page, INotifyPropertyChanged
    {
        private readonly string _isolatedStorageSubscriptionKeyFileName = "Subscription.txt";
        private readonly string _isolatedStorageSubscriptionEndPointFileName = "SubscriptionEndPoint.txt";

        private readonly string _defaultSubscriptionKeyPromptMessage = "Paste your subscription key here firstly";
        private readonly string _defaultSubscriptionEndPointPromptMessage = "Paste your EndPoint here to start";

        private static string s_subscriptionKey, s_subscriptionEndPoint;

        private SampleScenarios _sampleScenarios;
        public SubscriptionKeyPage(SampleScenarios sampleScenarios)
        {
            InitializeComponent();
            _sampleScenarios = sampleScenarios;

            DataContext = this;
            SubscriptionKey = GetSubscriptionKeyFromIsolatedStorage();
            SubscriptionEndPoint = GetSubscriptionEndPointFromIsolatedStorage();
        }

        /// <summary>
        /// Gets or sets subscription key
        /// </summary>
        public string SubscriptionKey
        {
            get
            {
                return s_subscriptionKey;
            }

            set
            {
                s_subscriptionKey = value;
                OnPropertyChanged<string>();
                _sampleScenarios.SubscriptionKey = s_subscriptionKey;
            }
        }

        /// <summary>
        /// Gets or sets subscription endpoint
        /// </summary>
        public string SubscriptionEndPoint
        {
            get
            {
                return s_subscriptionEndPoint;
            }

            set
            {
                s_subscriptionEndPoint = value;
                OnPropertyChanged<string>();
                _sampleScenarios.SubscriptionEndPoint = s_subscriptionEndPoint;
            }
        }

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper function for INotifyPropertyChanged interface
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="caller">Property name</param>
        private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(caller));
            }
        }


        /// <summary>
        /// Gets the subscription key from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionKeyFromIsolatedStorage()
        {
            string subscriptionKey = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Open, isoStore))
                    {
                        using (var reader = new StreamReader(iStream))
                        {
                            subscriptionKey = reader.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionKey = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = _defaultSubscriptionKeyPromptMessage;
            }
            return subscriptionKey;
        }

        /// <summary>
        /// Gets the subscription endpoint from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionEndPointFromIsolatedStorage()
        {
            string subscriptionEndPoint = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndPoint = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndPointFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndPoint = new StreamReader(iStreamForEndPoint))
                        {
                            subscriptionEndPoint = readerForEndPoint.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionEndPoint = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionEndPoint))
            {
                subscriptionEndPoint = _defaultSubscriptionEndPointPromptMessage;
            }
            return subscriptionEndPoint;
        }


        /// <summary>
        /// Saves the subscription key to isolated storage.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        private void SaveSubscriptionKeyToIsolatedStorage(string subscriptionKey)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionKey);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the subscription endpoint to isolated storage.
        /// </summary>
        /// <param name="subscriptionEndPoint">The subscription key.</param>
        private void SaveSubscriptionEndPointToIsolatedStorage(string subscriptionEndPoint)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndPointFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionEndPoint);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the subscription key save button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSubscriptionKeyToIsolatedStorage(SubscriptionKey);
                SaveSubscriptionEndPointToIsolatedStorage(SubscriptionEndPoint);
                //MessageBox.Show(SubscriptionKey + '\n' + SubscriptionEndPoint);
                MessageBox.Show("Subscription key and endpoint is saved in your disk.\nYou do not need to paste the key next time.", "Subscription Key");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to save subscription & endpoint key. Error message: " + exception.Message,
                    "Subscription Key", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SubscriptionKey = _defaultSubscriptionKeyPromptMessage;
                SubscriptionEndPoint = _defaultSubscriptionEndPointPromptMessage;
                SaveSubscriptionEndPointToIsolatedStorage("");
                SaveSubscriptionKeyToIsolatedStorage("");
                MessageBox.Show("Subscription key is deleted from your disk.", "Subscription Key");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to delete subscription key. Error message: " + exception.Message,
                    "Subscription Key", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/cognitive-services/en-us/sign-up");
        }
    }
}
