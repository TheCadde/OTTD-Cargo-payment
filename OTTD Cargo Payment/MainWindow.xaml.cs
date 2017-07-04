// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using OTTD_Cargo_Payment.CargoDefinitions;
using OTTD_Cargo_Payment.CargoDefinitions.Metrics;
using OTTD_Cargo_Payment.NMLTemplates;

using OxyPlot;
using OxyPlot.Axes;

using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace OTTD_Cargo_Payment {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private PlotModel plotModel;

        public CargoScheme CurrentScheme;

        public MainWindow() {
            InitializeComponent();

            SetupPlotModel();

            AddCargoSchemesToDropdown();
            //GraphCargoes();
        }

        private void AddCargoSchemesToDropdown(CargoScheme trySelectScheme = null) {
            CargoSchemeDropdown.Items.Clear();
            foreach (var def in DefaultDefinitions.Defaults) {
                CargoSchemeDropdown.Items.Add(def);
            }

            var files = Directory.GetFiles(Config.CargoSchemesDirectory, "*.xml");
            foreach (var file in files) {
                try {
                    var def = CargoScheme.Load(file);
                    CargoSchemeDropdown.Items.Add(def);
                } catch (Exception ex) {
                    var fileName = new FileInfo(file).Name;
                    var wnd = new ExceptionWindow(ex) {Title = $"Something went wrong loading Cargo Scheme file '{fileName}'"};
                    wnd.ShowDialog();
                }
            }
            if (trySelectScheme != null) {
                foreach (var item in CargoSchemeDropdown.Items) {
                    var scheme = item as CargoScheme;
                    if (scheme == null || scheme.SchemeName != trySelectScheme.SchemeName)
                        continue;

                    CargoSchemeDropdown.SelectedItem = item;
                    return;
                }
            }
            CargoSchemeDropdown.SelectedIndex = 0;
        }

        private void GraphCargoes() {
            plotModel.Series.Clear();
            Legend.Children.Clear();

            int start;
            int end;
            int interval;

            if (!int.TryParse(StartDistance.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out start))
                start = 20;
            if (!int.TryParse(EndDistance.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out end))
                end = 20;
            if (!int.TryParse(DistanceInterval.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out interval))
                interval = 0;

            //var cargoDefinitions = DefaultDefinitions.GetSchemeByName("ECS Vectors 2012").CargoDefinitions;
            var cargoDefinitions = CurrentScheme.CargoDefinitions;
            cargoDefinitions = cargoDefinitions
                .OrderByDescending(x => x.CargoLabel == "PASS")
                .ThenByDescending(x => x.CargoLabel == "MAIL")
                .ThenByDescending(x => x.CargoLabel == "TOUR")
                .ThenBy(x => x.TypeName)
                .ToList();
            foreach (var cargoDefinition in cargoDefinitions)
                GraphCargo(cargoDefinition, start, end, interval);

            plotModel.InvalidatePlot(true);
            plotModel.ResetAllAxes();
        }

        private void GraphCargo(CargoDefinition cargoDef, int? startDist = null, int? maxDist = null, int? interval = null) {
            if (!interval.HasValue)
                interval = 20;
            if (!startDist.HasValue)
                startDist = interval.Value;
            if (!maxDist.HasValue)
                maxDist = (ushort)(startDist.Value + 1);

            var callbackRange = cargoDef.GetCallbackResultsForDistances(startDist.Value, maxDist.Value, interval.Value);

            var lineSerieses = new List<LineSeries>();
            foreach (var metrics in callbackRange.CallbackMetricses) {
                var lineSeries = new LineSeries {
                                     Title = $"{cargoDef.TypeName} ({metrics.Distance})",
                                     RenderInLegend = false,
                                     TextColor = OxyColors.Yellow,
                                     StrokeThickness = 1,
                                     Color = OxyColor.FromArgb(cargoDef.Color.A, cargoDef.Color.R, cargoDef.Color.G, cargoDef.Color.B),
                                 };
                var adjustedDataPoints = new List<DataPoint>();
                foreach (var dataPoint in metrics.DataPoints) {
                    adjustedDataPoints.Add(new DataPoint(dataPoint.X, dataPoint.Y * cargoDef.PriceFactor));
                }
                lineSeries.Points.AddRange(adjustedDataPoints);
                lineSeries.Tag = metrics;
                lineSeries.IsVisible = cargoDef.Visible;
                plotModel.Series.Add(lineSeries);
                lineSerieses.Add(lineSeries);
            }
            CreateLegendItem(cargoDef, lineSerieses);
            Debug.WriteLine(callbackRange.GetDebugInfo(cargoDef.TypeName));
        }

        private void CreateLegendItem(CargoDefinition cargoDef, List<LineSeries> lineSerieses) {
            var label = new Label {
                            Foreground = cargoDef.Visible ? Brushes.Yellow : Brushes.DarkGray,
                            FontSize = 12,
                            Padding = new Thickness(3, 0, 0, 0),
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = cargoDef.TypeName,
                            Margin = new Thickness(0, 0, 7, 0),
                        };
            var dockPanel = new DockPanel {
                                Height = 20,
                                Tag = new Tuple<CargoDefinition, List<LineSeries>, Label>(cargoDef, lineSerieses, label),
                            };
            var fillBrush = new SolidColorBrush();
            var colorBinding = new Binding("Color") {
                                   Source = cargoDef
                               };
            BindingOperations.SetBinding(fillBrush, SolidColorBrush.ColorProperty, colorBinding);
            dockPanel.Children.Add(new Rectangle {
                                       Width = 15,
                                       Height = 11,
                                       Fill = fillBrush,
                                       Stroke = Brushes.Black,
                                       StrokeThickness = 1,
                                       VerticalAlignment = VerticalAlignment.Center,
                                       Margin = new Thickness(7, 0, 0, 0),
                                   });
            dockPanel.Children.Add(label);

            dockPanel.MouseEnter += (sender, args) => { dockPanel.Background = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0)); };
            dockPanel.MouseLeave += (sender, args) => { dockPanel.Background = Brushes.Transparent; };
            dockPanel.MouseDown += (sender, args) => {
                if (args.ChangedButton == MouseButton.Left) {
                    cargoDef.Visible = !cargoDef.Visible;
                    SetVisibility();
                } else if (args.ChangedButton == MouseButton.Right) {
                    var menu = new ContextMenu();

                    var showDebugMenuItem = new MenuItem { Header = "Show info",};
                    showDebugMenuItem.Click += (m, ea) => {
                        ShowDebug(cargoDef, lineSerieses);
                    };
                    menu.Items.Add(showDebugMenuItem);

                    var editMenuItem = new MenuItem { Header = "Edit",};
                    editMenuItem.Click += (m, ea) => {
                        OpenEditor(cargoDef);
                    };
                    menu.Items.Add(editMenuItem);

                    var removeMenuItem = new MenuItem { Header = "Remove" };
                    removeMenuItem.Click += (o, eventArgs) => {
                        CurrentScheme.CargoDefinitions.Remove(cargoDef);
                        GraphCargoes();
                    };
                    menu.Items.Add(removeMenuItem);

                    dockPanel.ContextMenu = menu;

                }
            };

            Legend.Children.Add(dockPanel);
        }

        private void ShowDebug(CargoDefinition cargoDef, List<LineSeries> lineSerieses) {
            var first = lineSerieses.First().Tag as CallbackPerDistanceMetrics;
            if (first == null) 
                return;
            var content = first.GetHeaderString(cargoDef.TypeName);
            foreach (var series in lineSerieses) {
                var metrics = series.Tag as CallbackPerDistanceMetrics;
                if (metrics == null)
                    continue;
                content += $"\n{metrics.GetDataString()}";
            }
            var wnd = new DebugInfoWindow(cargoDef.TypeName, content);
            wnd.Show();
        }

        private void OpenEditor(CargoDefinition cargoDef) {
            var wnd = new CargoDefinitionEditor(cargoDef);
            wnd.RefreshOnly.Click += (o, eventArgs) => GraphCargoes();
            wnd.RefreshAll.Click += (o, eventArgs) => {
                GraphCargoes();
                SetVisibility(true);
            };
            wnd.RefreshOnlyThis.Click += (o, eventArgs) => {
                GraphCargoes();
                SetVisibility(false);
                cargoDef.Visible = true;
                SetVisibility();
            };
            wnd.Show();
        }

        private void SetupPlotModel() {
            plotModel = new PlotModel {
                            LegendTextColor = OxyColors.Yellow,
                            LegendTitleColor = OxyColors.LightGreen,
                            LegendBorderThickness = 1,
                            LegendBorder = OxyColors.White,
                            LegendBackground = OxyColor.FromRgb(0x50, 0x50, 0x50),
                            LegendPlacement = LegendPlacement.Outside,

                            LegendTitle = "Cargoes",
                            IsLegendVisible = false,

                            DefaultColors = new List<OxyColor> {
                                                OxyColor.FromRgb(0xFF, 0x40, 0x40),
                                                OxyColor.FromRgb(0x40, 0xFF, 0x40),
                                                OxyColor.FromRgb(0x50, 0x50, 0xFF),
                                                OxyColor.FromRgb(0xFF, 0xFF, 0x40),
                                                OxyColor.FromRgb(0x40, 0xFF, 0xFF),
                                                OxyColor.FromRgb(0xFF, 0x40, 0xFF),
                                                OxyColor.FromRgb(0xFF, 0xFF, 0xFF),
                                                OxyColor.FromRgb(0x80, 0x40, 0x40),
                                                OxyColor.FromRgb(0x40, 0x80, 0x40),
                                                OxyColor.FromRgb(0x50, 0x50, 0x90),
                                                OxyColor.FromRgb(0x80, 0x80, 0x40),
                                                OxyColor.FromRgb(0x40, 0x80, 0x80),
                                                OxyColor.FromRgb(0x80, 0x40, 0x80),
                                                OxyColor.FromRgb(0x80, 0x80, 0x80),
                                            }
                        };

            var gridLineColor = OxyColor.FromArgb(0x80, 0x20, 0x20, 0x20);
            plotModel.Axes.Add(
                         new LinearAxis {
                             Title = "Days",
                             TitleColor = OxyColors.Yellow,
                             Position = AxisPosition.Bottom,
                             Minimum = 0,

                             MajorStep = 10,
                             MinorTickSize = 2,
                             TicklineColor = OxyColors.DeepSkyBlue,
                             MajorGridlineThickness = 1,
                             MajorGridlineColor = gridLineColor,
                             MajorGridlineStyle = LineStyle.Solid,
                             TextColor = OxyColors.Yellow
                         }
                     );
            plotModel.Axes.Add(
                         new LinearAxis {
                             Title = "Profit",
                             TitleColor = OxyColors.Yellow,
                             Position = AxisPosition.Left,
                             Minimum = 0,
                             MinorTickSize = 2,
                             TicklineColor = OxyColors.DeepSkyBlue,
                             MinorGridlineThickness = 1,
                             MinorGridlineColor = gridLineColor,
                             MinorGridlineStyle = LineStyle.Solid,
                             MajorGridlineThickness = 1,
                             MajorGridlineColor = gridLineColor,
                             MajorGridlineStyle = LineStyle.Solid,
                             TextColor = OxyColors.Yellow
                         }
                     );
            Plot.Model = plotModel;
        }
        
        #region Other stuff
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }

        private void EnableAll_MouseDown(object sender, MouseButtonEventArgs e) {
            SetVisibility(true);
        }

        private void SetVisibility(bool? visible = null) {
            foreach (var legendChild in Legend.Children) {
                var tuple = (legendChild as DockPanel)?.Tag as Tuple<CargoDefinition, List<LineSeries>, Label>;
                if (tuple?.Item1 == null || tuple.Item2 == null)
                    continue;
                tuple.Item1.Visible = visible ?? tuple.Item1.Visible;
                foreach (var series in tuple.Item2)
                    series.IsVisible = tuple.Item1.Visible;
                tuple.Item3.Foreground = tuple.Item1.Visible ? Brushes.Yellow : Brushes.DarkGray; 
            }
            plotModel.InvalidatePlot(true);
        }

        private void DisableAll_MouseDown(object sender, MouseButtonEventArgs e) {
            SetVisibility(false);
        }
        #endregion
        private void CargoScheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0)
                return;
            CurrentScheme = e.AddedItems[0] as CargoScheme;
            GraphCargoes();
        }

        private void Redraw_OnClick(object sender, RoutedEventArgs e) {
            GraphCargoes();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e) {
            var clone = CurrentScheme.Clone() as CargoScheme;
            if (clone == null)
                throw new NullReferenceException();
            if (clone.IsDefault) {
                var wnd = new RenameCargoScheme(clone, "Rename clone of default cargo.");
                wnd.ShowDialog();
                clone.IsDefault = false;
            }
            clone.Save();

            AddCargoSchemesToDropdown(clone);
        }

        private void AddPerishable_OnClick(object sender, RoutedEventArgs e) {
            var cargoDefinition = new PerishableCargoDefinition();
            CurrentScheme.CargoDefinitions.Add(cargoDefinition);
            GraphCargoes();
            OpenEditor(cargoDefinition);
        }
        private void AddBulk_OnClick(object sender, RoutedEventArgs e) {
            var cargoDefinition = new BulkCargoDefinition();
            CurrentScheme.CargoDefinitions.Add(cargoDefinition);
            GraphCargoes();
            OpenEditor(cargoDefinition);
        }

        private void Compile_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show(NMLWriter.WriteNML(CurrentScheme));
        }
    }
}
