namespace NUnit.Util
{
	using System;
	using System.Collections;
	using System.IO;
	using NUnit.Core;

	/// <summary>
	/// ProxyTestRunner is the abstract base for all client-side
	/// TestRunner implementations that delegate to a remote
	/// TestRunner. All calls are simply passed on to the
	/// TestRunner that is provided to the constructor.
	/// 
	/// This class is similar to DelegatingTestRunner in the
	/// NUnit core, but is separate because of how it is used.
	/// 
	/// Although the class is abstract, it has no abstract 
	/// methods specified because each implementation will
	/// need to override different methods. All methods are
	/// specified using interface syntax and the derived class
	/// must explicitly implement TestRunner in order to 
	/// redefine the selected methods.
	/// </summary>
	public abstract class ProxyTestRunner : TestRunner
	{
		#region Instance Variables

		/// <summary>
		/// Our runner ID
		/// </summary>
		protected int runnerID;

		/// <summary>
		/// The downstream TestRunner
		/// </summary>
		private TestRunner testRunner;

		/// <summary>
		/// The event listener for the currently running test
		/// </summary>
		protected EventListener listener;

		#endregion

		#region Construction
		public ProxyTestRunner(TestRunner testRunner)
		{
			this.testRunner = testRunner;
			this.runnerID = testRunner.ID;
		}

		/// <summary>
		/// Protected constructor for runners that create their own
		/// specialized downstream runner.
		/// </summary>
		protected ProxyTestRunner( int runnerID )
		{
			this.runnerID = runnerID;
		}
		#endregion

		#region Properties
		public virtual int ID
		{
			get { return runnerID; }
		}

		public virtual bool Running
		{
			get { return testRunner != null && testRunner.Running; }
		}

		public virtual IList AssemblyInfo
		{
			get { return testRunner == null ? null : testRunner.AssemblyInfo; }
		}

		public virtual ITest Test
		{
			get { return testRunner == null ? null : testRunner.Test; }
		}

		public virtual TestResult TestResult
		{
			get { return testRunner == null ? null : testRunner.TestResult; }
		}

		/// <summary>
		/// Protected property copies any settings to the downstream test runner
		/// when it is set. Derived runners overriding this should call the base
		/// or copy the settings themselves.
		/// </summary>
		protected virtual TestRunner TestRunner
		{
			get { return testRunner; }
			set { testRunner = value; }
		}
		#endregion

		#region Load and Unload Methods
		public virtual bool Load( TestPackage package )
		{
			return this.testRunner.Load( package );
		}

		public virtual void Unload()
		{
            if ( this.testRunner != null )
			    this.testRunner.Unload();
		}
		#endregion

		#region CountTestCases
		public virtual int CountTestCases( ITestFilter filter )
		{
			return this.testRunner.CountTestCases( filter );
		}
		#endregion

		#region Methods for Running Tests
		public virtual TestResult Run(EventListener listener)
		{
			// Save active listener for derived classes
			this.listener = listener;
			return this.testRunner.Run(listener);
		}

		public virtual TestResult Run(EventListener listener, ITestFilter filter)
		{
			// Save active listener for derived classes
			this.listener = listener;
			return this.testRunner.Run(listener, filter);
		}

		public virtual void BeginRun( EventListener listener )
		{
			// Save active listener for derived classes
			this.listener = listener;
			this.testRunner.BeginRun( listener );
		}

		public virtual void BeginRun( EventListener listener, ITestFilter filter )
		{
			// Save active listener for derived classes
			this.listener = listener;
			this.testRunner.BeginRun( listener, filter );
		}

		public virtual TestResult EndRun()
		{
			return this.testRunner.EndRun();
		}

		public virtual void CancelRun()
		{
			this.testRunner.CancelRun();
		}

		public virtual void Wait()
		{
			this.testRunner.Wait();
		}
		#endregion

		#region InitializeLifetimeService Override
//		public override object InitializeLifetimeService()
//		{
//			return null;
//		}
		#endregion

	}
}
