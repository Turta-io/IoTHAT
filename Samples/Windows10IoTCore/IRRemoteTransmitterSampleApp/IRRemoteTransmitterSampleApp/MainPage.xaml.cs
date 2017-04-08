using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Diagnostics;
using TurtaIoTHAT;

namespace IRRemoteTransmitterSampleApp
{
    public sealed partial class MainPage : Page
    {
        // IR Remote Controller
        static IRRemoteController irRemoteController;

        // Sensor timer
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize IR Remote Controller and timer
            Initialize();
        }

        private void Initialize()
        {
            // Initialize and configure IR Remote Controller
            irRemoteController = new IRRemoteController(false);

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private static void SensorTimerTick(object state)
        {
            // Create 4 Byte payload
            byte[] irTxBuffer = { 0x01, 0x02, 0x03, 0x04 };

            // Transmit data using NEC Protocol
            irRemoteController.Send4Byte(irTxBuffer);

            // Write confirmation to output / immediate window
            Debug.WriteLine("Data sent.");
        }
    }
}
