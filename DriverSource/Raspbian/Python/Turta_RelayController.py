# Turta IoT HAT Helper for Raspbian
# Distributed under the terms of the MIT license.

# Python Driver for Relay Controller
# Version 1.01
# Updated: July 14th, 2018

# For hardware info, visit www.turta.io/iothat
# For questions e-mail turta@turta.io

import RPi.GPIO as GPIO

class RelayController:
    """Relay Controller"""

    #Variables
    is_initialized = False

    #Pins
    relay1, relay2 = 20, 12

    #Initialize
    def __init__(self):
        GPIO.setwarnings(False)
        GPIO.setmode(GPIO.BCM)
        GPIO.setup(self.relay1, GPIO.OUT)
        GPIO.setup(self.relay2, GPIO.OUT)
        GPIO.output(self.relay1, GPIO.LOW)
        GPIO.output(self.relay2, GPIO.LOW)
        self.is_initialized = True
        return

    #Relay Control
    def set_relay(self, ch, st):
        """Controls the relay.
        :param ch: Relay channel. 1 or 2.
        :param st: Relay state. True of False."""
        if (ch == 1):
            GPIO.output(self.relay1, GPIO.HIGH if st else GPIO.LOW)
        elif (ch == 2):
            GPIO.output(self.relay2, GPIO.HIGH if st else GPIO.LOW)
        return

    #Relay Readout
    def read_relay_state(self, ch):
        """Reads the relay state.
        :param ch: Relay channel. 1 or 2."""
        if (ch == 1):
            return GPIO.input(self.relay1)
        elif (ch == 2):
            return GPIO.input(self.relay2)

    #Disposal
    def __del__(self):
        """Releases the resources."""
        if self.is_initialized:
            GPIO.output(self.relay1, GPIO.LOW)
            GPIO.output(self.relay2, GPIO.LOW)
            GPIO.cleanup()
            del self.is_initialized
        return
