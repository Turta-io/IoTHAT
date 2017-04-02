/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace TurtaIoTHAT
{
    public class MAX30100Sensor : IDisposable
    {
        #region Enumerations

        public enum EnableAlmostFull : byte
        {
            Enabled = 0b10000000,
            Disabled = 0b00000000
        }

        public enum EnableTemperatureReady : byte
        {
            Enabled = 0b01000000,
            Disabled = 0b00000000
        }

        public enum EnableHRReady : byte
        {
            Enabled = 0b00100000,
            Disabled = 0b00000000
        }

        public enum EnableSPO2Ready : byte
        {
            Enabled = 0b00010000,
            Disabled = 0b00000000
        }

        public enum Shutdown : byte
        {
            PowerSave = 0b10000000,
            Normal = 0b00000000
        }

        public enum Reset : byte
        {
            Reset = 0b01000000,
            Normal = 0b00000000
        }

        public enum TemperatureEnable : byte
        {
            SingleRead = 0b00001000,
            Disabled = 0b00000000
        }

        public enum Mode : byte
        {
            HROnlyEnabled = 0b00000010,
            SPO2Enabled = 0b00000011
        }

        public enum SPO2HighResolutionEnable : byte
        {
            Enabled = 0b01000000,
            Disabled = 0b00000000
        }

        public enum SPO2SampleRate : byte
        {
            SPS_0050 = 0b00000000,
            SPS_0100 = 0b00000100,
            SPS_0167 = 0b00001000,
            SPS_0200 = 0b00001100,
            SPS_0400 = 0b00010000,
            SPS_0600 = 0b00010100,
            SPS_0800 = 0b00011000,
            SPS_1000 = 0b00011100
        }

        public enum LEDPulseWidth : byte
        {
            PW_0200uS_ADC_13bit = 0b00000000,
            PW_0400uS_ADC_14bit = 0b00000001,
            PW_0800uS_ADC_15bit = 0b00000010,
            PW_1600uS_ADC_16bit = 0b00000011
        }
        
        public enum REDLEDCurrent : byte
        {
            MA_00_0 = 0b00000000,
            MA_04_4 = 0b00010000,
            MA_07_6 = 0b00100000,
            MA_11_0 = 0b00110000,
            MA_14_2 = 0b01000000,
            MA_17_4 = 0b01010000,
            MA_20_8 = 0b01100000,
            MA_24_0 = 0b01110000,
            MA_27_1 = 0b10000000,
            MA_30_6 = 0b10010000,
            MA_33_8 = 0b10100000,
            MA_37_0 = 0b10110000,
            MA_40_2 = 0b11000000,
            MA_43_6 = 0b11010000,
            MA_46_8 = 0b11100000,
            MA_50_0 = 0b11110000
        }

        public enum IRLEDCurrent : byte
        {
            MA_00_0 = 0b00000000,
            MA_04_4 = 0b00000001,
            MA_07_6 = 0b00000010,
            MA_11_0 = 0b00000011,
            MA_14_2 = 0b00000100,
            MA_17_4 = 0b00000101,
            MA_20_8 = 0b00000110,
            MA_24_0 = 0b00000111,
            MA_27_1 = 0b00001000,
            MA_30_6 = 0b00001001,
            MA_33_8 = 0b00001010,
            MA_37_0 = 0b00001011,
            MA_40_2 = 0b00001100,
            MA_43_6 = 0b00001101,
            MA_46_8 = 0b00001110,
            MA_50_0 = 0b00001111
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice max30100 = null;

        // I2C Slave Address
        internal const byte MAX30100_I2C_ADDRESS = 0x57;

        // Registers: Status
        private const byte MAX30100_INT_STATUS = 0x00;
        private const byte MAX30100_INT_ENABLE = 0x01;

        // Registers: FIFO
        private const byte MAX30100_FIFO_WRITEPOINTER = 0x02;
        private const byte MAX30100_FIFO_OVERFLOWCOUNTER = 0x03;
        private const byte MAX30100_FIFO_READPOINTER = 0x04;
        private const byte MAX30100_FIFO_DATAREGISTER = 0x05;

        // Registers: Configuration
        private const byte MAX30100_CONFIG_MODE = 0x06;
        private const byte MAX30100_CONFIG_SPO2 = 0x07;
        private const byte MAX30100_CONFIG_LED = 0x09;

        // Registers: Temperature
        private const byte MAX30100_TEMPERATURE_INTEGER = 0x16;
        private const byte MAX30100_TEMPERATURE_FRACTION = 0x17;

        // Registers: Part ID
        private const byte MAX30100_REVISION_ID = 0xFE;
        private const byte MAX30100_PART_ID = 0xFF;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates MAX30100 sensor to get HR and SPO2.
        /// </summary>
        public MAX30100Sensor()
        {
            // Initiate the sensor.
            Initialize();
        }

        #endregion

        #region I2CCom

        /// <summary>
        /// Initiates the sensor.
        /// </summary>
        private async void Initialize()
        {
            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(MAX30100_I2C_ADDRESS);

                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                max30100 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                await ModeConfiguration(Shutdown.Normal, Reset.Normal, TemperatureEnable.SingleRead, Mode.SPO2Enabled);
                await InterruptEnable(EnableAlmostFull.Disabled, EnableTemperatureReady.Disabled, EnableHRReady.Disabled, EnableSPO2Ready.Disabled);
                await SPO2Configuration(SPO2HighResolutionEnable.Enabled, SPO2SampleRate.SPS_0100, LEDPulseWidth.PW_1600uS_ADC_16bit);
                await LEDConfiguration(REDLEDCurrent.MA_14_2, IRLEDCurrent.MA_14_2);
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
            max30100.Write(data);
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

            max30100.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte[] ReadRegister_FourBytesArray(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            max30100.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }
        
        #endregion

        #region Sensor Configuration

        /// <summary>
        /// Toggles the interrupt modes.
        /// </summary>
        /// <param name="enableAlmostFull">FIFO almost full interrupt.</param>
        /// <param name="enableTemperatureReady">Temperature ready interrupt.</param>
        /// <param name="enableHRReady">HR ready interrupt.</param>
        /// <param name="enableSPO2Ready">SPO2 ready interrupt.</param>
        /// <returns></returns>
        public async Task InterruptEnable(EnableAlmostFull enableAlmostFull, EnableTemperatureReady enableTemperatureReady, EnableHRReady enableHRReady, EnableSPO2Ready enableSPO2Ready)
        {
            byte configCommand = 0x00;

            configCommand += (byte)enableAlmostFull;
            configCommand += (byte)enableTemperatureReady;
            configCommand += (byte)enableHRReady;
            configCommand += (byte)enableSPO2Ready;

            WriteRegister(new byte[] { MAX30100_INT_ENABLE, Convert.ToByte(configCommand) });

            await Task.Delay(1);
        }

        /// <summary>
        /// Configures the sensor.
        /// </summary>
        /// <param name="shutdown">Shutdown mode.</param>
        /// <param name="reset">Reset trigger.</param>
        /// <param name="temperatureEnable">Temperature measurement.</param>
        /// <param name="mode">Sensor mode.</param>
        /// <returns></returns>
        public async Task ModeConfiguration(Shutdown shutdown, Reset reset, TemperatureEnable temperatureEnable, Mode mode)
        {
            byte configCommand = 0x00;

            configCommand += (byte)shutdown;
            configCommand += (byte)reset;
            configCommand += (byte)temperatureEnable;
            configCommand += (byte)mode;

            WriteRegister(new byte[] { MAX30100_CONFIG_MODE, Convert.ToByte(configCommand) });

            WriteRegister(new byte[] { MAX30100_FIFO_WRITEPOINTER, 0x00 });
            WriteRegister(new byte[] { MAX30100_FIFO_OVERFLOWCOUNTER, 0x00 });
            WriteRegister(new byte[] { MAX30100_FIFO_READPOINTER, 0x00 });

            await Task.Delay(1);
        }
        
        /// <summary>
        /// SPO2 Measurement configuration.
        /// </summary>
        /// <param name="spo2HighResolutionEnable">SPO2 High resolution.</param>
        /// <param name="spo2SampleRate">SPO2 Sample rate.</param>
        /// <param name="ledPulseWidth">LED Pulse width.</param>
        /// <returns></returns>
        public async Task SPO2Configuration(SPO2HighResolutionEnable spo2HighResolutionEnable, SPO2SampleRate spo2SampleRate, LEDPulseWidth ledPulseWidth)
        {
            byte configCommand = 0x00;

            configCommand += (byte)spo2HighResolutionEnable;
            configCommand += (byte)spo2SampleRate;
            configCommand += (byte)ledPulseWidth;

            WriteRegister(new byte[] { MAX30100_CONFIG_SPO2, Convert.ToByte(configCommand) });

            await Task.Delay(1);
        }

        /// <summary>
        /// Red and IR LED configuration.
        /// </summary>
        /// <param name="redLedCurrent">Red LED current.</param>
        /// <param name="irLedCurrent">IR LED current.</param>
        /// <returns></returns>
        public async Task LEDConfiguration(REDLEDCurrent redLedCurrent, IRLEDCurrent irLedCurrent)
        {
            byte configCommand = 0x00;

            configCommand += (byte)redLedCurrent;
            configCommand += (byte)irLedCurrent;

            WriteRegister(new byte[] { MAX30100_CONFIG_LED, Convert.ToByte(configCommand) });

            await Task.Delay(1);
        }

        /// <summary>
        /// Shuts down the LEDs and the sensor.
        /// </summary>
        public async void PowerSave()
        {
            await LEDConfiguration(REDLEDCurrent.MA_00_0, IRLEDCurrent.MA_00_0);
            await ModeConfiguration(Shutdown.PowerSave, Reset.Normal, TemperatureEnable.Disabled, Mode.HROnlyEnabled);
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Reads HR and SPO2 data from the FIFO.
        /// </summary>
        /// <returns>Average HR and SPO2 values respectively.</returns>
        private int[] ReadFIFOData()
        {
            int[] avgData = { 0, 0 };
            byte[] fifoTemp = new byte[4];

            int fifoWRPointer = ReadRegister_OneByte(MAX30100_FIFO_WRITEPOINTER);
            int fifoRDPointer = ReadRegister_OneByte(MAX30100_FIFO_READPOINTER);
            //int availableSamples = fifoWRPointer - fifoRDPointer;
            int availableSamples = 16;

            if (availableSamples > 0)
            {
                for (int i = 0; i < availableSamples; i++)
                {
                    fifoTemp = ReadRegister_FourBytesArray(MAX30100_FIFO_DATAREGISTER);

                    avgData[0] += (UInt16)(fifoTemp[0] << 8) + fifoTemp[1];
                    avgData[1] += (UInt16)(fifoTemp[2] << 8) + fifoTemp[3];
                }

                avgData[0] /= availableSamples;
                avgData[1] /= availableSamples;
            }

            return avgData;
        }

        /// <summary>
        /// Reads HR and SPO2.
        /// </summary>
        /// <returns>HR and SPO2 values respectively.</returns>
        public int[] ReadHRandSPO2()
        {
            int[] avgData = ReadFIFOData();

            avgData[0] /= 200;

            return avgData;
        }

        #endregion
        
        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            PowerSave();
            max30100.Dispose();
        }

        #endregion
    }
}
