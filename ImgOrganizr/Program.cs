using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ImgOrganizr.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ImgOrganizr
{
    internal class Program
    {
        /// <summary>
        /// The entry point for the program.
        /// </summary>
        /// <param name="args">CLI arguments. The firs is directory, the second is the regex.</param>
        private static void Main(string[] args)
        {

            // Create service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();


            // Get logger from service provider
            Logger logger = serviceProvider.GetRequiredService<Logger>();
            ConsoleLogger consoleLogger = serviceProvider.GetRequiredService<ConsoleLogger>();
            FileLogger fileLogger = serviceProvider.GetRequiredService<FileLogger>();
            logger.MessageLogged += consoleLogger.Log;
            logger.MessageLogged += fileLogger.Log;


            // Use logger
            logger.Log(new LogMessage(message: "Hello, ImgOrganizr!", color: "green", level: "INFO" ));


            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Config.json", optional: false, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

            CustomConfig customConfig = new CustomConfig();//configuration.GetSection("CustomConfig").Get<CustomConfig>();

            if(customConfig == null )
            {
                logger.Log(new LogMessage(message: "Error: Please provide the configuration file.", color: "red", level: "ERROR" ));
                return;
            }

            DisplayBanner();

            string dir = string.Empty;//customConfig.inputDirectories[0];
            string regexPattern = string.Empty;//customConfig.searchPatterns[0];

            if (!Directory.Exists(dir))
            {
                logger.Log(new LogMessage(message: $"The directory '{dir}' does not exist.", color: "red", level: "ERROR"));
                return;
            }

            if (string.IsNullOrEmpty(regexPattern))
            {
                logger.Log(new LogMessage(message: "The regex is not given.", color: "red", level: "ERROR" ));
                return;
            }

            Boolean success = false;

            try
            {

                foreach (string inputDirectory in customConfig.inputDirectories)
                {

                    if (!Directory.Exists(inputDirectory))
                    {
                        logger.Log(new LogMessage(message: $"The directory '{inputDirectory}' does not exist.", color: "red", level: "ERROR" ));
                        continue;
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
                        logger.Log(new LogMessage(message: $"Found {files.Length} files in {inputDirectory}", color: "blue", level: "INFO" ));
                    }

                    //CopyFilesAndFoldersToWorkingDirectory();
                    //CreateBackup();
                    //SetMetadataCorrectly();
                    //RenameFiles();
                    //MoveToOuputFolder();
                }

                Processor.CreateBackupFolder(dir);
                Processor.SetMetaData(dir, regexPattern);
                var uniqueNumbersPerDay = RenameFiles(dir);
                RenderLiveDisplay(dir, uniqueNumbersPerDay);

                Processor.MoveFiles(dir);
                success = true;
            }
            catch (Exception ex)
            {
                // After operations are done
                logger.Log(new LogMessage(message: "Something went wrong!", color: "red", level: "ERROR" ));
                logger.Log(new LogMessage(message: ex.Message, color: "white", level: "DETAILS" ));
                success = false;
            }

            if (success)
            {
                // After operations are done
                logger.Log(new LogMessage(message: "All operations finished!", color: "green", level: "INFO" ));
            }

            // Pause and wait for user input before exiting
            logger.Log(new LogMessage(message: "Press any key to exit...", color: "yellow", level: "INFO" ));
            Console.ReadKey();

        }

        /// <summary>
        /// Displays the application's banner.
        /// </summary>
        private static void DisplayBanner()
        {
            AnsiConsole.Write(
                new FigletText("Img Organizr")
                .Centered()
                .Color(Spectre.Console.Color.Red)
            );
        }

        /// <summary>
        /// Renames files in the specified directory. 
        /// </summary>
        /// <param name="dir">Directory path</param>
        private static Dictionary<string,int> RenameFiles(string dir)
        {
            var uniqueNumbersPerDay = new Dictionary<string, int>();


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

            return uniqueNumbersPerDay;
        }

        private static void RenderLiveDisplay(string dir, Dictionary<string, int> uniqueNumbersPerDay)
        {

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Old File Name");
            table.AddColumn("New File Name");

            // Create LiveDisplay
            var live = AnsiConsole.Live(table);
            live.Start(ctx =>
            {
                string[] searchPatterns = { "*.jpg", "*.jpeg" };
                foreach (string filePath in searchPatterns.SelectMany(sp => Directory.GetFiles(dir, sp)))
                {
                    string oldFileName = Path.GetFileName(filePath);
                    DateTime? dateTaken = DateTimeExtractor.ExtractDateTimeCreated(filePath);

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
                        string? directoryName = Path.GetDirectoryName(filePath);

                        string newFilePath;
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            newFilePath = Path.Combine(directoryName, newFileName);
                        }
                        else
                        {
                            newFilePath = newFileName;
                        }

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

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register your logger with the dependency injection container
            services.AddSingleton<Logger>();
            services.AddSingleton<ConsoleLogger>();
            services.AddSingleton<FileLogger>(_ => new FileLogger("log.txt"));
        }
    }
}
