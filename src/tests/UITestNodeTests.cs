#region Copyright (c) 2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
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
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using NUnit.Core;
using NUnit.Framework;
using NUnit.Util;
using NUnit.Tests.Assemblies;

namespace NUnit.Tests.Util
{
	/// <summary>
	/// Summary description for TestInfoTests.
	/// </summary>
	[TestFixture]	
	public class UITestNodeTests
	{
		TestSuite testSuite;
		TestSuite testFixture;
		NUnit.Core.TestCase testCase1;

		[SetUp]
		public void SetUp()
		{
			MockTestFixture mock = new MockTestFixture();
			testSuite = new TestSuite("MyTestSuite");
			testSuite.Add( mock );

			testFixture = (TestSuite)testSuite.Tests[0];

			testCase1 = (NUnit.Core.TestCase)testFixture.Tests[0];
		}

		[Test]
		public void Construction()
		{
			UITestNode test1 = new UITestNode( testSuite );
			Assert.Equals( "MyTestSuite", test1.Name );
			Assert.Equals( "MyTestSuite", test1.FullName );
			Assert.True( "ShouldRun", test1.ShouldRun );
			Assert.True( "IsSuite", test1.IsSuite );
			Assert.False( "!IsTestCase", test1.IsTestCase );
			Assert.False( "!IsFixture", test1.IsFixture );
			Assert.Equals( 5, test1.CountTestCases );

			UITestNode test2 = new UITestNode( testFixture, true );
			Assert.Equals( "MockTestFixture", test2.Name );
			Assert.Equals( "NUnit.Tests.Assemblies.MockTestFixture", test2.FullName );
			Assert.True( "ShouldRun", test2.ShouldRun );
			Assert.True( "IsSuite", test2.IsSuite );
			Assert.False( "!IsTestCase", test2.IsTestCase );
			Assert.True( "IsFixture", test2.IsFixture );
			Assert.Equals( 5, test2.CountTestCases );

			UITestNode test3 = new UITestNode( testCase1 );
			Assert.Equals( "MockTest1", test3.Name );
			Assert.Equals( "NUnit.Tests.Assemblies.MockTestFixture.MockTest1", test3.FullName );
			Assert.True( "ShouldRun", test3.ShouldRun );
			Assert.False( "!IsSuite", test3.IsSuite );
			Assert.True( "IsTestCase", test3.IsTestCase );
			Assert.False( "!IsFixture", test3.IsFixture );
			Assert.Equals( 1, test3.CountTestCases );
		}

		[Test]
		public void PopulateTests()
		{
			UITestNode test1 = new UITestNode( testSuite );
			Assert.False( "Default should be to not populate", test1.Populated );
			test1.PopulateTests();
			Assert.True( "Should be populated after call", test1.Populated );
			UITestNode test1Child = test1.Tests[0] as UITestNode;
			Assert.True( "Child should be populated", test1Child.Populated );

			UITestNode test2 = new UITestNode( testSuite, true );
			Assert.True( "Should be populated initialy", test2.Populated );
			UITestNode test2Child = test2.Tests[0] as UITestNode;
			Assert.True( "Child should be populated", test2Child.Populated );

			UITestNode test3 = new UITestNode( testSuite );
			int count = test3.CountTestCases;
			Assert.True( "CountTestCases should populate", test3.Populated );

			UITestNode test4 = new UITestNode( testSuite );
			UITestNode test4Child = test4.Tests[0] as UITestNode;
			Assert.True( "Accessing Tests should populate", test4.Populated );

			UITestNode test5 = new UITestNode( testSuite );
			bool fixture = test5.IsFixture;
			Assert.True( "IsFixture should populate", test5.Populated );
		}

		[Test]
		public void Conversion()
		{
			UITestNode test1 = testSuite;
			Assert.Equals( "MyTestSuite", test1.Name );
			Assert.Equals( "MyTestSuite", test1.FullName );
			Assert.True( "ShouldRun", test1.ShouldRun );
			Assert.True( "IsSuite", test1.IsSuite );
			Assert.False( "!IsTestCase", test1.IsTestCase );
			Assert.False( "!IsFixture", test1.IsFixture );
			Assert.Equals( 5, test1.CountTestCases );

			UITestNode test2 = testFixture;
			Assert.Equals( "MockTestFixture", test2.Name );
			Assert.Equals( "NUnit.Tests.Assemblies.MockTestFixture", test2.FullName );
			Assert.True( "ShouldRun", test2.ShouldRun );
			Assert.True( "IsSuite", test2.IsSuite );
			Assert.False( "!IsTestCase", test2.IsTestCase );
			Assert.True( "IsFixture", test2.IsFixture );
			Assert.Equals( 5, test2.CountTestCases );

			UITestNode test3 = testCase1;
			Assert.Equals( "MockTest1", test3.Name );
			Assert.Equals( "NUnit.Tests.Assemblies.MockTestFixture.MockTest1", test3.FullName );
			Assert.True( "ShouldRun", test3.ShouldRun );
			Assert.False( "!IsSuite", test3.IsSuite );
			Assert.True( "IsTestCase", test3.IsTestCase );
			Assert.False( "!IsFixture", test3.IsFixture );
			Assert.Equals( 1, test3.CountTestCases );
		}
	}
}
