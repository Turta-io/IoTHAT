using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using TurtaIoTHAT;

namespace IRRemoteReceiverSampleApp
{
    public sealed partial class MainPage : Page
    {
        // IR Remote Controller
        static IRRemoteController irRemoteController;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize IR Remote Controller and set event handler
            Initialize();
        }

        private void Initialize()
        {
            // Initialize and configure IR Remote Controller
            irRemoteController = new IRRemoteController(true);

            // Subscribe to IR Remote Data Received event
            irRemoteController.IRRemoteDataReceived += IRRemoteDataReceived;
        }

        private void IRRemoteDataReceived(object sender, IRRemoteDataEventArgs e)
        {
            // Write IR Remote Data to output / immediate window
            Debug.Write("IR Remote Data: ");
            Debug.Write(e.RemoteData[0].ToString());
            Debug.Write(", ");
            Debug.Write(e.RemoteData[1].ToString());
            Debug.Write(", ");
            Debug.Write(e.RemoteData[2].ToString());
            Debug.Write(", ");
            Debug.WriteLine(e.RemoteData[3].ToString());
        }
    }
}
