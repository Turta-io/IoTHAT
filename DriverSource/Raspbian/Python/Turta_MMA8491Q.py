# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for NXP MMA8491Q 3-Axis Accelerometer & Tilt Sensor
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

import time
import RPi.GPIO as GPIO
from smbus import SMBus

class MMA8491QSensor:
    "MMA8491Q Sensor"

    #Pins
    mma8491qEn, mma8491qInt = 5, 17

    #I2C Slave Address
    I2C_ADDRESS = 0x55

    #Registers
    MMA8491Q_STATUS = 0x00
    MMA8491Q_OUT_X_MSB = 0x01
    MMA8491Q_OUT_Y_MSB = 0x03
    MMA8491Q_OUT_Z_MSB = 0x05

    #I2C Config
    bus = SMBus(1)

    #I2C Communication

    def _read_register_1ubyte(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 1)
        return buffer[0]

    def _read_2bytes_as_ushort_rs2b(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        return (buffer[0] << 6) + (buffer[1] >> 2)

    def _read_6bytes_array(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        return self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 6)

    def __init__(self):
        """Initiates the APDS-9960 sensor to get ambient light, RGB light and proximity."""
        GPIO.setwarnings(False)
        GPIO.setmode(GPIO.BCM)
        GPIO.setup(self.mma8491qEn, GPIO.OUT)
        GPIO.setup(self.mma8491qInt, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)

    #Sensor Readouts

    def _convert_to_g(self, analog_data):
        """Converts raw sensor data to G value.
        :param analog_data: Raw sensor output."""
        if ((analog_data & 0x2000) == 0x2000): #Zero or negative G
            return (0x3FFF - analog_data) / -1024.0
        else: #Positive G
            return analog_data / 1024.0

    def read_x_axis(self):
        """Reads the X-axis G value."""
        GPIO.output(self.mma8491qEn, GPIO.HIGH)
        time.sleep(0.001)

        while ((self._read_register_1ubyte(self.MMA8491Q_STATUS) & 0x01) != 0x01):
            time.sleep(0.001)
        tempData = self._read_2bytes_as_ushort_rs2b(self.MMA8491Q_OUT_X_MSB)
        GPIO.output(self.mma8491qEn, GPIO.LOW)

        return self._convert_to_g(tempData)

    def read_y_axis(self):
        """Reads the Y-axis G value."""
        GPIO.output(self.mma8491qEn, GPIO.HIGH)
        time.sleep(0.001)

        while ((self._read_register_1ubyte(self.MMA8491Q_STATUS) & 0x02) != 0x02):
            time.sleep(0.001)
        tempData = self._read_2bytes_as_ushort_rs2b(self.MMA8491Q_OUT_Y_MSB)
        GPIO.output(self.mma8491qEn, GPIO.LOW)

        return self._convert_to_g(tempData)

    def read_z_axis(self):
        """Reads the Z-axis G value."""
        GPIO.output(self.mma8491qEn, GPIO.HIGH)
        time.sleep(0.001)

        while ((self._read_register_1ubyte(self.MMA8491Q_STATUS) & 0x04) != 0x04):
            time.sleep(0.001)
        tempData = self._read_2bytes_as_ushort_rs2b(self.MMA8491Q_OUT_Z_MSB)
        GPIO.output(self.mma8491qEn, GPIO.LOW)

        return self._convert_to_g(tempData)

    def read_xyz_axis(self):
        """Reads the X, Y and Z-Axis G values respectively."""
        xyz = [ 0, 0, 0 ]

        GPIO.output(self.mma8491qEn, GPIO.HIGH)
        time.sleep(0.001)

        while ((self._read_register_1ubyte(self.MMA8491Q_STATUS) & 0x08) != 0x08):
            time.sleep(0.001)
        xyzArray = self._read_6bytes_array(self.MMA8491Q_OUT_X_MSB)
        GPIO.output(self.mma8491qEn, GPIO.LOW)

        xyz[0] = self._convert_to_g((xyzArray[0] << 6) + (xyzArray[1] >> 2)) #X-Axis
        xyz[1] = self._convert_to_g((xyzArray[2] << 6) + (xyzArray[3] >> 2)) #Y-Axis
        xyz[2] = self._convert_to_g((xyzArray[4] << 6) + (xyzArray[5] >> 2)) #Z-Axis

        return xyz

    def read_tilt_state(self):
        """Reads the tilt state.
        Returns True if acceleration is > 0.688g or X/Y axis > 45. False if not."""
        GPIO.output(self.mma8491qEn, GPIO.HIGH)
        time.sleep(0.001)
        state = False if GPIO.input(self.mma8491qInt) else True
        GPIO.output(self.mma8491qEn, GPIO.LOW)

        return state

    #Disposal
    def __del__(self):
        """Releases the resources."""
        return
