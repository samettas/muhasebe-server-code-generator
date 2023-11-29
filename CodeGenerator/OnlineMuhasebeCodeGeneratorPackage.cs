global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Threading;

namespace OnlineMuhasebeCodeGenerator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]    
    [ProvideToolWindowVisibility(typeof(MyToolWindow), VSConstants.UICONTEXT.NoSolution_string)]    
    [ProvideToolWindowVisibility(typeof(MyToolWindow), VSConstants.UICONTEXT.SolutionHasSingleProject_string)]    
    [ProvideToolWindowVisibility(typeof(MyToolWindow), VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]    
    [ProvideToolWindowVisibility(typeof(MyToolWindow), VSConstants.UICONTEXT.EmptySolution_string)]        
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.OnlineMuhasebeCodeGeneratorString)]
    public sealed class OnlineMuhasebeCodeGeneratorPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();

            this.RegisterToolWindows();
        }
    }
}