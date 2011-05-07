using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Propaganda.UPNP
{
    public interface IUPNPService
    {
        string FriendlyName { get; set; }

        string Manufacturer { get; set; }

        string ManufacturerURL { get; set; }

        string ModelName { get; set; }

        string ModelDescription { get; set; }

        string ModelNumber { get; set; }

        string DeviceURN { get; set; }

        bool HasPresentation { get; set; }

        Image Icon { get; set; }

        Image Icon2 { get; set; }
    }
}
