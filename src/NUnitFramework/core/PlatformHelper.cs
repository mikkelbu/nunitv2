using System;
using System.Collections;
using System.Reflection;

namespace NUnit.Core
{
	public class PlatformHelper
	{
		private OperatingSystem os;
		private RuntimeFramework rt;

		private static readonly string PlatformType = "NUnit.Framework.PlatformAttribute";

		/// <summary>
		/// Comma-delimited list of all supported OS platform constants
		/// </summary>
		public static readonly string OSPlatforms =
			"Win,Win32,Win32S,Win32NT,Win32Windows,WinCE,Win95,Win98,WinMe,NT3,NT4,NT5,Win2K,WinXP,Unix,Linux";
		
		/// <summary>
		/// Comma-delimited list of all supported Runtime platform constants
		/// </summary>
		public static readonly string RuntimePlatforms =
			"Net,NetCF,SSCLI,Rotor,Mono";

		/// <summary>
		/// Default constructor uses the operating system and
		/// common language runtime of the system.
		/// </summary>
		public PlatformHelper()
		{
			this.os = Environment.OSVersion;
			this.rt = RuntimeFramework.CurrentFramework;
		}

		/// <summary>
		/// Contruct a PlatformHelper for a particular operating
		/// system and common language runtime. Used in testing.
		/// </summary>
		/// <param name="os">OperatingSystem to be used</param>
		/// <param name="framework">RuntimeFramework to be used</param>
		public PlatformHelper( OperatingSystem os, RuntimeFramework rt )
		{
			this.os = os;
			this.rt = rt;
		}

		/// <summary>
		/// Test to determine if one of a collection of platforms
		/// is being used currently.
		/// </summary>
		/// <param name="platforms"></param>
		/// <returns></returns>
		public bool IsPlatformSupported( string[] platforms )
		{
			foreach( string platform in platforms )
				if ( IsPlatformSupported( platform ) )
					return true;

			return false;
		}

		/// <summary>
		/// Tests to determine if the current platform is supported
		/// based on a platform attribute.
		/// </summary>
		/// <param name="platformAttribute">The attribute to examine</param>
		/// <returns></returns>
		public bool IsPlatformSupported( Attribute platformAttribute )
		{
			//Use reflection to avoid dependency on a particular framework version
			string include = (string)Reflect.GetPropertyValue( 
				platformAttribute, "Include", 
				BindingFlags.Public | BindingFlags.Instance );

			string exclude = (string)Reflect.GetPropertyValue(
				platformAttribute, "Exclude", 
				BindingFlags.Public | BindingFlags.Instance );

			if ( include != null && !IsPlatformSupported( include ) )
				return false;

			if ( exclude != null && IsPlatformSupported( exclude ) )
				return false;

			return true;
		}

		/// <summary>
		/// Tests whether a particular member is supported on the
		/// current platform.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public bool IsPlatformSupported( MemberInfo member )
		{
			Attribute platformAttribute = 
				Reflect.GetAttribute( member, PlatformType, false );
			
			return platformAttribute == null
				|| IsPlatformSupported( platformAttribute );
		}

		/// <summary>
		/// Test to determine if the a particular platform or comma-
		/// delimited set of platforms is in use.
		/// </summary>
		/// <param name="platform">Name of the platform or comma-separated list of platfomr names</param>
		/// <returns>True if the platform is in use on the system</returns>
		public bool IsPlatformSupported( string platform )
		{
			if ( platform.IndexOf( ',' ) >= 0 )
				return IsPlatformSupported( platform.Split( new char[] { ',' } ) );

			string platformName = platform.ToUpper().Trim();
			bool nameOK = false;

			string versionSpecification = null;

			string[] parts = platformName.Split( new char[] { '-' } );
			if ( parts.Length == 2 )
			{
				platformName = parts[0];
				versionSpecification = parts[1];
			}

			switch( platformName.ToUpper() )
			{
				case "WIN":
				case "WIN32":
					nameOK = os.Platform.ToString().StartsWith( "Win" );
					break;
				case "WIN32S":
					nameOK = os.Platform == PlatformID.Win32S;
					break;
				case "WIN32WINDOWS":
					nameOK = os.Platform == PlatformID.Win32Windows;
					break;
				case "WIN32NT":
					nameOK = os.Platform == PlatformID.Win32NT;
					break;
				case "WINCE":
					nameOK = (int)os.Platform == 3;  // Not defined in .NET 1.0
					break;
				case "WIN95":
					nameOK = os.Platform == PlatformID.Win32Windows && os.Version.Major == 4 && os.Version.Minor == 0;
					break;
				case "WIN98": 
					nameOK = os.Platform == PlatformID.Win32Windows && os.Version.Major == 4 && os.Version.Minor == 10;
					break;
				case "WINME":
					nameOK = os.Platform == PlatformID.Win32Windows && os.Version.Major == 4 && os.Version.Minor == 90;
					break;
				case "NT3":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 3;
					break;
				case "NT4":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 4;
					break;
				case "NT5":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 5;
					break;
				case "WIN2K":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 5 && os.Version.Minor == 0;
					break;
				case "WINXP":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 5 && os.Version.Minor == 1;
					break;
				case "WIN2003SERVER":
					nameOK = os.Platform == PlatformID.Win32NT && os.Version.Major == 5 && os.Version.Minor == 2;
					break;
				case "UNIX":
				case "LINUX":
					nameOK = (int)os.Platform == 128;  // Not defined in .NET 1.0 or 1.1
					break;
				case "NET":
					nameOK = rt.Runtime == RuntimeType.Net;
					break;
				case "NETCF":
					nameOK = rt.Runtime == RuntimeType.NetCF;
					break;
				case "SSCLI":
				case "ROTOR":
					nameOK = rt.Runtime == RuntimeType.SSCLI;
					break;
				case "MONO":
					nameOK = rt.Runtime == RuntimeType.Mono;
					// Special handling because Mono 1.0 profile has version 1.1
					if ( versionSpecification == "1.0" )
						versionSpecification = "1.1";
					break;
				default:
					throw new ArgumentException( "Invalid platform name", platform.ToString() );
			}

			if ( !nameOK ) 
				return false;

			if ( versionSpecification == null )
				return true;

			Version version = new Version( versionSpecification );

			if ( rt.Version.Major != version.Major )
				return false;
			
			if ( rt.Version.Minor != version.Minor )
				return false;

			if ( version.Build != -1 && rt.Version.Build != version.Build )
				return false;

			if ( version.Revision != -1 && rt.Version.Revision != version.Revision )
				return false;

			return true;
		}
	}
}
