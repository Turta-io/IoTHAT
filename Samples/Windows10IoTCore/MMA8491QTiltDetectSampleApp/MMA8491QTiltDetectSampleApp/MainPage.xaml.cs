using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace MMA8491QTiltDetectSampleApp
{
    public sealed partial class MainPage : Page
    {
        // MMA8491Q Sensor
        static MMA8491QSensor accel;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize sensor and timer
            Initialize();
        }

        private void Initialize()
        {
            // Create sensor instance
            accel = new MMA8491QSensor(MMA8491QSensor.Modes.TiltSensor);

            // Subscribe to Tilt Changed event
            accel.TiltChanged += TiltChanged;
        }

        private void TiltChanged(object sender, MMA8491QTiltEventArgs e)
        {
            // Write tilt state to output / immediate window
            Debug.WriteLine(e.TiltDetected ? "Tilt detected." : "No tilt.");
        }
    }
}
