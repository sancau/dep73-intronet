namespace DB73.Helpers
{
    using DB73.ViewModels;
    using DB73.Models;

    using System.Linq;
    using System.Collections.ObjectModel;

    public static class WorkspaceCaster<VM>
        where VM : class 
    {
        public static void Show(object parameter, MainWindowViewModel container, VM instance)
        {
            VM workspace =
               container.Workspaces.FirstOrDefault(vm => vm is VM)
               as VM;

            if (workspace == null)
            {
                workspace = instance;
                container.Workspaces.Add(workspace as WorkspaceViewModel);
            }
            container.SetActiveWorkspace(workspace as WorkspaceViewModel);
        }

        public static void ShowMultiple(object parameter, MainWindowViewModel container, VM instance)
        {
            VM workspace =
               container.Workspaces.FirstOrDefault(vm => vm is VM)
               as VM;

            workspace = instance;
            container.Workspaces.Add(workspace as WorkspaceViewModel);

            container.SetActiveWorkspace(workspace as WorkspaceViewModel);
        }

        public static void Show(MainWindowViewModel container, VM instance)
        {
            VM workspace =
               container.Workspaces.FirstOrDefault(vm => vm is VM)
               as VM;

            if (workspace == null)
            {
                workspace = instance;
                container.Workspaces.Add(workspace as WorkspaceViewModel);
            }
            container.SetActiveWorkspace(workspace as WorkspaceViewModel);

            if (workspace is DocumentsViewModel)
            {
                var obj = workspace as DocumentsViewModel;
                obj.RefreshView();
            }
        }

        public static void ShowMultiple(MainWindowViewModel container, VM instance)
        {
            VM workspace =
               container.Workspaces.FirstOrDefault(vm => vm is VM)
               as VM;

            workspace = instance;
            container.Workspaces.Add(workspace as WorkspaceViewModel);

            container.SetActiveWorkspace(workspace as WorkspaceViewModel);

            if (workspace is DocumentsViewModel)
            {
                var obj = workspace as DocumentsViewModel;
                obj.RefreshView();
            }
        }

        public static void ShowInstead(MainWindowViewModel container, VM instance, Link link)
        {
            VM workspace =
               container.Workspaces.FirstOrDefault(vm => vm is VM)
               as VM;

            if (workspace == null)
            {
                workspace = instance;
                container.Workspaces.Add(workspace as WorkspaceViewModel);
            }

            container.SetActiveWorkspace(workspace as WorkspaceViewModel);

            if (workspace is DocumentsViewModel)
            {
                var obj = workspace as DocumentsViewModel;
                obj.RefreshView();
                obj.ShowLink(link);
            }
        }
    }
}
