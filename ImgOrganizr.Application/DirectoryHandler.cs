using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public class DirectoryHandler
    {
        public static string CreateRunFolder(string baseRunDirectory)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string runFolderName = $"run_{timestamp}";
            string runFolderPath = Path.Combine(baseRunDirectory, runFolderName);
            Directory.CreateDirectory(runFolderPath);
            return runFolderPath;
        }

        public static void CopyFilesToRunFolder(string[] inputDirectories, string runFolderPath)
        {
            string[] searchPatterns = { "*.jpg", "*.jpeg" };
            
            foreach (string inputDir in inputDirectories)
            {
                if (!Directory.Exists(inputDir))
                    continue;
                    
                foreach (string pattern in searchPatterns)
                {
                    foreach (string sourceFile in Directory.GetFiles(inputDir, pattern, SearchOption.AllDirectories))
                    {
                        string fileName = Path.GetFileName(sourceFile);
                        string destFile = Path.Combine(runFolderPath, fileName);
                        
                        // Handle duplicate filenames
                        int counter = 1;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);
                        
                        while (File.Exists(destFile))
                        {
                            destFile = Path.Combine(runFolderPath, $"{fileNameWithoutExt}_{counter}{extension}");
                            counter++;
                        }
                        
                        File.Copy(sourceFile, destFile);
                    }
                }
            }
        }

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
