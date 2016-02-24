namespace DB73.Helpers
{
    using System;

    using DB73.ViewModels;
    using DB73.Models;

    public static class LinkViewCaster
    {
        public static void ShowLink(Link link)
        {
            switch (link.LinkedType)
            {
                case "Document":/*
                    WorkspaceCaster<DocumentsViewModel>.
                         ShowInstead(MainWindowViewModel.MainWindow, new DocumentsViewModel(link), link);
                    MainWindowViewModel.Window.Focus();*/
                    break;
                case "Folder": /*
                    WorkspaceCaster<DocumentsViewModel>.
                        ShowInstead(MainWindowViewModel.MainWindow, new DocumentsViewModel(link), link);
                    MainWindowViewModel.Window.Focus(); */
                    break;
                case "TestSystem":
                    WorkspaceCaster<InventoryViewModel>.
                        ShowMultiple(MainWindowViewModel.MainWindow, new InventoryViewModel(link));
                    MainWindowViewModel.Window.Focus();
                    break;
                case "Tool":
                    WorkspaceCaster<InventoryViewModel>.
                        ShowMultiple(MainWindowViewModel.MainWindow, new InventoryViewModel(link));
                    MainWindowViewModel.Window.Focus();
                    break;
                case "MiscItem":
                    WorkspaceCaster<InventoryViewModel>.
                        ShowMultiple(MainWindowViewModel.MainWindow, new InventoryViewModel(link));
                    MainWindowViewModel.Window.Focus();
                    break;
                case "BugTicket":

                    var ticket = link.GetObject() as BugTicket;

                    if (ticket == null) return;

                    SatteliteWindow.ShowSatteliteWindow(new DB73.Views.SingleTicketView(),
                        new SingleTicketViewModel(ticket));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
