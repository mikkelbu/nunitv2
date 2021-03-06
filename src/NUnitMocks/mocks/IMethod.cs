// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The IMethod interface represents an method or other named object that 
	/// is both callable and self-verifying.
	/// </summary>
    [Obsolete("NUnit now uses NSubstitute")]
    public interface IMethod : IVerify, ICall
	{
		/// <summary>
		/// The name of the object
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Tell the object to expect a certain call.
		/// </summary>
		/// <param name="call"></param>
		void Expect( ICall call );
	}
}
