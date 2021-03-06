﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using BundlerMinifierVsix.Commands;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BundlerMinifierVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", BundlerMinifier.Constants.VERSION, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidBundlerPackageString)]
    [ProvideOptionPage(typeof(Options), "Web", Vsix.Name, 101, 102, true, new[] { "bundle", "minify" }, ProvidesLocalizedCategoryName = false)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class BundlerMinifierPackage : Package
    {
        public static DTE2 _dte;
        public static Dispatcher _dispatcher;
        public static Package Package;
        public static BundlerMinifierPackage _instance;
        SolutionEvents _solutionEvents;

        public static Options Options { get; private set; }

        protected override void Initialize()
        {
            _instance = this;
            _dte = GetService(typeof(DTE)) as DTE2;
            _dispatcher = Dispatcher.CurrentDispatcher;
            Package = this;
            Options = (Options)GetDialogPage(typeof(Options));

            Logger.Initialize(this, Vsix.Name);

            Events2 events = _dte.Events as Events2;
            _solutionEvents = events.SolutionEvents;

            _solutionEvents.AfterClosing += () => { ErrorList.CleanAllErrors(); };
            _solutionEvents.ProjectRemoved += (project) => { ErrorList.CleanAllErrors(); };

            CreateBundle.Initialize(this);
            UpdateBundle.Initialize(this);
            UpdateAllFiles.Initialize(this);
            BundleOnBuild.Initialize(this);
            RemoveBundle.Initialize(this);
            ClearOutputFiles.Initialize(this);
            ToggleProduceOutput.Initialize(this);
            OpenSettings.Initialize(this);
            ProjectEventCommand.Initialize(this);
            ConvertToGulp.Initialize(this);

            base.Initialize();
        }

        public static bool IsDocumentDirty(string documentPath, out IVsPersistDocData persistDocData)
        {
            var serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);

            IVsHierarchy vsHierarchy;
            uint itemId, docCookie;
            VsShellUtilities.GetRDTDocumentInfo(
                serviceProvider, documentPath, out vsHierarchy, out itemId, out persistDocData, out docCookie);
            if (persistDocData != null)
            {
                int isDirty;
                persistDocData.IsDocDataDirty(out isDirty);
                return isDirty == 1;
            }

            return false;
        }
    }
}
