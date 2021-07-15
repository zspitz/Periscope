using System;
using System.Diagnostics;
using System.Windows;

namespace Periscope {
    public partial class ExpressionRootPrompt {
        public ExpressionRootPrompt() {
            InitializeComponent();

            link.RequestNavigate += (s, e) => Process.Start(link.NavigateUri.ToString());
        }

        private void Window_ContentRendered(object sender, EventArgs e) => txbExpression.Focus();

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
