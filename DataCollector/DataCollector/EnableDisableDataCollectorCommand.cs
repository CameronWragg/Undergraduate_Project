using System;
using System.ComponentModel.Design;
using DataCollector.Properties;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace DataCollector
{
    internal sealed class EnableDisableDataCollectorCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("e95849ef-b22a-499c-8fcc-735465eeae93");
        private readonly AsyncPackage package;

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
        
        public static EnableDisableDataCollectorCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new EnableDisableDataCollectorCommand(package, commandService);
            
        }

        // MENU EXECUTION ADDED
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool swap = !Settings.Default.EnableDataCollector;
            Settings.Default.EnableDataCollector = swap;
            Settings.Default.Save();
            MenuCommand command = sender as MenuCommand;
            command.Checked = Settings.Default.EnableDataCollector;

            switch (Settings.Default.EnableDataCollector)
            {
                case true:
                    ErrorHandler.AddMessage("Data Collector Extension Running.");
                    break;
                case false:
                    ErrorHandler.AddWarning("You have disabled the Data Collector Extension. Please consider re-enabling.");
                    break;
                default:
                    break;
            }
        }
    }
}
