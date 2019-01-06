# IoT HAT
IoT HAT orchestrates high-end components demanding in IoT scenarios. It combines sensors, relays, IO’s and an IR remote transceiver on a single board. This allows you to easily create complex scenarios without the hassle of cable clutter. Whether you are a beginner or a professional, IoT HAT will help you to develop the best in the shortest possible time.  

## Documentation

Visit [docs.turta.io](https://docs.turta.io) for documentation.

## Sensors and Devices

### Bosch Sensortec BME680 Environmental Sensor

Measures temperature, relative humidity, pressure and gas resistance. Calculates altitude and indoor air quality.

* Gas Resistance: Measures gas resistance in Ohms. If used with Bosch's drivers, 0 to 500 IAQ (Indoor Air Quality) result can be read.
* Temperature: Measures -40C to 85C temperature with +/- 1C accuracy and 0.01C resolution.
* Humidity: Measures relative humidity within 0% to 100% range, in 3% accuracy and 0.008% resolution.
* Pressure: Measures air pressure from 300 to 1100hPa in 0.18Pa resolution.
* Altitude: The driver calculates altitude from sea level if air pressure at the sea level is given.

The sensor uses 0x76 address over the I2C bus.

_IoT HAT will heat up as your Raspberry Pi warms up in hot environments. In this case, the sensor will read the temperature, humidity and pressure above normal levels. Keeping the device upright helps hot air to escape._


### Broadcom APDS-9960 Ambient light, RGB, Gesture and Proximity Sensor

Measures ambient light, RGB values and proximity. Detects hand gestures.

* Ambient Light: Measures ambient light with UV and IR blocking features.
* Color Detection: Measures RGBC channels with UV and IR blocking features.
* Hand gesture detection: Detects left, right, up and down directions within 30cm to the sensor.
* Proximity detection: Detects distance to the object up to 30cm from the sensor.

The sensor uses 0x39 address on the I2C bus.

### Vishay VEML6075 UV Sensor

Measures UVA and UVB. Calculates UV A Index, UVB Index and average UV Index.

* UVA: Measures wavelenghts between 315nm to 400nm in 16-bit resolution.
* UVB: Measures wavelenghts between 280nm to 315nm in 16-bit resolution.
* UV Index: Calculates UV radiation.

The sensor uses 0x10 address over the I2C bus.

### NXP MMA8491Q Accelerometer & Tilt Sensor

Measures 3 axis acceleration. Generates interrupt on tilt detect.

* Acceleration: Measures +/- 8g acceleration data with 1 mg accuracy.
* Tilt Detection: Generates interrupt over 0.688g acceleration or 43.5 degrees of tilt. IoT HAT uses Z-axis interrupt output.

The sensor uses 0x55 address over the I2C bus.

### AM312 Passive IR Motion Sensor

Detects human movement.

* Motion Detection: Detects the movement of heat emitting objects.

The sensor uses GPIO25 pin to generate interrupt.

### LCA717S Solid State Relay

Turns DC devices on or off.

* 2x Relays: DC30V 2A solid state relays can switch small devices.

Relays can be activated using GPIO 20 and 12 pins respectively.

_Do not use devices whose peak current consumption will be over 2A._

### LTV-827S Photocoupler

Optically isolates 4x inputs.

* 4x Photocoupler Inputs: Reads optically isolated 5V inputs.

Photocoupler inputs can be read using GPIO 13, 19, 16 and 26 pins respectively.

### Vishay TSOP75338W IR Receiver & VSMB10940X01 IR Transmitter

Decodes and encodes 38KHz NEC protocol IR remote commands.

* Infrared Receiver: Decodes 38KHz NEC protocol messages. NEC Protocol transfers 4-Bytes of data. On message receive, onboard microcontroller generates an interrupt. Then, received message can be read over the I2C bus.
* Infrared Transmitter: Encodes 4-Bytes of data to 38KHz NEC protocol. The IR emitter is at 940nm and 104mW power.

The microcontroller uses 0x28 address over the I2C bus. Interrupt pin is GPIO18.

### ADC

Measures 4x analog inputs.

* 4x Analog Inputs: Measures input voltages from 0V to 3.3V with 1/1024 (10-bits) resolution.

The microcontroller uses 0x28 address over the I2C bus.

### I2C and I/O Sockets

Board has 1x I2C and 4x I/O connection.

* I2C Socket: Provides I2C connection for external devices, such as sensors and displays.
* 4x I/O Sockets: Each socket provides analog input and GPIO pin. GPIO Pins are directly conected to the Raspberry Pi.

GPIO Pins on I/O sockets are connected to Raspberry Pi's GPIO 21, 22, 23 and 24 pins respectively.