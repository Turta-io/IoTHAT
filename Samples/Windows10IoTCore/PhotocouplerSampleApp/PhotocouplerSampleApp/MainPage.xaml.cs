using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using TurtaIoTHAT;

namespace PhotocouplerSampleApp
{
    public sealed partial class MainPage : Page
    {
        // Photocoupler Inputs
        static PhotocouplerInput photocouplerInput;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize Photocoupler Inputs and set event handler
            Initialize();
        }

        private void Initialize()
        {
            // Initialize and configure Photocoupler Input
            photocouplerInput = new PhotocouplerInput(20);

            // Subscribe to Photocoupler Input Changed event
            photocouplerInput.PhotocouplerInputChanged += PhotocouplerInputChanged;
        }

        private void PhotocouplerInputChanged(object sender, PhotocouplerInputEventArgs e)
        {
            // Write photocoupler input state to output / immediate window
            Debug.Write("Photocoupler Interrupt: ");
            Debug.Write("Ch " + e.Ch.ToString() + " ");
            Debug.WriteLine(e.State ? "Active" : "Passive");
        }
    }
}
