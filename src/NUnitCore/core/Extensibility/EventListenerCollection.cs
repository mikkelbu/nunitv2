// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************
using System;
using System.Collections;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// EventListenerCollection holds multiple event listeners
	/// and relays all event calls to each of them.
	/// </summary>
	public class EventListenerCollection : ExtensionPoint, EventListener
	{
		#region Constructor
		public EventListenerCollection( IExtensionHost host )
			: base( "EventListeners", host ) { }
		#endregion

		#region EventListener Members
		public void RunStarted(string name, int testCount)
		{
			foreach( EventListener listener in Extensions )
				listener.RunStarted( name, testCount );
		}

		public void RunFinished(TestResult result)
		{
			foreach( EventListener listener in Extensions )
				listener.RunFinished( result );
		}

		public void RunFinished(Exception exception)
		{
			foreach( EventListener listener in Extensions )
				listener.RunFinished( exception );
		}

		public void SuiteStarted(TestName testName)
		{
			foreach( EventListener listener in Extensions )
				listener.SuiteStarted( testName );
		}

		public void SuiteFinished(TestResult result)
		{
			foreach( EventListener listener in Extensions )
				listener.SuiteFinished( result );
		}

		public void TestStarted(TestName testName)
		{
			foreach( EventListener listener in Extensions )
				listener.TestStarted( testName );
		}

		public void TestFinished(TestResult result)
		{
			foreach( EventListener listener in Extensions )
				listener.TestFinished( result );
		}

		public void UnhandledException(Exception exception)
		{
			foreach( EventListener listener in Extensions )
				listener.UnhandledException( exception );
		}

		public void TestOutput(TestOutput testOutput)
		{
			foreach( EventListener listener in Extensions )
				listener.TestOutput( testOutput );
		}

		#endregion

		#region ExtensionPoint Overrides
		protected override bool IsValidExtension(object extension)
		{
			return extension is EventListener; 
		}
		#endregion
	}
}
