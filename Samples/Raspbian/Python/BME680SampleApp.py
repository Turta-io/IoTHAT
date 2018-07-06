#! /usr/bin/python

import time
import BME680Sensor

#Variables
#Sea level pressure in bar
slp = 1000.0 #Update this from weather forecast to get precise altitude

#Initialize
BME680Sensor.Init()

try:
    while True:
        #Hint: To get temperature, pressure and humidity readings at the same time,
        #call BME680Sensor.ReadTPH() method.

        #Read & print temperature
        print "Temperature.....: " + str(round(BME680Sensor.ReadTemperature(), 1)) + "C"

        #Read & print humidity
        print "Humidity........: %" + str(round(BME680Sensor.ReadHumidity(), 1)) + "RH"

        #Read & print pressure
        print "Pressure........: " + str(round(BME680Sensor.ReadPressure(), 1)) + "Pa"

        #Read & print altitude
        print "Altitude........: " + str(round(BME680Sensor.ReadAltitude(slp), 1)) + "m"

        #Read & print gas resistance
        print "Gas Resistance..: " + str(round(BME680Sensor.ReadGasResistance(), 1)) + "Ohms"

        #Rest a bit
        print "-----"
        time.sleep(10.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    #Release the resources
    BME680Sensor.Dispose()
