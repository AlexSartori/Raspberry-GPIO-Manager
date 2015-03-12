<h1 style="display: inline; color:red">
RaspberryGPIOManager  &nbsp;
 <a href="http://www.raspberrypi.org">
  <img src="http://www.raspberrypi.org/wp-content/uploads/2012/03/Raspi_Colour_R.png" height="70" alt="raspberrypi.org" align="bottom">
 </a>
</h1>

This simple C# library allows you to easily manage any GPIO pin on your Raspberry Pi.
If you wish to learn more about GPIO, you may want to look at [this](http://elinux.org/RPi_Low-level_peripherals "Low-Level peripherals reference").
To see an example on how the library works, see below.

***

GPIOPinDriver object
-------------------------------
The GPIOPinDriver object is the one you need to build in order to perform any action.
In its constructor you have to specify the GPIO pin to associate, and you may also want to give the direction and the initial value to assign.

`var pin1 = new GPIOPinDriver(GPIOPinDriver.Pin.GPIO23);`

`var pin2 = new GPIOPinDriver(GPIOPinDriver.Pin.GPIO23, GPIOPinDriver.GPIODirection.Out, GPIOPinDriver.GPIOState.Low);`



GPIOPinDriver properties
-------------------------------
Once you have created your object(s), you can access following properties:

`Direction` -  In or Out, (treat it as an input or output pin)

`GPIOPin`   -  [Readonly] The pin that is being controlled.

`State`     -  The current value of the signal. In output mode this property can be used to switch on or off the pin,
             in input mode to read the value of it (High/Low).


Coming Soon
--------------------------------
- Possibility to handle events on GPIO pin state changes
- Object to handle serial communication


Notes
-------------------------------
GPIOPinDriver class implements IDisposable interface, that is to say it should be disposed at the end, calling either `Dispose()` or `Unexport()` function.

**IMPORTANT**: Accessing GPIO pins require root permissions!


---
*Raspberry Pi is a trademark of the Raspberry Pi Foundation.*
