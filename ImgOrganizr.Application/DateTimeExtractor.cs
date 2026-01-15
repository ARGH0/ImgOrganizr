using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ImgOrganizr.Application
{
    public static class DateTimeExtractor
    {
        public static DateTime? ExtractDateTimeCreated(string filePath)
        {
            return File.GetCreationTime(filePath);
        }

        public static DateTime? ExtractDateTimeTaken(string filePath)
        {
            try
            {
                using (Image image = Image.FromFile(filePath))
                {
                    var propItem = image.GetPropertyItem(36867); // 36867 is the id for 'Date Taken'
                    if (propItem?.Value != null)
                    {
                        string dateTakenStr = Encoding.UTF8.GetString(propItem.Value).Trim('\0');
                        return DateTime.ParseExact(dateTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static DateTime? ExtractDateTimeChanged(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public static DateTime? ExtractDateFromFileName(string filePath, string regexPattern)
        {
            // Fallback: Try to extract date from filename using regex
            if (!string.IsNullOrEmpty(regexPattern))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var match = Regex.Match(fileName, regexPattern);

                if (match.Success)
                {
                    string extractedDate = match.Groups[0].Value;
                    DateTime dateTime = DateTime.ParseExact(extractedDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    return dateTime;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts 'Date Taken' from the metadata of the image file.
        /// </summary>
        /// <param name="filePath">Path to the image file.</param>
        /// <returns>Date taken as DateTime or null.</returns>
        public static DateTime? ExtractDateTaken(string filePath, string regexPattern)
        {
            DateTime? result = ExtractDateTimeTaken(filePath);

            if (result != null)
            {
                return result;
            }

            result = ExtractDateFromFileName(filePath, regexPattern);

            if (result != null)
            {
                return result;
            }

            result = ExtractDateTimeChanged(filePath);

            return result;
        }

    }
}
