/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 - 2018 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using Windows.Devices.Gpio;

namespace TurtaIoTHAT
{
    #region Events

    // Data for Events
    public class PhotocouplerInputEventArgs : EventArgs
    {
        private int ch;
        private bool state;

        // Constructor
        public PhotocouplerInputEventArgs(int ch, bool state)
        {
            this.ch = ch;
            this.state = state;
        }

        public int Ch
        {
            get { return ch; }
        }

        public bool State
        {
            get { return state; }
        }
    }

    // Delegate Decleration
    public delegate void PhotocouplerInputEventHandler(object sender, PhotocouplerInputEventArgs e);

    #endregion

    public class PhotocouplerInput : IDisposable
    {
        #region Globals

        // GPIO Device
        private static GpioPin photocoupler1, photocoupler2, photocoupler3, photocoupler4;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the LTV-827S photocouplers to detect isolated 5V input.
        /// </summary>
        /// <param name="debounceTimeout">Debounce timeout in miliseconds to eliminate the contact flickering. 0 to disable the filter.</param>
        public PhotocouplerInput(int debounceTimeout)
        {
            TimeSpan debounceTimeoutTimeSpan = new TimeSpan(0, 0, 0, 0, debounceTimeout);

            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pins.
            photocoupler1 = gpioController.OpenPin(13);
            photocoupler2 = gpioController.OpenPin(19);
            photocoupler3 = gpioController.OpenPin(16);
            photocoupler4 = gpioController.OpenPin(26);

            photocoupler1.SetDriveMode(GpioPinDriveMode.InputPullDown);
            photocoupler2.SetDriveMode(GpioPinDriveMode.InputPullDown);
            photocoupler3.SetDriveMode(GpioPinDriveMode.InputPullDown);
            photocoupler4.SetDriveMode(GpioPinDriveMode.InputPullDown);

            photocoupler1.DebounceTimeout = debounceTimeoutTimeSpan;
            photocoupler2.DebounceTimeout = debounceTimeoutTimeSpan;
            photocoupler3.DebounceTimeout = debounceTimeoutTimeSpan;
            photocoupler4.DebounceTimeout = debounceTimeoutTimeSpan;

            photocoupler1.ValueChanged += Photocoupler1_ValueChanged;
            photocoupler2.ValueChanged += Photocoupler2_ValueChanged;
            photocoupler3.ValueChanged += Photocoupler3_ValueChanged;
            photocoupler4.ValueChanged += Photocoupler4_ValueChanged;
        }

        #endregion

        #region Input Readout

        /// <summary>
        /// Reads current input state.
        /// </summary>
        /// <param name="ch">Photocoupler input channel. 1, 2, 3 or 4.</param>
        /// <returns>True if input is high. False if input is low.</returns>
        public bool ReadInputState(int ch)
        {
            bool st;

            switch (ch)
            {
                case 1:
                    st = (photocoupler1.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 2:
                    st = (photocoupler2.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 3:
                    st = (photocoupler3.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 4:
                    st = (photocoupler4.Read() == GpioPinValue.High) ? true : false;
                    break;
                default:
                    st = false;
                    break;
            }

            return st;
        }

        #endregion

        #region Interrupts

        /// <summary>
        /// Will be fired when photocoupler input 1 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Photocoupler1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(1, true);
                OnPhotocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(1, false);
                OnPhotocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when photocoupler input 2 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>

        private void Photocoupler2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(2, true);
                OnPhotocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(2, false);
                OnPhotocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when photocoupler input 3 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Photocoupler3_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(3, true);
                OnPhotocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(3, false);
                OnPhotocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when photocoupler input 4 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Photocoupler4_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(4, true);
                OnPhotocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                PhotocouplerInputEventArgs ea = new PhotocouplerInputEventArgs(4, false);
                OnPhotocouplerInputChange(ea);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on optocoupler input change.
        /// </summary>
        public event PhotocouplerInputEventHandler PhotocouplerInputChanged;

        protected virtual void OnPhotocouplerInputChange(PhotocouplerInputEventArgs e)
        {
            PhotocouplerInputEventHandler handler = PhotocouplerInputChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            photocoupler1.ValueChanged -= Photocoupler1_ValueChanged;
            photocoupler2.ValueChanged -= Photocoupler2_ValueChanged;
            photocoupler3.ValueChanged -= Photocoupler3_ValueChanged;
            photocoupler4.ValueChanged -= Photocoupler4_ValueChanged;

            photocoupler1.Dispose();
            photocoupler2.Dispose();
            photocoupler3.Dispose();
            photocoupler4.Dispose();
        }

        #endregion
    }
}
