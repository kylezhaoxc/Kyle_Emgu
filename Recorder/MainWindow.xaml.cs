using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BetterTogether.Device;
using BetterTogether.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using Search4Match;


namespace Recorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameProcessor _searcher;
        private ICamera _camera;
        private IPairedDevice _device;
        private MemoryStream _stream;
        private BitmapImage _bitmapImage;
        bool isRecording = false;
        int index = 0;
        public MainWindow()
        {
            InitializeComponent();
            FileHelper.CleanFolder();
            InitBetterTogether();
        }

        #region BETTER_TOGETHER
        private void InitBetterTogether()
        {
            // Initializes the device discovery service. By default NFC pairing is disabled, and WiFi broadcast pairing is enabled.
            DeviceFinder.Initialize("Robot 01");

            // Subscribe to an event that indicates that a connection request has arrived.
            DeviceFinder.DeviceConnectionAccepting += DeviceFinder_DeviceConnectionAccepting;

            // Subscribe to an event that indicates that connection status has changed.
            DeviceFinder.ConnectionStatusChanged += DeviceFinder_ConnectionStatusChanged;

            try
            {
                // Start device discovery through NFC pairing. The connection will be established using Wi-Fi.
                DeviceFinder.Start(ConnectionActionType.WIFI);
            }
            catch (Exception)
            {
                //MessageBox.Show(exp.Message);
            }
        }

        private static void DeviceFinder_DeviceConnectionAccepting(object sender, ConnectionAcceptingEventArgs e)
        {
            e.ConnectionDeferral.AcceptAlways();
        }

        private void DeviceFinder_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            switch (e.ConnectionStatus)
            {
                case ConnectionStatus.NFC_TAPPED:
                    // User performed an NFC tap with the local device.
                    break;
                case ConnectionStatus.CONNECTED:
                    // Connection succeeded.

                    OnDeviceConnected(e.Device);
                    break;
                case ConnectionStatus.FAILED:

                    // Connection failed.
                    break;
            }
        }

        private async void OnDeviceConnected(IPairedDevice pairedDevice)
        {
            //StatusMessage.Visibility = Visibility.Collapsed;

            _device = pairedDevice;

            // Tell the camera object the 
            // resolution we want for the incoming video.
            // Here we use the 1st available resolution
            _camera = await _device.CameraManager.OpenAsync(
                CameraLocation.Back,
                _device.CameraManager.GetAvailableCaptureResolutions(
                    CameraLocation.Back)[0]
                );
            ell_flag.Fill = Brushes.Green;
            ell_flag.Stroke = Brushes.Green;
            // Please notice the preview resolution is different to capture resolution
            await _camera.SetPreviewResolutionAsync(new Size(800, 448));
            _camera.PreviewFrameAvailable += _camera_PreviewFrameAvailable;
        }
        #endregion
        private async void _camera_PreviewFrameAvailable(object sender, PreviewArrivedEventArgs e)
        {
            try
            {
                _stream = new MemoryStream(e.Frame.ImageStream);

                if (null == _stream)

                    return;

                await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        try
                        {

                            _bitmapImage = new BitmapImage();
                            _bitmapImage.BeginInit();
                            _bitmapImage.StreamSource = _stream; // Copy stream to local
                            _bitmapImage.EndInit();
                            cam.Source = _bitmapImage;
                            if (isRecording)
                            {
                                using (Image<Bgr, byte> rec_frame = new Image<Bgr, byte>(UiHandler.Bmimg2Bitmap(_bitmapImage)))
                                {
                                    rec_frame.Save(AppDomain.CurrentDomain.BaseDirectory + "img\\" + (++index) + ".jpg");
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            // ignored
                        }
                    }));
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private void BeginRec_Click(object sender, RoutedEventArgs e)
        {
            isRecording = true;
        }

        private void EndRec_Click(object sender, RoutedEventArgs e)
        {
            isRecording = false;
            //_searcher = FrameProcessor.GetProcessor(index);
        }
    }
}
