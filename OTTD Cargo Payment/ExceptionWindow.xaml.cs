using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OTTD_Cargo_Payment {
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window {
        public ExceptionWindow(Exception ex) {
            InitializeComponent();

            var message = $"Exception: {ex.Message}\n";
            var currentException = ex;
            while (currentException.InnerException != null) {
                currentException = currentException.InnerException;
                message += $"InnerException: {currentException.Message}\n";
            }
            message += $"\n\n{ex.StackTrace}";
            MessageText.Text = message;
        }

        private void CopyToClipboard_OnClick(object sender, RoutedEventArgs e) {
            Clipboard.SetText(MessageText.Text);
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
