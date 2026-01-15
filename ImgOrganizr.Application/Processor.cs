using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgOrganizr.Application
{
    public static class Processor
    {

        /// <summary>
        /// Moves files into subdirectories based on their 'Date Taken'.
        /// </summary>
        /// <param name="dir">Directory path</param>
        public static void MoveFiles(string dir)
        {
            string[] searchPatterns = { "*.jpg" };
            foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
            {
                DateTime? dateTaken = DateTimeExtractor.ExtractDateTimeCreated(filePath);

                if (dateTaken != null)
                {
                    string newDir = Path.Combine(dir, dateTaken.Value.Year.ToString(), dateTaken.Value.Month.ToString("D2"), dateTaken.Value.Day.ToString("D2"));
                    Directory.CreateDirectory(newDir);

                    string newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
                    File.Move(filePath, newFilePath);
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

        public static void SetMetaData(string dir, string regexPattern)
        {
            string[] searchPatterns = { "*.jpg", "*.jpeg" };
            foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
            {
                DateTime? dateTaken = DateTimeExtractor.ExtractDateTaken(filePath, regexPattern);
                if (dateTaken != null)
                {
                    File.SetCreationTime(filePath, dateTaken.Value);
                }
            }
        }
    }
}
