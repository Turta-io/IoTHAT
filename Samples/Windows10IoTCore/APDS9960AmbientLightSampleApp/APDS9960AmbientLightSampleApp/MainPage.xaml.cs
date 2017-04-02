using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace APDS9960AmbientLightSampleApp
{
    public sealed partial class MainPage : Page
    {
        // APDS-9960 Sensor
        static APDS9960Sensor apds;

        // Sensor timer
        Timer sensorTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize sensor and timer
            Initialize();
        }

        private async void Initialize()
        {
            // Initialize and configure sensor
            await InitializeAPDS9960();

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private async Task InitializeAPDS9960()
        {
            // Create sensor instance
            // Ambient & RGB light: enabled, proximity: disabled, gesture: disabled
            apds = new APDS9960Sensor(true, false, false);

            // Delay 1ms
            await Task.Delay(1);
        }

        private static void SensorTimerTick(object state)
        {
            // Write sensor data to output / intermediate window
            Debug.WriteLine("Ambient Light: " + apds.ReadAmbientLight().ToString());
            Debug.WriteLine("-----");
        }
    }
}