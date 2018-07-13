# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for Broadcom / Avago APDS-9960 Ambient Light, Color, Proximity & Gesture Sensor
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

import time
from enum import IntEnum
from smbus import SMBus

#Enumerations

#ENABLE - GEN: Gesture Enable
class ENABLE_GEN(IntEnum):
    OFF = 0b00000000
    ON = 0b01000000

#ENABLE - PIEN: Proximity Interrupt Enable
class ENABLE_PIEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00100000

#ENABLE - AIEN: ALS Interrupt Enable
class ENABLE_AIEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00010000

#ENABLE - WEN: Wait Enable
class ENABLE_WEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00001000

#ENABLE - PEN: Proximity Detect Enable
class ENABLE_PEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00000100

#ENABLE - AEN: ALS Enable
class ENABLE_AEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00000010

#ENABLE - PON: Power On
class ENABLE_PON(IntEnum):
    OFF = 0b00000000
    ON = 0b00000001

#CONTROL - LDRIVE: LED Drive Strenght
class CONTROL_LDRIVE(IntEnum):
    MA_100 = 0b00000000
    MA_050 = 0b01000000
    MA_025 = 0b10000000
    MA_012_5 = 0b11000000

#CONTROL - PGAIN: Proximity Gain Control
class CONTROL_PGAIN(IntEnum):
    X1 = 0b00000000
    X2 = 0b00000100
    X4 = 0b00001000
    X8 = 0b00001100

#CONTROL - AGAIN: ALS and Color Gain Control
class CONTROL_AGAIN(IntEnum):
    X01 = 0b00000000
    X04 = 0b00000001
    X16 = 0b00000010
    X64 = 0b00000011

#GCONF2 - GGAIN: Gesture Gain Control
class GCONF2_GGAIN(IntEnum):
    X1 = 0b00000000
    X2 = 0b00100000
    X4 = 0b01000000
    X8 = 0b01100000

#GCONF2 - GLDRIVE: Gesture LED Drive Strenght
class GCONF2_GLDRIVE(IntEnum):
    MA_100 = 0b00000000
    MA_050 = 0b00001000
    MA_025 = 0b00010000
    MA_012_5 = 0b00011000

#GCONF2 - GWTIME: Gesture Wait Time
class GCONF2_GWTIME(IntEnum):
    MS_00_0 = 0b00000000
    MS_02_8 = 0b00000001
    MS_05_6 = 0b00000010
    MS_08_4 = 0b00000011
    MS_14_0 = 0b00000100
    MS_22_4 = 0b00000101
    MS_30_8 = 0b00000110
    MS_39_2 = 0b00000111

#GCONF4 - GFIFO_CLR: Gesture FIFO Clear
class GCONF4_GFIFO_CLR(IntEnum):
    OFF = 0b00000000
    ON = 0b00000100

#GCONF4 - GIEN: Gesture Interrupt Enable
class GCONF4_GIEN(IntEnum):
    OFF = 0b00000000
    ON = 0b00000010

#GCONF4 - GMODE Values
class GCONF4_GMODE(IntEnum):
    ALS_PROX_COLOR = 0b00000000
    GESTURE = 0b00000001

