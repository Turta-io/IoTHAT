# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for Bosch Sensortec BME680 Environmental Sensor
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

import time
import math
from enum import IntEnum
from smbus import SMBus

#Enumerations

class OperationModes(IntEnum):
    Sleep = 0b00000000
    ForcedMode = 0b00000001

class TemperatureOversamplings(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00100000
    x02 = 0b01000000
    x04 = 0b01100000
    x08 = 0b10000000
    x16 = 0b10100000

class HumidityOversamplings(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00000001
    x02 = 0b00000010
    x04 = 0b00000011
    x08 = 0b00000100
    x16 = 0b00000101

class PressureOversamplings(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00000100
    x02 = 0b00001000
    x04 = 0b00001100
    x08 = 0b00010000
    x16 = 0b00010100

class IIRFilterCoefficients(IntEnum):
    FC_000 = 0b00000000
    FC_001 = 0b00000100
    FC_003 = 0b00001000
    FC_007 = 0b00001100
    FC_015 = 0b00010000
    FC_031 = 0b00010100
    FC_063 = 0b00011000
    FC_127 = 0b00011100

class HeaterProfileSetPoints(IntEnum):
    SP_0 = 0b00000000
    SP_1 = 0b00000001
    SP_2 = 0b00000010
    SP_3 = 0b00000011
    SP_4 = 0b00000100
    SP_5 = 0b00000101
    SP_6 = 0b00000110
    SP_7 = 0b00000111
    SP_8 = 0b00001000
    SP_9 = 0b00001001

class BME680Sensor:
    """BME680 Sensor"""

    #I2C Slave Address
    I2C_ADDRESS = 0x76

    #Registers
    BME680_STATUS = 0x73
    BME680_RESET = 0xE0
    BME680_ID = 0xD0
    BME680_CONFIG = 0x75
    BME680_CTRL_MEAS = 0x74
    BME680_CTRL_HUM = 0x72
    BME680_CTRL_GAS_1 = 0x71
    BME680_CTRL_GAS_0 = 0x70

    BME680_GAS_WAIT_0 = 0x64
    BME680_GAS_WAIT_1 = 0x65
    BME680_GAS_WAIT_2 = 0x66
    BME680_GAS_WAIT_3 = 0x67
    BME680_GAS_WAIT_4 = 0x68
    BME680_GAS_WAIT_5 = 0x69
    BME680_GAS_WAIT_6 = 0x6A
    BME680_GAS_WAIT_7 = 0x6B
    BME680_GAS_WAIT_8 = 0x6C
    BME680_GAS_WAIT_9 = 0x6D

    BME680_RES_HEAT_0 = 0x5A
    BME680_RES_HEAT_1 = 0x5B
    BME680_RES_HEAT_2 = 0x5C
    BME680_RES_HEAT_3 = 0x5D
    BME680_RES_HEAT_4 = 0x5E
    BME680_RES_HEAT_5 = 0x5F
    BME680_RES_HEAT_6 = 0x60
    BME680_RES_HEAT_7 = 0x61
    BME680_RES_HEAT_8 = 0x62
    BME680_RES_HEAT_9 = 0x63

    BME680_IDAC_HEAT_0 = 0x50
    BME680_IDAC_HEAT_1 = 0x51
    BME680_IDAC_HEAT_2 = 0x52
    BME680_IDAC_HEAT_3 = 0x53
    BME680_IDAC_HEAT_4 = 0x54
    BME680_IDAC_HEAT_5 = 0x55
    BME680_IDAC_HEAT_6 = 0x56
    BME680_IDAC_HEAT_7 = 0x57
    BME680_IDAC_HEAT_8 = 0x58
    BME680_IDAC_HEAT_9 = 0x59

    #Registers: Readout
    BME680_GAS_R_MSB = 0x2A
    BME680_GAS_R_LSB = 0x2B
    BME680_HUM_MSB = 0x25
    BME680_HUM_LSB = 0x26
    BME680_TEMP_MSB = 0x22
    BME680_TEMP_LSB = 0x23
    BME680_TEMP_XLSB = 0x24
    BME680_PRESS_MSB = 0x1F
    BME680_PRESS_LSB = 0x20
    BME680_PRESS_XLSB = 0x21
    BME680_EAS_STATUS_0 = 0x1D

    #Registers: Calibration
    BME680_T2_LSB_REG = 0x8A
    BME680_T2_MSB_REG = 0x8B
    BME680_T3_REG = 0x8C
    BME680_P1_LSB_REG = 0x8E
    BME680_P1_MSB_REG = 0x8F
    BME680_P2_LSB_REG = 0x90
    BME680_P2_MSB_REG = 0x91
    BME680_P3_REG = 0x92
    BME680_P4_LSB_REG = 0x94
    BME680_P4_MSB_REG = 0x95
    BME680_P5_LSB_REG = 0x96
    BME680_P5_MSB_REG = 0x97
    BME680_P7_REG = 0x98
    BME680_P6_REG = 0x99
    BME680_P8_LSB_REG = 0x9C
    BME680_P8_MSB_REG = 0x9D
    BME680_P9_LSB_REG = 0x9E
    BME680_P9_MSB_REG = 0x9F
    BME680_P10_REG = 0xA0
    BME680_H2_MSB_REG = 0xE1
    BME680_H2_LSB_REG = 0xE2
    BME680_H1_LSB_REG = 0xE2
    BME680_H1_MSB_REG = 0xE3
    BME680_H3_REG = 0xE4
    BME680_H4_REG = 0xE5
    BME680_H5_REG = 0xE6
    BME680_H6_REG = 0xE7
    BME680_H7_REG = 0xE8
    BME680_T1_LSB_REG = 0xE9
    BME680_T1_MSB_REG = 0xEA
    BME680_GH2_LSB_REG = 0xEB
    BME680_GH2_MSB_REG = 0xEC
    BME680_GH1_REG = 0xED
    BME680_GH3_REG = 0xEE
    BME680_RES_HEAT_VAL = 0x00
    BME680_RES_HEAT_RANGE = 0x02
    BME680_RANGE_SW_ERR = 0x04

    #Data: Calibration
    calT1 = None
    calT2 = None
    calT3 = None
    calP1 = None
    calP2 = None
    calP3 = None
    calP4 = None
    calP5 = None
    calP6 = None
    calP7 = None
    calP8 = None
    calP9 = None
    calP10 = None
    calH1 = None
    calH2 = None
    calH3 = None
    calH4 = None
    calH5 = None
    calH6 = None
    calH7 = None
    calGH1 = None
    calGH2 = None
    calGH3 = None
    fineTemperature = None
    calAmbTemp = 25
    calResHeatRange = None
    calResHeatVal = None
    calRangeSwErr = None

    #Data: Gas range constants for resistance calculation 
    const_array1 = [ 1, 1, 1, 1, 1, 0.99, 1, 0.992, 1, 1, 0.998, 0.995, 1, 0.99, 1, 1 ]
    const_array2 = [ 8000000, 4000000, 2000000, 1000000, 499500.4995, 248262.1648, 125000, 63004.03226, 31281.28128, 15625, 7812.5, 3906.25, 1953.125, 976.5625, 488.28125, 244.140625 ]

    #I2C Config
    bus = SMBus(1)

    #I2C Communication

    def _write_register(self, reg_addr, data):
        """Writes data to the I2C device.
        :param reg_addr: Register address.
        :param data: Data.
        """
        self.bus.write_i2c_block_data(self.I2C_ADDRESS, reg_addr, [ data & 0xFF ])

    def _read_register_1ubyte(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 1)
        return buffer[0]

    def _read_register_1sbyte(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 1)
        val = buffer[0]
        if val & (1 << 7) != 0:
            val = val - (1 << 7)
        return val

    def _read_2bytes_as_ushort_lsbfirst(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        return buffer[0] + (buffer[1] << 8)

    def _read_2bytes_as_short_lsbfirst(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        val = buffer[0] + (buffer[1] << 8)
        if val & (1 << 15) != 0:
            val = val - (1 << 15)
        return val

    def __init__(self):
        """Initiates the BME680 sensor to get air quality level, temperature, humidity, pressure and altitude."""
        self._read_calibration_data()
        self.configure_sensor(
            TemperatureOversamplings.x08,
            PressureOversamplings.x16,
            HumidityOversamplings.x08,
            IIRFilterCoefficients.FC_003,
            250,
            250)

    #Sensor Configuration

    def check_sensor(self):
        """Verifies the sensor ID."""
        return True if (self._read_register_1ubyte(self.BME680_ID) == 0x61) else False

    def reset_sensor(self):
        """Initiates a soft-reset procedure, which has the same effect like power-on reset."""
        self._write_register(self.BME680_RESET, 0xB6)

    def configure_sensor(self, temperature_oversampling, pressure_oversampling, humidity_oversampling, iir_filter, heat_duration, heat_temperature):
        """Sets the configuration data.
        :param temperature_oversampling: Temperature oversampling.
        :param pressure_oversampling: Pressure oversampling.
        :param humidity_oversampling: Humidity oversampling.
        :param iir_filter: IIR Filter.
        :param heat_duration: Gas sensor heat duration in ms. Max value is 252.
        :param heat_temperature: Gas sensor heat temperature in C. Max value is 400.
        """
        #Select humidity oversampling.
        self._write_register(self.BME680_CTRL_HUM, humidity_oversampling)
        time.sleep(0.001)

        #Select temperature and pressure oversamplings.
        configValue = 0x00
        configValue |= temperature_oversampling
        configValue |= pressure_oversampling
        self._write_register(self.BME680_CTRL_MEAS, configValue)
        time.sleep(0.001)

        #Select IIR Filter for temperature sensor.
        self._write_register(self.BME680_CONFIG, iir_filter)
        time.sleep(0.001)

        #Enable gas measurements.
        self._set_gas_measurement(True)
        time.sleep(0.001)

        #Select index of heater set-point.
        self._select_heater_profile_setpoint(HeaterProfileSetPoints.SP_0)
        time.sleep(0.001)

        #Define heater-on time.
        self._write_register(self.BME680_GAS_WAIT_0, self._calculate_heat_duration(heat_duration))
        time.sleep(0.001)

        #Set heater temperature.
        self._write_register(self.BME680_RES_HEAT_0, self._calculate_heater_resistance(heat_temperature))
        time.sleep(0.001)

        #Set mode to forced mode.
        configValue = self._read_register_1ubyte(self.BME680_CTRL_MEAS)
        configValue |= OperationModes.ForcedMode
        self._write_register(self.BME680_CTRL_MEAS, configValue)
        time.sleep(0.001)

    #Calibration and Compensation

    def _read_calibration_data(self):
        """Reads the factory out calibration data from the sensor."""
        #Declare global variables.
        global calT1
        global calT2
        global calT3
        global calP1
        global calP2
        global calP3
        global calP4
        global calP5
        global calP6
        global calP7
        global calP8
        global calP9
        global calP10
        global calH1
        global calH2
        global calH3
        global calH4
        global calH5
        global calH6
        global calH7
        global calGH1
        global calGH2
        global calGH3
        global calResHeatRange
        global calResHeatVal
        global calRangeSwErr

        #Temperature calibration.
        calT1 = self._read_2bytes_as_ushort_lsbfirst(self.BME680_T1_LSB_REG)
        calT2 = self._read_2bytes_as_short_lsbfirst(self.BME680_T2_LSB_REG)
        calT3 = self._read_register_1sbyte(self.BME680_T3_REG)

        #Pressure calibration.
        calP1 = self._read_2bytes_as_ushort_lsbfirst(self.BME680_P1_LSB_REG)
        calP2 = self._read_2bytes_as_short_lsbfirst(self.BME680_P2_LSB_REG)
        calP3 = self._read_register_1sbyte(self.BME680_P3_REG)
        calP4 = self._read_2bytes_as_short_lsbfirst(self.BME680_P4_LSB_REG)
        calP5 = self._read_2bytes_as_short_lsbfirst(self.BME680_P5_LSB_REG)
        calP6 = self._read_register_1sbyte(self.BME680_P6_REG)
        calP7 = self._read_register_1sbyte(self.BME680_P7_REG)
        calP8 = self._read_2bytes_as_short_lsbfirst(self.BME680_P8_LSB_REG)
        calP9 = self._read_2bytes_as_short_lsbfirst(self.BME680_P9_LSB_REG)
        calP10 = self._read_register_1ubyte(self.BME680_P10_REG)

        #Humidity calibration.
        calH1 = self._read_register_1ubyte(self.BME680_H1_MSB_REG) << 4 | (self._read_register_1ubyte(self.BME680_H1_LSB_REG) & 0x0F)
        calH2 = self._read_register_1ubyte(self.BME680_H2_MSB_REG) << 4 | ((self._read_register_1ubyte(self.BME680_H2_LSB_REG)) >> 4)
        calH3 = self._read_register_1sbyte(self.BME680_H3_REG)
        calH4 = self._read_register_1sbyte(self.BME680_H4_REG)
        calH5 = self._read_register_1sbyte(self.BME680_H5_REG)
        calH6 = self._read_register_1ubyte(self.BME680_H6_REG)
        calH7 = self._read_register_1sbyte(self.BME680_H7_REG)

        #Gas calibration.
        calGH1 = self._read_register_1sbyte(self.BME680_GH1_REG)
        calGH2 = self._read_2bytes_as_short_lsbfirst(self.BME680_GH2_LSB_REG)
        calGH3 = self._read_register_1sbyte(self.BME680_GH3_REG)

        #Heat calibration.
        calResHeatRange = (self._read_register_1ubyte(self.BME680_RES_HEAT_RANGE) & 0x30) / 16
        calResHeatVal = self._read_register_1sbyte(self.BME680_RES_HEAT_VAL)
        calRangeSwErr = (self._read_register_1sbyte(self.BME680_RANGE_SW_ERR) & 0xF0) / 16

    def _compensate_temperature(self, tempADC):
        """Compensates the temperature.
        :param tempADC: Analog temperature value.
        """
        #Declare global variable.
        global fineTemperature

        var1 = (((tempADC / 16384.0) - (calT1 / 1024.0)) * calT2)
        var2 = ((((tempADC / 131072.0) - (calT1 / 8192.0)) * ((tempADC / 131072.0) - (calT1 / 8192.0))) * (calT3 * 16.0))
        fineTemperature = (var1 + var2)
        val = fineTemperature / 5120.0

        return float(val)

    def _compensate_pressure(self, presADC):
        """Compensates the pressure.
        :param presADC: Analog pressure value.
        """
        var1 = (float(fineTemperature) / 2.0) - 64000.0
        var2 = var1 * var1 * (float(calP6) / 131072.0)
        var2 = var2 + (var1 * float(calP5) * 2.0)
        var2 = (var2 / 4.0) + (float(calP4) * 65536.0)
        var1 = (((float(calP3) * var1 * var1) / 16384.0) + (float(calP2) * var1)) / 524288.0
        var1 = (1.0 + (var1 / 32768.0)) * float(calP1)
        val = 1048576.0 - float(presADC)

        if var1 != 0:
            val = ((val - (var2 / 4096.0)) * 6250.0) / var1
            var1 = (calP9 * val * val) / 2147483648.0
            var2 = val * (float(calP8) / 32768.0)
            var3 = (val / 256.0) * (val / 256.0) * (val / 256.0) * (calP10 / 131072.0)
            val = val + (var1 + var2 + var3 + (float(calP7) * 128.0)) / 16.0
        else:
            val = 0

        return float(val)

    def _compensate_humidity(self, humADC):
        """Compensates the humidity.
        :param humADC: Analog humidity value.
        """
        temp_comp = fineTemperature / 5120.0
        var1 = humADC - ((calH1 * 16.0) + ((calH3 / 2.0) * temp_comp))
        var2 = var1 * (((calH2 / 262144.0) * (1.0 + ((calH4 / 16384.0) * temp_comp) + ((calH5 / 1048576.0) * temp_comp * temp_comp))))
        var3 = calH6 / 16384.0
        var4 = calH7 / 2097152.0
        val = var2 + ((var3 + (var4 * temp_comp)) * var2 * var2)

        if val > 100.0:
            val = 100.0
        elif val < 0.0:
            val = 0.0

        return float(val)

    #Sensor Readouts

    def _force_read(self, gas_measurement_enabled):
        """Triggers all measurements, and then waits for measurement completion.
        :param gas_measurement_enabled: Enable or disable gas measurement.
        """
        self._set_gas_measurement(gas_measurement_enabled)

        temp = self._read_register_1ubyte(self.BME680_CTRL_MEAS)
        temp |= OperationModes.ForcedMode
        self._write_register(self.BME680_CTRL_MEAS, temp)

        while(self._get_measuring_status()):
            time.sleep(0.001)

        if (gas_measurement_enabled):
            while (self._get_gas_measuring_status()):
                time.sleep(0.001)

    def read_temperature(self):
        """Reads the temperature in Celcius."""
        self._force_read(False)

        tempADC = (self._read_register_1ubyte(self.BME680_TEMP_MSB) << 12) | (self._read_register_1ubyte(self.BME680_TEMP_LSB) << 4) | (self._read_register_1ubyte(self.BME680_TEMP_XLSB) >> 4)

        return float(self._compensate_temperature(tempADC))

    def read_humidity(self):
        """Reads the relative humidity."""
        self._force_read(False)

        humADC = (self._read_register_1ubyte(self.BME680_HUM_MSB) << 8) | (self._read_register_1ubyte(self.BME680_HUM_LSB))

        return float(self._compensate_humidity(humADC))

    def read_pressure(self):
        """Reads the pressure in Pa."""
        self._force_read(False)

        presADC = (self._read_register_1ubyte(self.BME680_PRESS_MSB) << 12) | (self._read_register_1ubyte(self.BME680_PRESS_LSB) << 4) | (self._read_register_1ubyte(self.BME680_PRESS_XLSB) >> 4)

        return float(self._compensate_pressure(presADC))

    def read_altitude(self, meanSeaLevelPressureInBar):
        """Reads the altitude from the sea level in meters.
        :param meanSeaLevelPressureInBar: Mean sea level pressure in bar. Will be used for altitude calculation from the pressure.
        """
        phPa = self.read_pressure() / 100.0

        return float(44330.0 * (1.0 - math.pow((phPa / meanSeaLevelPressureInBar), 0.1903)))

    def read_gas_resistance(self):
        """Reads the gas resistance."""
        #Declare global variables
        global calAmbTemp

        self._force_read(True)

        tempADC = (self._read_register_1ubyte(self.BME680_TEMP_MSB) << 12) | (self._read_register_1ubyte(self.BME680_TEMP_LSB) << 4) | (self._read_register_1ubyte(self.BME680_TEMP_XLSB) >> 4)
        gasResADC = (self._read_register_1ubyte(self.BME680_GAS_R_MSB) << 2) | (self._read_register_1ubyte(self.BME680_GAS_R_LSB) >> 6)
        gasRange = self._read_register_1ubyte(self.BME680_GAS_R_LSB) & 0x0F

        calAmbTemp = self._compensate_temperature(tempADC)
        val = self._calculate_gas_resistance(gasResADC, gasRange)

        return float(val)

    def read_tph(self):
        """Reads temperature, pressure and relative humidity."""
        resultsTPH = [ 0.0, 0.0, 0.0 ]

        self._force_read(False)

        tempADC = (self._read_register_1ubyte(self.BME680_TEMP_MSB) << 12) | (self._read_register_1ubyte(self.BME680_TEMP_LSB) << 4) | (self._read_register_1ubyte(self.BME680_TEMP_XLSB) >> 4)
        presADC = (self._read_register_1ubyte(self.BME680_PRESS_MSB) << 12) | (self._read_register_1ubyte(self.BME680_PRESS_LSB) << 4) | (self._read_register_1ubyte(self.BME680_PRESS_XLSB) >> 4)
        humADC = (self._read_register_1ubyte(self.BME680_HUM_MSB) << 8) | (self._read_register_1ubyte(self.BME680_HUM_LSB))

        resultsTPH[0] = float(self._compensate_temperature(tempADC))
        resultsTPH[1] = float(self._compensate_pressure(presADC))
        resultsTPH[2] = float(self._compensate_humidity(humADC))

        return resultsTPH

    #Internal Methods

    def _set_gas_measurement(self, state):
        """Turns gas measurement on of off.
        :param state: Gas measurement mode. True for on, false for off.
        """
        configValue = self._read_register_1ubyte(self.BME680_CTRL_GAS_1)

        if state:
            configValue |= 0b00010000
        else:
            configValue &= 0b11101111

        self._write_register(self.BME680_CTRL_GAS_1, configValue)

    def _select_heater_profile_setpoint(self, heaterProfileSetPoint):
        """Selects heater set-points of the sensor that will be used in forced mode.
        :param heaterProfileSetPoint: Heater profile set-point.
        """
        self._write_register(self.BME680_CTRL_GAS_1, heaterProfileSetPoint)

    def _get_measuring_status(self):
        """Gets the measuring status from the measuring bit."""
        readValue = self._read_register_1ubyte(self.BME680_EAS_STATUS_0)

        return True if ((readValue & 0b00100000) == 0b00100000) else False

    def _get_gas_measuring_status(self):
        """Gets the gas measurement running status from the gas_measuring bit."""
        readValue = self._read_register_1ubyte(self.BME680_EAS_STATUS_0)

        return True if ((readValue & 0b01000000) == 0b01000000) else False

    def _get_new_data_status(self):
        """Gets the new data status from the new_data_0 bit."""
        readValue = self._read_register_1ubyte(self.BME680_EAS_STATUS_0)

        return True if ((readValue & 0b10000000) == 0b10000000) else False

    def _calculate_gas_resistance(self, gasResADC, gasRange):
        """Calculates the gas resistance value.
        :param gasResADC: ADC resistance value.
        :param gasRange: ADC range.
        """
        var1 = (1340.0 + 5.0 * calRangeSwErr) * self.const_array1[gasRange]
        gasres = var1 * self.const_array2[gasRange] / (gasResADC - 512.0 + var1)

        return gasres

    def _calculate_heater_resistance(self, target_temp):
        """Calculates the heater resistance value for target heater resistance (Res_Heat_X) registers.
        :param targetTemp: Target temperature.
        """
        if target_temp > 400: #Maximum temperature
            target_temp = 400

        var1 = (calGH1 / 16.0) + 49.0
        var2 = ((calGH2 / 32768.0) * 0.0005) + 0.00235
        var3 = calGH3 / 1024.0
        var4 = var1 * (1.0 + (var2 * target_temp))
        var5 = var4 + (var3 * self.calAmbTemp)
        res_heat = 3.4 * ((var5 * (4 / (4 + calResHeatRange)) * (1 / (1 + (calResHeatVal * 0.002)))) - 25)

        return int(res_heat)

    def _calculate_heat_duration(self, dur):
        """Calculates the heat duration value for gas sensor wait time (Gas_Wait_X) registers.
        :param dur: Wait duration in ms. Max value is 252.
        """
        factor = 0

        if dur >= 0xFC:
            durval = 0xFF #Max duration
        else:
            while dur > 0x3F:
                dur = dur / 4
                factor += 1
            durval = dur + (factor * 64)

        return durval

    #Disposal
    def __del__(self):
        """Releases the resources."""
        #Reset sensor to turn off its functionality.
        self.reset_sensor()

        global calT1
        global calT2
        global calT3
        global calP1
        global calP2
        global calP3
        global calP4
        global calP5
        global calP6
        global calP7
        global calP8
        global calP9
        global calP10
        global calH1
        global calH2
        global calH3
        global calH4
        global calH5
        global calH6
        global calH7
        global calGH1
        global calGH2
        global calGH3
        global fineTemperature
        global calAmbTemp
        global calResHeatRange
        global calResHeatVal
        global calRangeSwErr

        del calT1
        del calT2
        del calT3
        del calP1
        del calP2
        del calP3
        del calP4
        del calP5
        del calP6
        del calP7
        del calP8
        del calP9
        del calP10
        del calH1
        del calH2
        del calH3
        del calH4
        del calH5
        del calH6
        del calH7
        del calGH1
        del calGH2
        del calGH3
        del fineTemperature
        del calAmbTemp
        del calResHeatRange
        del calResHeatVal
        del calRangeSwErr
