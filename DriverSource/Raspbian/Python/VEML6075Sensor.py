# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Vishay VEML6075 UV Light Sensor Python Driver
# Version 1.00 (Initial Release)
# Updated: July 6th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions, visit www.turta.io/forum or e-mail turta@turta.io

import time
from enum import IntEnum
from smbus import SMBus

#Enumerations

class IntegrationTime(IntEnum):
    IT_050ms = 0b00000000
    IT_100ms = 0b00010000
    IT_200ms = 0b00100000
    IT_400ms = 0b00110000
    IT_800ms = 0b01000000

class DynamicSetting(IntEnum):
    Normal = 0b00000000
    High = 0b00001000

class Trigger(IntEnum):
    NoActiveForceTrigger = 0b00000000
    TriggerOneMeasurement = 0b00000100

class ActiveForceMode(IntEnum):
    NormalMode = 0b00000000
    ActiveForceMode = 0b00000010

class PowerMode(IntEnum):
    PowerOn = 0b00000000
    ShutDown = 0b00000001

#I2C Slave Address
I2C_ADDRESS = 0x10

#Registers
VEML6075_UV_CONF = 0x00
VEML6075_UVA_DATA = 0x07
VEML6075_DUMMY = 0x08
VEML6075_UVB_DATA = 0x09
VEML6075_UVCOMP1_DATA = 0x0A
VEML6075_UVCOMP2_DATA = 0x0B
VEML6075_ID = 0x0C

#Default Values
uva_a_coef = 2.22 #UVA VIS Coefficient
uva_b_coef = 1.33 #VA IR Coefficient
uvb_c_coef = 2.95 #UVB VIS Coefficient
uvb_d_coef = 1.74 #UVB IR Coefficient
uva_resp = 0.001461 #UVA Responsivity
uvb_resp = 0.002591 #UVB Responsivity

#Correction Factors
k1 = 0
k2 = 0

#I2C Config
bus = SMBus(1)

#I2C Communication

def WriteRegisterTwoBytesArray(regAddr, data):
    """Writes data to the I2C device.
    :param regAddr: Register address.
    :param data: Data.
    """
    bus.write_i2c_block_data(I2C_ADDRESS, regAddr, data)

def Read2BytesAsUShortLSBFirst(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 2)
    return buffer[0] + (buffer[1] << 8)

def Read2BytesArray(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    return bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 2)

def Init():
    """Initiates the VEML6075 sensor to get UVA, UVB and UVIndex."""
    Config(
        IntegrationTime.IT_800ms,
        DynamicSetting.High,
        Trigger.NoActiveForceTrigger,
        ActiveForceMode.NormalMode,
        PowerMode.PowerOn)

#Sensor Configuration

def Config(integrationTime, dynamicSetting, trigger, activeForceMode, powerMode):
    """Configures the VEML6075 sensor.
    :param integrationTime: UV integration time.
    :param dynamicSetting: Dynamic setting.
    :param trigger: Measurement trigger.
    :param activeForceMode: Active force mode.
    :param powerMode: Power mode."""

    configCommand = 0x00

    configCommand = integrationTime | dynamicSetting | trigger | activeForceMode | powerMode

    WriteRegisterTwoBytesArray(VEML6075_UV_CONF, [configCommand, 0x00])

#Sensor Readouts

def TriggerOneMeasurement():
    """Triggers one time measurement for Active Force Mode enabled scenarios."""
    tempConfig = Read2BytesArray(VEML6075_UV_CONF)

    tempConfig[0] |= 0b00000100

    WriteRegisterTwoBytesArray(VEML6075_UV_CONF, tempConfig)

def Read_RAW_UVA():
    """Reads RAW UVA."""
    return float(Read2BytesAsUShortLSBFirst(VEML6075_UVA_DATA))

def Read_RAW_UVB():
    """Reads RAW UVB."""
    return float(Read2BytesAsUShortLSBFirst(VEML6075_UVB_DATA))

