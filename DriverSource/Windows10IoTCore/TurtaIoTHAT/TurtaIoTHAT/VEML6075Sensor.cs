/* Turta® IoT HAT Helper for Windows® 10 IoT Core
 * Copyright © 2017 Turta
 * Distributed under the terms of the MIT license.
 */

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace TurtaIoTHAT
{
    public class VEML6075Sensor : IDisposable
    {
        #region Enumerations

        public enum IntegrationTime : byte
        {
            IT_050ms = 0b00000000,
            IT_100ms = 0b00010000,
            IT_200ms = 0b00100000,
            IT_400ms = 0b00110000,
            IT_800ms = 0b01000000
        }

        public enum DynamicSetting : byte
        {
            Normal = 0b00000000,
            High = 0b00001000
        }

        public enum Trigger : byte
        {
            NoActiveForceTrigger = 0b00000000,
            TriggerOneMeasurement = 0b00000100
        }

        public enum ActiveForceMode : byte
        {
            NormalMode = 0b00000000,
            ActiveForceMode = 0b00000010
        }

        public enum PowerMode : byte
        {
            PowerOn = 0b00000000,
            ShutDown = 0b00000001
        }

        #endregion

        #region Globals

        // I2C Device
        private I2cDevice veml6075 = null;

        // I2C Slave Address
        internal const byte VEML6075_I2C_ADDRESS = 0x10;

        // Registers
        private const byte VEML6075_UV_CONF = 0x00;
        private const byte VEML6075_UVA_DATA = 0x07;
        private const byte VEML6075_DUMMY = 0x08;
        private const byte VEML6075_UVB_DATA = 0x09;
        private const byte VEML6075_UVCOMP1_DATA = 0x0A;
        private const byte VEML6075_UVCOMP2_DATA = 0x0B;
        private const byte VEML6075_ID = 0x0C;

        // Default Values
        private double uva_a_coef = 2.22; // UVA VIS Coefficient
        private double uva_b_coef = 1.33; // VA IR Coefficient
        private double uvb_c_coef = 2.95; // UVB VIS Coefficient
        private double uvb_d_coef = 1.74; // UVB IR Coefficient
        private double uva_resp = 0.001461; // UVA Responsivity
        private double uvb_resp = 0.002591; // UVB Responsivity

        // Correction Factors
        private double k1 = 0;
        private double k2 = 0;

        // System
        private bool isInitialized = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initiates the VEML6075 sensor to get UVA, UVB and UVIndex.
        /// </summary>
        public VEML6075Sensor()
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
                I2cConnectionSettings settings = new I2cConnectionSettings(VEML6075_I2C_ADDRESS);

                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1"));

                veml6075 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                isInitialized = true;
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
            veml6075.Write(data);
            await Task.Delay(1);
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

            veml6075.WriteRead(writeBuffer, readBuffer);
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

            veml6075.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        #endregion

        #region Sensor Configuration

        /// <summary>
        /// Configures the VEML6075 sensor. Verifies if the settings are stored.
        /// </summary>
        /// <param name="UV_IT">UV integration time.</param>
        /// <param name="HD">Dynamic setting.</param>
        /// <param name="UV_TRIG">Measurement trigger.</param>
        /// <param name="UV_AF">Active force mode.</param>
        /// <param name="SD">Power mode.</param>
        /// <returns>True if settings are stored. False if not.</returns>
        public async Task<bool> Config(IntegrationTime UV_IT, DynamicSetting HD, Trigger UV_TRIG, ActiveForceMode UV_AF, PowerMode SD)
        {
            int tryCounter = 0;

            while (!isInitialized)
            {
                await Task.Delay(1);
                if (tryCounter++ > 1000)
                    return false;
            }

            try
            {
                byte configCommand = 0x00;

                configCommand += (byte)UV_IT;
                configCommand += (byte)HD;
                configCommand += (byte)UV_TRIG;
                configCommand += (byte)UV_AF;
                configCommand += (byte)SD;

                WriteRegister(new byte[] { VEML6075_UV_CONF, Convert.ToByte(configCommand), 0x00 });

                if ((configCommand & 0b11111011) == ReadRegister_TwoBytes(VEML6075_UV_CONF))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Sensor Readouts

        /// <summary>
        /// Triggers one time measurement for Active Force Mode enabled scenarios.
        /// </summary>
        public void TriggerOneMeasurement()
        {
            byte[] tempConfig = ReadRegister_TwoBytesArray(VEML6075_UV_CONF);

            tempConfig[0] |= 0b00000100;

            WriteRegister(new byte[] { VEML6075_UV_CONF, tempConfig[0], tempConfig[1] });
        }

        /// <summary>
        /// Reads RAW UVA.
        /// </summary>
        /// <returns>RAW UVA Value.</returns>
        public UInt16 Read_RAW_UVA()
        {
            return ReadRegister_TwoBytes(VEML6075_UVA_DATA);
        }

        /// <summary>
        /// Reads RAW UVB.
        /// </summary>
        /// <returns>RAW UVB Value.</returns>
        public UInt16 Read_RAW_UVB()
        {
            return ReadRegister_TwoBytes(VEML6075_UVB_DATA);
        }

        /// <summary>
        /// Reads RAW UVD.
        /// </summary>
        /// <returns>RAW UVD Value.</returns>
        public UInt16 Read_RAW_UVD()
        {
            return ReadRegister_TwoBytes(VEML6075_DUMMY);
        }

        /// <summary>
        /// Reads Noise Compensation Channel 1 data which allows only visible noise to pass through.
        /// </summary>
        /// <returns>UV Comp 1 Value.</returns>
        public UInt16 Read_RAW_UVCOMP1()
        {
            return ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
        }

        /// <summary>
        /// Reads Noise Compensation Channel 2 data which allows only infrared noise to pass through.
        /// </summary>
        /// <returns>UV Comp 2 Value.</returns>
        public UInt16 Read_RAW_UVCOMP2()
        {
            return ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);
        }

        /// <summary>
        /// Calculates Compensated UVA.
        /// </summary>
        /// <returns>UVA Comp Value.</returns>
        public double Calculate_Compensated_UVA()
        {
            // Formula:
            // UVAcalc = UVA - a x UVcomp1 - b x UVcomp2

            UInt16 uva = ReadRegister_TwoBytes(VEML6075_UVA_DATA);
            UInt16 uvcomp1 = ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
            UInt16 uvcomp2 = ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);

            Double UVAcalc = uva - uva_a_coef * uvcomp1 - uva_b_coef * uvcomp2;

            return UVAcalc;
        }

        /// <summary>
        /// Calculates Compensated UVB.
        /// </summary>
        /// <returns>UVB Comp Value.</returns>
        public double Calculate_Compensated_UVB()
        {
            // Formula:
            // UVBcalc = UVB - c x UVcomp1 - d x UVcomp2

            UInt16 uvb = ReadRegister_TwoBytes(VEML6075_UVB_DATA);
            UInt16 uvcomp1 = ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
            UInt16 uvcomp2 = ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);

            Double UVBcalc = uvb - uvb_c_coef * uvcomp1 - uvb_d_coef * uvcomp2;

            return UVBcalc;
        }

        /// <summary>
        /// Calculates the UV Index A.
        /// </summary>
        /// <returns>UV Index A Value.</returns>
        public double Calculate_UV_Index_A()
        {
            // Formula:
            // UVIA = UVAcalc x k1 x UVAresponsivity

            UInt16 uva = ReadRegister_TwoBytes(VEML6075_UVA_DATA);
            UInt16 uvcomp1 = ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
            UInt16 uvcomp2 = ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);

            Double UVAcalc = uva - uva_a_coef * uvcomp1 - uva_b_coef * uvcomp2;

            Double UVIA = UVAcalc * k1 * uva_resp;

            return UVIA;
        }

        /// <summary>
        /// Calculates the UV Index B.
        /// </summary>
        /// <returns>UV Index B Value.</returns>
        public double Calculate_UV_Index_B()
        {
            // Formula:
            // UVIB = UVBcalc x k2 x UVBresponsivity

            UInt16 uvb = ReadRegister_TwoBytes(VEML6075_UVB_DATA);
            UInt16 uvcomp1 = ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
            UInt16 uvcomp2 = ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);

            Double UVBcalc = uvb - uvb_c_coef * uvcomp1 - uvb_d_coef * uvcomp2;

            Double UVIB = UVBcalc * k2 * uvb_resp;

            return UVIB;
        }

        /// <summary>
        /// Calculates the Average UV Index.
        /// </summary>
        /// <returns>Average UV Index Value.</returns>
        public double Calculate_Average_UV_Index()
        {
            // Formula:
            //UVAcomp = (UVA - UVD) - a * (UVcomp1 - UVD) - b * (UVcomp2 - UVD);
            //UVBcomp = (UVB - UVD) - c * (UVcomp1 - UVD) - d * (UVcomp2 - UVD);
            //UVI = ((UVBcomp * UVBresp) + (UVAcomp * UVAresp)) / 2;

            UInt16 uva = ReadRegister_TwoBytes(VEML6075_UVA_DATA);
            UInt16 uvb = ReadRegister_TwoBytes(VEML6075_UVB_DATA);
            UInt16 uvd = ReadRegister_TwoBytes(VEML6075_DUMMY);
            UInt16 uvcomp1 = ReadRegister_TwoBytes(VEML6075_UVCOMP1_DATA);
            UInt16 uvcomp2 = ReadRegister_TwoBytes(VEML6075_UVCOMP2_DATA);

            double UVAcomp = (uva - uvd) - uva_a_coef * (uvcomp1 - uvd) - uva_b_coef * (uvcomp2 - uvd);
            double UVBcomp = (uvb - uvd) - uvb_c_coef * (uvcomp1 - uvd) - uvb_d_coef * (uvcomp2 - uvd);
            double UVI = ((UVBcomp * uvb_resp) + (UVAcomp * uva_resp)) / 2;

            if (UVI < 0) UVI = 0;

            return UVI;
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            isInitialized = false;
            veml6075.Dispose();
        }

        #endregion
    }
}
