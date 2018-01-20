using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryGPIOManager
{
    class PinAlreadyExportedException : Exception
    {
        public PinAlreadyExportedException()
            : base()
        { }

        public PinAlreadyExportedException(string message)
            : base(message)
        { }
    }
}
