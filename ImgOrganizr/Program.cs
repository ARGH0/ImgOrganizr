namespace ImgOrganizr
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Spectre.Console;

    class Program
    {
        /// <summary>
        /// The entry point for the program.
        /// </summary>
		/// <param name="args">CLI arguments. The first is one of the commands --rename, --move and the second is directory, the third is the regex depending on the chosen command.</param>
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                AnsiConsole.MarkupLine("[red]Error: Please provide the directory and at least one operation (--rename, --move).[/]");
                return;
            }

            DisplayBanner();

			var operations = args[0];

			string dir = args[1];
			string regexPattern = args[2];

            if (!Directory.Exists(dir))
            {
                AnsiConsole.MarkupLine($"[red]Error: The directory '{dir}' does not exist.[/]");
                return;
            }

			if (String.IsNullOrEmpty(regexPattern))
			{
				AnsiConsole.MarkupLine($"[red]Error: The regex is not given.[/]");
				return;
			}

            if (operations.Contains("--rename"))
            {
				RenameFiles(dir, regexPattern);
            }

            if (operations.Contains("--move"))
            {
				MoveFiles(dir, regexPattern);
            }

			return;
        }

        /// <summary>
        /// Displays the application's banner.
        /// </summary>
        static void DisplayBanner()
        {
            AnsiConsole.Write(
                new FigletText("Img Organizr")
                .Centered()
                .Color(Spectre.Console.Color.Red)
            );
        }

        /// <summary>
        /// Extracts the 'Date Taken' from an image's metadata.
        /// </summary>
        /// <param name="filePath">The path of the image file.</param>
        /// <returns>The DateTime representing the 'Date Taken'.</returns>
        static DateTime? ExtractDateTaken(string filePath)
        {
            try
            {
                using (Image image = Image.FromFile(filePath))
            {
                var propItem = image.GetPropertyItem(36867); // 36867 is the id for 'Date Taken'
                string dateTakenStr = Encoding.UTF8.GetString(propItem.Value).Trim('\0');
                return DateTime.ParseExact(dateTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            }
            catch (ArgumentException)
            {
                // Return null if the property is not found.
                return null;
            }
        }

        /// <summary>
        /// Renames image files in a directory based on 'Date Taken' and a unique number.
        /// </summary>
        /// <param name="dir">The directory containing the image files.</param>
        static void RenameFiles(string dir)
        {
            var uniqueNumbersPerDay = new Dictionary<string, int>();
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Old File Name");
            table.AddColumn("New File Name");

            // Create LiveDisplay
            var live = AnsiConsole.Live(table);

            live.Start(ctx =>
            {
                foreach (var filePath in Directory.GetFiles(dir, "*.jpg"))
                {
                    string oldFileName = Path.GetFileName(filePath);
                    DateTime? dateTaken = ExtractDateTaken(filePath);

                    if (dateTaken != null)
                    {
                        string dateTakenFormatted = dateTaken.Value.ToString("dd_MM_yyyy");

                        if (!uniqueNumbersPerDay.ContainsKey(dateTakenFormatted))
                        {
                            uniqueNumbersPerDay[dateTakenFormatted] = 1;
                        }
                        else
                        {
                            uniqueNumbersPerDay[dateTakenFormatted]++;
                        }

                        string uniqueNumber = uniqueNumbersPerDay[dateTakenFormatted].ToString("D4");
                        string newFileName = $"{dateTakenFormatted}_{uniqueNumber}_image.jpg";
                        string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                        File.Move(filePath, newFilePath);
                        table.AddRow(oldFileName, newFileName);
                    }
                    else
                    {
                        // DateTaken is null, keeping original filename
                        table.AddRow(oldFileName, "Kept Original");
                    }

                    // Update the LiveDisplay
                    ctx.Refresh();
                }
            });
        }

        /// <summary>
        /// Moves image files into subdirectories based on their 'Date Taken'.
        /// </summary>
        /// <param name="dir">The directory containing the image files.</param>
        static void MoveFiles(string dir)
        {
            foreach (var filePath in Directory.GetFiles(dir, "*.jpg"))
            {
                DateTime? dateTaken = ExtractDateTaken(filePath);

                string newDir = Path.Combine(dir, dateTaken?.Year.ToString(), dateTaken?.Month.ToString("D2"), dateTaken?.Day.ToString("D2"));
                Directory.CreateDirectory(newDir);

                string newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
                File.Move(filePath, newFilePath);
            }
        }
    }

}