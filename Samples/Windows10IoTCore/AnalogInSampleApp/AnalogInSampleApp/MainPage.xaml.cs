using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Diagnostics;
using TurtaIoTHAT;

namespace AnalogInSampleApp
{
    public sealed partial class MainPage : Page
    {
        // I/O Port
        static IOPort ioPort;

        // Sensor timer
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize I/O Port and timer
            Initialize();
        }

        private void Initialize()
        {
            // Initialize and configure I/O Port
            ioPort = new IOPort(false, false, false, false);

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private static void SensorTimerTick(object state)
        {
            // Write ADC data to output / immediate window
            Debug.WriteLine("AIn 1: " + ioPort.ReadAnalogInput(1, false).ToString("0.000"));
            Debug.WriteLine("AIn 2: " + ioPort.ReadAnalogInput(2, false).ToString("0.000"));
            Debug.WriteLine("AIn 3: " + ioPort.ReadAnalogInput(3, false).ToString("0.000"));
            Debug.WriteLine("AIn 4: " + ioPort.ReadAnalogInput(4, false).ToString("0.000"));
            Debug.WriteLine("-----");
        }
    }
}
