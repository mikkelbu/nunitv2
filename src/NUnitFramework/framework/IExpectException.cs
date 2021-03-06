// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;

namespace NUnit.Framework
{
    /// <summary>
    /// Interface implemented by a user fixture in order to
    /// validate any expected exceptions. It is only called
    /// for test methods marked with the ExpectedException
    /// attribute.
    /// </summary>
	public interface IExpectException
    {
		/// <summary>
		/// Method to handle an expected exception
		/// </summary>
		/// <param name="ex">The exception to be handled</param>
        void HandleException(Exception ex);
    }
}