def Read_RAW_UVD():
    """Reads RAW UVD."""
    return float(Read2BytesAsUShortLSBFirst(VEML6075_DUMMY))

def Read_RAW_UVCOMP1():
    """Reads Noise Compensation Channel 1 data which allows only visible noise to pass through."""
    return float(Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA))

def Read_RAW_UVCOMP2():
    """Reads Noise Compensation Channel 2 data which allows only infrared noise to pass through."""
    return float(Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA))

def Calculate_Compensated_UVA():
    """Calculates Compensated UVA."""
    #Formula:
    #UVAcalc = UVA - a x UVcomp1 - b x UVcomp2

    uva = Read2BytesAsUShortLSBFirst(VEML6075_UVA_DATA)
    uvcomp1 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA)
    uvcomp2 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA)

    uVAcalc = uva - uva_a_coef * uvcomp1 - uva_b_coef * uvcomp2

    return float(uVAcalc)

def Calculate_Compensated_UVB():
    """Calculates Compensated UVB."""
    #Formula:
    #UVBcalc = UVB - c x UVcomp1 - d x UVcomp2

    uvb = Read2BytesAsUShortLSBFirst(VEML6075_UVB_DATA)
    uvcomp1 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA)
    uvcomp2 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA)

    uVBcalc = uvb - uvb_c_coef * uvcomp1 - uvb_d_coef * uvcomp2

    return float(uVBcalc)

def Calculate_UV_Index_A():
    """Calculates the UV Index A."""
    #Formula:
    #UVIA = UVAcalc x k1 x UVAresponsivity

    uva = Read2BytesAsUShortLSBFirst(VEML6075_UVA_DATA)
    uvcomp1 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA)
    uvcomp2 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA)

    uVAcalc = uva - uva_a_coef * uvcomp1 - uva_b_coef * uvcomp2

    uVIA = uVAcalc * k1 * uva_resp

    return float(uVIA)

def Calculate_UV_Index_B():
    """Calculates the UV Index B."""
    #Formula:
    #UVIB = UVBcalc x k2 x UVBresponsivity

    uvb = Read2BytesAsUShortLSBFirst(VEML6075_UVB_DATA)
    uvcomp1 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA)
    uvcomp2 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA)

    uVBcalc = uvb - uvb_c_coef * uvcomp1 - uvb_d_coef * uvcomp2

    uVIB = uVBcalc * k2 * uvb_resp

    return float(uVIB)

def Calculate_Average_UV_Index():
    """Calculates the Average UV Index."""
    #Formula:
    #UVAcomp = (UVA - UVD) - a * (UVcomp1 - UVD) - b * (UVcomp2 - UVD)
    #UVBcomp = (UVB - UVD) - c * (UVcomp1 - UVD) - d * (UVcomp2 - UVD)
    #UVI = ((UVBcomp * UVBresp) + (UVAcomp * UVAresp)) / 2

    uva = Read2BytesAsUShortLSBFirst(VEML6075_UVA_DATA)
    uvb = Read2BytesAsUShortLSBFirst(VEML6075_UVB_DATA)
    uvd = Read2BytesAsUShortLSBFirst(VEML6075_DUMMY)
    uvcomp1 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP1_DATA)
    uvcomp2 = Read2BytesAsUShortLSBFirst(VEML6075_UVCOMP2_DATA)

    uVAcomp = (uva - uvd) - uva_a_coef * (uvcomp1 - uvd) - uva_b_coef * (uvcomp2 - uvd)
    uVBcomp = (uvb - uvd) - uvb_c_coef * (uvcomp1 - uvd) - uvb_d_coef * (uvcomp2 - uvd)
    uVI = ((uVBcomp * uvb_resp) + (uVAcomp * uva_resp)) / 2

    if uVI < 0:
        uVI = 0

    return float(uVI)

def Dispose():
    """Releases The Resources."""
    global bus
    del bus
