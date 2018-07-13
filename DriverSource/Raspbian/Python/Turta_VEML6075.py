# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for Vishay VEML6075 UV Light Sensor
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

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

class VEML6075Sensor:
    """VEML6075 Sensor"""

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

    def _write_register_2bytes_array(self, reg_addr, data):
        """Writes data to the I2C device.
        :param reg_addr: Register address.
        :param data: Data.
        """
        self.bus.write_i2c_block_data(self.I2C_ADDRESS, reg_addr, data)

    def _read_2bytes_as_ushort_lsbfirst(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        return buffer[0] + (buffer[1] << 8)

    def _read_2bytes_array(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        return self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)

    def __init__(self):
        """Initiates the VEML6075 sensor to get UVA, UVB and UVIndex."""
        self.config(
            IntegrationTime.IT_800ms,
            DynamicSetting.High,
            Trigger.NoActiveForceTrigger,
            ActiveForceMode.NormalMode,
            PowerMode.PowerOn)

    #Sensor Configuration

    def config(self, integration_time, dynamic_setting, trigger, active_force_mode, power_mode):
        """Configures the VEML6075 sensor.
        :param integration_time: UV integration time.
        :param dynamic_setting: Dynamic setting.
        :param trigger: Measurement trigger.
        :param active_force_mode: Active force mode.
        :param power_mode: Power mode."""

        config_command = 0x00

        config_command = integration_time | dynamic_setting | trigger | active_force_mode | power_mode

        self._write_register_2bytes_array(self.VEML6075_UV_CONF, [config_command, 0x00])

    #Sensor Readouts

    def _trigger_one_measurement(self):
        """Triggers one time measurement for Active Force Mode enabled scenarios."""
        tempConfig = self._read_2bytes_array(self.VEML6075_UV_CONF)

        tempConfig[0] |= 0b00000100

        self._write_register_2bytes_array(self.VEML6075_UV_CONF, tempConfig)

    def _read_raw_uva(self):
        """Reads RAW UVA."""
        return float(self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVA_DATA))

    def _read_raw_uvb(self):
        """Reads RAW UVB."""
        return float(self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVB_DATA))

    def _read_raw_uvd(self):
        """Reads RAW UVD."""
        return float(self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_DUMMY))

    def _read_raw_uvcomp1(self):
        """Reads Noise Compensation Channel 1 data which allows only visible noise to pass through."""
        return float(self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA))

    def _read_raw_uvcomp2(self):
        """Reads Noise Compensation Channel 2 data which allows only infrared noise to pass through."""
        return float(self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA))

    def calculate_compensated_uva(self):
        """Calculates Compensated UVA."""
        #Formula:
        #UVAcalc = UVA - a x UVcomp1 - b x UVcomp2

        uva = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVA_DATA)
        uvcomp1 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA)
        uvcomp2 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA)

        uVAcalc = uva - self.uva_a_coef * uvcomp1 - self.uva_b_coef * uvcomp2

        return float(uVAcalc)

    def calculate_compensated_uvb(self):
        """Calculates Compensated UVB."""
        #Formula:
        #UVBcalc = UVB - c x UVcomp1 - d x UVcomp2

        uvb = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVB_DATA)
        uvcomp1 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA)
        uvcomp2 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA)

        uVBcalc = uvb - self.uvb_c_coef * uvcomp1 - self.uvb_d_coef * uvcomp2

        return float(uVBcalc)

    def calculate_uv_index_a(self):
        """Calculates the UV Index A."""
        #Formula:
        #UVIA = UVAcalc x k1 x UVAresponsivity

        uva = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVA_DATA)
        uvcomp1 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA)
        uvcomp2 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA)

        uVAcalc = uva - self.uva_a_coef * uvcomp1 - self.uva_b_coef * uvcomp2

        uVIA = uVAcalc * self.k1 * self.uva_resp

        return float(uVIA)

    def calculate_uv_index_b(self):
        """Calculates the UV Index B."""
        #Formula:
        #UVIB = UVBcalc x k2 x UVBresponsivity

        uvb = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVB_DATA)
        uvcomp1 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA)
        uvcomp2 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA)

        uVBcalc = uvb - self.uvb_c_coef * uvcomp1 - self.uvb_d_coef * uvcomp2

        uVIB = uVBcalc * self.k2 * self.uvb_resp

        return float(uVIB)

    def calculate_average_uv_index(self):
        """Calculates the Average UV Index."""
        #Formula:
        #UVAcomp = (UVA - UVD) - a * (UVcomp1 - UVD) - b * (UVcomp2 - UVD)
        #UVBcomp = (UVB - UVD) - c * (UVcomp1 - UVD) - d * (UVcomp2 - UVD)
        #UVI = ((UVBcomp * UVBresp) + (UVAcomp * UVAresp)) / 2

        uva = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVA_DATA)
        uvb = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVB_DATA)
        uvd = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_DUMMY)
        uvcomp1 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP1_DATA)
        uvcomp2 = self._read_2bytes_as_ushort_lsbfirst(self.VEML6075_UVCOMP2_DATA)

        uVAcomp = (uva - uvd) - self.uva_a_coef * (uvcomp1 - uvd) - self.uva_b_coef * (uvcomp2 - uvd)
        uVBcomp = (uvb - uvd) - self.uvb_c_coef * (uvcomp1 - uvd) - self.uvb_d_coef * (uvcomp2 - uvd)
        uVI = ((uVBcomp * self.uvb_resp) + (uVAcomp * self.uva_resp)) / 2

        if uVI < 0:
            uVI = 0

        return float(uVI)

    #Disposal
    def __del__(self):
        """Releases the resources."""
        #Turn off the sensor.
        self.config(
            IntegrationTime.IT_050ms,
            DynamicSetting.Normal,
            Trigger.NoActiveForceTrigger,
            ActiveForceMode.ActiveForceMode,
            PowerMode.ShutDown)
