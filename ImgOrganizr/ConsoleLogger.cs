using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgOrganizr.Application;
using Spectre.Console;

namespace ImgOrganizr
{
    public class ConsoleLogger : ILogger
    {
        public void Log(LogMessage logMessage)
        {
            // Log to console with color and level
            AnsiConsole.MarkupLine($"[{logMessage.Color}]{logMessage.Level}: {logMessage.Message}[/]");
        }
    }
}
