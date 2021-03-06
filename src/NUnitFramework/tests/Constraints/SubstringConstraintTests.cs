// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;

namespace NUnit.Framework.Constraints
{
    [TestFixture]
    public class SubstringTest : ConstraintTestBase, IExpectException
    {
		[SetUp]
        public void SetUp()
        {
            theConstraint = new SubstringConstraint("hello");
            expectedDescription = "String containing \"hello\"";
            stringRepresentation = "<substring \"hello\">";
        }

        internal object[] SuccessData = new object[] { "hello", "hello there", "I said hello", "say hello to fred" };
        
        internal object[] FailureData = new object[] { "goodbye", "HELLO", "What the hell?", string.Empty, null };

        internal string[] ActualValues = new string[] { "\"goodbye\"", "\"HELLO\"", "\"What the hell?\"", "<string.Empty>", "null" }; 

        public void HandleException(Exception ex)
        {
            Assert.That(ex.Message, new EqualConstraint(
                TextMessageWriter.Pfx_Expected + "String containing \"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa...\"" + Environment.NewLine +
                TextMessageWriter.Pfx_Actual   + "\"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx...\"" + Environment.NewLine));
        }
    }

    [TestFixture]
    public class SubstringTestIgnoringCase : ConstraintTestBase
    {
		[SetUp]
        public void SetUp()
        {
            theConstraint = new SubstringConstraint("hello").IgnoreCase;
            expectedDescription = "String containing \"hello\", ignoring case";
            stringRepresentation = "<substring \"hello\">";
        }

        internal object[] SuccessData = new object[] { "Hello", "HellO there", "I said HELLO", "say hello to fred" };
        
        internal object[] FailureData = new object[] { "goodbye", "What the hell?", string.Empty, null };

        internal string[] ActualValues = new string[] { "\"goodbye\"", "\"What the hell?\"", "<string.Empty>", "null" };
    }

    //[TestFixture]
    //public class EqualIgnoringCaseTest : ConstraintTest
    //{
    //    [SetUp]
    //    public void SetUp()
    //    {
    //        Matcher = new EqualConstraint("Hello World!").IgnoreCase;
    //        Description = "\"Hello World!\", ignoring case";
    //    }

    //    object[] SuccessData = new object[] { "hello world!", "Hello World!", "HELLO world!" };
            
    //    object[] FailureData = new object[] { "goodbye", "Hello Friends!", string.Empty, null };

    //    string[] ActualValues = new string[] { "\"goodbye\"", "\"Hello Friends!\"", "<string.Empty>", "null" };
    //}
}
