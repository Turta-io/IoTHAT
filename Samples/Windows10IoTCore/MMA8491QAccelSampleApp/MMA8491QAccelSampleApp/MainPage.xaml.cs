using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace MMA8491QAccelSampleApp
{
    public sealed partial class MainPage : Page
    {
        // MMA8491Q Sensor
        static MMA8491QSensor accel;

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
            accel = new MMA8491QSensor(MMA8491QSensor.Modes.Accelerometer);

            // Configure timer to 2000ms delayed start and 1000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 1000);
        }

        private static void SensorTimerTick(object state)
        {
            // Write sensor data to output / immediate window
            double[] g = accel.ReadXYZAxis();

            Debug.WriteLine("Accel X: " + g[0].ToString("0.00") + "G");
            Debug.WriteLine("Accel Y: " + g[1].ToString("0.00") + "G");
            Debug.WriteLine("Accel Z: " + g[2].ToString("0.00") + "G");
            Debug.WriteLine("-----");
        }
    }
}
