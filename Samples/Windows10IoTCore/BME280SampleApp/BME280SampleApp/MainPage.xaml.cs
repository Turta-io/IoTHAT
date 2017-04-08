using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace BME280SampleApp
{
    public sealed partial class MainPage : Page
    {
        // BME280 Sensor
        static BME280Sensor bme;

        // Sea level pressure in bar
        // Update this from weather forecast to get precise altitude
        static double slp = 1033.0;

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
            await InitializeBME280();

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private async Task InitializeBME280()
        {
            // Create sensor instance
            bme = new BME280Sensor();

            // Optional advanced sensor configuration
            await bme.SetOversamplingsAndMode(
                BME280Sensor.HumidityOversampling.x04,
                BME280Sensor.TemperatureOversampling.x04,
                BME280Sensor.PressureOversampling.x04,
                BME280Sensor.SensorMode.Normal);

            // Optional advanced sensor configuration
            await bme.SetConfig(
                BME280Sensor.InactiveDuration.ms0500,
                BME280Sensor.FilterCoefficient.fc04);
        }

        private static void SensorTimerTick(object state)
        {
            // Write sensor data to output / immediate window
            Debug.WriteLine("Temperature..: " + bme.ReadTemperature().ToString("00.0") + "C");
            Debug.WriteLine("Humidity.....: %" + bme.ReadHumidity().ToString("00.0" + "RH"));
            Debug.WriteLine("Pressure.....: " + bme.ReadPressure().ToString(".0") + "Pa");
            Debug.WriteLine("Altitude.....: " + bme.ReadAltitude(slp).ToString(".0") + "m");
            Debug.WriteLine("-----");
        }
    }
}
