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

using OTTD_Cargo_Payment.CargoDefinitions;

namespace OTTD_Cargo_Payment {
    /// <summary>
    /// Interaction logic for RenameCargoScheme.xaml
    /// </summary>
    public partial class RenameCargoScheme : Window {
        public RenameCargoScheme(CargoScheme scheme, string message = "") {
            InitializeComponent();

            Title = $"{Title}{(!string.IsNullOrWhiteSpace(message) ? $" - {message}" : "")}";

            SchemeName.SetBinding(TextBox.TextProperty, new Binding("SchemeName") { Source = scheme, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, });
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
