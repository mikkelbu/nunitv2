// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;

namespace NUnit.Core
{
	/// <summary>
	/// TestFixture is a surrogate for a user test fixture class,
	/// containing one or more tests.
	/// </summary>
	public class TestFixture : TestSuite
	{
		#region Constructors
        public TestFixture(Type fixtureType)
            : base(fixtureType) { }
        public TestFixture(Type fixtureType, object[] arguments)
            : base(fixtureType, arguments) { }
        #endregion

		#region TestSuite Overrides

        /// <summary>
        /// Gets a string representing the kind of test
        /// that this object represents, for use in display.
        /// </summary>
        public override string TestType
        {
            get { return "TestFixture"; }
        }

        public override TestResult Run(EventListener listener, ITestFilter filter)
        {
            using ( new DirectorySwapper( AssemblyHelper.GetDirectoryName( FixtureType.Assembly ) ) )
            {
                return base.Run(listener, filter);
            }
        }
		#endregion
	}
}
