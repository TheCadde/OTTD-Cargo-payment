﻿using System;
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
    /// Interaction logic for DebugInfoWindow.xaml
    /// </summary>
    public partial class DebugInfoWindow : Window {
        public DebugInfoWindow(string title, string content) {
            InitializeComponent();
            Title = $"Cargo series information - {title}";
            TextBlock.Text = content;
        }

        private void DebugInfoWindow_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
