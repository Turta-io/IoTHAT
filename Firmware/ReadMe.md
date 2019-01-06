# IoT HAT Firmware
IoT Node's onboard microcontroller is responsible for reading analog inputs and handling infrared remote controller communications. As we increase the capabilities of your device, you may wish to update the firmware according to this documentation.

## Notice
_Firmware update process requires essential microcontroller programming experience. Any mistakes during the update makes your device inoperable._

## Prerequisites
You'll need a PIC programmer, just like PICKit 3, and 5x male to male jumper wires.

## Connection Schema
IoT HAT uses Microchip PIC16F182X series microcontroller. It's ICSP pins are exposed via IoT HAT's 40-pin headers on the rear side. Use the following schema to connect the programmer to your IoT HAT.

![IoTHAT ICSP Connection](https://turta.io/githubimg/IoTHAT_ICSP.png)

IoT HAT Header Pin 12 -> ICSP CLK  
IoT HAT Header Pin 13 -> ICSP DAT  
IoT HAT Header Pin 17 -> ICSP 3.3V  
IoT HAT Header Pin 20 -> ICSP GND  
IoT HAT Header Pin 26 -> ICSP MCLR  

## Programmer Settings
There are two important settings you'll need to make on the programmer software.

* Set the VDD setting to 3.3V. Do not use the default 5V setting.
* Select "power target circuit from tool" setting on the programmer, to supply power to the microcontroller during the programming.

Two different PIC models have been used in the IoT HAT's production. You can find the appropriate HEX file in the HEX folder.

## Change Log
* FW 1.05 (February 25, 2018):
IR Remote transmitter timings are improved.
* FW 1.04 (April 05, 2017):
I2C Communication settings are improved.