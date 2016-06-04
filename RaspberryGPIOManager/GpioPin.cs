using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

		/// <summary>
		/// The mode that pin has right now.
		/// </summary>
		public enum GPIOMode
		{
			Normal,
			Flicker,
			Dimmer
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
                    throw new InvalidOperationException("State of an input pin can only be read.");
                else
                {
                    File.WriteAllText(String.Format("{0}gpio{1}/value", GPIO_ROOT_DIR, GPIOPin.ToString().Substring(4)), (value == GPIOState.High ? "1" : "0"));
                }
            }
        }

		private GPIOMode _gpioMode;
		/// <summary>
		/// The current mode of a GPIO pin.
		/// </summary>
		public GPIOMode Mode
		{
			get { return _gpioMode; }
			set
			{
				if (_disposed)
					throw new ObjectDisposedException("Selected pin has been disposed.");
				else if (this.Direction == GPIODirection.In)
					throw new InvalidOperationException("Input pins do not have dimm or flick modes.");
				else
				{
					_gpioMode = value;
					if (_gpioMode == GPIOMode.Flicker)
					{
						Flick();
					}
					if (_gpioMode == GPIOMode.Dimmer)
					{
						Dimm();
					}
				}
			}
		}

		private int _gpioFlickMilisecons;
		/// <summary>
		/// Flick interval (ms) for a Flicker GPIO pin.
		/// </summary>
		public int FlickMilisecons
		{
			get { return _gpioFlickMilisecons; }
			set
			{
				if (_disposed)
					throw new ObjectDisposedException("Selected pin has been disposed.");
				else if (this.Direction == GPIODirection.In)
					throw new InvalidOperationException("An input pin cannot flick.");
				else
				{
					_gpioFlickMilisecons = value; "0"));
				}
			}
		}

		private int _gpioDimmPercent;
		/// <summary>
		/// Dimming percent for a Dimmer GPIO pin.
		/// </summary>
		public int DimmPercent
		{
			get { return _gpioDimmPercent; }
			set
			{
				if (_disposed)
					throw new ObjectDisposedException("Selected pin has been disposed.");
				else if (this.Direction == GPIODirection.In)
					throw new InvalidOperationException("An input pin cannot dimm.");
				else
				{
					_gpioDimmPercent = value;
				}
			}
		}


        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with direction set to OUT and initial value to LOW.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed</param>
        public GPIOPinDriver(Pin gpioPin)
            : this(gpioPin, GPIODirection.Out, GPIOState.Low, GPIOMode.Normal, 1000, 10) { }
        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with the given direction and initial value set to LOW.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed.</param>
        /// <param name="direction">The direction the GPIO pin should have.</param>
        public GPIOPinDriver(Pin gpioPin, GPIODirection direction)
			: this(gpioPin, direction, GPIOState.Low, GPIOMode.Normal, 1000, 10) { }
        public GPIOPinDriver(Pin gpioPin, GPIODirection direction, bool initialValue)
            : this(gpioPin, direction, (initialValue == false) ? GPIOState.Low : GPIOState.High, GPIOMode.Normal, 1000, 10) { }
        /// <summary>
        /// Sets up an interface for accessing the specified GPIO pin with the given direction and given initial value.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin to be accessed.</param>
        /// <param name="direction">The direction the GPIO pin should have.</param>
        /// <param name="initialValue">The initial value the GPIO pin should have.</param>
		public GPIOPinDriver(Pin gpioPin, GPIODirection direction, GPIOState initialValue, GPIOMode mode, int blinkMilisecons, int dimmPercent)
        {
            _disposed = false;
            this.GPIOPin = gpioPin;
            this.Direction = direction;
            if (this.Direction == GPIODirection.Out) {
                this.State = initialValue;
            }
			this.FlickMilisecons = blinkMilisecons;
			this.DimmPercent = dimmPercent;
			this.Mode = mode;
        }

		/// <summary>
		/// Starts the flicking action.
		/// </summary>
		private async void Flick()
		{
			// This method runs asynchronously.
			await Task.Run(() => FlickAsync());
		}
			
		private void FlickAsync()
		{
			GPIOState objState = this.State;
			do {
				this.State = GPIOState.High;
				System.Threading.Thread.Sleep (this.FlickMilisecons);
				this.State = GPIOState.Low;
				System.Threading.Thread.Sleep (this.FlickMilisecons);
			} while (this.Mode == GPIOMode.Flicker);
			this.State = objState;
		}

		/// <summary>
		/// Starts the dimming action.
		/// </summary>
		private async void Dimm()
		{
			// This method runs asynchronously.
			await Task.Run(() => DimmAsync());
		}

		private void DimmAsync()
		{
			GPIOState objState = this.State;
			do {
				this.State = GPIOState.High;
				System.Threading.Thread.Sleep (this.DimmPercent/10);
				this.State = GPIOState.Low;
				System.Threading.Thread.Sleep (10-(this.DimmPercent/10));
			} while (this.Mode == GPIOMode.Dimmer);
			this.State = objState;
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
			this.Mode = GPIOMode.Normal;
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
