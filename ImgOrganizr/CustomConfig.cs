using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr
{
    internal class CustomConfig
    {
        internal CustomConfig()
        {
            inputDirectories = new string[] { };
            outputDirectory = string.Empty;
            workingDirectory = string.Empty;
            backupDirectory = string.Empty;
            searchPatterns = new string[] { };
        }

        public string[] inputDirectories { get; set; }
        public string outputDirectory { get; set; }
        public string workingDirectory { get; set; }
        public string backupDirectory { get; set; }
        public string[] searchPatterns { get; set; }
    }
}
