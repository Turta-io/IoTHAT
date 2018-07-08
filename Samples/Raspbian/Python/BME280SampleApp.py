#! /usr/bin/python

import time
import BME280Sensor

#Variables
#Sea level pressure in bar
slp = 1000.0 #Update this from weather forecast to get precise altitude

#Initialize
BME280Sensor.Init()

try:
    while True:
        #Hint: To get temperature, pressure and humidity readings at the same time,
        #call BME280Sensor.ReadTPH() method.

        #Read & print temperature
        print "Temperature.....: " + str(round(BME280Sensor.ReadTemperature(), 1)) + "C"

        #Read & print humidity
        print "Humidity........: %" + str(round(BME280Sensor.ReadHumidity(), 1)) + "RH"

        #Read & print pressure
        print "Pressure........: " + str(round(BME280Sensor.ReadPressure(), 1)) + "Pa"

        #Read & print altitude
        print "Altitude........: " + str(round(BME280Sensor.ReadAltitude(slp), 1)) + "m"

        #Rest a bit
        print "-----"
        time.sleep(10.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    #Release the resources
    BME280Sensor.Dispose()
