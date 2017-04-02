/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using Windows.Devices.Gpio;

namespace TurtaIoTHAT
{
    #region Events

    // Data for Events
    public class PIRMotionDetectEventArgs : EventArgs
    {
        private bool motion;

        // Constructor
        public PIRMotionDetectEventArgs(bool motion)
        {
            this.motion = motion;
        }

        public bool Motion
        {
            get { return motion; }
        }
    }

    // Delegate Decleration
    public delegate void PIRMotionDetectEventHandler(object sender, PIRMotionDetectEventArgs e);

    #endregion

    public class PIRSensor
    {
        #region Globals

        // GPIO Device
        private static GpioPin pirInt;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the AS312 / AM312 / AL312 PIR sensor to detect human motion.
        /// </summary>
        public PIRSensor()
        {
            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pin.
            pirInt = gpioController.OpenPin(25);
            pirInt.SetDriveMode(GpioPinDriveMode.Input);
            pirInt.DebounceTimeout = new TimeSpan(100000);
            pirInt.ValueChanged += PIRInt_ValueChanged;
        }

        #endregion

        #region Interrupts

        /// <summary>
        /// Will be fired when motion detection state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PIRInt_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                PIRMotionDetectEventArgs ea = new PIRMotionDetectEventArgs(true);
                OnPIRMotionStateChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PIRMotionDetectEventArgs ea = new PIRMotionDetectEventArgs(false);
                OnPIRMotionStateChange(ea);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on PIR motion detect state change.
        /// </summary>
        public event PIRMotionDetectEventHandler PIRMotionDetectStateChanged;

        protected virtual void OnPIRMotionStateChange(PIRMotionDetectEventArgs e)
        {
            PIRMotionDetectEventHandler handler = PIRMotionDetectStateChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            pirInt.ValueChanged -= PIRInt_ValueChanged;
            pirInt.Dispose();
        }

        #endregion
    }
}
