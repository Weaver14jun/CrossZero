using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GreeterServer
{
    public class ServerModel
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public bool Status { get; set; }
    }
}