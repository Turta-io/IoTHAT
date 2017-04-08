using Windows.UI.Xaml.Controls;
using System.Threading;
using TurtaIoTHAT;

namespace RelayControllerSampleApp
{
    public sealed partial class MainPage : Page
    {
        // Relay Controller
        static RelayController relayController;

        // Relay Timer
        Timer relayTimer;

        // Relay State
        static bool relayState = false;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize Relay Controller and timer
            Initialize();
        }

        private void Initialize()
        {
            // Initialize Relay Controller
            relayController = new RelayController();

            // Configure timer to 2000ms delayed start and 10000ms interval
            relayTimer = new Timer(new TimerCallback(RelayTimerTick), null, 2000, 10000);
        }

        private static void RelayTimerTick(object state)
        {
            // Set Relay 1's state
            relayController.SetRelay(1, relayState);

            // Set Relay 2's state
            relayController.SetRelay(2, !relayState);

            // Invert relay state variable
            relayState = !relayState;
        }
    }
}
