﻿namespace ImgOrganizr
{
using System;
using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
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

			string dir = args[0];
			string regexPattern = args[1];

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

			CreateBackupFolder(dir);
			SetMetaData(dir, regexPattern);
			RenameFiles(dir);
			MoveFiles(dir);

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

		static DateTime? ExtractDateTimeCreated(string filePath)
		{
			return File.GetCreationTime(filePath);
		}

		static DateTime? ExtractDateTimeTaken(string filePath)
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
				return null;
			}
		}

		static DateTime? ExtractDateTimeChanged(string filePath)
		{
			return File.GetLastWriteTime(filePath);
		}

		static DateTime? ExtractDateFromFileName(string filePath, string regexPattern)
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
		static DateTime? ExtractDateTaken(string filePath, string regexPattern)
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



		/// <summary>
		/// Renames files in the specified directory. 
		/// </summary>
		/// <param name="dir">Directory path</param>
		static void RenameFiles(string dir)
		{
			var uniqueNumbersPerDay = new Dictionary<string, int>();
			var table = new Table().Border(TableBorder.Rounded);
			table.AddColumn("Old File Name");
			table.AddColumn("New File Name");

			// Populate uniqueNumbersPerDay dictionary by scanning existing files
			foreach (var existingFilePath in Directory.GetFiles(dir, "*.jpg"))
			{
				string existingFileName = Path.GetFileNameWithoutExtension(existingFilePath);
				var match = Regex.Match(existingFileName, @"(\d{2}_\d{2}_\d{4})_(\d{4})_image");
				if (match.Success)
				{
					string date = match.Groups[1].Value;
					int uniqueNumber = int.Parse(match.Groups[2].Value);

					if (!uniqueNumbersPerDay.ContainsKey(date) || uniqueNumbersPerDay[date] < uniqueNumber)
					{
						uniqueNumbersPerDay[date] = uniqueNumber;
					}
				}
			}

			// Create LiveDisplay
			var live = AnsiConsole.Live(table);
			live.Start(ctx =>
			{
				string[] searchPatterns = { "*.jpg", "*.jpeg" };
				foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
				{
					string oldFileName = Path.GetFileName(filePath);
					DateTime? dateTaken = ExtractDateTimeCreated(filePath);

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
		static void MoveFiles(string dir)
		{
			string[] searchPatterns = { "*.jpg" };
			foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
			{
				DateTime? dateTaken = ExtractDateTimeCreated(filePath);

				if (dateTaken != null)
				{
					string newDir = Path.Combine(dir, dateTaken.Value.Year.ToString(), dateTaken.Value.Month.ToString("D2"), dateTaken.Value.Day.ToString("D2"));
					Directory.CreateDirectory(newDir);

					string newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
					File.Move(filePath, newFilePath);
				}
			}
		}

		static void CreateBackupFolder(string dir)
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

		static void SetMetaData(string dir, string regexPattern)
		{
			string[] searchPatterns = { "*.jpg", "*.jpeg" };
			foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
			{
				DateTime? dateTaken = ExtractDateTaken(filePath, regexPattern);
				if (dateTaken != null)
				{
					File.SetCreationTime(filePath, dateTaken.Value);
				}
			}
		}
	}
}
