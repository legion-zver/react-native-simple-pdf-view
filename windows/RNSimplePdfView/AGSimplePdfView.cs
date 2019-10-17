using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace RNSimplePdfView
{
    class AGSimplePdfView : ContentControl
    {
        private ScrollViewer scrollViewer;
        private ItemsControl items = new ItemsControl();
        private PdfDocument pdfDocument = null;
        private String pdfDocumentPath = "";
        private UInt64 loadingIndex = 0;

        private ObservableCollection<BitmapImage> pdfPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

        public AGSimplePdfView()
        {
            scrollViewer = new ScrollViewer
            {
                Background = new SolidColorBrush(Colors.Transparent),
                ZoomMode = ZoomMode.Enabled,
                // Align to RN defaults
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollMode = ScrollMode.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollMode = ScrollMode.Auto,
                // The default tab index keeps the ScrollViewer (and its children) outside the normal flow of tabIndex==0 controls.
                // We force a better default, at least until we start supporting TabIndex/IsTabStop properties on RCTScrollView.
                TabIndex = 0,
            };
            // Create items template
            items.ItemTemplate = XamlReader.Load(
                "<DataTemplate><Image Source = \"{Binding}\" Margin = \"0 2\" /></ DataTemplate>"
            ) as DataTemplate;
            items.ItemsSource = pdfPages;
            // Set content
            scrollViewer.Content = items;
            Content = scrollViewer;
        }

        public void Reset()
        {
            pdfDocumentPath = "";
            pdfDocument = null;
            pdfPages.Clear();
        }

        public void ScrollTopPage(uint pageIndex)
        {
            if (pdfDocument == null)
            {
                return;
            }
        }

        public void Load(String path)
        {
            if (pdfDocumentPath.Equals(path) && pdfDocument != null)
            {
                return;
            }
            Reset();
            try
            {
                if (
                    path.StartsWith("http://") ||
                    path.StartsWith("https://") ||
                    path.StartsWith("ftp://")
                    )
                {
                    loadFromHttp(path);
                    return;
                }
                loadFromLocal(path);
            } catch
            {

            }
        }

        private async void loadFromLocal(String path)
        {
            var index = nextLoadingIndex();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
            setPdfDocument(doc, index);
        }

        private async void loadFromHttp(String uri)
        {
            var index = nextLoadingIndex();
            HttpClient client = new HttpClient();
            var stream = await client.GetStreamAsync(uri);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            PdfDocument doc = await PdfDocument.LoadFromStreamAsync(memoryStream.AsRandomAccessStream());
            setPdfDocument(doc, index);
        }

        private UInt64 nextLoadingIndex()
        {
            loadingIndex++;
            if (loadingIndex >= 64000)
            {
                loadingIndex = 0;
            }
            return loadingIndex;
        }

        private async void setPdfDocument(PdfDocument pdfDoc, UInt64 index)
        {
            if (loadingIndex != index)
            {
                return;
            }
            pdfDocument = pdfDoc; pdfPages.Clear();
            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();
                var page = pdfDoc.GetPage(i);
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }
                pdfPages.Add(image);
            }
            scrollViewer.ChangeView(0, 0, 1);
        }
    }
}
