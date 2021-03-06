// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// Class adapted from NUnitAddin for use in handling assemblies that are not
    /// found in the test AppDomain.
	/// </summary>
    public class AssemblyResolver : MarshalByRefObject, IDisposable
	{
		static Logger log = InternalTrace.GetLogger(typeof(AssemblyResolver));

		private class AssemblyCache
		{
			private Hashtable _resolved = new Hashtable();

			public bool Contains( string name )
			{
				return _resolved.ContainsKey( name );
			}

			public Assembly Resolve( string name )
			{
				if ( _resolved.ContainsKey( name ) )
					return (Assembly)_resolved[name];
				
				return null;
			}

			public void Add( string name, Assembly assembly )
			{
				_resolved[name] = assembly;
			}
		}

		private AssemblyCache _cache = new AssemblyCache();

		private ArrayList _dirs = new ArrayList();

		public AssemblyResolver()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		public void AddFile( string file )
		{
			Assembly assembly = Assembly.LoadFrom( file );
			_cache.Add(assembly.GetName().FullName, assembly);
		}

		public void AddFiles( string directory, string pattern )
		{
			if ( Directory.Exists( directory ) )
				foreach( string file in Directory.GetFiles( directory, pattern ) )
					AddFile( file );
		}

		public void AddDirectory( string directory )
		{
			if ( Directory.Exists( directory ) )
				_dirs.Add( directory );
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string fullName = args.Name;
			int index = fullName.IndexOf(',');
			if(index == -1)							// Only resolve using full name.
			{
				log.Debug( string.Format("Not a strong name: {0}", fullName ) );
				return null;
			}

			if ( _cache.Contains( fullName ) )
			{
				log.Info( string.Format( "Resolved from Cache: {0}", fullName ) );
				return _cache.Resolve(fullName);
			}

			foreach( string dir in _dirs )
			{
				foreach( string file in Directory.GetFiles( dir, "*.dll" ) )
				{
					string fullFile = Path.Combine( dir, file );
                    using (AssemblyReader rdr = new AssemblyReader(fullFile))
                    {
                        try
                        {
                            if (rdr.IsDotNetFile)
                            {
                                if (AssemblyName.GetAssemblyName(fullFile).FullName == fullName)
                                {
                                    log.Info(string.Format("Added to Cache: {0}", fullFile));
                                    AddFile(fullFile);
                                    return _cache.Resolve(fullName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Unable to load addin assembly", ex);
                            throw;
                        }
                    }
				}
			}

			log.Debug( string.Format( "Not in Cache: {0}", fullName) ); 
			return null;
		}
	}
}
