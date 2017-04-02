using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using TurtaIoTHAT;

namespace VEML6075SampleApp
{
    public sealed partial class MainPage : Page
    {
        // VEML6075 Sensor
        static VEML6075Sensor veml;

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
            await InitializeVEML6075();

            // Configure timer to 2000ms delayed start and 2000ms interval
            sensorTimer = new Timer(new TimerCallback(SensorTimerTick), null, 2000, 2000);
        }

        private async Task InitializeVEML6075()
        {
            // Create sensor instance
            veml = new VEML6075Sensor();

            // Advanced sensor configuration
            await veml.Config(
                VEML6075Sensor.IntegrationTime.IT_800ms,
                VEML6075Sensor.DynamicSetting.High,
                VEML6075Sensor.Trigger.NoActiveForceTrigger,
                VEML6075Sensor.ActiveForceMode.NormalMode,
                VEML6075Sensor.PowerMode.PowerOn
                );
        }

        private static void SensorTimerTick(object state)
        {
            // Write sensor data to output / intermediate window
            Debug.WriteLine("UVA........: " + veml.Read_RAW_UVA().ToString());
            Debug.WriteLine("UVB........: " + veml.Read_RAW_UVB().ToString());
            Debug.WriteLine("UVA Index..: " + veml.Calculate_UV_Index_A().ToString());
            Debug.WriteLine("UVB Index..: " + veml.Calculate_UV_Index_B().ToString());
            Debug.WriteLine("UV Index...: " + veml.Calculate_Average_UV_Index().ToString());
            Debug.WriteLine("-----");
        }
    }
}
