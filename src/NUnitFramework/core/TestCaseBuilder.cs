#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright � 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright � 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright � 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Configuration;

	/// <summary>
	/// Summary description for TestCaseBuilder.
	/// </summary>
	public class TestCaseBuilder
	{
		private static readonly string TestType = "NUnit.Framework.TestAttribute";
		private static readonly string IgnoreType = "NUnit.Framework.IgnoreAttribute";
		private static readonly string ExplicitType = "NUnit.Framework.ExplicitAttribute";
		private static readonly string CategoryType = "NUnit.Framework.CategoryAttribute";
		private static readonly string PlatformType = "NUnit.Framework.PlatformAttribute";

		private static Hashtable builders;
		private static NormalBuilder normalBuilder = new NormalBuilder();

		private static bool allowOldStyleTests;

		private static void InitBuilders() 
		{
			builders = new Hashtable();
			builders["NUnit.Framework.ExpectedExceptionAttribute"] = new ExpectedExceptionBuilder();
		}

		static TestCaseBuilder()
		{
			NameValueCollection settings = (NameValueCollection)
				ConfigurationSettings.GetConfig( "NUnit/TestCaseBuilder" );

			try
			{
				string oldStyle = settings["OldStyleTestCases"];
				if ( oldStyle != null )
					allowOldStyleTests = Boolean.Parse( oldStyle );
			}
			catch
			{
				// Use default values
			}
		}

		private static TestCase MakeTestCase(Type fixtureType, MethodInfo method) 
		{
			if (builders == null)
				InitBuilders();

			object[] attributes = method.GetCustomAttributes(false);
			
			foreach (object attribute in attributes) 
			{
				ITestBuilder builder = GetBuilder(attribute);
				if (builder != null)
					return builder.Make (fixtureType, method, attribute);
			}

			return normalBuilder.Make (fixtureType, method);
		}

		private static ITestBuilder GetBuilder(object attribute)
		{
			Type attributeType = attribute.GetType();
			ITestBuilder builder = (ITestBuilder) builders[attribute.GetType().FullName];
			if (builder == null)
			{
				object[] attributes = attributeType.GetCustomAttributes(typeof(TestBuilderAttribute), false);
				if (attributes != null && attributes.Length > 0)
				{
					TestBuilderAttribute builderAttribute = (TestBuilderAttribute) attributes[0];
					Type builderType = builderAttribute.BuilderType;
					builder = (ITestBuilder) Activator.CreateInstance(builderType);
					builders[attributeType.FullName] = builder;
				}
			}

			return builder;
		}

		/// <summary>
		/// Make a test case from a given fixture type and method
		/// </summary>
		/// <param name="fixtureType">The fixture type</param>
		/// <param name="method">MethodInfo for the particular method</param>
		/// <returns>A test case or null</returns>
		public static TestCase Make(Type fixtureType, MethodInfo method)
		{
			TestCase testCase = null;

			Attribute testAttribute = Reflect.GetAttribute( method, TestType, false );
			if( testAttribute != null || allowOldStyleTests && IsObsoleteTestMethod( method ) )
			{
				if (!method.IsStatic &&
					!method.IsAbstract &&
					 method.IsPublic &&
					 method.GetParameters().Length == 0 &&
					 method.ReturnType.Equals(typeof(void) ) )
				{
					testCase = MakeTestCase(fixtureType, method);

					Attribute platformAttribute = 
						Reflect.GetAttribute( method, PlatformType, false );
					if ( platformAttribute != null )
					{
						PlatformHelper helper = new PlatformHelper();
						if ( !helper.IsPlatformSupported( platformAttribute ) )
						{
							testCase.ShouldRun = false;
							testCase.IgnoreReason = "Not running on correct platform";
						}
					}

					Attribute ignoreAttribute =
						Reflect.GetAttribute( method, IgnoreType, false );
					if ( ignoreAttribute != null )
					{
						testCase.ShouldRun = false;
						testCase.IgnoreReason = (string)
							Reflect.GetPropertyValue( 
								ignoreAttribute, 
								"Reason",
								BindingFlags.Public | BindingFlags.Instance );
					}

					Attribute[] categoryAttributes = 
						Reflect.GetAttributes( method, CategoryType, false );
					if ( categoryAttributes.Length > 0 )
					{
						ArrayList categories = new ArrayList();
						foreach( Attribute categoryAttribute in categoryAttributes) 
						{
							string category = (string)
								Reflect.GetPropertyValue( 
									categoryAttribute, 
									"Name",
									BindingFlags.Public | BindingFlags.Instance );
							CategoryManager.Add( category );
							categories.Add( category );
						}
						testCase.Categories = categories;
					}

					testCase.IsExplicit = Reflect.HasAttribute( method, ExplicitType, false );
				
					if ( testAttribute != null )
						testCase.Description = (string)Reflect.GetPropertyValue( 
							testAttribute, 
							"Description", 
							BindingFlags.Public | BindingFlags.Instance );
				}
				else
				{
					testCase = new NotRunnableTestCase(method);
				}
			}

			return testCase;
		}

		private static bool IsObsoleteTestMethod(MethodInfo methodToCheck)
		{
			if ( methodToCheck.Name.ToLower().StartsWith("test") )
			{
				object[] attributes = methodToCheck.GetCustomAttributes( false );

				foreach( Attribute attribute in attributes )
				{
					string typeName = attribute.GetType().FullName;
					if( typeName == "NUnit.Framework.SetUpAttribute" ||
						typeName == "NUnit.Framework.TestFixtureSetUpAttribute" ||
						typeName == "NUnit.Framework.TearDownAttribute" || 
						typeName == "NUnit.Framework.TestFixtureTearDownAttribute" )
					{
						return false;
					}
				}

				return true;	
			}

			return false;
		}
	}
}

