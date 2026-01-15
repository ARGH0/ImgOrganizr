using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Log(LogMessage logMessage)
        {
            // Log to file with level
            using (StreamWriter writer = File.AppendText(_filePath))
            {
                writer.WriteLine($"{logMessage.Level}: {logMessage.Message}");
            }
        }
    }
}
