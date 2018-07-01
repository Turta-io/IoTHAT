using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace BME680SampleApp
{
    public sealed partial class MainPage : Page
    {
        // BME680 Sensor
        static BME680Sensor bme;

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

        private void Initialize()
        {
            // Create sensor instance
            bme = new BME680Sensor();

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private static void SensorTimerTick(object state)
        {
            // Write sensor data to output / immediate window
            Debug.WriteLine("Temperature.....: " + bme.ReadTemperature().ToString("00.0") + "C");
            Debug.WriteLine("Humidity........: %" + bme.ReadHumidity().ToString("00.0" + "RH"));
            Debug.WriteLine("Pressure........: " + bme.ReadPressure().ToString(".0") + "Pa");
            Debug.WriteLine("Altitude........: " + bme.ReadAltitude(slp).ToString(".0") + "m");
            Debug.WriteLine("Gas Resistance..: " + bme.ReadGasResistance().ToString(".0") + "Ohms");
            Debug.WriteLine("-----");
        }
    }
}
