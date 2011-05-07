using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Propaganda.Domain.UPnP
{
   public class Container
    {
       public int Id { get; set; }

       public string Title { get; set; }

       public string Creator { get; set; }

       public Uri Resource { get; set; }

       public string Class { get; set; }
    }
}
