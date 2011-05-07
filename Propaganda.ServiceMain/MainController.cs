using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Context;
using Spring.Context.Support;

namespace Propaganda.ServiceMain
{
    public class MainController
    {

        public void LoadServices()
        {
            // get the Spring context
            var context = ContextRegistry.GetContext();

            // load core services

            // load audio services
            //var  

            // load video services

            // load WCF services for UPNP
        }
    }
}
