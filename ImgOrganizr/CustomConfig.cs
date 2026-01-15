using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr
{
    public class CustomConfig
    {
        public CustomConfig()
        {
            inputDirectories = new string[] { };
            outputDirectory = string.Empty;
            workingDirectory = string.Empty;
            backupDirectory = string.Empty;
            failedDirectory = string.Empty;
            runDirectory = string.Empty;
            searchPatterns = new string[] { };
        }

        public string[] inputDirectories { get; set; }
        public string outputDirectory { get; set; }
        public string workingDirectory { get; set; }
        public string backupDirectory { get; set; }
        public string failedDirectory { get; set; }
        public string runDirectory { get; set; }
        public string[] searchPatterns { get; set; }
    }
}