class APDS9960Sensor:
    """APDS-9960 Sensor"""

    #I2C Slave Address
    I2C_ADDRESS = 0x39

    #Registers
    APDS9960_ENABLE = 0x80
    APDS9960_ATIME = 0x81
    APDS9960_WTIME = 0x83
    APDS9960_AILTL = 0x84
    APDS9960_AILTH = 0x85
    APDS9960_AIHTL = 0x86
    APDS9960_AIHTH = 0x87
    APDS9960_PILT = 0x89
    APDS9960_PIHT = 0x8B
    APDS9960_PERS = 0x8C
    APDS9960_CONFIG1 = 0x8D
    APDS9960_PPULSE = 0x8E
    APDS9960_CONTROL = 0x8F
    APDS9960_CONFIG2 = 0x90
    APDS9960_ID = 0x92
    APDS9960_STATUS = 0x93
    APDS9960_CDATAL = 0x94
    APDS9960_CDATAH = 0x95
    APDS9960_RDATAL = 0x96
    APDS9960_RDATAH = 0x97
    APDS9960_GDATAL = 0x98
    APDS9960_GDATAH = 0x99
    APDS9960_BDATAL = 0x9A
    APDS9960_BDATAH = 0x9B
    APDS9960_PDATA = 0x9C
    APDS9960_POFFSET_UR = 0x9D
    APDS9960_POFFSET_DL = 0x9E
    APDS9960_CONFIG3 = 0x9F
    APDS9960_GPENTH = 0xA0
    APDS9960_GEXTH = 0xA1
    APDS9960_GCONF1 = 0xA2
    APDS9960_GCONF2 = 0xA3
    APDS9960_GOFFSET_U = 0xA4
    APDS9960_GOFFSET_D = 0xA5
    APDS9960_GOFFSET_L = 0xA7
    APDS9960_GOFFSET_R = 0xA9
    APDS9960_GPULSE = 0xA6
    APDS9960_GCONF3 = 0xAA
    APDS9960_GCONF4 = 0xAB
    APDS9960_GFLVL = 0xAE
    APDS9960_GSTATUS = 0xAF
    APDS9960_IFORCE = 0xE4
    APDS9960_PICLEAR = 0xE5
    APDS9960_CICLEAR = 0xE6
    APDS9960_AICLEAR = 0xE7
    APDS9960_GFIFO_U = 0xFC
    APDS9960_GFIFO_D = 0xFD
    APDS9960_GFIFO_L = 0xFE
    APDS9960_GFIFO_R = 0xFF

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

    def _read_2bytes_as_ushort_lsbfirst(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        buffer = self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 2)
        return buffer[0] + (buffer[1] << 8)

    def _read_8bytes_array(self, reg_addr):
        """Reads data from the I2C device.
        :param reg_addr: Read register address.
        """
        return self.bus.read_i2c_block_data(self.I2C_ADDRESS, reg_addr, 8)

    def __init__(self):
        """Initiates the APDS-9960 sensor to get ambient light, RGB light and proximity"""
        self._set_initial_settings()
        self.set_mode(True, True, False)
        time.sleep(0.5)

    #Sensor Configuration

    def _set_initial_settings(self):
        """Writes the initial settings to the sensor."""
        #Set enable register to turn off all functionality but power.
        self._write_register(self.APDS9960_ENABLE, 0x01)

        #Set ATIME to 72 cycles, 200ms, 65535 max count.
        self._write_register(self.APDS9960_ATIME, 0xB6)

        #Set wait time to 20ms.
        self._write_register(self.APDS9960_WTIME, 0xF9)

        #Set ALS interrupt low thresold.
        self._write_register(self.APDS9960_AILTL, 0xFF) #Low
        self._write_register(self.APDS9960_AILTH, 0xFF) #High

        #Set ALS interrupt high thresold.
        self._write_register(self.APDS9960_AIHTL, 0x00) #Low
        self._write_register(self.APDS9960_AIHTH, 0x00) #High

        #Set proximity interrupt low thresold.
        self._write_register(self.APDS9960_PILT, 0)

        #Set proximity interrupt high thresold.
        self._write_register(self.APDS9960_PIHT, 255)

        #Set interrupt persistence filters.
        self._write_register(self.APDS9960_PERS, 0x11)

        #Set configuration register one: No 12x wait (WLONG = 0).
        self._write_register(self.APDS9960_CONFIG1, 0x60)

        #Set proximity pulse count to 8 and lenght to 8us.
        self._write_register(self.APDS9960_PPULSE, 0x48)

        #Set control register one:
        #LDRIVE: LED Drive Strenght to 100mA
        #PGAIN: Proximity Gain to 4x
        #AGAIN: ALS and Color Gain to 4x
        self._write_register(self.APDS9960_CONTROL, CONTROL_LDRIVE.MA_100 | CONTROL_PGAIN.X4 | CONTROL_AGAIN.X04)

        #Set configuration register two:
        #PSIEN: Disabled
        #CPSIEN: Disabled
        #LED_BOOST: 100%
        #Field 0: 1
        self._write_register(self.APDS9960_CONFIG2, 0x01)

        #Set proximity offset for up and right photodiodes to 0.
        self._write_register(self.APDS9960_POFFSET_UR, 0x00)

        #Set proximity offset for down and left photodiodes to 0.
        self._write_register(self.APDS9960_POFFSET_DL, 0x00)

        #Set configuration register three: Enable all photodiodes, no PCMP, no SAI.
        self._write_register(self.APDS9960_CONFIG3, 0x00)

        #Set gesture proximity enter thresold to 50.
        self._write_register(self.APDS9960_GPENTH, 50)

        #Set gesture exit thresold to 25.
        self._write_register(self.APDS9960_GEXTH, 25)

        #Set gesture configuration one: Interrupt after 4 dataset, end at 2nd, all directions are active.
        self._write_register(self.APDS9960_GCONF1, 0x41)

        #Set gesture configuration register two:
        #GGAIN: Gesture Gain to x4
        #GLDRIVE: Gesture LED Drive Strenght to 100mA
        #GWTIME: Gesture Wait Time to 2.8ms
        self._write_register(self.APDS9960_GCONF2, GCONF2_GGAIN.X4 | GCONF2_GLDRIVE.MA_100 | GCONF2_GWTIME.MS_02_8)

        #Set gesture offsets to 0.
        self._write_register(self.APDS9960_GOFFSET_U, 0x00)
        self._write_register(self.APDS9960_GOFFSET_D, 0x00)
        self._write_register(self.APDS9960_GOFFSET_L, 0x00)
        self._write_register(self.APDS9960_GOFFSET_R, 0x00)

        #Set gesture pulse count to 32 and lenght to 8us.
        self._write_register(self.APDS9960_GPULSE, 0x96)

        #Set gesture configuration three: All photodiodes are enabled to gather results during gesture.
        self._write_register(self.APDS9960_GCONF3, 0x00)

        #Set gesture configuration four.
        self._write_register(self.APDS9960_GCONF4, GCONF4_GFIFO_CLR.OFF | GCONF4_GIEN.OFF | GCONF4_GMODE.ALS_PROX_COLOR)

    def set_mode(self, ambient_and_rgb_light_enabled, proximity_detection_enabled, gesture_recognition_enabled):
        """Toggles the sensor modes.
        :param ambient_and_rgb_light_enabled: Ambient light and RGB light sense.
        :param proximity_detection_enabled: Proximity detection.
        :param gesture_recognition_enabled: Gesture recognition.
        """
        enableCommand = 0x00

        #Ambient and RGB Light
        if ambient_and_rgb_light_enabled:
            enableCommand |= ENABLE_PON.ON
            enableCommand |= ENABLE_AEN.ON

        #Proximity Detection
        if proximity_detection_enabled:
            enableCommand |= ENABLE_PON.ON
            enableCommand |= ENABLE_PEN.ON

        #Gesture Recognition
        if gesture_recognition_enabled:
            enableCommand |= ENABLE_PON.ON
            enableCommand |= ENABLE_GEN.ON

        #Enable Wait
        enableCommand |= ENABLE_WEN.ON

        self._write_register(self.APDS9960_ENABLE, enableCommand)

    #Sensor Readouts

    def read_ambient_light(self):
        """Reads the ambient light value."""
        return int(self._read_2bytes_as_ushort_lsbfirst(self.APDS9960_CDATAL))

    def read_rgb_light(self):
        """Reads the RGB light values.
        Red, green and blue light values respectively."""
        rgb = [ 0, 0, 0 ]

        crgb = self._read_8bytes_array(self.APDS9960_CDATAL)

        rgb[0] = int(crgb[2] | (crgb[3] << 8)) #Red channel
        rgb[1] = int(crgb[4] | (crgb[5] << 8)) #Green channel
        rgb[2] = int(crgb[6] | (crgb[7] << 8)) #Blue channel

        return rgb

    def read_proximity(self):
        """Reads the proximity value."""
        return int(self._read_register_1ubyte(self.APDS9960_PDATA))

    #Disposal
    def __del__(self):
        """Releases the resources."""
        #Turn off the sensor functionality.
        self.set_mode(False, False, False)
