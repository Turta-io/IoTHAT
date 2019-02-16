# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for IO Port
# Version 1.02
# Updated: February 16th, 2019

# Visit https://docs.turta.io for documentation.

from time import sleep
import RPi.GPIO as GPIO
from smbus import SMBus

class IOPort:
    "IO Port"

    #Variables
    is_initialized = False

    #Pins
    d1, d2, d3, d4 = 21, 22, 23, 24

    #I2C Slave Address
    I2C_ADDRESS = 0x28

    #Registers
    MCU_ANALOGIN_CH1 = 0x10
    MCU_ANALOGIN_CH2 = 0x11
    MCU_ANALOGIN_CH3 = 0x12
    MCU_ANALOGIN_CH4 = 0x13

    #I2C Config
    bus = SMBus(1)

    #I2C Communication
    def _read_2bytes(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        return buffer

    def __init__(self, d1In, d2In, d3In, d4In):
        GPIO.setwarnings(False)
        GPIO.setmode(GPIO.BCM)

        if (d1In):
            GPIO.setup(self.d1, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)
        else:
            GPIO.setup(self.d1, GPIO.OUT)
            GPIO.output(self.d1, GPIO.LOW)

        if (d2In):
            GPIO.setup(self.d2, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)
        else:
            GPIO.setup(self.d2, GPIO.OUT)
            GPIO.output(self.d2, GPIO.LOW)

        if (d3In):
            GPIO.setup(self.d3, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)
        else:
            GPIO.setup(self.d3, GPIO.OUT)
            GPIO.output(self.d3, GPIO.LOW)

        if (d4In):
            GPIO.setup(self.d4, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)
        else:
            GPIO.setup(self.d4, GPIO.OUT)
            GPIO.output(self.d4, GPIO.LOW)

        self.is_initialized = True
        return

    #Digital Output Control
    def set_digital(self, ch, st):
        """Sets the digital output state.
        :param ch: IO Channel.
        :param st: Pin State.
        """
        if (ch == 1):
            GPIO.output(self.d1, GPIO.HIGH if st else GPIO.LOW)
        elif (ch == 2):
            GPIO.output(self.d2, GPIO.HIGH if st else GPIO.LOW)
        elif (ch == 3):
            GPIO.output(self.d3, GPIO.HIGH if st else GPIO.LOW)
        elif (ch == 4):
            GPIO.output(self.d4, GPIO.HIGH if st else GPIO.LOW)

        return

    #Digital Input Readout
    def read_digital(self, ch):
        """Reads the digital input.
        :param ch: IO Channel.
        """
        if (ch == 1):
            return GPIO.input(self.d1)
        elif (ch == 2):
            return GPIO.input(self.d2)
        elif (ch == 3):
            return GPIO.input(self.d3)
        elif (ch == 4):
            return GPIO.input(self.d4)
        else:
            return 0

    #Analog Input Readout
    def read_analog(self, ch):
        """Reads the analog input.
        :param ch: IO Channel.
        """
        if (ch == 1):
            dtemp = self._read_2bytes(self.MCU_ANALOGIN_CH1)
        elif (ch == 2):
            dtemp = self._read_2bytes(self.MCU_ANALOGIN_CH2)
        elif (ch == 3):
            dtemp = self._read_2bytes(self.MCU_ANALOGIN_CH3)
        elif (ch == 4):
            dtemp = self._read_2bytes(self.MCU_ANALOGIN_CH4)
        else:
            return 0

        return float((((dtemp[1] % 128) << 8) + (dtemp[0] % 256)) / 1023.0)

    #Disposal
    def __del__(self):
        """Releases the resources."""
        if self.is_initialized:
            GPIO.cleanup()
            del self.is_initialized
        return