using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DataCollector
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(DataCollectorPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class DataCollectorPackage : AsyncPackage
    {
        public const string PackageGuidString = "7cec100b-e144-4764-83d3-4522060059e5";

        public static bool Enabled = true;

        public DataCollectorPackage()
        {
           
        }


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            //string projectId = "cmp3060m-csw";
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await EnableDisableDataCollectorCommand.InitializeAsync(this);
            await RDTClass.InitializeAsync(this);
            IVsRunningDocumentTable rdt = (IVsRunningDocumentTable)
                await GetServiceAsync(typeof(SVsRunningDocumentTable));
            Assumes.Present(rdt);
            rdt.AdviseRunningDocTableEvents(RDTClass.mRDTClass, out RDTClass.rdtCookie);
            //CloudAuth.AuthImplicit(projectId);
            ErrorHandler.Initialize(this);
            ErrorHandler.AddMessage("Data Collector Extension Running.");
        }

    }
}
