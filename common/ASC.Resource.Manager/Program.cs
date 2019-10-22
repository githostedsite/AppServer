﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASC.Common.Utils;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASC.Resource.Manager
{
    class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Export);
        }

        public static void Export(Options options)
        {
            var services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var ResourceData = serviceProvider.GetService<ResourceData>();

            var cultures = new List<string>();
            var projects = new List<ResFile>();
            var enabledSettings = new EnabledSettings();
            Action<ResourceData, string, string, string, string, string, string> export = null;

            try
            {
                var (project, module, filePath, exportPath, culture, format, key) = options;

                if (format == "json")
                {
                    export = JsonManager.Export;
                }
                else
                {
                    export = ResxManager.Export;
                }

                if (string.IsNullOrEmpty(exportPath))
                {
                    exportPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                if (!Path.IsPathRooted(exportPath))
                {
                    exportPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), exportPath));
                }

                if (!Directory.Exists(exportPath))
                {
                    Console.WriteLine("Error!!! Export path doesn't exist! Please enter a valid directory path.");
                    return;
                }

                enabledSettings = serviceProvider.GetService<IConfiguration>().GetSetting<EnabledSettings>("enabled");
                cultures = ResourceData.GetCultures().Where(r => r.Available).Select(r => r.Title).Intersect(enabledSettings.Langs).ToList();
                projects = ResourceData.GetAllFiles();

                ExportWithProject(project, module, filePath, culture, exportPath, key);

                Console.WriteLine("The data has been successfully exported!");
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }

            void ExportWithProject(string projectName, string moduleName, string fileName, string culture, string exportPath, string key = null)
            {
                if (!string.IsNullOrEmpty(projectName))
                {
                    ExportWithModule(projectName, moduleName, fileName, culture, exportPath, key);
                }
                else
                {
                    var projectToExport = projects
                        .Where(r => string.IsNullOrEmpty(r.ModuleName) || r.ModuleName == moduleName)
                        .Where(r => string.IsNullOrEmpty(r.FileName) || r.FileName == fileName)
                        .Select(r => r.ProjectName)
                        .Intersect(enabledSettings.Projects);

                    foreach (var p in projectToExport)
                    {
                        ExportWithModule(p, moduleName, fileName, culture, exportPath, key);
                    }
                }
            }

            void ExportWithModule(string projectName, string moduleName, string fileName, string culture, string exportPath, string key = null)
            {
                if (!string.IsNullOrEmpty(moduleName))
                {
                    ExportWithFile(projectName, moduleName, fileName, culture, exportPath, key);
                }
                else
                {
                    var moduleToExport = projects
                        .Where(r => r.ProjectName == projectName)
                        .Where(r => string.IsNullOrEmpty(r.FileName) || r.FileName == fileName)
                        .Select(r => r.ModuleName);

                    foreach (var m in moduleToExport)
                    {
                        ExportWithFile(projectName, m, fileName, culture, exportPath, key);
                    }
                }
            }
            void ExportWithFile(string projectName, string moduleName, string fileName, string culture, string exportPath, string key = null)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    ExportWithCulture(projectName, moduleName, fileName, culture, exportPath, key);
                }
                else
                {
                    foreach (var f in projects.Where(r => r.ProjectName == projectName && r.ModuleName == moduleName).Select(r => r.FileName))
                    {
                        ExportWithCulture(projectName, moduleName, f, culture, exportPath, key);
                    }
                }
            }
            void ExportWithCulture(string projectName, string moduleName, string fileName, string culture, string exportPath, string key)
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    export(ResourceData, projectName, moduleName, fileName, culture, exportPath, key);
                }
                else
                {
                    ParallelEnumerable.ForAll(cultures.AsParallel(), c => export(ResourceData, projectName, moduleName, fileName, c, exportPath, key));
                }
            }
        }

        public static string CheckExist(string resName, string fullClassName)
        {
            var bag = new ConcurrentBag<string>();
            var path = "..\\..\\..\\..\\..\\";

            var csFiles = Directory.GetFiles(Path.GetFullPath(path), "*.cs", SearchOption.AllDirectories);
            var xmlFiles = Directory.GetFiles(Path.GetFullPath(path), "*.xml", SearchOption.AllDirectories);

            string localInit() => "";

            Func<string, ParallelLoopState, long, string, string> func(string regexp) => (f, state, index, a) =>
            {
                var data = File.ReadAllText(f);
                var regex = new Regex(regexp);
                var matches = regex.Matches(data);
                if (matches.Count > 0)
                {
                    return a + "," + string.Join(",", matches.Select(r => r.Groups[1].Value));
                }
                return a;
            };

            void localFinally(string r)
            {
                if (!bag.Contains(r) && !string.IsNullOrEmpty(r))
                {
                    bag.Add(r.Trim(','));
                }
            }

            _ = Parallel.ForEach(csFiles, localInit, func(@$"\W+{resName}\.(\w*)"), localFinally);
            _ = Parallel.ForEach(csFiles, localInit, func(@$"CustomNamingPeople\.Substitute\<{resName}\>\(""(\w*)""\)"), localFinally);
            _ = Parallel.ForEach(xmlFiles, localInit, func(@$"\|(\w*)\|{fullClassName.Replace(".", "\\.")}"), localFinally);

            return string.Join(',', bag.ToArray());
        }
    }
}
