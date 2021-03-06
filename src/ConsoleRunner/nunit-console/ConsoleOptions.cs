// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

namespace NUnit.ConsoleRunner
{
	using System;
    using System.Collections.Generic;
	using Codeblast;
	using NUnit.Util;
    using NUnit.Core;

	public class ConsoleOptions : CommandLineOptions
	{
		[Option(Short="load", Description = "Test fixture or namespace to be loaded (Deprecated)")]
		public string fixture;

        // NOTE: Here and further below we are using a trick to allow use of both old and
        // new versions of the option. The option with the OptionAttribute will be listed
        // in the help and is the official, new option. The option that is simply defined
        // as a public field, without an attribute, will be used if found but not shown
        // in any help message. This allows us to separate the two forms and produce a
        // compatibility issue if the old form is used.
        //
        // We are doing this for:
        //    test vs run
        //    testlist vs runlist
        //    result vs xml
        //    noresult vs noxml

        [Option(Description = "Name of the test case(s), fixture(s) or namespace(s) to run")]
        public string test;
		public string run;

        [Option(Description = "Name of a file containing a list of the tests to run, one per line")]
        public string testlist;
        public string runlist;

		[Option(Description = "Project configuration (e.g.: Debug) to load")]
		public string config;

		[Option(Description = "Name of XML result file (Default: TestResult.xml)")]
		public string result;
        public string xml;

		[Option(Description = "Display XML to the console (Deprecated)")]
		public bool xmlConsole;

        [Option(Description = "Suppress XML result output")]
        public bool noresult;
        public bool noxml;

		[Option(Short="out", Description = "File to receive test output")]
		public string output;

		[Option(Description = "File to receive test error output")]
		public string err;

        [Option(Description = "Work directory for output files")]
        public string work;

		[Option(Description = "Label each test in stdOut")]
		public bool labels = false;

        [Option(Description = "Set internal trace level: Off, Error, Warning, Info, Verbose")]
        public InternalTraceLevel trace;

		[Option(Description = "List of categories to include")]
		public string include;

		[Option(Description = "List of categories to exclude")]
		public string exclude;

        [Option(Short = "compat", Description = "Produce NUnit3 Compatibility Report")]
        public bool compatibility;

#if CLR_2_0 || CLR_4_0
        [Option(Description = "Framework version to be used for tests")]
        public string framework;

        [Option(Description = "Process model for tests: Single, Separate, Multiple")]
		public ProcessModel process;
#endif

		[Option(Description = "AppDomain Usage for tests: None, Single, Multiple")]
		public DomainUsage domain;

        [Option(Description = "Apartment for running tests: MTA (Default), STA")]
        public System.Threading.ApartmentState apartment = System.Threading.ApartmentState.Unknown;

        [Option(Description = "Disable shadow copy when running in separate domain")]
		public bool noshadow;

		[Option (Description = "Disable use of a separate thread for tests")]
		public bool nothread;

		[Option(Description = "Base path to be used when loading the assemblies")]
 		public string basepath;
 
 		[Option(Description = "Additional directories to be probed when loading assemblies, separated by semicolons")]
 		public string privatebinpath;

        [Option(Description = "Set timeout for each test case in milliseconds")]
        public int timeout;

		[Option(Description = "Wait for input before closing console window")]
		public bool wait = false;

		[Option(Description = "Do not display the logo")]
		public bool nologo = false;

		[Option(Description = "Do not display progress" )]
		public bool nodots = false;

        [Option(Description = "Stop after the first test failure or error")]
        public bool stoponerror = false;

        [Option(Description = "Erase any leftover cache files and exit")]
        public bool cleanup;

        [Option(Short = "?", Description = "Display help")]
		public bool help = false;

		public ConsoleOptions( params string[] args ) : base( args ) {}

		public ConsoleOptions( bool allowForwardSlash, params string[] args ) : base( allowForwardSlash, args ) {}

		public bool Validate()
		{
			if(isInvalid) return false; 

			if(NoArgs) return true; 

			if(ParameterCount >= 1) return true; 

			return false;
		}

//		protected override bool IsValidParameter(string parm)
//		{
//			return Services.ProjectLoadService.CanLoadProject( parm ) || PathUtils.IsAssemblyFileType( parm );
//		}


        public bool IsTestProject
        {
            get
            {
                return ParameterCount == 1 && Services.ProjectService.CanLoadProject((string)Parameters[0]);
            }
        }

        public IEnumerable<Compatibility.Issue> CompatibilityIssues
        {
            get
            {
                if (process == ProcessModel.Default)
                    yield return new Issue("Warning", "The --process option defaults to Multiple in NUnit 3.");
                else // No point in giving both warnings
                if (domain == DomainUsage.Default)
                    yield return new Issue("Warning", "The --domain option defaults to Multiple in NUnit 3.");               

                if (fixture != null)
                    yield return new Issue("Error", "The --fixture option is no longer supported in NUnit 3. Use --test or --where to filter tests at the time of execution instead.");

                if (run != null)
                    yield return new Issue("Error", "The --run option is no longer supported in NUnit 3. Use --test or --where.");

                if (runlist != null)
                    yield return new Issue("Error", "The --runlist option is no longer supported in NUnit 3. Use --testlist.");

                if (include != null)
                    yield return new Issue("Error", "The --include option is no longer supported in NUnit 3. Use --where.");

                if (exclude != null)
                    yield return new Issue("Error", "The --exclude option is no longer supported in NUnit 3. Use --where.");

                if (apartment != System.Threading.ApartmentState.Unknown)
                    yield return new Issue("Error", "The --apartment option is no longer supported in NUnit 3. Use ApartmentAttribute");

                if (xml != null)
                    yield return new Issue("Error", "The --xml option is no longer supported in NUnit 3. Use --result.");

                if (noxml)
                    yield return new Issue("Error", "The --noxml option is no longer supported in NUnit 3. Use --noresult.");

                if (xmlConsole)
                    yield return new Issue("Error", "The --xmlConsole option is no longer supported in NUnit 3.");

                if (basepath != null)
                    yield return new Issue("Error", "The --basepath option is no longer supported in NUnit 3.");

                if (privatebinpath != null)
                    yield return new Issue("Error", "The --privatebinpath option is no longer supported in NUnit 3.");

                if (cleanup)
                    yield return new Issue("Warning", "The --cleanup option is no longer supported or needed in NUnit 3.");

                if (nodots)
                    yield return new Issue("Warning", "The --nodots option is no longer supported or needed in NUnit 3.");

            }
        }

        private class Issue : Compatibility.Issue
        {
            public Issue(string level, string message)
                : base(level, "Console Command-line", message) { }
        }

        private string UnsupportedOption(string name)
        {
            return string.Format("Option --{0} not supported in NUnit 3.", name);
        }

		public override void Help()
		{
			Console.WriteLine();
			Console.WriteLine( "NUNIT-CONSOLE [inputfiles] [options]" );
			Console.WriteLine();
			Console.WriteLine( "Runs a set of NUnit tests from the console." );
			Console.WriteLine();
			Console.WriteLine( "You may specify one or more assemblies or a single" );
			Console.WriteLine( "project file of type .nunit." );
			Console.WriteLine();
			Console.WriteLine( "Options:" );
			base.Help();
			Console.WriteLine();
			Console.WriteLine( "Options that take values may use an equal sign, a colon" );
			Console.WriteLine( "or a space to separate the option from its value." );
			Console.WriteLine();
		}
	}
}