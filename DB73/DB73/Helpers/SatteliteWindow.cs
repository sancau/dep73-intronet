namespace DB73.Helpers
{
    using DB73.ViewModels;

    using System.Windows;

    public class SatteliteWindow
    {
        private static Window ProcessingWindow { get; set; }
        private static Window SatteliteWindowInstance { get; set; }

        public static void ShowSatteliteWindow(Window windowInstance, WorkspaceViewModel viewmodel)
        {
            SatteliteWindowInstance = windowInstance;
            SatteliteWindowInstance.DataContext = viewmodel;
            SatteliteWindowInstance.ShowDialog();
        }

        public static void CloseSatteliteWindow()
        {
            SatteliteWindowInstance.Close();
        }

        public static void OnProcessStart(string info)
        {
            if (ProcessingWindow == null)
            {
                ProcessingWindow = new DB73.Views.ProcessingView(info);
                ProcessingWindow.Show();
            }
        }

        public static void OnProcessComplete()
        {
            ProcessingWindow.Close();
            ProcessingWindow = null;
        }
    }
}
