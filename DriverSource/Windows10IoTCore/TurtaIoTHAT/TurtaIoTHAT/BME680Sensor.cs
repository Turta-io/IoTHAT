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
    public class BME680Sensor : IDisposable
    {
        #region Enumerations

        public enum OperationModes : byte
        {
            Sleep = 0b00000000,
            ForcedMode = 0b00000001
        }

        public enum TemperatureOversamplings : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00100000,
            x02 = 0b01000000,
            x04 = 0b01100000,
            x08 = 0b10000000,
            x16 = 0b10100000
        }

        public enum HumidityOversamplings : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00000001,
            x02 = 0b00000010,
            x04 = 0b00000011,
            x08 = 0b00000100,
            x16 = 0b00000101
        }

        public enum PressureOversamplings : byte
        {
            Skipped = 0b00000000,
            x01 = 0b00000100,
            x02 = 0b00001000,
            x04 = 0b00001100,
            x08 = 0b00010000,
            x16 = 0b00010100
        }

        public enum IIRFilterCoefficients : byte
        {
            FC_000 = 0b00000000,
            FC_001 = 0b00000100,
            FC_003 = 0b00001000,
            FC_007 = 0b00001100,
            FC_015 = 0b00010000,
            FC_031 = 0b00010100,
            FC_063 = 0b00011000,
            FC_127 = 0b00011100
        }

        public enum HeaterProfileSetPoints : byte
        {
            SP_0 = 0b00000000,
            SP_1 = 0b00000001,
            SP_2 = 0b00000010,
            SP_3 = 0b00000011,
            SP_4 = 0b00000100,
            SP_5 = 0b00000101,
            SP_6 = 0b00000110,
            SP_7 = 0b00000111,
            SP_8 = 0b00001000,
            SP_9 = 0b00001001
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice BME680 = null;

        // I2C Slave Address
        internal const byte BME680_I2C_ADDRESS = 0x76;

        // Registers
        private const byte BME680_STATUS = 0x73;
        private const byte BME680_RESET = 0xE0;
        private const byte BME680_ID = 0xD0;
        private const byte BME680_CONFIG = 0x75;
        private const byte BME680_CTRL_MEAS = 0x74;
        private const byte BME680_CTRL_HUM = 0x72;
        private const byte BME680_CTRL_GAS_1 = 0x71;
        private const byte BME680_CTRL_GAS_0 = 0x70;

        private const byte BME680_GAS_WAIT_0 = 0x64;
        private const byte BME680_GAS_WAIT_1 = 0x65;
        private const byte BME680_GAS_WAIT_2 = 0x66;
        private const byte BME680_GAS_WAIT_3 = 0x67;
        private const byte BME680_GAS_WAIT_4 = 0x68;
        private const byte BME680_GAS_WAIT_5 = 0x69;
        private const byte BME680_GAS_WAIT_6 = 0x6A;
        private const byte BME680_GAS_WAIT_7 = 0x6B;
        private const byte BME680_GAS_WAIT_8 = 0x6C;
        private const byte BME680_GAS_WAIT_9 = 0x6D;

        private const byte BME680_RES_HEAT_0 = 0x5A;
        private const byte BME680_RES_HEAT_1 = 0x5B;
        private const byte BME680_RES_HEAT_2 = 0x5C;
        private const byte BME680_RES_HEAT_3 = 0x5D;
        private const byte BME680_RES_HEAT_4 = 0x5E;
        private const byte BME680_RES_HEAT_5 = 0x5F;
        private const byte BME680_RES_HEAT_6 = 0x60;
        private const byte BME680_RES_HEAT_7 = 0x61;
        private const byte BME680_RES_HEAT_8 = 0x62;
        private const byte BME680_RES_HEAT_9 = 0x63;

        private const byte BME680_IDAC_HEAT_0 = 0x50;
        private const byte BME680_IDAC_HEAT_1 = 0x51;
        private const byte BME680_IDAC_HEAT_2 = 0x52;
        private const byte BME680_IDAC_HEAT_3 = 0x53;
        private const byte BME680_IDAC_HEAT_4 = 0x54;
        private const byte BME680_IDAC_HEAT_5 = 0x55;
        private const byte BME680_IDAC_HEAT_6 = 0x56;
        private const byte BME680_IDAC_HEAT_7 = 0x57;
        private const byte BME680_IDAC_HEAT_8 = 0x58;
        private const byte BME680_IDAC_HEAT_9 = 0x59;

        // Registers: Readout
        private const byte BME680_GAS_R_MSB = 0x2A;
        private const byte BME680_GAS_R_LSB = 0x2B;
        private const byte BME680_HUM_MSB = 0x25;
        private const byte BME680_HUM_LSB = 0x26;
        private const byte BME680_TEMP_MSB = 0x22;
        private const byte BME680_TEMP_LSB = 0x23;
        private const byte BME680_TEMP_XLSB = 0x24;
        private const byte BME680_PRESS_MSB = 0x1F;
        private const byte BME680_PRESS_LSB = 0x20;
        private const byte BME680_PRESS_XLSB = 0x21;
        private const byte BME680_EAS_STATUS_0 = 0x1D;

        // Registers: Calibration
        private const byte BME680_T2_LSB_REG = 0x8A;
        private const byte BME680_T2_MSB_REG = 0x8B;
        private const byte BME680_T3_REG = 0x8C;
        private const byte BME680_P1_LSB_REG = 0x8E;
        private const byte BME680_P1_MSB_REG = 0x8F;
        private const byte BME680_P2_LSB_REG = 0x90;
        private const byte BME680_P2_MSB_REG = 0x91;
        private const byte BME680_P3_REG = 0x92;
        private const byte BME680_P4_LSB_REG = 0x94;
        private const byte BME680_P4_MSB_REG = 0x95;
        private const byte BME680_P5_LSB_REG = 0x96;
        private const byte BME680_P5_MSB_REG = 0x97;
        private const byte BME680_P7_REG = 0x98;
        private const byte BME680_P6_REG = 0x99;
        private const byte BME680_P8_LSB_REG = 0x9C;
        private const byte BME680_P8_MSB_REG = 0x9D;
        private const byte BME680_P9_LSB_REG = 0x9E;
        private const byte BME680_P9_MSB_REG = 0x9F;
        private const byte BME680_P10_REG = 0xA0;
        private const byte BME680_H2_MSB_REG = 0xE1;
        private const byte BME680_H2_LSB_REG = 0xE2;
        private const byte BME680_H1_LSB_REG = 0xE2;
        private const byte BME680_H1_MSB_REG = 0xE3;
        private const byte BME680_H3_REG = 0xE4;
        private const byte BME680_H4_REG = 0xE5;
        private const byte BME680_H5_REG = 0xE6;
        private const byte BME680_H6_REG = 0xE7;
        private const byte BME680_H7_REG = 0xE8;
        private const byte BME680_T1_LSB_REG = 0xE9;
        private const byte BME680_T1_MSB_REG = 0xEA;
        private const byte BME680_GH2_LSB_REG = 0xEB;
        private const byte BME680_GH2_MSB_REG = 0xEC;
        private const byte BME680_GH1_REG = 0xED;
        private const byte BME680_GH3_REG = 0xEE;
        private const byte BME680_RES_HEAT_VAL = 0x00;
        private const byte BME680_RES_HEAT_RANGE = 0x02;
        private const byte BME680_RANGE_SW_ERR = 0x04;

        // Data: Calibration
        private UInt16 calT1;
        private Int16 calT2;
        private short calT3;
        private UInt16 calP1;
        private Int16 calP2;
        private short calP3;
        private Int16 calP4;
        private Int16 calP5;
        private short calP6;
        private short calP7;
        private Int16 calP8;
        private Int16 calP9;
        private ushort calP10;
        private UInt16 calH1;
        private UInt16 calH2;
        private short calH3;
        private short calH4;
        private short calH5;
        private ushort calH6;
        private short calH7;
        private short calGH1;
        private Int16 calGH2;
        private short calGH3;
        private double fineTemperature = 0;
        private short calAmbTemp = 25;
        private ushort calResHeatRange;
        private short calResHeatVal;
        private ushort calRangeSwErr;

        // Data: Gas range constants for resistance calculation 
        private readonly double[] const_array1 = { 1, 1, 1, 1, 1, 0.99, 1, 0.992, 1, 1, 0.998, 0.995, 1, 0.99, 1, 1 };
        private readonly double[] const_array2 = { 8000000, 4000000, 2000000, 1000000, 499500.4995, 248262.1648, 125000, 63004.03226, 31281.28128, 15625, 7812.5, 3906.25, 1953.125, 976.5625, 488.28125, 244.140625 };

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the BME680 sensor to get air quality level, temperature, humidity, pressure and altitude.
        /// </summary>
        public BME680Sensor()
        {
            // Initiate the sensor.
            Initialize();
        }

        #endregion

        #region I2CCom

        /// <summary>
        /// Initiates the sensor with the default configuration.
        /// </summary>
        private async void Initialize()
        {
            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(BME680_I2C_ADDRESS)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Shared
                };

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                BME680 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                ResetSensor();
                Task.Delay(10).Wait();
                ReadCalibrationData();
                ConfigureSensor(
                    TemperatureOversamplings.x08,
                    PressureOversamplings.x16,
                    HumidityOversamplings.x08,
                    IIRFilterCoefficients.FC_003,
                    250,
                    250);
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
            BME680.Write(data);
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

            BME680.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="reg">Read address.</param>
        /// <returns>Register data.</returns>
        private UInt16 ReadRegister_TwoBytes_LSBFirst(byte reg)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { reg };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            BME680.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);

            return value;
        }

        #endregion

        #region Sensor Configuration

        /// <summary>
        /// Verifies the sensor ID.
        /// </summary>
        /// <returns>True if sensor responses correctly. False if not.</returns>
        private bool CheckSensor()
        {
            return ReadRegister_OneByte(BME680_ID) == 0x61 ? true : false;
        }

        /// <summary>
        /// Initiates a soft-reset procedure, which has the same effect like power-on reset.
        /// </summary>
        private void ResetSensor()
        {
            WriteRegister(new byte[] { BME680_RESET, 0xB6 });
        }

        /// <summary>
        /// Sets the initial configuration data.
        /// </summary>
        /// <param name="temperatureOversampling">Temperature oversampling.</param>
        /// <param name="pressureOversampling">Pressure oversampling.</param>
        /// <param name="humidityOversampling">Humidity oversampling.</param>
        /// <param name="iirFilter">IIR Filter.</param>
        /// <param name="heatDuration">Gas sensor heat duration in ms. Max value is 252.</param>
        /// <param name="heatTemperature">Gas sensor heat temperature in C. Max value is 400.</param>
        private void ConfigureSensor(TemperatureOversamplings temperatureOversampling, PressureOversamplings pressureOversampling, HumidityOversamplings humidityOversampling, IIRFilterCoefficients iirFilter, ushort heatDuration, uint heatTemperature)
        {
            // Select humidity oversampling.
            WriteRegister(new byte[] { BME680_CTRL_HUM, (byte)humidityOversampling });
            Task.Delay(1).Wait();

            // Select temperature and pressure oversamplings.
            byte configValue = 0x00;
            configValue |= (byte)temperatureOversampling;
            configValue |= (byte)pressureOversampling;
            WriteRegister(new byte[] { BME680_CTRL_MEAS, Convert.ToByte(configValue) });
            Task.Delay(1).Wait();

            // Select IIR Filter for temperature sensor.
            WriteRegister(new byte[] { BME680_CONFIG, (byte)iirFilter });
            Task.Delay(1).Wait();

            // Enable gas measurements.
            SetGasMeasurement(true);
            Task.Delay(1).Wait();

            // Select index of heater set-point.
            SelectHeaterProfileSetPoint(HeaterProfileSetPoints.SP_0);
            Task.Delay(1).Wait();

            // Define heater-on time.
            WriteRegister(new byte[] { BME680_GAS_WAIT_0, CalculateHeatDuration(heatDuration) });
            Task.Delay(1).Wait();

            // Set heater temperature.
            WriteRegister(new byte[] { BME680_RES_HEAT_0, CalculateHeaterResistance(heatTemperature) });
            Task.Delay(1).Wait();

            // Set mode to forced mode.
            configValue = ReadRegister_OneByte(BME680_CTRL_MEAS);
            configValue |= (byte)OperationModes.ForcedMode;
            WriteRegister(new byte[] { BME680_CTRL_MEAS, configValue });
            Task.Delay(1).Wait();
        }

        #endregion

        #region Calibration and Compensation

        /// <summary>
        /// Reads the factory out calibration data from the sensor.
        /// </summary>
        /// <returns></returns>
        private void ReadCalibrationData()
        {
            // Temperature calibration.
            calT1 = ReadRegister_TwoBytes_LSBFirst(BME680_T1_LSB_REG);
            calT2 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_T2_LSB_REG);
            calT3 = ReadRegister_OneByte(BME680_T3_REG);

            // Pressure calibration.
            calP1 = ReadRegister_TwoBytes_LSBFirst(BME680_P1_LSB_REG);
            calP2 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_P2_LSB_REG);
            calP3 = ReadRegister_OneByte(BME680_P3_REG);
            calP4 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_P4_LSB_REG);
            calP5 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_P5_LSB_REG);
            calP6 = ReadRegister_OneByte(BME680_P6_REG);
            calP7 = ReadRegister_OneByte(BME680_P7_REG);
            calP8 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_P8_LSB_REG);
            calP9 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_P9_LSB_REG);
            calP10 = ReadRegister_OneByte(BME680_P10_REG);

            // Humidity calibration.
            calH1 = (UInt16)(ReadRegister_OneByte(BME680_H1_MSB_REG) << 4 | (ReadRegister_OneByte(BME680_H1_LSB_REG) & 0x0F));
            calH2 = (UInt16)(ReadRegister_OneByte(BME680_H2_MSB_REG) << 4 | ((ReadRegister_OneByte(BME680_H2_LSB_REG)) >> 4));
            calH3 = ReadRegister_OneByte(BME680_H3_REG);
            calH4 = ReadRegister_OneByte(BME680_H4_REG);
            calH5 = ReadRegister_OneByte(BME680_H5_REG);
            calH6 = ReadRegister_OneByte(BME680_H6_REG);
            calH7 = ReadRegister_OneByte(BME680_H7_REG);

            // Gas calibration.
            calGH1 = ReadRegister_OneByte(BME680_GH1_REG);
            calGH2 = (Int16)ReadRegister_TwoBytes_LSBFirst(BME680_GH2_LSB_REG);
            calGH3 = ReadRegister_OneByte(BME680_GH3_REG);

            // Heat calibration.
            calResHeatRange = (ushort)((ReadRegister_OneByte(BME680_RES_HEAT_RANGE) & 0x30) / 16);
            calResHeatVal = ReadRegister_OneByte(BME680_RES_HEAT_VAL);
            calRangeSwErr = (ushort)((ReadRegister_OneByte(BME680_RANGE_SW_ERR) & 0xF0) / 16);
        }

        /// <summary>
        /// Compensates the temperature.
        /// </summary>
        /// <param name="tempADC">Analog temperature value.</param>
        /// <returns>Temperature in Celcius.</returns>
        private double CompansateTemperature(int tempADC)
        {
            double val, var1, var2;

            var1 = (((tempADC / 16384.0) - (calT1 / 1024.0)) * calT2);
            var2 = ((((tempADC / 131072.0) - (calT1 / 8192.0)) * ((tempADC / 131072.0) - (calT1 / 8192.0))) * (calT3 * 16.0));
            fineTemperature = (var1 + var2);
            val = fineTemperature / 5120.0;

            return val;
        }

        /// <summary>
        /// Compensates the pressure.
        /// </summary>
        /// <param name="presADC">Analog pressure value.</param>
        /// <returns>Pressure.</returns>
        private double CompansatePressure(int presADC)
        {
            double val, var1, var2, var3;

            var1 = (fineTemperature / 2.0) - 64000.0;
            var2 = var1 * var1 * (calP6 / 131072.0);
            var2 = var2 + (var1 * calP5 * 2.0);
            var2 = (var2 / 4.0) + (calP4 * 65536.0);
            var1 = (((calP3 * var1 * var1) / 16384.0) + (calP2 * var1)) / 524288.0;
            var1 = (1.0 + (var1 / 32768.0)) * calP1;
            val = 1048576.0f - presADC;

            if (var1 != 0)
            {
                val = ((val - (var2 / 4096.0)) * 6250.0) / var1;
                var1 = (calP9 * val * val) / 2147483648.0;
                var2 = val * (calP8 / 32768.0);
                var3 = (val / 256.0) * (val / 256.0) * (val / 256.0) * (calP10 / 131072.0);
                val = val + (var1 + var2 + var3 + (calP7 * 128.0)) / 16.0;
            }
            else
                val = 0;

            return val;
        }

        /// <summary>
        /// Compensates the humidity.
        /// </summary>
        /// <param name="humADC">Analog humidity value.</param>
        /// <returns>Relative humidity.</returns>
        private double CompansateHumidity(int humADC)
        {
            double val, var1, var2, var3, var4, temp_comp;

            temp_comp = fineTemperature / 5120.0;
            var1 = humADC - ((calH1 * 16.0) + ((calH3 / 2.0) * temp_comp));
            var2 = var1 * (((calH2 / 262144.0) * (1.0 + ((calH4 / 16384.0) * temp_comp) + ((calH5 / 1048576.0) * temp_comp * temp_comp))));
            var3 = calH6 / 16384.0;
            var4 = calH7 / 2097152.0;
            val = var2 + ((var3 + (var4 * temp_comp)) * var2 * var2);

            if (val > 100.0)
                val = 100.0;
            else if (val < 0.0)
                val = 0.0;

            return val;
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Triggers all measurements, and then waits for measurement completion.
        /// </summary>
        /// <param name="gasMeasurementEnabled">Enable or disable gas measurement.</param>
        private void ForceRead(bool gasMeasurementEnabled)
        {
            byte temp;

            SetGasMeasurement(gasMeasurementEnabled);

            temp = ReadRegister_OneByte(BME680_CTRL_MEAS);
            temp |= (byte)OperationModes.ForcedMode;
            WriteRegister(new byte[] { BME680_CTRL_MEAS, temp });
            
            while(GetMeasuringState())
                Task.Delay(1).Wait();

            if (gasMeasurementEnabled)
                while (GetGasMeasuringStatus())
                    Task.Delay(1).Wait();
        }
        
        /// <summary>
        /// Reads the temperature in Celcius.
        /// </summary>
        /// <returns>Temperature in Celcius.</returns>
        public double ReadTemperature()
        {
            int tempADC;

            ForceRead(false);

            tempADC = ReadRegister_OneByte(BME680_TEMP_MSB) * 4096;
            tempADC += ReadRegister_OneByte(BME680_TEMP_LSB) * 16;
            tempADC += ReadRegister_OneByte(BME680_TEMP_XLSB) / 16;

            return CompansateTemperature(tempADC);
        }

        /// <summary>
        /// Reads the relative humidity.
        /// </summary>
        /// <returns>Relative humidity.</returns>
        public double ReadHumidity()
        {
            int humADC;

            ForceRead(false);

            humADC = ReadRegister_OneByte(BME680_HUM_MSB) * 256;
            humADC += ReadRegister_OneByte(BME680_HUM_LSB);

            return CompansateHumidity(humADC);
        }

        /// <summary>
        /// Reads the pressure in Pa.
        /// </summary>
        /// <returns>Pressure in Pa.</returns>
        public double ReadPressure()
        {
            int presADC;

            ForceRead(false);

            presADC = ReadRegister_OneByte(BME680_PRESS_MSB) * 4096;
            presADC += ReadRegister_OneByte(BME680_PRESS_LSB) * 16;
            presADC += ReadRegister_OneByte(BME680_PRESS_XLSB) / 16;

            return CompansatePressure(presADC);
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

        /// <summary>
        /// Reads the gas resistance.
        /// </summary>
        /// <returns>Gas resistance in Ohms.</returns>
        public double ReadGasResistance()
        {
            ushort gasRange;
            int gasResADC, tempADC;
            double val;
            
            ForceRead(true);

            tempADC = ReadRegister_OneByte(BME680_TEMP_MSB) * 4096;
            tempADC += ReadRegister_OneByte(BME680_TEMP_LSB) * 16;
            tempADC += ReadRegister_OneByte(BME680_TEMP_XLSB) / 16;
            gasResADC = ReadRegister_OneByte(BME680_GAS_R_MSB) * 4;
            gasResADC += ReadRegister_OneByte(BME680_GAS_R_LSB) / 64;
            gasRange = (ushort)(ReadRegister_OneByte(BME680_GAS_R_LSB) & 0x0F);

            calAmbTemp = Convert.ToInt16(CompansateTemperature(tempADC));
            val = CalculateGasResistance(gasResADC, gasRange);

            return val;
        }

        /// <summary>
        /// Reads temperature, pressure and relative humidity.
        /// </summary>
        /// <returns>Temperature in Celcius, pressure in Pa and relative humidity respectively.</returns>
        public double[] ReadTPH()
        {
            int tempADC, humADC, presADC;
            double[] resultsTPH = { 0.0, 0.0, 0.0 };

            ForceRead(false);

            tempADC = ReadRegister_OneByte(BME680_TEMP_MSB) * 4096;
            tempADC += ReadRegister_OneByte(BME680_TEMP_LSB) * 16;
            tempADC += ReadRegister_OneByte(BME680_TEMP_XLSB) / 16;

            presADC = ReadRegister_OneByte(BME680_PRESS_MSB) * 4096;
            presADC += ReadRegister_OneByte(BME680_PRESS_LSB) * 16;
            presADC += ReadRegister_OneByte(BME680_PRESS_XLSB) / 16;

            humADC = ReadRegister_OneByte(BME680_HUM_MSB) * 256;
            humADC += ReadRegister_OneByte(BME680_HUM_LSB);

            resultsTPH[0] = CompansateTemperature(tempADC);
            resultsTPH[1] = CompansatePressure(presADC);
            resultsTPH[2] = CompansateHumidity(humADC);

            return resultsTPH;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Turns gas measurement on of off.
        /// </summary>
        /// <param name="state">Gas measurement mode. True for on, false for off.</param>
        private void SetGasMeasurement(bool state)
        {
            byte configValue;
            configValue = ReadRegister_OneByte(BME680_CTRL_GAS_1);

            if (state)
                configValue |= 0b00010000;
            else
                configValue &= 0b11101111;

            WriteRegister(new byte[] { BME680_CTRL_GAS_1, configValue });
        }

        /// <summary>
        /// Selects heater set-points of the sensor that will be used in forced mode.
        /// </summary>
        /// <param name="heaterProfileSetPoint">Heater profile set-point.</param>
        private void SelectHeaterProfileSetPoint(HeaterProfileSetPoints heaterProfileSetPoint)
        {
            WriteRegister(new byte[] { BME680_CTRL_GAS_1, (byte)heaterProfileSetPoint });
        }

        /// <summary>
        /// Gets the measuring status from the measuring bit.
        /// </summary>
        /// <returns>True if all conversions are running. False if not.</returns>
        private bool GetMeasuringState()
        {
            byte readValue = ReadRegister_OneByte(BME680_EAS_STATUS_0);

            return ((readValue & 0b00100000) == 0b00100000) ? true : false;
        }

        /// <summary>
        /// Gets the gas measurement running status from the gas_measuring bit.
        /// </summary>
        /// <returns>True if gas measurement is running. False if not.</returns>
        private bool GetGasMeasuringStatus()
        {
            byte readValue = ReadRegister_OneByte(BME680_EAS_STATUS_0);

            return ((readValue & 0b01000000) == 0b01000000) ? true : false;
        }

        /// <summary>
        /// Gets the new data status from the new_data_0 bit.
        /// </summary>
        /// <returns>True if measured data are stored into the output data registers. False if not.</returns>
        private bool GetNewDataStatus()
        {
            byte readValue = ReadRegister_OneByte(BME680_EAS_STATUS_0);

            return ((readValue & 0b10000000) == 0b10000000) ? true : false;
        }

        /// <summary>
        /// Calculates the gas resistance value.
        /// </summary>
        /// <param name="gasResADC">ADC resistance value.</param>
        /// <param name="gasRange">ADC range.</param>
        /// <returns>Gas resistance.</returns>
        private double CalculateGasResistance(int gasResADC, ushort gasRange)
        {
            double var1 = (1340.0 + 5.0 * calRangeSwErr) * const_array1[gasRange];
            double gasres = var1 * const_array2[gasRange] / (gasResADC - 512.0 + var1);

            return gasres;
        }
        
        /// <summary>
        /// Calculates the heater resistance value for target heater resistance (Res_Heat_X) registers.
        /// </summary>
        /// <param name="targetTemp"></param>
        /// <returns></returns>
        private byte CalculateHeaterResistance(uint targetTemp)
        {
            double var1 = 0, var2 = 0, var3 = 0, var4 = 0, var5 = 0;
            byte res_heat = 0;

            if (targetTemp > 400) // Maximum temperature
                targetTemp = 400;

            var1 = (calGH1 / 16.0) + 49.0;
            var2 = ((calGH2 / 32768.0) * 0.0005) + 0.00235;
            var3 = calGH3 / 1024.0;
            var4 = var1 * (1.0 + (var2 * targetTemp));
            var5 = var4 + (var3 * calAmbTemp);
            res_heat = (byte)(3.4 * ((var5 * (4 / (4 + calResHeatRange)) * (1 / (1 + (calResHeatVal * 0.002)))) - 25));

            return res_heat;
        }

        /// <summary>
        /// Calculates the heat duration value for gas sensor wait time (Gas_Wait_X) registers.
        /// </summary>
        /// <param name="dur">Wait duration in ms. Max value is 252.</param>
        /// <returns>Wait duration parameter in byte.</returns>
        private byte CalculateHeatDuration(int dur)
        {
            ushort factor = 0, durval;

            if (dur >= 0xFC)
            {
                durval = 0xFF; // Max duration
            }
            else
            {
                while (dur > 0x3F)
                {
                    dur = dur / 4;
                    factor += 1;
                }
                durval = (ushort)(dur + (factor * 64));
            }

            return Convert.ToByte(durval);
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            BME680.Dispose();

            calT1 = 0;
            calT2 = 0;
            calT3 = 0;
            calP1 = 0;
            calP2 = 0;
            calP3 = 0;
            calP4 = 0;
            calP5 = 0;
            calP6 = 0;
            calP7 = 0;
            calP8 = 0;
            calP9 = 0;
            calP10 = 0;
            calH1 = 0;
            calH2 = 0;
            calH3 = 0;
            calH4 = 0;
            calH5 = 0;
            calH6 = 0;
            calH7 = 0;
            calGH1 = 0;
            calGH2 = 0;
            calGH3 = 0;
            fineTemperature = 0;
            calAmbTemp = 0;
            calResHeatRange = 0;
            calResHeatVal = 0;
            calRangeSwErr = 0;
        }

        #endregion
    }
}
