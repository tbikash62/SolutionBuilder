﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using MSBuildTools;


namespace SolutionBuilder
{

	/// <summary>
	/// This creates a solution file.
	/// This tool searches for all project files in a given directory and then
	/// parses each file examining dependencies. It then generates a solution file
	/// with those dependencies spelled out. 
	/// This does 100% of the work of finding the dependencies, and there is no need to edit anything afterwards.
	/// </summary>
	class Program
	{
		static void PrintHelp()
		{
			Console.WriteLine("SolutionBuilder <directory> <xml build list> <output_solution_file> <configuration> <platform> [xml build list] [ProjectItemsName]");
			Console.WriteLine("<directory>            - The directory to search for .vcxproj and .csproj files. This will search recursively.");
			Console.WriteLine("<output_solution_file> - The full path to save the solution file that is generated");
			Console.WriteLine("<configuration>        - The configuration needed to build against");
			Console.WriteLine("<platform>             - The platform needed to build against");
			Console.WriteLine("[xml build list]       - Optional: The full path to an MSBuild file containing an official list of projects to build.");
			Console.WriteLine("[ProjectItemsName]     - Optional: Required if the xml build list option is specified. The item in the itemgroup for which to pull the official build list from");
		}

		static void Main(string[] args)
		{
			DirectoryInfo search_dir;
			FileInfo output_solution_file;
			String Configuration;
			String Platform;
			bool build_parallel = false;
			if ((args.Length == 2) && (String.Compare(args[1], "ProjectRefConvert", StringComparison.OrdinalIgnoreCase) == 0))
			{
				search_dir = new DirectoryInfo(args[0]);
				if (!search_dir.Exists)
				{
					PrintHelp();
					return;
				}
				var sb = new MSBuildTools.SolutionBuilder(search_dir, "AnyCPU", "Debug", build_parallel);
				sb.WriteProjectReferences(OperationType.SearchDirectory, "Reference");
			}
			else if (String.Compare(args[0], "ProjectRefConvert", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (args.Length == 3)
				{
					var itemsFile = new FileInfo(args[1]);
					string itemsName = args[2];
					
					if (itemsFile.Exists)
					{
						var sb = new MSBuildTools.SolutionBuilder("AnyCPU", "Debug", itemsFile, itemsName);
						sb.WriteProjectReferencesForFile();
					}
				}
			}
			else if ((args.Length >= 2) && (String.Compare(args[0], "FindOrphans", StringComparison.OrdinalIgnoreCase) == 0))
			{
				search_dir = new DirectoryInfo(args[1]);
				if (!search_dir.Exists)
				{
					PrintHelp();
					return;
				}
				var finder = new MSBuildTools.FindOrphans(search_dir);
				int count = finder.Find();
				Console.WriteLine("Found {0} orphaned files", count);
				if ((args.Length == 3) && (String.Compare(args[2], "fix", StringComparison.OrdinalIgnoreCase) == 0))
				{
					finder.FixItemLists();
				}
			}
			else if (args.Length == 4)
			{
				search_dir = new DirectoryInfo(args[0]);
				if (!search_dir.Exists)
				{
					PrintHelp();
					return;
				}
				output_solution_file = new FileInfo(args[1]);
				Configuration = args[2];
				Platform = args[3];

				var sb = new MSBuildTools.SolutionBuilder(search_dir, Platform, Configuration, build_parallel);
				sb.WriteSolution(output_solution_file.FullName, false);
				sb.WriteDGML(search_dir.FullName, Path.GetFileNameWithoutExtension(output_solution_file.FullName));
			}
			else if (args.Length == 5)
			{
				search_dir = new DirectoryInfo(args[0]);
				if (!search_dir.Exists)
				{
					PrintHelp();
					return;
				}

				output_solution_file = new FileInfo(args[1]);
				Configuration = args[2];
				Platform = args[3];
				FileInfo build_list = new FileInfo(args[4]);

				if (!build_list.Exists)
				{
					Console.WriteLine("Error! The file {0} does NOT exist.", build_list.FullName);
					PrintHelp();
					return;
				}

				var sb = new MSBuildTools.SolutionBuilder(search_dir, Platform, Configuration, build_list);
				sb.WriteSolution(output_solution_file.FullName, false);
				sb.WriteDGML(search_dir.FullName, Path.GetFileNameWithoutExtension(output_solution_file.FullName));
			}
			else if (args.Length == 6)
			{
				search_dir = new DirectoryInfo(args[0]);
				if (!search_dir.Exists)
				{
					PrintHelp();
					return;
				}

				output_solution_file    = new FileInfo(args[1]);
				Configuration           = args[2];
				Platform                = args[3];
				FileInfo xml_build_list = new FileInfo(args[4]);
				String ProjectsItemName = args[5];

				if (!xml_build_list.Exists)
				{
					Console.WriteLine("Error! The file {0} does NOT exist.", xml_build_list.FullName);
					PrintHelp();
					return;
				}

				var sb = new MSBuildTools.SolutionBuilder(search_dir, Platform, Configuration, xml_build_list, ProjectsItemName, build_parallel);
				sb.WriteSolution(output_solution_file.FullName, false);
				sb.WriteDGML(search_dir.FullName, Path.GetFileNameWithoutExtension(output_solution_file.FullName));
			}
			else
			{
				PrintHelp();
				return;
			}
		}
	}

}
