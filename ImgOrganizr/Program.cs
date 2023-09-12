namespace ImgOrganizr
{
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		/// Extracts 'Date Taken' from the metadata of the image file.
		/// </summary>
		/// <param name="filePath">Path to the image file.</param>
		/// <returns>Date taken as DateTime or null.</returns>
		static DateTime? ExtractDateTaken(string filePath, string regexPattern)
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
			catch
			{
				// Fallback: Try to extract date from filename using regex
				if (!string.IsNullOrEmpty(regexPattern))
				{
					var fileName = Path.GetFileNameWithoutExtension(filePath);
					var match = Regex.Match(fileName, regexPattern);

					if (match.Success)
					{
						string extractedDate = match.Groups[0].Value;
						return DateTime.ParseExact(extractedDate, "yyyyMMdd", CultureInfo.InvariantCulture);
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Renames files in the specified directory. 
		/// </summary>
		/// <param name="dir">Directory path</param>
		static void RenameFiles(string dir, string regexPattern)
		{
			var uniqueNumbersPerDay = new Dictionary<string, int>();
			var table = new Table().Border(TableBorder.Rounded);
			table.AddColumn("Old File Name");
			table.AddColumn("New File Name");

			var live = AnsiConsole.Live(table);
			live.Start(ctx =>
			{
				string[] searchPatterns = { "*.jpg", "*.jpeg" };
				foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
				{
					string oldFileName = Path.GetFileName(filePath);
					DateTime? dateTaken = ExtractDateTaken(filePath, regexPattern);

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
						// Keep the original name if DateTaken is null
						table.AddRow(oldFileName, "Kept Original");
					}

					// Update the LiveDisplay
					ctx.Refresh();
				}
			});
		}

		/// <summary>
		/// Moves files into subdirectories based on their 'Date Taken'.
		/// </summary>
		/// <param name="dir">Directory path</param>
		static void MoveFiles(string dir, string regexPattern)
		{
			string[] searchPatterns = { "*.jpg", "*.jpeg" };
			foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
			{
				DateTime? dateTaken = ExtractDateTaken(filePath, regexPattern);

				if (dateTaken != null)
				{
					string newDir = Path.Combine(dir, dateTaken.Value.Year.ToString(), dateTaken.Value.Month.ToString("D2"), dateTaken.Value.Day.ToString("D2"));
					Directory.CreateDirectory(newDir);

					string newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
					File.Move(filePath, newFilePath);
				}
			}
		}
	}
}
