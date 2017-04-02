/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 Turta
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
    public class APDS9960GestureDetectEventArgs : EventArgs
    {
        public enum GestureDirections
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        private GestureDirections gestureDirection;

        // Constructor
        public APDS9960GestureDetectEventArgs(GestureDirections gestureDirection)
        {
            this.gestureDirection = gestureDirection;
        }

        public GestureDirections GestureDirection
        {
            get { return gestureDirection; }
        }
    }

    // Delegate Decleration
    public delegate void APDS9960GestureDetectEventHandler(object sender, APDS9960GestureDetectEventArgs e);

    #endregion

    public class APDS9960Sensor : IDisposable
    {
        #region Enumerations

        // ENABLE - GEN: Gesture Enable
        private enum ENABLE_GEN : byte
        {
            OFF = 0b00000000,
            ON = 0b01000000
        }

        // ENABLE - PIEN: Proximity Interrupt Enable
        private enum ENABLE_PIEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00100000
        }

        // ENABLE - AIEN: ALS Interrupt Enable
        private enum ENABLE_AIEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00010000
        }

        // ENABLE - WEN: Wait Enable
        private enum ENABLE_WEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00001000
        }

        // ENABLE - PEN: Proximity Detect Enable
        private enum ENABLE_PEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00000100
        }

        // ENABLE - AEN: ALS Enable
        private enum ENABLE_AEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00000010
        }

        // ENABLE - PON: Power On
        private enum ENABLE_PON : byte
        {
            OFF = 0b00000000,
            ON = 0b00000001
        }

        // CONTROL - LDRIVE: LED Drive Strenght
        private enum CONTROL_LDRIVE : byte
        {
            MA_100 = 0b00000000,
            MA_050 = 0b01000000,
            MA_025 = 0b10000000,
            MA_012_5 = 0b11000000
        }

        // CONTROL - PGAIN: Proximity Gain Control
        private enum CONTROL_PGAIN : byte
        {
            X1 = 0b00000000,
            X2 = 0b00000100,
            X4 = 0b00001000,
            X8 = 0b00001100
        }

        // CONTROL - AGAIN: ALS and Color Gain Control
        private enum CONTROL_AGAIN : byte
        {
            X01 = 0b00000000,
            X04 = 0b00000001,
            X16 = 0b00000010,
            X64 = 0b00000011
        }

        // GCONF2 - GGAIN: Gesture Gain Control
        private enum GCONF2_GGAIN : byte
        {
            X1 = 0b00000000,
            X2 = 0b00100000,
            X4 = 0b01000000,
            X8 = 0b01100000
        }

        // GCONF2 - GLDRIVE: Gesture LED Drive Strenght
        private enum GCONF2_GLDRIVE : byte
        {
            MA_100 = 0b00000000,
            MA_050 = 0b00001000,
            MA_025 = 0b00010000,
            MA_012_5 = 0b00011000
        }

        // GCONF2 - GWTIME: Gesture Wait Time
        private enum GCONF2_GWTIME : byte
        {
            MS_00_0 = 0b00000000,
            MS_02_8 = 0b00000001,
            MS_05_6 = 0b00000010,
            MS_08_4 = 0b00000011,
            MS_14_0 = 0b00000100,
            MS_22_4 = 0b00000101,
            MS_30_8 = 0b00000110,
            MS_39_2 = 0b00000111
        }

        // GCONF4 - GFIFO_CLR: Gesture FIFO Clear
        private enum GCONF4_GFIFO_CLR : byte
        {
            OFF = 0b00000000,
            ON = 0b00000100
        }

        // GCONF4 - GIEN: Gesture Interrupt Enable
        private enum GCONF4_GIEN : byte
        {
            OFF = 0b00000000,
            ON = 0b00000010
        }

        // GCONF4 - GMODE Values
        private enum GCONF4_GMODE : byte
        {
            ALS_PROX_COLOR = 0b00000000,
            GESTURE = 0b00000001
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice apds9960 = null;

        // Interrupt Pin
        private static GpioPin apds9960Int;

        // I2C Slave Address
        internal const byte APDS9960_I2C_ADDRESS = 0x39;

        // Registers
        private const byte APDS9960_ENABLE = 0x80;
        private const byte APDS9960_ATIME = 0x81;
        private const byte APDS9960_WTIME = 0x83;
        private const byte APDS9960_AILTL = 0x84;
        private const byte APDS9960_AILTH = 0x85;
        private const byte APDS9960_AIHTL = 0x86;
        private const byte APDS9960_AIHTH = 0x87;
        private const byte APDS9960_PILT = 0x89;
        private const byte APDS9960_PIHT = 0x8B;
        private const byte APDS9960_PERS = 0x8C;
        private const byte APDS9960_CONFIG1 = 0x8D;
        private const byte APDS9960_PPULSE = 0x8E;
        private const byte APDS9960_CONTROL = 0x8F;
        private const byte APDS9960_CONFIG2 = 0x90;
        private const byte APDS9960_ID = 0x92;
        private const byte APDS9960_STATUS = 0x93;
        private const byte APDS9960_CDATAL = 0x94;
        private const byte APDS9960_CDATAH = 0x95;
        private const byte APDS9960_RDATAL = 0x96;
        private const byte APDS9960_RDATAH = 0x97;
        private const byte APDS9960_GDATAL = 0x98;
        private const byte APDS9960_GDATAH = 0x99;
        private const byte APDS9960_BDATAL = 0x9A;
        private const byte APDS9960_BDATAH = 0x9B;
        private const byte APDS9960_PDATA = 0x9C;
        private const byte APDS9960_POFFSET_UR = 0x9D;
        private const byte APDS9960_POFFSET_DL = 0x9E;
        private const byte APDS9960_CONFIG3 = 0x9F;
        private const byte APDS9960_GPENTH = 0xA0;
        private const byte APDS9960_GEXTH = 0xA1;
        private const byte APDS9960_GCONF1 = 0xA2;
        private const byte APDS9960_GCONF2 = 0xA3;
        private const byte APDS9960_GOFFSET_U = 0xA4;
        private const byte APDS9960_GOFFSET_D = 0xA5;
        private const byte APDS9960_GOFFSET_L = 0xA7;
        private const byte APDS9960_GOFFSET_R = 0xA9;
        private const byte APDS9960_GPULSE = 0xA6;
        private const byte APDS9960_GCONF3 = 0xAA;
        private const byte APDS9960_GCONF4 = 0xAB;
        private const byte APDS9960_GFLVL = 0xAE;
        private const byte APDS9960_GSTATUS = 0xAF;
        private const byte APDS9960_IFORCE = 0xE4;
        private const byte APDS9960_PICLEAR = 0xE5;
        private const byte APDS9960_CICLEAR = 0xE6;
        private const byte APDS9960_AICLEAR = 0xE7;
        private const byte APDS9960_GFIFO_U = 0xFC;
        private const byte APDS9960_GFIFO_D = 0xFD;
        private const byte APDS9960_GFIFO_L = 0xFE;
        private const byte APDS9960_GFIFO_R = 0xFF;

        // Timer
        private Timer gestureDetectionTimer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the APDS-9960 sensor to get ambient light, RGB light, proximity and gesture direction.
        /// </summary>
        /// <param name="ambientAndRGBLightEnabled">Ambient light and RGB light sense.</param>
        /// <param name="proximityDetectionEnabled">Proximity detection.</param>
        /// <param name="gestureRecognitionEnabled">Gesture recognition.</param>
        public APDS9960Sensor(bool ambientAndRGBLightEnabled, bool proximityDetectionEnabled, bool gestureRecognitionEnabled)
        {
            // Initiate the GPIO Controller.
            GpioController gpioController = GpioController.GetDefault();

            // Configure the pin.
            apds9960Int = gpioController.OpenPin(04);
            apds9960Int.SetDriveMode(GpioPinDriveMode.InputPullUp);
            apds9960Int.ValueChanged += APDS9960Int_ValueChanged;

            // Initiate the sensor.
            Initialize(ambientAndRGBLightEnabled, proximityDetectionEnabled, gestureRecognitionEnabled);
        }

        #endregion

        #region I2CCom

        /// <summary>
        /// Initiates the sensor.
        /// </summary>
        /// <param name="ambientAndRGBLightEnabled">Ambient light and RGB light sense.</param>
        /// <param name="proximityDetectionEnabled">Proximity detection.</param>
        /// <param name="gestureRecognitionEnabled">Gesture recognition.</param>
        private async void Initialize(bool ambientAndRGBLightEnabled, bool proximityDetectionEnabled, bool gestureRecognitionEnabled)
        {
            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(APDS9960_I2C_ADDRESS);

                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                apds9960 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                await SetInitialSettings();
                await SetMode(ambientAndRGBLightEnabled, proximityDetectionEnabled, gestureRecognitionEnabled);
            }
            catch (Exception e)
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
            apds9960.Write(data);
            await Task.Delay(1);
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

            apds9960.WriteRead(writeBuffer, readBuffer);

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

            apds9960.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);

            return value;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte[] ReadRegister_EightBytesArray(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            apds9960.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }
        
        #endregion

        #region Sensor Configuration

        /// <summary>
        /// Writes the initial settings to the sensor.
        /// </summary>
        /// <returns></returns>
        private async Task SetInitialSettings()
        {
            // Set enable register to turn off all functionality but power
            WriteRegister(new byte[] { APDS9960_ENABLE, 0x01 });

            // Set ATIME to 72 cycles, 200ms, 65535 max count
            WriteRegister(new byte[] { APDS9960_ATIME, 0xB6 });

            // Set wait time to 20ms
            WriteRegister(new byte[] { APDS9960_WTIME, 0xF9 });

            // Set ALS interrupt low thresold
            WriteRegister(new byte[] { APDS9960_AILTL, 0xFF }); // Low
            WriteRegister(new byte[] { APDS9960_AILTH, 0xFF }); // High

            // Set ALS interrupt high thresold
            WriteRegister(new byte[] { APDS9960_AIHTL, 0x00 }); // Low
            WriteRegister(new byte[] { APDS9960_AIHTH, 0x00 }); // High

            // Set proximity interrupt low thresold
            WriteRegister(new byte[] { APDS9960_PILT, 0 });

            // Set proximity interrupt high thresold
            WriteRegister(new byte[] { APDS9960_PIHT, 255 });

            // Set interrupt persistence filters
            WriteRegister(new byte[] { APDS9960_PERS, 0x11 });

            // Set configuration register one: No 12x wait (WLONG = 0)
            WriteRegister(new byte[] { APDS9960_CONFIG1, 0x60 });

            // Set proximity pulse count to 8 and lenght to 8us
            WriteRegister(new byte[] { APDS9960_PPULSE, 0x48 });

            // Set control register one:
            // LDRIVE: LED Drive Strenght to 100mA
            // PGAIN: Proximity Gain to 4x
            // AGAIN: ALS and Color Gain to 4x
            WriteRegister(new byte[] { APDS9960_CONTROL, (byte)CONTROL_LDRIVE.MA_100 | (byte)CONTROL_PGAIN.X4 | (byte)CONTROL_AGAIN.X04 });

            // Set configuration register two:
            // PSIEN: Disabled
            // CPSIEN: Disabled
            // LED_BOOST: 100%
            // Field 0: 1
            WriteRegister(new byte[] { APDS9960_CONFIG2, 0x01 });

            // Set proximity offset for up and right photodiodes to 0
            WriteRegister(new byte[] { APDS9960_POFFSET_UR, 0x00 });

            // Set proximity offset for down and left photodiodes to 0
            WriteRegister(new byte[] { APDS9960_POFFSET_DL, 0x00 });

            // Set configuration register three: Enable all photodiodes, no PCMP, no SAI
            WriteRegister(new byte[] { APDS9960_CONFIG3, 0x00 });

            // Set gesture proximity enter thresold to 50
            WriteRegister(new byte[] { APDS9960_GPENTH, 50 });

            // Set gesture exit thresold to 25
            WriteRegister(new byte[] { APDS9960_GEXTH, 25 });

            // Set gesture configuration one: Interrupt after 4 dataset, end at 2nd, all directions are active.
            WriteRegister(new byte[] { APDS9960_GCONF1, 0x41 });

            // Set gesture configuration register two:
            // GGAIN: Gesture Gain to x4
            // GLDRIVE: Gesture LED Drive Strenght to 100mA
            // GWTIME: Gesture Wait Time to 2.8ms
            WriteRegister(new byte[] { APDS9960_GCONF2, (byte)GCONF2_GGAIN.X4 | (byte)GCONF2_GLDRIVE.MA_100 | (byte)GCONF2_GWTIME.MS_02_8 });

            // Set gesture offsets to 0
            WriteRegister(new byte[] { APDS9960_GOFFSET_U, 0x00 });
            WriteRegister(new byte[] { APDS9960_GOFFSET_D, 0x00 });
            WriteRegister(new byte[] { APDS9960_GOFFSET_L, 0x00 });
            WriteRegister(new byte[] { APDS9960_GOFFSET_R, 0x00 });

            // Set gesture pulse count to 32 and lenght to 8us
            WriteRegister(new byte[] { APDS9960_GPULSE, 0x96 });
            
            // Set gesture configuration three: All photodiodes are enabled to gather results during gesture
            WriteRegister(new byte[] { APDS9960_GCONF3, 0x00 });

            // Set gesture configuration four
            WriteRegister(new byte[] { APDS9960_GCONF4, (byte)GCONF4_GFIFO_CLR.OFF | (byte)GCONF4_GIEN.OFF | (byte)GCONF4_GMODE.ALS_PROX_COLOR });

            await Task.Delay(1);
        }

        /// <summary>
        /// Toggles the sensor modes.
        /// </summary>
        /// <param name="ambientAndRGBLightEnabled">Ambient light and RGB light sense.</param>
        /// <param name="proximityDetectionEnabled">Proximity detection.</param>
        /// <param name="gestureRecognitionEnabled">Gesture recognition.</param>
        /// <returns></returns>
        public async Task SetMode(bool ambientAndRGBLightEnabled, bool proximityDetectionEnabled, bool gestureRecognitionEnabled)
        {
            byte enableCommand = 0x00;

            // Ambient and RGB Light
            if (ambientAndRGBLightEnabled)
            {
                enableCommand |= (byte)ENABLE_PON.ON;
                enableCommand |= (byte)ENABLE_AEN.ON;
            }

            // Proximity Detection
            if (proximityDetectionEnabled)
            {
                enableCommand |= (byte)ENABLE_PON.ON;
                enableCommand |= (byte)ENABLE_PEN.ON;
            }

            // Gesture Recognition
            if (gestureRecognitionEnabled)
            {
                enableCommand |= (byte)ENABLE_PON.ON;
                enableCommand |= (byte)ENABLE_GEN.ON;
            }
            
            // Enable Wait
            enableCommand |= (byte)ENABLE_WEN.ON;

            WriteRegister(new byte[] { APDS9960_ENABLE, Convert.ToByte(enableCommand) });

            await Task.Delay(1);
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Reads the ambient light value.
        /// </summary>
        /// <returns>Ambient light value.</returns>
        public int ReadAmbientLight()
        {
            return (int)ReadRegister_TwoBytes(APDS9960_CDATAL);
        }

        /// <summary>
        /// Reads the RGB light values.
        /// </summary>
        /// <returns>Red, green and blue light values respectively.</returns>
        public int[] ReadRGBLight()
        {
            int[] rgb = { 0, 0, 0 };

            byte[] crgb = ReadRegister_EightBytesArray(APDS9960_CDATAL);

            rgb[0] = (int)(crgb[2] + (crgb[3] << 8)); // Red channel
            rgb[1] = (int)(crgb[4] + (crgb[5] << 8)); // Green channel
            rgb[2] = (int)(crgb[6] + (crgb[7] << 8)); // Blue channel

            return rgb;
        }

        /// <summary>
        /// Reads the proximity value.
        /// </summary>
        /// <returns>Proximity value.</returns>
        public int ReadProximity()
        {
            return (int)ReadRegister_OneByte(APDS9960_PDATA);
        }

        #endregion

        #region Interrupts

        /// <summary>
        /// Configures and starts the gesture detection timer.
        /// </summary>
        private void StartGestureDetectionTimer()
        {
            gestureDetectionTimer = new Timer(new TimerCallback(GestureCheckTimerTick), null, 0, 100);
        }

        /// <summary>
        /// Stops the gesture detection timer.
        /// </summary>
        private void StopGestureDetectionTimer()
        {
            gestureDetectionTimer.Dispose();
        }

        /// <summary>
        /// Checks registers to detect gesture movement.
        /// </summary>
        /// <param name="state"></param>
        private void GestureCheckTimerTick(object state)
        {

        }

        /// <summary>
        /// Runs on sensor interrupt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void APDS9960Int_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            
        }

        /// <summary>
        /// Fires the gesture interrupt event.
        /// </summary>
        private void GestureInterruptDetected()
        {
            APDS9960GestureDetectEventArgs ea = new APDS9960GestureDetectEventArgs(APDS9960GestureDetectEventArgs.GestureDirections.None);
            OnGestureDetected(ea);
        }

        #endregion

        #region Events

        /// <summary>
        /// Notifies on gesture detection.
        /// </summary>
        public event APDS9960GestureDetectEventHandler GestureDetected;

        protected virtual void OnGestureDetected(APDS9960GestureDetectEventArgs e)
        {
            APDS9960GestureDetectEventHandler handler = GestureDetected;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            if (gestureDetectionTimer != null)
                gestureDetectionTimer.Dispose();

            apds9960.Dispose();
        }

        #endregion
    }
}
