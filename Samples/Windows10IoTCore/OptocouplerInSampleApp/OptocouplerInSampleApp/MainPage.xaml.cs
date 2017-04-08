using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using TurtaIoTHAT;

namespace OptocouplerInSampleApp
{
    public sealed partial class MainPage : Page
    {
        // Optocoupler Inputs
        static OptocouplerInput optocouplerInput;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize Optocoupler Inputs and set event handler
            Initialize();
        }

        private void Initialize()
        {
            // Initialize and configure Optocoupler Input
            optocouplerInput = new OptocouplerInput(20);

            // Subscribe to Optocoupler Input Changed event
            optocouplerInput.OptocouplerInputChanged += OptocouplerInputChanged;
        }

        private void OptocouplerInputChanged(object sender, OctocouplerInputEventArgs e)
        {
            // Write optocoupler input state to output / immediate window
            Debug.Write("Optocoupler Interrupt: ");
            Debug.Write("Ch " + e.Ch.ToString() + " ");
            Debug.WriteLine(e.State ? "Active" : "Passive");
        }
    }
}
