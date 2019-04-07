//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PhotoLab
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private ImageFileInfo persistedItem;

        public ObservableCollection<ImageFileInfo> Images { get; } = new ObservableCollection<ImageFileInfo>();
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;

            if (Images.Count == 0)
            {
                await GetItemsAsync();
            }

            base.OnNavigatedTo(e);
        }

        private async Task GetItemsAsync()
        {
            // https://docs.microsoft.com/uwp/api/windows.ui.xaml.controls.image#Windows_UI_Xaml_Controls_Image_Source
            // See "Using a stream source to show images from the Pictures library".
            // This code is modified to get images from the app folder.

            // Get the app folder where the images are stored.
            StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets\\Samples");

            // Get and process files in folder
            IReadOnlyList<StorageFile> fileList = await assets.GetFilesAsync();
            foreach (StorageFile file in fileList)
            {
                // Limit to only png or jpg files.
                if (file.ContentType == "image/png" || file.ContentType == "image/jpeg")
                {
                    ImageFileInfo image = await LoadImageInfo(file);
                    if (!Images.Contains(image))
                    {
                        Images.Add(image);
                    }
                }
            }

            // Populate the new ImageGridView with the loaded images
            // Replaced with XAMl binding:
            // ImageGridView.ItemsSource = Images;
        }

        public async static Task<ImageFileInfo> LoadImageInfo(StorageFile file)
        {
            // Open a stream for the selected file.
            // The 'using' block ensures the stream is disposed
            // after the image is loaded.
            using (IRandomAccessStream fileStream = await file.OpenReadAsync())
            {
                // Create a bitmap to be the image source.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                var properties = await file.Properties.GetImagePropertiesAsync();
                ImageFileInfo info = new ImageFileInfo(
                    properties, file, bitmapImage,
                    file.DisplayName, file.DisplayType);

                return info;
            }
        }

        public double ItemSize
        {
            get => _itemSize;
            set
            {
                if (_itemSize != value)
                {
                    _itemSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemSize)));
                }
            }
        }
        private double _itemSize;

        private void TextBlock_SelectionChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        // My added functions

        private void MoveImageForward()
        {
            Nullable<Int32> index = ImageGridView.SelectedIndex as Nullable<Int32>;

            // Do nothing if no image is selected
            if (index == -1 || index == null)
            {
                return;
            }

            if (index == (Images.Count -1))
            {
                Images.Move((Int32)index, 0);
            }
            else
            {
                Images.Move((Int32)index, (Int32)index + 1);
            }
        }

        private void MoveImageBack()
        {
            Nullable<Int32> index = ImageGridView.SelectedIndex as Nullable<Int32>;

            // Do nothing if no image is selected
            if (index == -1 || index == null)
            {
                return;
            }

            if (index == 0)
            {
                Images.Move((Int32)index, Images.Count-1);
            }
            else
            {
                Images.Move((Int32)index, (Int32)index - 1);
            }
        }


        private void DeletedSelectedImage() =>
            Images.Remove(ImageGridView.SelectedItem as ImageFileInfo);


        private void DeleteAllImages() =>
            Images.Clear();

        private async void ReadImages() {
            Images.Clear();
            await GetItemsAsync();

        }

    }
}
