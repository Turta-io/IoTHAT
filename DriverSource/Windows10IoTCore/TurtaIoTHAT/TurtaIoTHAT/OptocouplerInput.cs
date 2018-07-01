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
    public class OctocouplerInputEventArgs : EventArgs
    {
        private int ch;
        private bool state;

        // Constructor
        public OctocouplerInputEventArgs(int ch, bool state)
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
    public delegate void OptocouplerInputEventHandler(object sender, OctocouplerInputEventArgs e);

    #endregion

    public class OptocouplerInput : IDisposable
    {
        #region Globals

        // GPIO Device
        private static GpioPin optocoupler1, optocoupler2, optocoupler3, optocoupler4;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the PC817 optocouplers to detect isolated 5V input.
        /// </summary>
        /// <param name="debounceTimeout">Debounce timeout in miliseconds to eliminate the contact flickering. 0 to disable the filter.</param>
        public OptocouplerInput(int debounceTimeout)
        {
            TimeSpan debounceTimeoutTimeSpan = new TimeSpan(0, 0, 0, 0, debounceTimeout);

            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pins.
            optocoupler1 = gpioController.OpenPin(13);
            optocoupler2 = gpioController.OpenPin(19);
            optocoupler3 = gpioController.OpenPin(16);
            optocoupler4 = gpioController.OpenPin(26);

            optocoupler1.SetDriveMode(GpioPinDriveMode.InputPullUp);
            optocoupler2.SetDriveMode(GpioPinDriveMode.InputPullUp);
            optocoupler3.SetDriveMode(GpioPinDriveMode.InputPullUp);
            optocoupler4.SetDriveMode(GpioPinDriveMode.InputPullUp);

            optocoupler1.DebounceTimeout = debounceTimeoutTimeSpan;
            optocoupler2.DebounceTimeout = debounceTimeoutTimeSpan;
            optocoupler3.DebounceTimeout = debounceTimeoutTimeSpan;
            optocoupler4.DebounceTimeout = debounceTimeoutTimeSpan;

            optocoupler1.ValueChanged += Optocoupler1_ValueChanged;
            optocoupler2.ValueChanged += Optocoupler2_ValueChanged;
            optocoupler3.ValueChanged += Optocoupler3_ValueChanged;
            optocoupler4.ValueChanged += Optocoupler4_ValueChanged;
        }

        #endregion

        #region Input Readout

        /// <summary>
        /// Reads current input state.
        /// </summary>
        /// <param name="ch">Optocoupler input channel. 1, 2, 3 or 4.</param>
        /// <returns>True if input is high. False if input is low.</returns>
        public bool ReadInputState(int ch)
        {
            bool st;

            switch (ch)
            {
                case 1:
                    st = (optocoupler1.Read() == GpioPinValue.Low) ? true : false;
                    break;
                case 2:
                    st = (optocoupler2.Read() == GpioPinValue.Low) ? true : false;
                    break;
                case 3:
                    st = (optocoupler3.Read() == GpioPinValue.Low) ? true : false;
                    break;
                case 4:
                    st = (optocoupler4.Read() == GpioPinValue.Low) ? true : false;
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
        /// Will be fired when optocoupler input 1 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Optocoupler1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(1, false);
                OnOptocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(1, true);
                OnOptocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when optocoupler input 2 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>

        private void Optocoupler2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(2, false);
                OnOptocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(2, true);
                OnOptocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when optocoupler input 3 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Optocoupler3_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(3, false);
                OnOptocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(3, true);
                OnOptocouplerInputChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when optocoupler input 4 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Optocoupler4_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(4, false);
                OnOptocouplerInputChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                OctocouplerInputEventArgs ea = new OctocouplerInputEventArgs(4, true);
                OnOptocouplerInputChange(ea);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on optocoupler input change.
        /// </summary>
        public event OptocouplerInputEventHandler OptocouplerInputChanged;

        protected virtual void OnOptocouplerInputChange(OctocouplerInputEventArgs e)
        {
            OptocouplerInputEventHandler handler = OptocouplerInputChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            optocoupler1.ValueChanged -= Optocoupler1_ValueChanged;
            optocoupler2.ValueChanged -= Optocoupler2_ValueChanged;
            optocoupler3.ValueChanged -= Optocoupler3_ValueChanged;
            optocoupler4.ValueChanged -= Optocoupler4_ValueChanged;

            optocoupler1.Dispose();
            optocoupler2.Dispose();
            optocoupler3.Dispose();
            optocoupler4.Dispose();
        }

        #endregion
    }
}
