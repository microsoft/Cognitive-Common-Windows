//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/ProjectOxford-ClientSDK
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

using System;
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
        private readonly string _defaultSubscriptionKeyPromptMessage = "Paste your subscription key here to start";
        private readonly string _defaultEndpointAddressPromptMessage = "Paste your endpoint address here to start";

        private static string s_subscriptionKey;
        private static string s_endpointAddress;

        private SampleScenarios _sampleScenarios;
        public SubscriptionKeyPage(SampleScenarios sampleScenarios)
        {
            InitializeComponent();
            _sampleScenarios = sampleScenarios;

            DataContext = this;
            var info = GetSubscriptionInfoFromIsolatedStorage();
            SubscriptionKey = info.Item1;
            EndpointAddress = info.Item2;
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
        /// Gets or sets subscription key
        /// </summary>
        public string EndpointAddress
        {
            get
            {
                return s_endpointAddress;
            }

            set
            {
                s_endpointAddress = value;
                OnPropertyChanged<string>();
                _sampleScenarios.EndpointAddress = s_endpointAddress;
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
        /// <returns>Tuple of (subscription-key, endpoint-address)</returns>
        private Tuple<string, string> GetSubscriptionInfoFromIsolatedStorage()
        {
            string subscriptionKey = null;
            string endpointAddress = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Open, isoStore))
                    {
                        using (var reader = new StreamReader(iStream))
                        {
                            subscriptionKey = reader.ReadLine();
                            if (!reader.EndOfStream)
                            {
                                endpointAddress = reader.ReadLine();
                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    // Override below
                }
            }
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = _defaultSubscriptionKeyPromptMessage;
            }
            if (string.IsNullOrEmpty(endpointAddress))
            {
                endpointAddress = _defaultEndpointAddressPromptMessage;
            }
            return new Tuple<string, string>(subscriptionKey, endpointAddress);
        }

        /// <summary>
        /// Saves the subscription key to isolated storage.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        /// <param name="endpointAddress">Endpoint address</param>
        private void SaveSubscriptionKeyToIsolatedStorage(string subscriptionKey, string endpointAddress)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionKey);
                        writer.WriteLine(endpointAddress);
                    }
                }
            }
        }

        private void GetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/cognitive-services/en-us/sign-up");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSubscriptionKeyToIsolatedStorage(SubscriptionKey, EndpointAddress);
                MessageBox.Show("Subscription info is saved in your disk.\nYou do not need to paste the key next time.", "Subscription Info");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to save subscription info. Error message: " + exception.Message,
                    "Subscription Info", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SubscriptionKey = _defaultSubscriptionKeyPromptMessage;
                EndpointAddress = _defaultEndpointAddressPromptMessage;
                SaveSubscriptionKeyToIsolatedStorage("", "");
                MessageBox.Show("Subscription info is deleted from your disk.", "Subscription Info");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to delete subscription info. Error message: " + exception.Message,
                    "Subscription Info", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
