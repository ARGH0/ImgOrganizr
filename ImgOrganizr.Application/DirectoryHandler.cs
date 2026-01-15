using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public class DirectoryHandler
    {
        public static void CreateBackupFolder(string dir)
        {
            string[] searchPatterns = { "*.jpg", "*.jpeg" };
            foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
            {
                string newDir = Path.Combine(dir, "backup");
                Directory.CreateDirectory(newDir);

                string newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
                File.Copy(filePath, newFilePath, false);
            }
        }

        public void CreateDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public void MoveDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
        {
            Directory.Move(sourceDirectoryPath, destinationDirectoryPath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath, true);
        }

        public string[] GetFiles(string directoryPath)
        {
            return Directory.GetFiles(directoryPath);
        }

    }
}
