/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 - 2018 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace TurtaIoTHAT
{
    public class BME280Sensor : IDisposable
    {
        #region Enumerations

        public enum HumidityOversampling : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00000001,
            x02 = 0b00000010,
            x04 = 0b00000011,
            x08 = 0b00000100,
            x16 = 0b00000101
        }

        public enum TemperatureOversampling : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00100000,
            x02 = 0b01000000,
            x04 = 0b01100000,
            x08 = 0b10000000,
            x16 = 0b10100000
        }

        public enum PressureOversampling : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00000100,
            x02 = 0b00001000,
            x04 = 0b00001100,
            x08 = 0b00010000,
            x16 = 0b00010100
        }

        public enum SensorMode : byte
        {
            Sleep = 0b00000000,
            Forced = 0b00000001,
            Normal = 0b00000011
        }

        public enum InactiveDuration : byte
        {
            ms0000_5 = 0b00000000,
            ms0062_5 = 0b00100000,
            ms0125 = 0b01000000,
            ms0250 = 0b01100000,
            ms0500 = 0b10000000,
            ms1000 = 0b10100000,
            ms0010 = 0b11000000,
            ms0020 = 0b11100000
        }

        public enum FilterCoefficient : byte
        {
            FilterOff = 0b00000000,
            fc02 = 0b00000100,
            fc04 = 0b00001000,
            fc08 = 0b00001100,
            fc16 = 0b00010000
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice bme280 = null;

        // I2C Slave Address
        internal const byte BME280_I2C_ADDRESS = 0x77;

        // Registers
        private const byte BME280_SIGNATURE = 0x60;
        private const byte BME280_ID = 0xD0;
        private const byte BME280_RESET = 0xE0;
        private const byte BME280_CTRL_HUM = 0xF2;
        private const byte BME280_STATUS = 0xF3;
        private const byte BME280_CTRL_MEAS = 0xF4;
        private const byte BME280_CONFIG = 0xF5;

        // Registers: Readout
        private const byte BME280_PRESS_MSB = 0xF7;
        private const byte BME280_PRESS_LSB = 0xF8;
        private const byte BME280_PRESS_XLSB = 0xF9;
        private const byte BME280_TEMP_MSB = 0xFA;
        private const byte BME280_TEMP_LSB = 0xFB;
        private const byte BME280_TEMP_XLSB = 0xFC;
        private const byte BME280_HUM_MSB = 0xFD;
        private const byte BME280_HUM_LSB = 0xFE;

        // Registers: Calibration
        private const byte BME280_DIG_T1 = 0x88;
        private const byte BME280_DIG_T2 = 0x8A;
        private const byte BME280_DIG_T3 = 0x8C;
        private const byte BME280_DIG_P1 = 0x8E;
        private const byte BME280_DIG_P2 = 0x90;
        private const byte BME280_DIG_P3 = 0x92;
        private const byte BME280_DIG_P4 = 0x94;
        private const byte BME280_DIG_P5 = 0x96;
        private const byte BME280_DIG_P6 = 0x98;
        private const byte BME280_DIG_P7 = 0x9A;
        private const byte BME280_DIG_P8 = 0x9C;
        private const byte BME280_DIG_P9 = 0x9E;
        private const byte BME280_DIG_H1 = 0xA1;
        private const byte BME280_DIG_H2 = 0xE1;
        private const byte BME280_DIG_H3 = 0xE3;
        private const byte BME280_DIG_H4 = 0xE4;
        private const byte BME280_DIG_H5 = 0xE5;
        private const byte BME280_DIG_H6 = 0xE7;

        // Data: Calibration
        private UInt16 calDig_T1;
        private Int16 calDig_T2;
        private Int16 calDig_T3;
        private UInt16 calDig_P1;
        private Int16 calDig_P2;
        private Int16 calDig_P3;
        private Int16 calDig_P4;
        private Int16 calDig_P5;
        private Int16 calDig_P6;
        private Int16 calDig_P7;
        private Int16 calDig_P8;
        private Int16 calDig_P9;
        private byte calDig_H1;
        private Int16 calDig_H2;
        private byte calDig_H3;
        private Int16 calDig_H4;
        private Int16 calDig_H5;
        private SByte calDig_H6;
        private Int32 fineTemperature = Int32.MinValue;

        // System
        private bool isInitialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the BME280 sensor to get temperature, humidity, pressure and altitude.
        /// </summary>
        public BME280Sensor()
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
                I2cConnectionSettings settings = new I2cConnectionSettings(BME280_I2C_ADDRESS)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Shared
                };

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                bme280 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                await ReadCalibrationData();

                isInitialized = true;

                await SetOversamplingsAndMode(HumidityOversampling.x04, TemperatureOversampling.x04, PressureOversampling.x04, SensorMode.Normal);
                await SetConfig(InactiveDuration.ms1000, FilterCoefficient.fc04);
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
            bme280.Write(data);
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

            bme280.WriteRead(writeBuffer, readBuffer);

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

            bme280.WriteRead(writeBuffer, readBuffer);
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
        private byte[] ReadRegister_TwoBytesArray(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            bme280.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private byte[] ReadRegister_ThreeBytesArray(byte reg)
        {
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00, 0x00 };

            bme280.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        #endregion

        #region Sensor Configuration

        /// <summary>
        /// Sets the oversamplings and sensor mode.
        /// </summary>
        /// <param name="ho">Humidity oversampling.</param>
        /// <param name="to">Temperature oversampling.</param>
        /// <param name="po">Pressure oversampling.</param>
        /// <param name="mode">Sensor mode.</param>
        /// <returns>True if successful. False if not.</returns>
        public async Task<bool> SetOversamplingsAndMode(HumidityOversampling ho, TemperatureOversampling to, PressureOversampling po, SensorMode mode)
        {
            int tryCounter = 0;

            while (!isInitialized)
            {
                await Task.Delay(10);
                if (tryCounter++ > 100)
                    return false;
            }

            byte ctrlMeasValue = 0x00;

            ctrlMeasValue += (byte)to;
            ctrlMeasValue += (byte)po;
            ctrlMeasValue += (byte)mode;

            WriteRegister(new byte[] { BME280_CTRL_HUM, (byte)ho });
            // Changes to "CTRL_HUM" only become effective after a write operation to "CTRL_MEAS".

            await Task.Delay(1);

            WriteRegister(new byte[] { BME280_CTRL_MEAS, Convert.ToByte(ctrlMeasValue) });

            await Task.Delay(1);

            return true;
        }

        /// <summary>
        /// Sets the sensor configuration.
        /// </summary>
        /// <param name="id">Inactive duration between normal mode measurements.</param>
        /// <param name="fc">Filter coefficient.</param>
        /// <returns>True if successful. False if not.</returns>
        public async Task<bool> SetConfig(InactiveDuration id, FilterCoefficient fc)
        {
            int tryCounter = 0;

            while (!isInitialized)
            {
                await Task.Delay(10);
                if (tryCounter++ > 100)
                    return false;
            }

            byte configValue = 0x00;

            configValue += (byte)id;
            configValue += (byte)fc;

            WriteRegister(new byte[] { BME280_CONFIG, Convert.ToByte(configValue) });

            await Task.Delay(1);

            return true;
        }

        #endregion

        #region Calibration and Compensation

        /// <summary>
        /// Reads the factory out calibration data from the sensor.
        /// </summary>
        /// <returns></returns>
        private async Task ReadCalibrationData()
        {
            // Temperature calibration
            calDig_T1 = ReadRegister_TwoBytes(BME280_DIG_T1);
            calDig_T2 = (Int16)ReadRegister_TwoBytes(BME280_DIG_T2);
            calDig_T3 = (Int16)ReadRegister_TwoBytes(BME280_DIG_T3);

            // Pressure calibration
            calDig_P1 = ReadRegister_TwoBytes(BME280_DIG_P1);
            calDig_P2 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P2);
            calDig_P3 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P3);
            calDig_P4 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P4);
            calDig_P5 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P5);
            calDig_P6 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P6);
            calDig_P7 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P7);
            calDig_P8 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P8);
            calDig_P9 = (Int16)ReadRegister_TwoBytes(BME280_DIG_P9);

            // Humidity calibration
            calDig_H1 = ReadRegister_OneByte(BME280_DIG_H1);
            calDig_H2 = (Int16)ReadRegister_TwoBytes(BME280_DIG_H2);
            calDig_H3 = ReadRegister_OneByte(BME280_DIG_H3);
            calDig_H4 = (Int16)((ReadRegister_OneByte(BME280_DIG_H4) << 4) | (ReadRegister_OneByte(BME280_DIG_H4 + 1) & 0xF));
            calDig_H5 = (Int16)((ReadRegister_OneByte(BME280_DIG_H5 + 1) << 4) | (ReadRegister_OneByte(BME280_DIG_H5) >> 4));
            calDig_H6 = (sbyte)ReadRegister_OneByte(BME280_DIG_H6);

            await Task.Delay(1);
        }

        /// <summary>
        /// Compensates the temperature.
        /// </summary>
        /// <param name="adcT">Analog temperature value.</param>
        /// <returns></returns>
        private double CompensateTemperature(Int32 adcT)
        {
            double var1, var2, t;

            var1 = (adcT / 16384.0 - calDig_T1 / 1024.0) * calDig_T2;
            var2 = ((adcT / 131072.0 - calDig_T1 / 8192.0) * (adcT / 131072.0 - calDig_T1 / 8192.0)) * calDig_T3;

            fineTemperature = (Int32)(var1 + var2);

            t = (var1 + var2) / 5120.0;

            return t;
        }

        /// <summary>
        /// Compensates the pressure.
        /// </summary>
        /// <param name="adcP">Analog pressure value.</param>
        /// <returns></returns>
        private double CompensatePressure(Int32 adcP)
        {
            double var1, var2, p;

            var1 = (fineTemperature / 2.0) - 64000.0;
            var2 = var1 * var1 * calDig_P6 / 32768.0;
            var2 = var2 + var1 * calDig_P5 * 2.0;
            var2 = (var2 / 4.0) + (calDig_P4 * 65536.0);
            var1 = (calDig_P3 * var1 * var1 / 524288.0 + calDig_P2 * var1) / 524288.0;
            var1 = (1.0 + var1 / 32768.0) * calDig_P1;

            if (var1 == 0.0)
                return 0;

            p = 1048576.0 - adcP;
            p = (p - (var2 / 4096.0)) * 6250.0 / var1;
            var1 = calDig_P9 * p * p / 2147483648.0;
            var2 = p * calDig_P8 / 32768.0;
            p = p + (var1 + var2 + calDig_P7) / 16.0;

            return p;
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="adcH">Analog humidity value.</param>
        /// <returns></returns>
        private double CompensateHumidity(Int32 adcH)
        {
            double varH;

            varH = fineTemperature - 76800.0;
            varH = (adcH - (calDig_H4 * 64.0 + calDig_H5 / 16384.0 * varH)) * (calDig_H2 / 65536.0 * (1.0 + calDig_H6 / 67108864.0 * varH * (1.0 + calDig_H3 / 67108864.0 * varH)));
            varH = varH * (1.0 - calDig_H1 * varH / 524288.0);

            if (varH > 100.0) varH = 100.0;
            else if (varH < 0.0) varH = 0.0;

            return varH;
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Reads the temperature in Celcius.
        /// </summary>
        /// <returns>Temperature in Celcius.</returns>
        public double ReadTemperature()
        {
            byte[] tRaw = ReadRegister_ThreeBytesArray(BME280_TEMP_MSB);

            return CompensateTemperature((tRaw[0] << 12) + (tRaw[1] << 4) + (tRaw[2] >> 4));
        }

        /// <summary>
        /// Reads the relative humidity.
        /// </summary>
        /// <returns>Relative humidity.</returns>
        public double ReadHumidity()
        {
            byte[] hRaw = ReadRegister_TwoBytesArray(BME280_HUM_MSB);

            return CompensateHumidity((hRaw[0] << 8) + hRaw[1]);
        }

        /// <summary>
        /// Reads the pressure in Pa.
        /// </summary>
        /// <returns>Pressure in Pa.</returns>
        public double ReadPressure()
        {
            byte[] pRaw = ReadRegister_ThreeBytesArray(BME280_PRESS_MSB);

            if (fineTemperature == Int32.MinValue)
                ReadTemperature();

            return CompensatePressure((pRaw[0] << 12) + (pRaw[1] << 4) + (pRaw[2] >> 4));
        }

        /// <summary>
        /// Reads the altitude from the sea level in meters.
        /// </summary>
        /// <param name="meanSeaLevelPressureInBar">Mean sea level pressure in bar. Will be used for altitude calculation from the pressure.</param>
        /// <returns>Altitude from the sea level in meters.</returns>
        public double ReadAltitude(double meanSeaLevelPressureInBar)
        {
            double phPa = ReadPressure() / 100;

            return 44330.0 * (1.0 - Math.Pow((phPa / meanSeaLevelPressureInBar), 0.1903));
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            isInitialized = false;

            bme280.Dispose();

            calDig_T1 = 0;
            calDig_T2 = 0;
            calDig_T3 = 0;
            calDig_P1 = 0;
            calDig_P2 = 0;
            calDig_P3 = 0;
            calDig_P4 = 0;
            calDig_P5 = 0;
            calDig_P6 = 0;
            calDig_P7 = 0;
            calDig_P8 = 0;
            calDig_P9 = 0;
            calDig_H1 = 0;
            calDig_H2 = 0;
            calDig_H3 = 0;
            calDig_H4 = 0;
            calDig_H5 = 0;
            calDig_H6 = 0;
            fineTemperature = 0;
        }

        #endregion
    }
}
