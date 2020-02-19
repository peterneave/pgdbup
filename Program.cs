using DbUp;
using DbUp.ScriptProviders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace pgdbup
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var connectionString = args.FirstOrDefault() ?? ConfigurationManager.ConnectionStrings["DatabaseContext"].ConnectionString;
            var scriptsFolder = GetChildFolder("Scripts");

            EnsureDatabase.For.PostgresqlDatabase(connectionString);
            var variables = LoadVariablesFromConfiguration();

            var timeStampedFileSystemOptions = GetFileSystemScriptOptionForTimeStampedSQL();

            var upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(scriptsFolder, timeStampedFileSystemOptions)
                    .WithVariables(GetPostgresKeyWordsDictionary())
                    .WithVariables(variables)
                    .LogToConsole()
                    .Build();
#if DEBUG
            DisplayScriptsToExecute(upgrader);
#endif

            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadKey();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();

#if DEBUG
            Console.ReadKey();
#endif
            return 0;
        }

        private static void DisplayScriptsToExecute(DbUp.Engine.UpgradeEngine upgrader)
        {
            var scriptsToApply = upgrader.GetScriptsToExecute();
            if (scriptsToApply.Count == 0) return;

            Console.WriteLine("About to run the following Scripts");
            foreach (var script in scriptsToApply)
            {
                Console.WriteLine($"- {script.Name}");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static string GetChildFolder(string scriptsFolder)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + '\\' + scriptsFolder;
        }

        private static IDictionary<string, string> GetPostgresKeyWordsDictionary()
        {
            var keyWords = new string[] { "do", "function", "body", "BODY" };
            var keyWordsDict = new Dictionary<string, string>();
            foreach (var keyWord in keyWords)
            {
                keyWordsDict.Add(keyWord, $"${keyWord}$");
            }

            return keyWordsDict;
        }

        /// <summary>
        /// Gets the file system script option for time stamped SQL.
        /// Scripts must be in the format YYYYDDMMhhmmZ-Description.sql
        /// </summary>
        /// <returns></returns>
        private static FileSystemScriptOptions GetFileSystemScriptOptionForTimeStampedSQL()
        {
            var regexMatch = $"^20\\d{{10}}Z-.*\\.sql";
            return new FileSystemScriptOptions
            {
                Filter = (file) =>
                {
                    var fileName = Path.GetFileName(file);
                    return Regex.Match(fileName, regexMatch, RegexOptions.IgnoreCase).Success;
                }
            };
        }

        private static Dictionary<string, string> LoadVariablesFromConfiguration()
        {
            var returnDict = new Dictionary<string, string>();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                returnDict.Add(key, ConfigurationManager.AppSettings[key]);
            }

            return returnDict;
        }
    }
}