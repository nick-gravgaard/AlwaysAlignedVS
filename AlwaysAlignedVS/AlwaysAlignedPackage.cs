using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace AlwaysAligned
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "2019.0.1", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidAlwaysAlignedPkgString)]
	[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class AlwaysAlignedPackage : AsyncPackage
	{
		private static AlwaysAlignedPackage _package;

		public static AlwaysAlignedPackage Instance
		{
			get { return _package; }
		}

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public AlwaysAlignedPackage()
		{
			_package = this;
		}

		/////////////////////////////////////////////////////////////////////////////
		// Overridden Package Implementation

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync( cancellationToken, progress );

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			AppInfo.appObject = (EnvDTE80.DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

			ExternalSettingsTracker.Start();

			OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			CommandID menuCommandID = new CommandID(GuidList.guidAlwaysAlignedCmdSet, (int)PkgCmdIDList.AlwaysAlignedMenu);
			OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
			menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatusSetMenuText);
			mcs.AddCommand(menuItem);
		}

		private void MenuItemCallback(object sender, EventArgs e)
		{
			IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			Guid clsid = Guid.Empty;
			int result;

			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
				uiShell.ShowMessageBox(
					0, ref clsid,
					"FirstPackage",
					string.Format(CultureInfo.CurrentCulture,
						"Inside {0}.MenuItemCallback()", this.ToString()),
					string.Empty, 0,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
					OLEMSGICON.OLEMSGICON_INFO,
					0, out result));
		}

		bool menuTextSet = false;
		private void OnBeforeQueryStatusSetMenuText(object sender, EventArgs e)
		{
			if (!menuTextSet)
			{
				var myCommand = sender as OleMenuCommand;
				if (myCommand != null)
				{
					myCommand.Text = "Always Aligned";
					menuTextSet = true;
				}
			}
		}
	}
}
