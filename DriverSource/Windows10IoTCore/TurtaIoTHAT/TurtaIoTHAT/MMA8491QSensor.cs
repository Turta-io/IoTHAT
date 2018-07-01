/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 - 2018 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace TurtaIoTHAT
{
    #region Events

    // Data for Events
    public class MMA8491QTiltEventArgs : EventArgs
    {
        private bool tiltDetected;

        // Constructor
        public MMA8491QTiltEventArgs(bool tiltDetected)
        {
            this.tiltDetected = tiltDetected;
        }

        public bool TiltDetected
        {
            get { return tiltDetected; }
        }
    }

    // Delegate Decleration
    public delegate void MMA8491QTiltEventHandler(object sender, MMA8491QTiltEventArgs e);

    #endregion

    public class MMA8491QSensor : IDisposable
    {
        #region Enumerations

        /// <summary>
        /// Sensor modes.
        /// </summary>
        public enum Modes
        {
            Accelerometer,
            TiltSensor
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice mma8491q = null;

        // Enable Pin
        private static GpioPin mma8491qEn;

        // Interrupt Pin
        private static GpioPin mma8491qInt;

        // I2C Slave Address
        internal const byte MMA8491Q_I2C_ADDRESS = 0x55;

        // Registers
        private const byte MMA8491Q_STATUS = 0x00;
        private const byte MMA8491Q_OUT_X_MSB = 0x01;
        private const byte MMA8491Q_OUT_Y_MSB = 0x03;
        private const byte MMA8491Q_OUT_Z_MSB = 0x05;

        // Timer
        Timer tiltSenseTimer;

        // Variables
        private bool prevTiltState = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the MMA8491Q sensor.
        /// </summary>
        public MMA8491QSensor(Modes sensorMode)
        {
            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pins.
            mma8491qEn = gpioController.OpenPin(5);
            mma8491qEn.SetDriveMode(GpioPinDriveMode.Output);

            mma8491qInt = gpioController.OpenPin(17);
            mma8491qInt.SetDriveMode(GpioPinDriveMode.Input);

            // Initiate the sensor.
            Initialize(sensorMode == Modes.TiltSensor ? true : false);
        }

        #endregion

        #region I2CCom

        /// <summary>
        /// Initiates the sensor.
        /// </summary>
        private async void Initialize(bool tiltSense)
        {
            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(MMA8491Q_I2C_ADDRESS)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Shared
                };

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                mma8491q = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                if (tiltSense)
                    tiltSenseTimer = new Timer(new TimerCallback(TiltCheck), null, 2000, 500);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns></returns>
        private byte ReadRegister_OneByte(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00 };

            mma8491q.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private UInt16 ReadRegister_TwoBytes_RS2B(byte reg)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            mma8491q.WriteRead(writeBuffer, readBuffer);
            value = (UInt16)((readBuffer[0] << 6) + (readBuffer[1] >> 2));

            return value;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte[] ReadRegister_SixBytesArray(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            mma8491q.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Converts raw sensor data to G value.
        /// </summary>
        /// <param name="analogData">Raw sensor data.</param>
        /// <returns>-8.0 to 8.0 G value.</returns>
        private double ConvertToG(ushort analogData)
        {
            if ((analogData & 0x2000) == 0x2000) // Zero or negative G
                return (0x3FFF - analogData) / -1024.0;
            else // Positive G
                return analogData / 1024.0;
        }

        /// <summary>
        /// Reads the X-axis value.
        /// </summary>
        /// <returns>X-Axis G value.</returns>
        public double ReadXAxis()
        {
            ushort tempData;

            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();

            while ((ReadRegister_OneByte(MMA8491Q_STATUS) & 0x01) != 0x01) { Task.Delay(1).Wait(); }
            tempData = ReadRegister_TwoBytes_RS2B(MMA8491Q_OUT_X_MSB);
            mma8491qEn.Write(GpioPinValue.Low);

            return ConvertToG(tempData);
        }

        /// <summary>
        /// Reads the Y-axis value.
        /// </summary>
        /// <returns>Y-Axis G value.</returns>
        public double ReadYAxis()
        {
            ushort tempData;

            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();

            while ((ReadRegister_OneByte(MMA8491Q_STATUS) & 0x02) != 0x02) { Task.Delay(1).Wait(); }
            tempData = ReadRegister_TwoBytes_RS2B(MMA8491Q_OUT_Y_MSB);
            mma8491qEn.Write(GpioPinValue.Low);

            return ConvertToG(tempData);
        }

        /// <summary>
        /// Reads the Z-axis value.
        /// </summary>
        /// <returns>Z-Axis G value.</returns>
        public double ReadZAxis()
        {
            ushort tempData;

            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();

            while ((ReadRegister_OneByte(MMA8491Q_STATUS) & 0x04) != 0x04) { Task.Delay(1).Wait(); }
            tempData = ReadRegister_TwoBytes_RS2B(MMA8491Q_OUT_Z_MSB);
            mma8491qEn.Write(GpioPinValue.Low);

            return ConvertToG(tempData);
        }

        /// <summary>
        /// Reads the X, Y and Z-Axis values.
        /// </summary>
        /// <returns>X, Y and Z-Axis G values respectively.</returns>
        public double[] ReadXYZAxis()
        {
            double[] xyz = { 0, 0, 0 };

            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();

            while ((ReadRegister_OneByte(MMA8491Q_STATUS) & 0x08) != 0x08) { Task.Delay(1).Wait(); }
            byte[] xyzArray = ReadRegister_SixBytesArray(MMA8491Q_OUT_X_MSB);
            mma8491qEn.Write(GpioPinValue.Low);

            xyz[0] = ConvertToG((ushort)((xyzArray[0] << 6) + (xyzArray[1] >> 2))); // X-Axis
            xyz[1] = ConvertToG((ushort)((xyzArray[2] << 6) + (xyzArray[3] >> 2))); // Y-Axis
            xyz[2] = ConvertToG((ushort)((xyzArray[4] << 6) + (xyzArray[5] >> 2))); // Z-Axis

            return xyz;
        }

        /// <summary>
        /// Reads the tilt state.
        /// </summary>
        /// <returns>True if acceleration is > 0.688g or X/Y axis > 45°. False if not.</returns>
        public bool ReadTiltState()
        {
            bool state;

            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();
            state = mma8491qInt.Read() == GpioPinValue.Low ? true : false;
            mma8491qEn.Write(GpioPinValue.Low);

            return state;
        }

        #endregion

        #region Interrupts

        /// <summary>
        /// Checks the tilt state. Tilt is being detected when acceleration is > 0.688g or X/Y axis > 45°.
        /// </summary>
        /// <param name="state">True if tilt detected. False if not.</param>
        private void TiltCheck(object state)
        {
            mma8491qEn.Write(GpioPinValue.High);
            Task.Delay(1).Wait();
            bool tiltState = mma8491qInt.Read() == GpioPinValue.Low ? true : false;
            mma8491qEn.Write(GpioPinValue.Low);

            if (prevTiltState != tiltState)
            {
                prevTiltState = tiltState;
                MMA8491QTiltEventArgs ea = new MMA8491QTiltEventArgs(tiltState);
                OnMMA8491QTiltChange(ea);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on sensor tilt change.
        /// </summary>
        public event MMA8491QTiltEventHandler TiltChanged;

        protected virtual void OnMMA8491QTiltChange(MMA8491QTiltEventArgs e)
        {
            TiltChanged?.Invoke(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            tiltSenseTimer.Dispose();

            mma8491qInt.Dispose();
            mma8491qEn.Dispose();
            mma8491q.Dispose();
        }

        #endregion
    }
}
