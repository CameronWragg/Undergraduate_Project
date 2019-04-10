using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DataCollector.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DataCollector
{
    /// Command handler
    internal sealed class EnableDisableDataCollectorCommand
    {
        /// Command ID.
        public const int CommandId = 0x0100;
        /// Command menu group (command set GUID).
        public static readonly Guid CommandSet = new Guid("e95849ef-b22a-499c-8fcc-735465eeae93");
        /// VS Package that provides this command, not null.
        private readonly AsyncPackage package;
        
        /// Initializes a new instance of the <see cref="EnableDisableDataCollectorCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private EnableDisableDataCollectorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID)
            {
                Checked = Settings.Default.EnableDataCollector
            };
            commandService.AddCommand(menuItem);
        }
        
        /// Gets the instance of the command.
        public static EnableDisableDataCollectorCommand Instance
        {
            get;
            private set;
        }
        /// Gets the service provider from the owner package.
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        /// Initializes the singleton instance of the command.
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in EnableDisableDataCollectorCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            DataCollectorPackage enable = new DataCollectorPackage
            {
                Enabled = true
            };
            ErrorHandler.AddMessage("Data Collector Extension Running.");
            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new EnableDisableDataCollectorCommand(package, commandService);

        }

        /// MENU EXECUTION ADDED
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DataCollectorPackage enable = new DataCollectorPackage();
            Settings.Default.EnableDataCollector = !Settings.Default.EnableDataCollector;
            Settings.Default.Save();
            var command = sender as MenuCommand;
            command.Checked = Settings.Default.EnableDataCollector;

            switch (Settings.Default.EnableDataCollector)
            {
                case true:
                    enable.Enabled = true;
                    ErrorHandler.AddMessage("Data Collector Extension Running.");
                    break;
                case false:
                    enable.Enabled = false;
                    ErrorHandler.AddWarning("You have disabled the Data Collector Extension. Please consider re-enabling.");
                    break;
                default:
                    break;
            }
        }
    }
}
