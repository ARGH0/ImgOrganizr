using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public class LogMessage
    {
        public LogMessage(string message, string color, string level)
        {
            Message = message;
            Color = color;
            Level = level;
        }

        public string Message { get; set; }
        public string Color { get; set; }
        public string Level { get; set; }
    }
}
