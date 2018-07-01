/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 - 2018 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace TurtaIoTHAT
{
    #region Events

    // Data for Events
    public class IOPortDigitalInputEventArgs : EventArgs
    {
        private int ch;
        private bool state;

        // Constructor
        public IOPortDigitalInputEventArgs(int ch, bool state)
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
    public delegate void IOPortDigitalInputEventHandler(object sender, IOPortDigitalInputEventArgs e);

    #endregion

    public class IOPort : IDisposable
    {
        #region Globals

        // I2C Device
        private I2cDevice mcuIO = null;

        // GPIO Device
        private static GpioPin d1, d2, d3, d4;

        // I2C Slave Address
        internal const byte MCU_I2C_ADDRESS = 0x28;

        // Registers
        private const byte MCU_ANALOGIN_CH1 = 0x10;
        private const byte MCU_ANALOGIN_CH2 = 0x11;
        private const byte MCU_ANALOGIN_CH3 = 0x12;
        private const byte MCU_ANALOGIN_CH4 = 0x13;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the IO Port function to use GPIO and analog input functions.
        /// </summary>
        /// <param name="d1In">Set true for input, false for output.</param>
        /// <param name="d2In">Set true for input, false for output.</param>
        /// <param name="d3In">Set true for input, false for output.</param>
        /// <param name="d4In">Set true for input, false for output.</param>
        public IOPort(bool d1In, bool d2In, bool d3In, bool d4In)
        {
            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pins.
            d1 = gpioController.OpenPin(21);
            d2 = gpioController.OpenPin(22);
            d3 = gpioController.OpenPin(23);
            d4 = gpioController.OpenPin(24);

            d1.SetDriveMode(d1In ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Output);
            d2.SetDriveMode(d2In ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Output);
            d3.SetDriveMode(d3In ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Output);
            d4.SetDriveMode(d4In ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Output);

            d1.DebounceTimeout = new TimeSpan(100000);
            d2.DebounceTimeout = new TimeSpan(100000);
            d3.DebounceTimeout = new TimeSpan(100000);
            d4.DebounceTimeout = new TimeSpan(100000);

            d1.ValueChanged += D1_ValueChanged;
            d2.ValueChanged += D2_ValueChanged;
            d3.ValueChanged += D3_ValueChanged;
            d4.ValueChanged += D4_ValueChanged;

            // Initiate the IO Port function.
            Initialize();
        }

        #endregion

        #region I2CCom

        /// <summary>
        /// Initiates the IO Port analog and capacitive touch functions.
        /// </summary>
        private async void Initialize()
        {
            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(MCU_I2C_ADDRESS)
                {
                    BusSpeed = I2cBusSpeed.StandardMode,
                    SharingMode = I2cSharingMode.Shared
                };

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                mcuIO = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="data">Address and data.</param>
        private async void WriteRegister(byte[] data)
        {
            mcuIO.Write(data);
            await Task.Delay(1);
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte ReadRegister_OneByte(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00 };

            mcuIO.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private UInt16 ReadRegister_TwoBytes(byte reg)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            mcuIO.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);

            return value;
        }

        #endregion

        #region Pin Output Control

        /// <summary>
        /// Writes to digital pin.
        /// </summary>
        /// <param name="ch">Digital output channel. 1, 2, 3 or 4.</param>
        /// <param name="st">True for high, false for low output state.</param>
        public void WriteDPinState(int ch, bool st)
        {
            switch (ch)
            {
                case 1:
                    if (d1.GetDriveMode() == GpioPinDriveMode.Output)
                        d1.Write(st ? GpioPinValue.High : GpioPinValue.Low);
                    break;
                case 2:
                    if (d2.GetDriveMode() == GpioPinDriveMode.Output)
                        d2.Write(st ? GpioPinValue.High : GpioPinValue.Low);
                    break;
                case 3:
                    if (d3.GetDriveMode() == GpioPinDriveMode.Output)
                        d3.Write(st ? GpioPinValue.High : GpioPinValue.Low);
                    break;
                case 4:
                    if (d4.GetDriveMode() == GpioPinDriveMode.Output)
                        d4.Write(st ? GpioPinValue.High : GpioPinValue.Low);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Input Readouts

        /// <summary>
        /// Reads current digital pin input state.
        /// </summary>
        /// <param name="ch">Digital input channel. 1, 2, 3 or 4.</param>
        /// <returns>True if input is high. False if input is low.</returns>
        public bool ReadDPinState(int ch)
        {
            bool st;

            switch (ch)
            {
                case 1:
                    st = (d1.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 2:
                    st = (d2.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 3:
                    st = (d3.Read() == GpioPinValue.High) ? true : false;
                    break;
                case 4:
                    st = (d4.Read() == GpioPinValue.High) ? true : false;
                    break;
                default:
                    st = false;
                    break;
            }

            return st;
        }

        /// <summary>
        /// Reads analog input value.
        /// </summary>
        /// <param name="ch">Analog input channel. 1, 2, 3 or 4.</param>
        /// <param name="multipleRead">Performs multiple reads and returns average value.</param>
        /// <returns>Analog read value, 0.0 to 1.0. 0 equals to 0V input and 1 equals to 3.3V input.</returns>
        public double ReadAnalogInput(int ch, bool multipleRead)
        {
            ushort tempRead = 0;

            switch (ch)
            {
                case 1:
                    if (multipleRead)
                    {
                        int tempReadInt = 0;

                        for (int i = 0; i < 50; i++)
                            tempReadInt += ReadRegister_TwoBytes(MCU_ANALOGIN_CH1);

                        tempRead = Convert.ToUInt16(tempReadInt / 50);
                    }
                    else
                        tempRead = ReadRegister_TwoBytes(MCU_ANALOGIN_CH1);
                    break;

                case 2:
                    if (multipleRead)
                    {
                        int tempReadInt = 0;

                        for (int i = 0; i < 50; i++)
                            tempReadInt += ReadRegister_TwoBytes(MCU_ANALOGIN_CH2);

                        tempRead = Convert.ToUInt16(tempReadInt / 50);
                    }
                    else
                        tempRead = ReadRegister_TwoBytes(MCU_ANALOGIN_CH2);
                    break;

                case 3:
                    if (multipleRead)
                    {
                        int tempReadInt = 0;

                        for (int i = 0; i < 50; i++)
                            tempReadInt += ReadRegister_TwoBytes(MCU_ANALOGIN_CH3);

                        tempRead = Convert.ToUInt16(tempReadInt / 50);
                    }
                    else
                        tempRead = ReadRegister_TwoBytes(MCU_ANALOGIN_CH3);
                    break;

                case 4:
                    if (multipleRead)
                    {
                        int tempReadInt = 0;

                        for (int i = 0; i < 50; i++)
                            tempReadInt += ReadRegister_TwoBytes(MCU_ANALOGIN_CH4);

                        tempRead = Convert.ToUInt16(tempReadInt / 50);
                    }
                    else
                        tempRead = ReadRegister_TwoBytes(MCU_ANALOGIN_CH4);
                    break;

                default:
                    tempRead = 0;
                    break;
            }

            return tempRead / 1023.0;
        }

        #endregion

        #region Interrupts

        /// <summary>
        /// Will be fired when digital input 1 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void D1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(1, true);
                OnIOPortDigitalInputStateChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(1, false);
                OnIOPortDigitalInputStateChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when digital input 2 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void D2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(2, true);
                OnIOPortDigitalInputStateChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(2, false);
                OnIOPortDigitalInputStateChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when digital input 3 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void D3_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(3, true);
                OnIOPortDigitalInputStateChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(3, false);
                OnIOPortDigitalInputStateChange(ea);
            }
        }

        /// <summary>
        /// Will be fired when digital input 4 state is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void D4_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(4, true);
                OnIOPortDigitalInputStateChange(ea);
            }
            else if (args.Edge == GpioPinEdge.FallingEdge)
            {
                IOPortDigitalInputEventArgs ea = new IOPortDigitalInputEventArgs(4, false);
                OnIOPortDigitalInputStateChange(ea);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on digital pin input change.
        /// </summary>
        public event IOPortDigitalInputEventHandler DigitalInputStateChanged;

        protected virtual void OnIOPortDigitalInputStateChange(IOPortDigitalInputEventArgs e)
        {
            IOPortDigitalInputEventHandler handler = DigitalInputStateChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            d1.ValueChanged -= D1_ValueChanged;
            d2.ValueChanged -= D2_ValueChanged;
            d3.ValueChanged -= D3_ValueChanged;
            d4.ValueChanged -= D4_ValueChanged;

            d1.Dispose();
            d2.Dispose();
            d3.Dispose();
            d4.Dispose();
        }

        #endregion
    }
}
