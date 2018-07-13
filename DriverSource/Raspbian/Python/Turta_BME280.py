# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for Bosch Sensortec BME280 Environmental Sensor
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

import time
import math
from enum import IntEnum
from smbus import SMBus

#Enumerations

class HumidityOversampling(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00000001
    x02 = 0b00000010
    x04 = 0b00000011
    x08 = 0b00000100
    x16 = 0b00000101

class TemperatureOversampling(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00100000
    x02 = 0b01000000
    x04 = 0b01100000
    x08 = 0b10000000
    x16 = 0b10100000

class PressureOversampling(IntEnum):
    Skipped = 0b00000000
    x01 = 0b00000100
    x02 = 0b00001000
    x04 = 0b00001100
    x08 = 0b00010000
    x16 = 0b00010100

class SensorMode(IntEnum):
    Sleep = 0b00000000
    Forced = 0b00000001
    Normal = 0b00000011

class InactiveDuration(IntEnum):
    ms0000_5 = 0b00000000
    ms0062_5 = 0b00100000
    ms0125 = 0b01000000
    ms0250 = 0b01100000
    ms0500 = 0b10000000
    ms1000 = 0b10100000
    ms0010 = 0b11000000
    ms0020 = 0b11100000

class FilterCoefficient(IntEnum):
    FilterOff = 0b00000000
    fc02 = 0b00000100
    fc04 = 0b00001000
    fc08 = 0b00001100
    fc16 = 0b00010000

class BME280Sensor:
    """BME280 Sensor"""

    #I2C Slave Address
    I2C_ADDRESS = 0x77

    #Registers
    BME280_SIGNATURE = 0x60
    BME280_ID = 0xD0
    BME280_RESET = 0xE0
    BME280_CTRL_HUM = 0xF2
    BME280_STATUS = 0xF3
    BME280_CTRL_MEAS = 0xF4
    BME280_CONFIG = 0xF5

    #Registers: Readout
    BME280_PRESS_MSB = 0xF7
    BME280_PRESS_LSB = 0xF8
    BME280_PRESS_XLSB = 0xF9
    BME280_TEMP_MSB = 0xFA
    BME280_TEMP_LSB = 0xFB
    BME280_TEMP_XLSB = 0xFC
    BME280_HUM_MSB = 0xFD
    BME280_HUM_LSB = 0xFE

    #Registers: Calibration
    BME280_DIG_T1 = 0x88
    BME280_DIG_T2 = 0x8A
    BME280_DIG_T3 = 0x8C
    BME280_DIG_P1 = 0x8E
    BME280_DIG_P2 = 0x90
    BME280_DIG_P3 = 0x92
    BME280_DIG_P4 = 0x94
    BME280_DIG_P5 = 0x96
    BME280_DIG_P6 = 0x98
    BME280_DIG_P7 = 0x9A
    BME280_DIG_P8 = 0x9C
    BME280_DIG_P9 = 0x9E
    BME280_DIG_H1 = 0xA1
    BME280_DIG_H2 = 0xE1
    BME280_DIG_H3 = 0xE3
    BME280_DIG_H4 = 0xE4
    BME280_DIG_H5 = 0xE5
    BME280_DIG_H6 = 0xE7

    #Data: Calibration
    calDig_T1 = None
    calDig_T2 = None
    calDig_T3 = None
    calDig_P1 = None
    calDig_P2 = None
    calDig_P3 = None
    calDig_P4 = None
    calDig_P5 = None
    calDig_P6 = None
    calDig_P7 = None
    calDig_P8 = None
    calDig_P9 = None
    calDig_H1 = None
    calDig_H2 = None
    calDig_H3 = None
    calDig_H4 = None
    calDig_H5 = None
    calDig_H6 = None
    fineTemperature = 0

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

    def _read_multiple_bytes_as_array(self, reg_addr, lenght):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        :param lenght: Data lenght
        """
        return self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, lenght)

    def __init__(self):
        """Initiates the BME680 sensor to get air quality level, temperature, humidity, pressure and altitude."""
        self._read_calibration_data()
        self.set_oversamplings_and_mode(
            HumidityOversampling.x08,
            TemperatureOversampling.x08,
            PressureOversampling.x16,
            SensorMode.Normal)
        self.set_config(
            InactiveDuration.ms1000,
            FilterCoefficient.fc04)

    #Sensor Configuration

    def set_oversamplings_and_mode(self, humidity_oversampling, temperature_oversampling, pressure_oversamling, mode):
        """Sets the oversamplings and sensor mode.
        :param humidity_oversampling: Humidity oversampling.
        :param temperature_oversampling: Temperature Oversampling.
        :param pressure_oversamling: Pressure oversampling.
        :param mode: Sensor mode."""
        self._write_register(self.BME280_CTRL_HUM, humidity_oversampling)

        time.sleep(0.001)

        self._write_register(self.BME280_CTRL_MEAS, temperature_oversampling | pressure_oversamling | mode)

    def set_config(self, inactive_duration, filter_coefficient):
        """Sets the sensor configuration.
        :param inactive_duration: Inactive duration between normal mode measurements.
        :param filter_coefficient: Filter coefficient."""
        self._write_register(self.BME280_CONFIG, inactive_duration | filter_coefficient)

    #Calibration and Compensation

    def _read_calibration_data(self):
        """Reads the factory out calibration data from the sensor."""
        #Declare global variables.
        global calDig_T1
        global calDig_T2
        global calDig_T3
        global calDig_P1
        global calDig_P2
        global calDig_P3
        global calDig_P4
        global calDig_P5
        global calDig_P6
        global calDig_P7
        global calDig_P8
        global calDig_P9
        global calDig_H1
        global calDig_H2
        global calDig_H3
        global calDig_H4
        global calDig_H5
        global calDig_H6

        #Temperature calibration
        calDig_T1 = self._read_2bytes_as_ushort_lsbfirst(self.BME280_DIG_T1)
        calDig_T2 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_T2)
        calDig_T3 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_T3)

        #Pressure calibration
        calDig_P1 = self._read_2bytes_as_ushort_lsbfirst(self.BME280_DIG_P1)
        calDig_P2 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P2)
        calDig_P3 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P3)
        calDig_P4 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P4)
        calDig_P5 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P5)
        calDig_P6 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P6)
        calDig_P7 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P7)
        calDig_P8 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P8)
        calDig_P9 = self._read_2bytes_as_short_lsbfirst(self.BME280_DIG_P9)

        #Humidity calibration
        calDig_H1 = self._read_register_1sbyte(self.BME280_DIG_H1)
        calDig_H2 = self._read_2bytes_as_ushort_lsbfirst(self.BME280_DIG_H2)
        calDig_H3 = self._read_register_1sbyte(self.BME280_DIG_H3)
        calDig_H4 = (self._read_register_1sbyte(self.BME280_DIG_H4) << 4) | (self._read_register_1sbyte(self.BME280_DIG_H4 + 1) & 0xF)
        calDig_H5 = self._read_register_1sbyte((self.BME280_DIG_H5 + 1) << 4) | (self._read_register_1sbyte(self.BME280_DIG_H5) >> 4)
        calDig_H6 = self._read_register_1sbyte(self.BME280_DIG_H6)

    def _compensate_temperature(self, temp_adc):
        """Compensates the temperature.
        :param temp_adc: Analog temperature value.
        """
        #Declare global variable.
        global fineTemperature

        var1 = (temp_adc / 16384.0 - calDig_T1 / 1024.0) * calDig_T2
        var2 = ((temp_adc / 131072.0 - calDig_T1 / 8192.0) * (temp_adc / 131072.0 - calDig_T1 / 8192.0)) * calDig_T3
        fineTemperature = (var1 + var2)
        val = fineTemperature / 5120.0

        return float(val)

    def _compensate_pressure(self, pres_adc):
        """Compensates the pressure.
        :param pres_adc: Analog pressure value.
        """
        var1 = (fineTemperature / 2.0) - 64000.0
        var2 = var1 * var1 * calDig_P6 / 32768.0
        var2 = var2 + var1 * calDig_P5 * 2.0
        var2 = (var2 / 4.0) + (calDig_P4 * 65536.0)
        var1 = (calDig_P3 * var1 * var1 / 524288.0 + calDig_P2 * var1) / 524288.0
        var1 = (1.0 + var1 / 32768.0) * calDig_P1

        if var1 == 0.0:
            return 0

        val = 1048576.0 - pres_adc
        val = (val - (var2 / 4096.0)) * 6250.0 / var1
        var1 = calDig_P9 * val * val / 2147483648.0
        var2 = val * calDig_P8 / 32768.0
        val = val + (var1 + var2 + calDig_P7) / 16.0

        return float(val)

    def _compensate_humidity(self, hum_adc):
        """Compensates the humidity.
        :param hum_adc: Analog humidity value.
        """
        val = fineTemperature - 76800.0
        val = (hum_adc - (calDig_H4 * 64.0 + calDig_H5 / 16384.0 * val)) * (calDig_H2 / 65536.0 * (1.0 + calDig_H6 / 67108864.0 * val * (1.0 + calDig_H3 / 67108864.0 * val)))
        val = val * (1.0 - calDig_H1 * val / 524288.0)

        if val > 100.0:
            val = 100.0
        elif val < 0.0:
            val = 0.0

        return val

    #Sensor Readouts

    def read_temperature(self):
        """Reads the temperature in Celcius."""
        tRaw = self._read_multiple_bytes_as_array(self.BME280_TEMP_MSB, 3)

        return float(self._compensate_temperature((tRaw[0] << 12) + (tRaw[1] << 4) + (tRaw[2] >> 4)))

    def read_humidity(self):
        """Reads the relative humidity."""
        hRaw = self._read_multiple_bytes_as_array(self.BME280_HUM_MSB, 2)

        return float(self._compensate_humidity((hRaw[0] << 8) + hRaw[1]))

    def read_pressure(self):
        """Reads the pressure in Pa."""
        pRaw = self._read_multiple_bytes_as_array(self.BME280_PRESS_MSB, 3)

        return float(self._compensate_pressure((pRaw[0] << 12) + (pRaw[1] << 4) + (pRaw[2] >> 4)))

    def read_altitude(self, meanSeaLevelPressureInBar):
        """Reads the altitude from the sea level in meters.
        :param meanSeaLevelPressureInBar: Mean sea level pressure in bar. Will be used for altitude calculation from the pressure.
        """
        phPa = self.read_pressure() / 100.0

        return float(44330.0 * (1.0 - math.pow((phPa / meanSeaLevelPressureInBar), 0.1903)))

    def read_tph(self):
        """Reads temperature, pressure and relative humidity."""
        resultsTPH = [ 0.0, 0.0, 0.0 ]

        tRaw = self._read_multiple_bytes_as_array(self.BME280_TEMP_MSB, 3)
        pRaw = self._read_multiple_bytes_as_array(self.BME280_PRESS_MSB, 3)
        hRaw = self._read_multiple_bytes_as_array(self.BME280_HUM_MSB, 2)

        resultsTPH[0] = float(self._compensate_temperature((tRaw[0] << 12) + (tRaw[1] << 4) + (tRaw[2] >> 4)))
        resultsTPH[1] = float(self._compensate_pressure((pRaw[0] << 12) + (pRaw[1] << 4) + (pRaw[2] >> 4)))
        resultsTPH[2] = float(self._compensate_humidity(hRaw[0] << 8) + hRaw[1])

        return resultsTPH

    #Disposal
    def __del__(self):
        """Releases the resources."""
        #Turn off the sensor.
        self.set_oversamplings_and_mode(
            HumidityOversampling.Skipped,
            TemperatureOversampling.Skipped,
            PressureOversampling.Skipped,
            SensorMode.Sleep)

        global calDig_T1
        global calDig_T2
        global calDig_T3
        global calDig_P1
        global calDig_P2
        global calDig_P3
        global calDig_P4
        global calDig_P5
        global calDig_P6
        global calDig_P7
        global calDig_P8
        global calDig_P9
        global calDig_H1
        global calDig_H2
        global calDig_H3
        global calDig_H4
        global calDig_H5
        global calDig_H6
        global fineTemperature

        del calDig_T1
        del calDig_T2
        del calDig_T3
        del calDig_P1
        del calDig_P2
        del calDig_P3
        del calDig_P4
        del calDig_P5
        del calDig_P6
        del calDig_P7
        del calDig_P8
        del calDig_P9
        del calDig_H1
        del calDig_H2
        del calDig_H3
        del calDig_H4
        del calDig_H5
        del calDig_H6
        del fineTemperature
