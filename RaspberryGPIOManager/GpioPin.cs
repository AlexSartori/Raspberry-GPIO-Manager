using System;
using System.Collections.Generic;
using System.IO;

namespace RaspberryGPIOManager
{
    public class GPIOPinDriver : IDisposable
    {
        private const string GPIO_ROOT_DIR = "/sys/class/gpio/";
        private static List<Pin> _exported_pins = new List<Pin>();
        private bool _disposed;

        /// <summary>
        /// P1 GPIO pins
        /// </summary>
        public enum Pin
        {
            GPIO2,
            GPIO3,
            GPIO4,
            GPIO7,
            GPIO8,
            GPIO9,
            GPIO10,
            GPIO11,
            GPIO14,
            GPIO15,
            GPIO17,
            GPIO18,
            GPIO22,
            GPIO23,
            GPIO24,
            GPIO25,
            GPIO27
        };
        /// <summary>
        /// Direction of the GPIO Pin
        /// </summary>
        public enum GPIODirection
        {
            In,
            Out
        };
        /// <summary>
        /// The value (High or Low) of a GPIO pin.
        /// </summary>
        public enum GPIOState
        {
            Low = 0,
            High = 1
        };

        private Pin _gpioPin;
        /// <summary>
        /// Gets the GPIO pin number.
        /// </summary>
        public Pin GPIOPin
        {
            get { return _gpioPin; }
            private set
            {
                lock (_exported_pins)
                {
                    if (_disposed)
                        throw new ObjectDisposedException("Selected pin has been disposed.");
                    else if (_exported_pins.IndexOf(value) != -1)
                        throw new PinAlreadyExportedException("Requested pin is already exported.");
                    else
                    {                                                                                       //  0  1  2  3  4  5
                        File.WriteAllText(GPIO_ROOT_DIR + "export", value.ToString().Substring(4));       //  G  P  I  O  8
                        _exported_pins.Add(value);
                        _gpioPin = value;  
                    }
                }
            }
        }

        private GPIODirection _gpioDirection;
        /// <summary>
        /// Gets or sets the direction of of an output GPIO pin.
        /// </summary>
        public GPIODirection Direction
        {
            get { return _gpioDirection; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("Selected pin has been disposed.");
                else
                {
                    File.WriteAllText(String.Format("{0}gpio{1}/direction", GPIO_ROOT_DIR, GPIOPin.ToString().Substring(4)), (value == GPIODirection.In ? "in" : "out"));
                    _gpioDirection = value;
                }
            }
        }

        /// <summary>
        /// The current value of a GPIO pin.
        /// </summary>
        public GPIOState State
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("Selected pin has been disposed.");
                else
                {
                    string state = File.ReadAllText(String.Format("{0}gpio{1}/value", GPIO_ROOT_DIR, GPIOPin.ToString().Substring(4)));
                    return (state[0] == '1' ? GPIOState.High : GPIOState.Low);
                }
            }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("Selected pin has been disposed.");
                else if (this.Direction == GPIODirection.In)
                    throw new InvalidOperationException("State of an input pin can only be read");
                else
                {
                    File.WriteAllText(String.Format("{0}gpio{1}/value", GPIO_ROOT_DIR, GPIOPin.ToString().Substring(4)), (value == GPIOState.High ? "1" : "0"));
                }
            }
        }


        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with direction set to OUT and initial value to LOW.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed</param>
        public GPIOPinDriver(Pin gpioPin)
            : this(gpioPin, GPIODirection.Out, GPIOState.Low) { }
        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with the given direction and initial value set to LOW.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed.</param>
        /// <param name="direction">The direction the GPIO pin should have.</param>
        public GPIOPinDriver(Pin gpioPin, GPIODirection direction)
            : this(gpioPin, direction, GPIOState.Low) { }
        /*public GPIOPinDriver(Pin gpioPin, GPIODirection direction, bool initialValue)
            : this(gpioPin, direction, (initialValue == false) ? GPIOState.Low : GPIOState.High) { }*/
        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with the given direction and given initial value.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed.</param>
        /// <param name="direction">The direction the GPIO pin should have.</param>
        /// <param name="initialValue">The initial value the GPIO pin should have.</param>
        public GPIOPinDriver(Pin gpioPin, GPIODirection direction, GPIOState initialValue)
        {
            _disposed = false;
            this.GPIOPin = gpioPin;
            this.Direction = direction;
            
            if (this.Direction == GPIODirection.Out) {
                this.State = initialValue;
            }
        }


        /// <summary>
        /// Unexports the GPIO interface.
        /// </summary>
        public void Unexport()
        {
	    if (!_disposed)
                Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException("Selected pin has already been disposed.");
             
            File.WriteAllText(GPIO_ROOT_DIR + "unexport", GPIOPin.ToString().Substring(4));
            _exported_pins.Remove(this.GPIOPin);
            _disposed = true;
        }

        ~GPIOPinDriver()
        {
            if (!_disposed)
                Dispose();
        }
    }
}
