# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# NXP MMA8491Q 3-Axis Accelerometer & Tilt Sensor Python Driver
# Version 1.00 (Initial Release)
# Updated: July 6th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions, visit www.turta.io/forum or e-mail turta@turta.io

import time
import RPi.GPIO as GPIO
from smbus import SMBus

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

def ReadRegisterOneUByte(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 1)
    return buffer[0]

def Read2BytesAsUShortRS2B(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    buffer = bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 2)
    return (buffer[0] << 6) + (buffer[1] >> 2)

def Read6BytesArray(regAddr):
    """Reads data from the I2C device.
    :param regAddr: Read register address.
    """
    return bus.read_i2c_block_data(I2C_ADDRESS, regAddr, 6)

def Init():
    """Initiates the APDS-9960 sensor to get ambient light, RGB light and proximity"""
    GPIO.setwarnings(False)
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(mma8491qEn, GPIO.OUT)
    GPIO.setup(mma8491qInt, GPIO.IN, pull_up_down = GPIO.PUD_DOWN)

#Sensor Readouts

def ConvertToG(analogData):
    """Converts raw sensor data to G value."""
    if ((analogData & 0x2000) == 0x2000): #Zero or negative G
        return (0x3FFF - analogData) / -1024.0
    else: #Positive G
        return analogData / 1024.0

def ReadXAxis():
    """Reads the X-axis G value."""
    GPIO.output(mma8491qEn, GPIO.HIGH)
    time.sleep(0.001)

    while ((ReadRegisterOneUByte(MMA8491Q_STATUS) & 0x01) != 0x01):
        time.sleep(0.001)
    tempData = Read2BytesAsUShortRS2B(MMA8491Q_OUT_X_MSB)
    GPIO.output(mma8491qEn, GPIO.LOW)

    return ConvertToG(tempData)

def ReadYAxis():
    """Reads the Y-axis G value."""
    GPIO.output(mma8491qEn, GPIO.HIGH)
    time.sleep(0.001)

    while ((ReadRegisterOneUByte(MMA8491Q_STATUS) & 0x02) != 0x02):
        time.sleep(0.001)
    tempData = Read2BytesAsUShortRS2B(MMA8491Q_OUT_Y_MSB)
    GPIO.output(mma8491qEn, GPIO.LOW)

    return ConvertToG(tempData)

def ReadZAxis():
    """Reads the Z-axis G value."""
    GPIO.output(mma8491qEn, GPIO.HIGH)
    time.sleep(0.001)

    while ((ReadRegisterOneUByte(MMA8491Q_STATUS) & 0x04) != 0x04):
        time.sleep(0.001)
    tempData = Read2BytesAsUShortRS2B(MMA8491Q_OUT_Z_MSB)
    GPIO.output(mma8491qEn, GPIO.LOW)

    return ConvertToG(tempData)

def ReadXYZAxis():
    """Reads the X, Y and Z-Axis G values respectively."""
    xyz = [ 0, 0, 0 ]

    GPIO.output(mma8491qEn, GPIO.HIGH)
    time.sleep(0.001)

    while ((ReadRegisterOneUByte(MMA8491Q_STATUS) & 0x08) != 0x08):
        time.sleep(0.001)
    xyzArray = Read6BytesArray(MMA8491Q_OUT_X_MSB)
    GPIO.output(mma8491qEn, GPIO.LOW)

    xyz[0] = ConvertToG((xyzArray[0] << 6) + (xyzArray[1] >> 2)) #X-Axis
    xyz[1] = ConvertToG((xyzArray[2] << 6) + (xyzArray[3] >> 2)) #Y-Axis
    xyz[2] = ConvertToG((xyzArray[4] << 6) + (xyzArray[5] >> 2)) #Z-Axis

    return xyz

def ReadTiltState():
    """Reads the tilt state.
    Returns True if acceleration is > 0.688g or X/Y axis > 45. False if not."""
    GPIO.output(mma8491qEn, GPIO.HIGH)
    time.sleep(0.001)
    state = False if GPIO.input(mma8491qInt) else True
    GPIO.output(mma8491qEn, GPIO.LOW)

    return state

def Dispose():
    """Releases The Resources."""
    global bus
    del bus
