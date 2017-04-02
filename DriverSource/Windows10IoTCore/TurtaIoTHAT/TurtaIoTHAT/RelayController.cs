/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using Windows.Devices.Gpio;

namespace TurtaIoTHAT
{
    public class RelayController : IDisposable
    {
        #region Globals

        // GPIO Device
        private static GpioPin relay1, relay2;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the solid state relays to turn on / off devices up to DC30V 2A each.
        /// </summary>
        public RelayController()
        {
            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pins.
            relay1 = gpioController.OpenPin(20);
            relay2 = gpioController.OpenPin(12);

            relay1.Write(GpioPinValue.Low);
            relay2.Write(GpioPinValue.Low);

            relay1.SetDriveMode(GpioPinDriveMode.Output);
            relay2.SetDriveMode(GpioPinDriveMode.Output);
        }

        #endregion

        #region Relay Control

        /// <summary>
        /// Controls the relay state.
        /// </summary>
        /// <param name="ch">Relay channel. 1 or 2.</param>
        /// <param name="state">Relay state. True for enable, false for disable.</param>
        public void SetRelay(int ch, bool state)
        {
            switch (ch)
            {
                case 1:
                    relay1.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                    break;

                case 2:
                    relay2.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Disables the relays and then cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            relay1.Write(GpioPinValue.Low);
            relay2.Write(GpioPinValue.Low);

            relay1.Dispose();
            relay2.Dispose();
        }

        #endregion
    }
}
