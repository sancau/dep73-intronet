using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DB73.Views
{
    /// <summary>
    /// Interaction logic for ProcessingView.xaml
    /// </summary>
    public partial class ProcessingView : Window
    {
        public ProcessingView()
        {
            InitializeComponent();
        }

        public ProcessingView(string processInfo)
            : this()
        {
            this.ProcessInfo.Content = processInfo;
        }
    }
}
