#! /usr/bin/python

import time
from turta_iothat import Turta_BME280
#Install IoT HAT library with "pip install turta-iothat"

#Variables
#Sea level pressure in bar
slp = 1000.0 #Update this from weather forecast to get precise altitude

#Initialize
bme280 = Turta_BME280.BME280Sensor()

try:
    while True:
        #Hint: To get temperature, pressure and humidity readings at the same time,
        #call BME280Sensor.ReadTPH() method.

        #Read & print temperature
        print("Temperature.....: " + str(round(bme280.read_temperature(), 1)) + "C")

        #Read & print humidity
        print("Humidity........: %" + str(round(bme280.read_humidity(), 1)) + "RH")

        #Read & print pressure
        print("Pressure........: " + str(round(bme280.read_pressure(), 1)) + "Pa")

        #Read & print altitude
        print("Altitude........: " + str(round(bme280.read_altitude(slp), 1)) + "m")

        #Rest a bit
        print("-----")
        time.sleep(10.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')