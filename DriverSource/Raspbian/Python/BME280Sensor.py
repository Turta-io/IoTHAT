# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Bosch Sensortec BME280 Environmental Sensor Python Driver
# Version 1.00 (Initial Release)
# Updated: July 8th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions, visit www.turta.io/forum or e-mail turta@turta.io

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

def WriteRegister(regAddr, data):
    """Writes data to the I2C device.
    :param regAddr: Register address.
    :param data: Data.
    """
    bus.write_i2c_block_data(I2C_ADDRESS, regAddr, [ data & 0xFF ])

def ReadRegisterOneUByte(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 1)
    return buffer[0]

def ReadRegisterOneSByte(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 1)
    val = buffer[0]
    if val & (1 << 7) != 0:
        val = val - (1 << 7)
    return val

def Read2BytesAsUShortLSBFirst(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 2)
    return buffer[0] + (buffer[1] << 8)

def Read2BytesAsShortLSBFirst(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 2)
    val = buffer[0] + (buffer[1] << 8)
    if val & (1 << 15) != 0:
        val = val - (1 << 15)
    return val

def ReadMultipleBytesAsArray(regAddr, lenght):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    :param lenght: Data lenght
    """
    return bus.read_i2c_block_data(I2C_ADDRESS, regAddr, lenght)

def Init():
    """Initiates the BME680 sensor to get air quality level, temperature, humidity, pressure and altitude."""
    ReadCalibrationData()
    SetOversamplingsAndMode(
        HumidityOversampling.x08,
        TemperatureOversampling.x08,
        PressureOversampling.x16,
        SensorMode.Normal)
    SetConfig(
        InactiveDuration.ms1000,
        FilterCoefficient.fc04)

#Sensor Configuration

def SetOversamplingsAndMode(humidityOversampling, temperatureOversampling, pressureOversamling, mode):
    """Sets the oversamplings and sensor mode.
    :param humidityOversampling: Humidity oversampling.
    :param temperatureOversampling: Temperature Oversampling.
    :param pressureOversampling: Pressure oversampling.
    :param mode: Sensor mode."""
    WriteRegister(BME280_CTRL_HUM, humidityOversampling)

    time.sleep(0.001)

    WriteRegister(BME280_CTRL_MEAS, temperatureOversampling | pressureOversamling | mode)

def SetConfig(inactiveDuration, filterCoefficient):
    """Sets the sensor configuration.
    :param inactiveDuration: Inactive duration between normal mode measurements.
    :param filterCoefficient: Filter coefficient."""
    WriteRegister(BME280_CONFIG, inactiveDuration | filterCoefficient)

#Calibration and Compensation

def ReadCalibrationData():
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
    calDig_T1 = Read2BytesAsUShortLSBFirst(BME280_DIG_T1)
    calDig_T2 = Read2BytesAsShortLSBFirst(BME280_DIG_T2)
    calDig_T3 = Read2BytesAsShortLSBFirst(BME280_DIG_T3)

    #Pressure calibration
    calDig_P1 = Read2BytesAsUShortLSBFirst(BME280_DIG_P1)
    calDig_P2 = Read2BytesAsShortLSBFirst(BME280_DIG_P2)
    calDig_P3 = Read2BytesAsShortLSBFirst(BME280_DIG_P3)
    calDig_P4 = Read2BytesAsShortLSBFirst(BME280_DIG_P4)
    calDig_P5 = Read2BytesAsShortLSBFirst(BME280_DIG_P5)
    calDig_P6 = Read2BytesAsShortLSBFirst(BME280_DIG_P6)
    calDig_P7 = Read2BytesAsShortLSBFirst(BME280_DIG_P7)
    calDig_P8 = Read2BytesAsShortLSBFirst(BME280_DIG_P8)
    calDig_P9 = Read2BytesAsShortLSBFirst(BME280_DIG_P9)

    #Humidity calibration
    calDig_H1 = ReadRegisterOneSByte(BME280_DIG_H1)
    calDig_H2 = Read2BytesAsUShortLSBFirst(BME280_DIG_H2)
    calDig_H3 = ReadRegisterOneSByte(BME280_DIG_H3)
    calDig_H4 = (ReadRegisterOneSByte(BME280_DIG_H4) << 4) | (ReadRegisterOneSByte(BME280_DIG_H4 + 1) & 0xF)
    calDig_H5 = ReadRegisterOneSByte((BME280_DIG_H5 + 1) << 4) | (ReadRegisterOneSByte(BME280_DIG_H5) >> 4)
    calDig_H6 = ReadRegisterOneSByte(BME280_DIG_H6)

def CompensateTemperature(tempADC):
    """Compensates the temperature.
    :param tempADC: Analog temperature value.
    """
    #Declare global variable.
    global fineTemperature

    var1 = (tempADC / 16384.0 - calDig_T1 / 1024.0) * calDig_T2
    var2 = ((tempADC / 131072.0 - calDig_T1 / 8192.0) * (tempADC / 131072.0 - calDig_T1 / 8192.0)) * calDig_T3
    fineTemperature = (var1 + var2)
    val = fineTemperature / 5120.0

    return float(val)

def CompensatePressure(presADC):
    """Compensates the pressure.
    :param presADC: Analog pressure value.
    """
    var1 = (fineTemperature / 2.0) - 64000.0
    var2 = var1 * var1 * calDig_P6 / 32768.0
    var2 = var2 + var1 * calDig_P5 * 2.0
    var2 = (var2 / 4.0) + (calDig_P4 * 65536.0)
    var1 = (calDig_P3 * var1 * var1 / 524288.0 + calDig_P2 * var1) / 524288.0
    var1 = (1.0 + var1 / 32768.0) * calDig_P1

    if var1 == 0.0:
        return 0

    val = 1048576.0 - presADC
    val = (val - (var2 / 4096.0)) * 6250.0 / var1
    var1 = calDig_P9 * val * val / 2147483648.0
    var2 = val * calDig_P8 / 32768.0
    val = val + (var1 + var2 + calDig_P7) / 16.0

    return float(val)

def CompensateHumidity(humADC):
    """Compensates the humidity.
    :param humADC: Analog humidity value.
    """
    val = fineTemperature - 76800.0
    val = (humADC - (calDig_H4 * 64.0 + calDig_H5 / 16384.0 * val)) * (calDig_H2 / 65536.0 * (1.0 + calDig_H6 / 67108864.0 * val * (1.0 + calDig_H3 / 67108864.0 * val)))
    val = val * (1.0 - calDig_H1 * val / 524288.0)

    if val > 100.0:
        val = 100.0
    elif val < 0.0:
        val = 0.0

    return val

#Sensor Readouts

def ReadTemperature():
    """Reads the temperature in Celcius."""
    tRaw = ReadMultipleBytesAsArray(BME280_TEMP_MSB, 3)

    return float(CompensateTemperature((tRaw[0] << 12) + (tRaw[1] << 4) + (tRaw[2] >> 4)))

def ReadHumidity():
    """Reads the relative humidity."""
    hRaw = ReadMultipleBytesAsArray(BME280_HUM_MSB, 2)

    return float(CompensateHumidity((hRaw[0] << 8) + hRaw[1]))

def ReadPressure():
    """Reads the pressure in Pa."""
    pRaw = ReadMultipleBytesAsArray(BME280_PRESS_MSB, 3)

    return float(CompensatePressure((pRaw[0] << 12) + (pRaw[1] << 4) + (pRaw[2] >> 4)))

def ReadAltitude(meanSeaLevelPressureInBar):
    """Reads the altitude from the sea level in meters.
    :param meanSeaLevelPressureInBar: Mean sea level pressure in bar. Will be used for altitude calculation from the pressure.
    """
    phPa = ReadPressure() / 100.0

    return float(44330.0 * (1.0 - math.pow((phPa / meanSeaLevelPressureInBar), 0.1903)))

def ReadTPH():
    """Reads temperature, pressure and relative humidity."""
    resultsTPH = [ 0.0, 0.0, 0.0 ]

    tRaw = ReadMultipleBytesAsArray(BME280_TEMP_MSB, 3)
    pRaw = ReadMultipleBytesAsArray(BME280_PRESS_MSB, 3)
    hRaw = ReadMultipleBytesAsArray(BME280_HUM_MSB, 2)

    resultsTPH[0] = float(CompensateTemperature((tRaw[0] << 12) + (tRaw[1] << 4) + (tRaw[2] >> 4)))
    resultsTPH[1] = float(CompensatePressure((pRaw[0] << 12) + (pRaw[1] << 4) + (pRaw[2] >> 4)))
    resultsTPH[2] = float(CompensateHumidity(CompensateHumidity((hRaw[0] << 8) + hRaw[1])))

    return resultsTPH

#Internal Methods

def Dispose():
    """Releases The Resources."""
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
    global bus

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
    del bus
