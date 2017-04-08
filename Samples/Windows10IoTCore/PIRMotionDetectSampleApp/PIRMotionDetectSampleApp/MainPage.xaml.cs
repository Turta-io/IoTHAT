using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using TurtaIoTHAT;

namespace PIRMotionDetectSampleApp
{
    public sealed partial class MainPage : Page
    {
        // PIR Motion Detect Sensor
        static PIRSensor pirSensor;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize PIR Motion Detect Sensor and set event handler
            Initialize();
        }

        private void Initialize()
        {
            // Initialize PIR Motion Detect Sensor
            pirSensor = new PIRSensor();

            // Subscribe to PIR Motion Detect State Changed event
            pirSensor.PIRMotionDetectStateChanged += PIRMotionDetectStateChanged;
        }

        private void PIRMotionDetectStateChanged(object sender, PIRMotionDetectEventArgs e)
        {
            // Write motion state to output / immediate window
            Debug.Write("PIR Interrupt: ");
            Debug.WriteLine(e.Motion ? "Active" : "Passive");
        }
    }
}
