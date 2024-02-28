using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public interface ILogger
    {
        void Log(LogMessage logMessage);
    }
}
