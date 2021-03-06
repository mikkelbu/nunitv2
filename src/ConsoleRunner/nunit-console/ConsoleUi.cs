// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System.Diagnostics;

namespace NUnit.ConsoleRunner
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Resources;
	using System.Text;
	using NUnit.Core;
	using NUnit.Core.Filters;
	using NUnit.Util;
	
	/// <summary>
	/// Summary description for ConsoleUi.
	/// </summary>
	public class ConsoleUi
	{
		public static readonly int OK = 0;
		public static readonly int INVALID_ARG = -1;
		public static readonly int FILE_NOT_FOUND = -2;
		public static readonly int FIXTURE_NOT_FOUND = -3;
		public static readonly int UNEXPECTED_ERROR = -100;

        private string workDir;

		public ConsoleUi()
		{
		}

		public int Execute( ConsoleOptions options )
		{
            this.workDir = options.work;
            if (workDir == null || workDir == string.Empty)
                workDir = Environment.CurrentDirectory;
            else
            {
                workDir = Path.GetFullPath(workDir);
                if (!Directory.Exists(workDir))
                    Directory.CreateDirectory(workDir);
            }

			TextWriter outWriter = Console.Out;
			bool redirectOutput = options.output != null && options.output != string.Empty;
			if ( redirectOutput )
			{
				StreamWriter outStreamWriter = new StreamWriter( Path.Combine(workDir, options.output) );
				outStreamWriter.AutoFlush = true;
				outWriter = outStreamWriter;
			}

			TextWriter errorWriter = Console.Error;
			bool redirectError = options.err != null && options.err != string.Empty;
			if ( redirectError )
			{
				StreamWriter errorStreamWriter = new StreamWriter( Path.Combine(workDir, options.err) );
				errorStreamWriter.AutoFlush = true;
				errorWriter = errorStreamWriter;
			}

            TestPackage package = MakeTestPackage(options);

            ProcessModel processModel = package.Settings.Contains("ProcessModel")
                ? (ProcessModel)package.Settings["ProcessModel"]
                : ProcessModel.Default;

            DomainUsage domainUsage = package.Settings.Contains("DomainUsage")
                ? (DomainUsage)package.Settings["DomainUsage"]
                : DomainUsage.Default;

            RuntimeFramework framework = package.Settings.Contains("RuntimeFramework")
                ? (RuntimeFramework)package.Settings["RuntimeFramework"]
                : RuntimeFramework.CurrentFramework;

#if CLR_2_0 || CLR_4_0
            Console.WriteLine("ProcessModel: {0}    DomainUsage: {1}", processModel, domainUsage);

            Console.WriteLine("Execution Runtime: {0}", framework);
#else
            Console.WriteLine("DomainUsage: {0}", domainUsage);

            if (processModel != ProcessModel.Default && processModel != ProcessModel.Single)
                Console.WriteLine("Warning: Ignoring project setting 'processModel={0}'", processModel);

            if (!RuntimeFramework.CurrentFramework.Supports(framework))
                Console.WriteLine("Warning: Ignoring project setting 'runtimeFramework={0}'", framework);
#endif

            using (TestRunner testRunner = new DefaultTestRunnerFactory().MakeTestRunner(package))
			{
                if (options.compatibility)
                    Compatibility.Initialize(workDir);

                testRunner.Load(package);

                if (testRunner.Test == null)
				{
					testRunner.Unload();
					Console.Error.WriteLine("Unable to locate fixture {0}", options.fixture);
					return FIXTURE_NOT_FOUND;
				}

				EventCollector collector = new EventCollector( options, outWriter, errorWriter );

				TestFilter testFilter;
					
				if(!CreateTestFilter(options, out testFilter))
					return INVALID_ARG;

				TestResult result = null;
				string savedDirectory = Environment.CurrentDirectory;
				TextWriter savedOut = Console.Out;
				TextWriter savedError = Console.Error;

				try
				{
					result = testRunner.Run( collector, testFilter, true, LoggingThreshold.Off );
				}
				finally
				{
					outWriter.Flush();
					errorWriter.Flush();

					if (redirectOutput)
						outWriter.Close();

					if (redirectError)
						errorWriter.Close();

					Environment.CurrentDirectory = savedDirectory;
					Console.SetOut( savedOut );
					Console.SetError( savedError );
				}

				Console.WriteLine();

                int returnCode = UNEXPECTED_ERROR;

                if (result != null)
                {
                    string xmlOutput = CreateXmlOutput(result);
                    if (options.xmlConsole)
                    {
                        Console.WriteLine(xmlOutput);
                    }
                    else
                    {
                        new ResultReporter(result, options).ReportResults();

                        // Check both new and old option forms
                        if (!options.noresult && !options.noxml)
                        {
                            // Write xml output here
                            string xmlResultFile = !string.IsNullOrEmpty(options.result)
                                ? options.result
                                : !string.IsNullOrEmpty(options.xml)
                                    ? options.xml
                                    : "TestResult.xml";

                            using (StreamWriter writer = new StreamWriter(Path.Combine(workDir, xmlResultFile)))
                            {
                                writer.Write(xmlOutput);
                            }
                        }
                    }

                    ResultSummarizer summary = new ResultSummarizer(result);
                    returnCode = summary.Errors + summary.Failures + summary.NotRunnable;
                }

				if (collector.HasExceptions)
				{
					collector.WriteExceptions();
					returnCode = UNEXPECTED_ERROR;
				}
            
				return returnCode;
			}
		}

        internal static bool CreateTestFilter(ConsoleOptions options, out TestFilter testFilter)
		{
			testFilter = TestFilter.Empty;

			SimpleNameFilter nameFilter = new SimpleNameFilter();

			if (options.run != null && options.run != string.Empty)
			{
				Console.WriteLine("Selected test(s): " + options.run);

				foreach (string name in TestNameParser.Parse(options.run))
					nameFilter.Add(name);

				testFilter = nameFilter;
			}

            // Check both new and old options
            var testList = options.testlist;
            if (string.IsNullOrEmpty(testList))
                testList = options.runlist;

			if (!string.IsNullOrEmpty(testList))
			{
				Console.WriteLine("Run list: " + testList);
				
				try
				{
					using (StreamReader rdr = new StreamReader(testList))
					{
						// NOTE: We can't use rdr.EndOfStream because it's
						// not present in .NET 1.x.
						string line = rdr.ReadLine();
						while (line != null && line.Length > 0)
						{
							if (line[0] != '#')
								nameFilter.Add(line);
							line = rdr.ReadLine();
						}
					}
				}
				catch (Exception e)
				{
					if (e is FileNotFoundException || e is DirectoryNotFoundException)
					{
						Console.WriteLine("Unable to locate file: " + testList);
						return false;
					}
					throw;
				}

				testFilter = nameFilter;
			}

			if (options.include != null && options.include != string.Empty)
			{
				TestFilter includeFilter = new CategoryExpression(options.include).Filter;
				Console.WriteLine("Included categories: " + includeFilter.ToString());

				if (testFilter.IsEmpty)
					testFilter = includeFilter;
				else
					testFilter = new AndFilter(testFilter, includeFilter);
			}

			if (options.exclude != null && options.exclude != string.Empty)
			{
				TestFilter excludeFilter = new NotFilter(new CategoryExpression(options.exclude).Filter);
				Console.WriteLine("Excluded categories: " + excludeFilter.ToString());

				if (testFilter.IsEmpty)
					testFilter = excludeFilter;
				else if (testFilter is AndFilter)
					((AndFilter) testFilter).Add(excludeFilter);
				else
					testFilter = new AndFilter(testFilter, excludeFilter);
			}

			if (testFilter is NotFilter)
				((NotFilter) testFilter).TopLevel = true;

			return true;
		}

		#region Helper Methods
        // TODO: See if this can be unified with the Gui's MakeTestPackage
        private TestPackage MakeTestPackage( ConsoleOptions options )
        {
			TestPackage package;
			DomainUsage domainUsage = DomainUsage.Default;
            ProcessModel processModel = ProcessModel.Default;
            RuntimeFramework framework = null;

            string[] parameters = new string[options.ParameterCount];
            for (int i = 0; i < options.ParameterCount; i++)
                parameters[i] = Path.GetFullPath((string)options.Parameters[i]);

			if (options.IsTestProject)
			{
				NUnitProject project = 
					Services.ProjectService.LoadProject(parameters[0]);

				string configName = options.config;
				if (configName != null)
					project.SetActiveConfig(configName);

				package = project.ActiveConfig.MakeTestPackage();
                processModel = project.ProcessModel;
                domainUsage = project.DomainUsage;
                framework = project.ActiveConfig.RuntimeFramework;
			}
			else if (parameters.Length == 1)
			{
                package = new TestPackage(parameters[0]);
				domainUsage = DomainUsage.Single;
			}
			else
			{
                // TODO: Figure out a better way to handle "anonymous" packages
				package = new TestPackage(null, parameters);
                package.AutoBinPath = true;
				domainUsage = DomainUsage.Multiple;
			}

			if (options.basepath != null && options.basepath != string.Empty)
 			{
 				package.BasePath = options.basepath;
 			}
 
 			if (options.privatebinpath != null && options.privatebinpath != string.Empty)
 			{
 				package.AutoBinPath = false;
				package.PrivateBinPath = options.privatebinpath;
 			}

#if CLR_2_0 || CLR_4_0
            if (options.framework != null)
                framework = RuntimeFramework.Parse(options.framework);

            if (options.process != ProcessModel.Default)
                processModel = options.process;
#endif

			if (options.domain != DomainUsage.Default)
				domainUsage = options.domain;

			package.TestName = options.fixture;
            
            package.Settings["ProcessModel"] = processModel;
            package.Settings["DomainUsage"] = domainUsage;
            
			if (framework != null)
                package.Settings["RuntimeFramework"] = framework;

            if (domainUsage == DomainUsage.None)
            {
                // Make sure that addins are available
                CoreExtensions.Host.AddinRegistry = Services.AddinRegistry;
            }

            package.Settings["ShadowCopyFiles"] = !options.noshadow;
			package.Settings["UseThreadedRunner"] = !options.nothread;
            package.Settings["DefaultTimeout"] = options.timeout;
            package.Settings["WorkDirectory"] = this.workDir;
            package.Settings["StopOnError"] = options.stoponerror;
            package.Settings["NUnit3Compatibility"] = options.compatibility;

            if (options.apartment != System.Threading.ApartmentState.Unknown)
                package.Settings["ApartmentState"] = options.apartment;

            return package;
		}

		private static string CreateXmlOutput( TestResult result )
		{
			StringBuilder builder = new StringBuilder();
			new XmlResultWriter(new StringWriter( builder )).SaveTestResult(result);

			return builder.ToString();
		}

        #endregion
    }
}

