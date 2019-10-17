using Newtonsoft.Json.Linq;
using ReactNative.UIManager;
using ReactNative.UIManager.Events;
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
        private string pdfDocumentSource = "";
        private ulong loadingIndex = 0;
        private double pdfImagesMaxWidth = 0;

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
                // Align to RN defaults
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollMode = ScrollMode.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollMode = ScrollMode.Auto,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ZoomMode = ZoomMode.Enabled,
                MinZoomFactor = 0.1f,
                MaxZoomFactor = 4.0f,
                // The default tab index keeps the ScrollViewer (and its children) outside the normal flow of tabIndex==0 controls.
                // We force a better default, at least until we start supporting TabIndex/IsTabStop properties on RCTScrollView.
                TabIndex = 0,
            };
            // Create items template
            items.ItemTemplate = XamlReader.Load(
                "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Image HorizontalAlignment = \"Center\" VerticalAlignment = \"Center\" Source =\"{Binding}\" Margin=\"0 2\" /></DataTemplate>"
            ) as DataTemplate;
            items.ItemsSource = pdfPages;
            // Set content
            scrollViewer.Content = items;
            scrollViewer.SizeChanged += scrollViewer_SizeChanged;
            Content = scrollViewer;
        }

        public void Destroy()
        {
            Reset();
            scrollViewer.SizeChanged -= scrollViewer_SizeChanged;
        }

        public void Reset()
        {
            pdfDocumentSource = "";
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

        public void Load(string source)
        {
            if (pdfDocumentSource.Equals(source) && pdfDocument != null)
            {
                return;
            }
            Reset();
            if (
                source.StartsWith("http://") ||
                source.StartsWith("https://") ||
                source.StartsWith("ftp://")
                )
            {
                loadFromHttp(source);
                return;
            }
            loadFromLocal(source);
        }

        private async void loadFromLocal(string source)
        {
            var index = nextLoadingIndex();
            pdfDocumentSource = source;
            emitStartLoadingEvent(source, false, index);
            try
            {
                StorageFile file = source.StartsWith("ms-") && source.Contains("://") ?
                    await StorageFile.GetFileFromApplicationUriAsync(new Uri(source)) :
                    await StorageFile.GetFileFromPathAsync(source);
                PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
                setPdfDocument(doc, source, index);
            }
            catch (Exception e)
            {
                emitErrorEvent(e.Message);
                emitEndLoadingEvent(source, false, 0, index);
            }
        }

        private async void loadFromHttp(string uri)
        {
            var index = nextLoadingIndex();
            pdfDocumentSource = uri;
            emitStartLoadingEvent(uri, true, index);
            try
            {
                HttpClient client = new HttpClient();
                var stream = await client.GetStreamAsync(uri);
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                // Read pdf
                PdfDocument doc = await PdfDocument.LoadFromStreamAsync(memoryStream.AsRandomAccessStream());
                setPdfDocument(doc, uri, index);
            }
            catch (Exception e)
            {
                emitErrorEvent(e.Message);
                emitEndLoadingEvent(uri, false, 0, index);
            }
        }

        private ulong nextLoadingIndex()
        {
            loadingIndex++;
            if (loadingIndex >= 64000)
            {
                loadingIndex = 0;
            }
            return loadingIndex;
        }

        private async void setPdfDocument(PdfDocument pdfDoc, string source, ulong index)
        {
            if (loadingIndex != index)
            {
                emitEndLoadingEvent(source, false, 0, index);
                return;
            }
            pdfDocument = pdfDoc;
            pdfPages.Clear();
            // Calc max width
            double maxWidth = 0;
            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                var page = pdfDoc.GetPage(i);
                if (maxWidth < page.Size.Width)
                {
                    maxWidth = page.Size.Width;
                }
            }
            pdfImagesMaxWidth = maxWidth;
            updateZoomFactor(scrollViewer, true);
            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();
                var page = pdfDoc.GetPage(i);
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }
                if (image.PixelWidth > pdfImagesMaxWidth)
                {
                    pdfImagesMaxWidth = image.PixelWidth;
                    updateZoomFactor(scrollViewer);
                }
                pdfPages.Add(image);
            }
            emitEndLoadingEvent(source, true, pdfDoc.PageCount, index);
            pdfDocumentSource = source;
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pdfDocument == null || pdfImagesMaxWidth == 0)
            {
                return;
            }
            updateZoomFactor(sender as ScrollViewer);
        }

        private void updateZoomFactor(ScrollViewer sv, bool scrollToTop = false)
        {
            if (pdfImagesMaxWidth <= 0 || sv.ViewportWidth <= 0)
            {
                return;
            }
            float zoomFactor = (float)(sv.ViewportWidth / pdfImagesMaxWidth);
            if (scrollToTop)
            {
                sv.ChangeView(0, 0, zoomFactor);
                return;
            }
            sv.ChangeView(null, null, zoomFactor);
        }

        private void emitStartLoadingEvent(string source, bool withNetworkRequest, ulong loadingIndex)
        {
            if (!this.HasTag())
            {
                return;
            }
            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new AGSimplePdfViewEvent(
                        this.GetTag(),
                        AGSimplePdfViewEventType.StartLoading,
                        new JObject
                        {
                            { "source", source },
                            { "loadingIndex",  loadingIndex },
                            { "withNetworkRequest", withNetworkRequest},
                        }));
        }

        private void emitEndLoadingEvent(string source, bool success, ulong pagesCount, ulong loadingIndex)
        {
            if (!this.HasTag())
            {
                return;
            }
            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new AGSimplePdfViewEvent(
                        this.GetTag(),
                        AGSimplePdfViewEventType.EndLoading,
                        new JObject
                        {
                            { "source", source },
                            { "success", success },
                            { "pagesCount", pagesCount },
                            { "loadingIndex", loadingIndex },
                        }));
        }

        private void emitErrorEvent(string message)
        {
            if (!this.HasTag())
            {
                return;
            }
            this.GetReactContext()
                .GetNativeModule<UIManagerModule>()
                .EventDispatcher
                .DispatchEvent(
                    new AGSimplePdfViewEvent(
                        this.GetTag(),
                        AGSimplePdfViewEventType.Error,
                        new JObject
                        {
                            { "message", message },
                        }));
        }

        // -----

        public enum AGSimplePdfViewEventType
        {
            StartLoading,
            EndLoading,
            Error,
        }

        public static string GetJavaScriptEventName(AGSimplePdfViewEventType type)
        {
            switch (type)
            {
                case AGSimplePdfViewEventType.Error:
                    return "topError";
                case AGSimplePdfViewEventType.StartLoading:
                    return "topStartLoading";
                case AGSimplePdfViewEventType.EndLoading:
                    return "topEndLoading";
                default:
                    return "topUnknownEvent";
            }
        }

        public class AGSimplePdfViewEvent : Event
        {
            private readonly AGSimplePdfViewEventType _type;
            private readonly JObject _data;

            public AGSimplePdfViewEvent(int viewTag, AGSimplePdfViewEventType type, JObject data) : base(viewTag)
            {
                _type = type;
                _data = data;
            }

            public override string EventName
            {
                get
                {
                    return GetJavaScriptEventName(_type);
                }
            }

            public override void Dispatch(RCTEventEmitter eventEmitter)
            {
                eventEmitter.receiveEvent(ViewTag, EventName, _data);
            }
        }
    }
}
