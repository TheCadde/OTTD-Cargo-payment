using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using OTTD_Cargo_Payment.Annotations;
using OTTD_Cargo_Payment.CargoDefinitions;

using OxyPlot;
using OxyPlot.Wpf;

using FontWeights = System.Windows.FontWeights;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using UIElement = System.Windows.UIElement;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace OTTD_Cargo_Payment {
    /// <summary>
    /// Interaction logic for CargoDefinitionEditor.xaml
    /// </summary>
    public partial class CargoDefinitionEditor : Window {
        private CargoDefinition definition;

        private ExceptionValidationRule exceptionRule = new ExceptionValidationRule {
                                                            };

        public CargoDefinitionEditor(CargoDefinition definition) {
            InitializeComponent();

            this.definition = definition;

            var titleBinding = new Binding("TypeName") { Source = definition, StringFormat = "Cargo definition editor - {0}", Mode = BindingMode.OneWay };
            SetBinding(TitleProperty, titleBinding);

            CreateControls();
        }

        private void CreateControls() {
            var propsWithAttrib = definition.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(DynamicBindingAttribute), false));
            propsWithAttrib = propsWithAttrib
                .OrderBy(
                    x => (x.GetCustomAttribute(typeof(DynamicBindingAttribute)) as DynamicBindingAttribute)?.GroupSortOrder ?? 0
                ).ThenBy(
                    x => (x.GetCustomAttribute(typeof(DynamicBindingAttribute)) as DynamicBindingAttribute)?.ItemSortOrder ?? 0
                );

            var lastGroup = int.MinValue;
            foreach (var prop in propsWithAttrib) {
                var attrib = prop.GetCustomAttribute(typeof(DynamicBindingAttribute)) as DynamicBindingAttribute;
                if (attrib == null)
                    throw new NullReferenceException("This should technically be impossible but... Property with attribute DynamicBindingAttribute did not have the attribute.");

                if (attrib.GroupSortOrder != lastGroup) {
                    CreateGroupHeader(attrib.Group);
                    lastGroup = attrib.GroupSortOrder;
                }

                CreateBinding(attrib.DisplayName, prop.Name, prop.PropertyType, attrib);
            }
        }

        private void CreateGroupHeader(string groupName) {
            Properties.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            var border = new Border {
                BorderThickness = new Thickness(0, 0, 0, 2),
                BorderBrush = new LinearGradientBrush(Colors.Yellow, Colors.DarkSlateBlue, new Point(0, 0), new Point(1, 0)),
            };
            var nameLabel = new Label {
                Foreground = Brushes.Yellow,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Content = groupName,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5, 10, 0, 5)
            };
            Grid.SetColumn(nameLabel, 0);
            Grid.SetRow(nameLabel, Properties.RowDefinitions.Count - 1);
            Grid.SetColumnSpan(nameLabel, 3);
            Properties.Children.Add(nameLabel);

            Grid.SetColumnSpan(border, 3);
            Grid.SetRow(border, Properties.RowDefinitions.Count - 1);
            Properties.Children.Add(border);
        }

        private void CreateBinding(string name, string propertyName, Type type, DynamicBindingAttribute attrib) {
            Properties.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            var border = new Border {
                             BorderThickness = new Thickness(0, 0, 0, 1),
                             BorderBrush = new LinearGradientBrush(Colors.Silver, Colors.DimGray, new Point(0, 0), new Point(1, 0)),
                         };
            var nameLabel = new Label {
                                Foreground = Brushes.Yellow,
                                Content = name,
                                VerticalAlignment = VerticalAlignment.Center,
                            };

            UIElement editControl = new Label {
                                        Background = Brushes.Magenta,
                                        Foreground = Brushes.Yellow,
                                        Content = "<MISSING>",
                                    };

            if (type == typeof(string) || type == typeof(int)) {
                var binding = new Binding(propertyName) {
                                  Source = definition,
                                  ValidationRules = {
                                                      exceptionRule  
                                                    },
                                  UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                              };
                editControl = new TextBox {
                                  Height = 25,
                                  Margin = new Thickness(5),
                                  VerticalAlignment = VerticalAlignment.Center,
                                  Foreground = Brushes.Yellow,
                                  //Background = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0x20)),
                                  Style = new Style {
                                              Setters = {
                                                            new Setter {
                                                                Property = BackgroundProperty,
                                                                Value = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0x20)),
                                                            }
                                                        },
                                              Triggers = {
                                                             new Trigger {
                                                                 Property = Validation.HasErrorProperty,
                                                                 Value = true,
                                                                 Setters = {
                                                                               new Setter {
                                                                                   Property = ToolTipProperty,
                                                                                   Value = new Binding("(Validation.Errors).CurrentItem.ErrorContent") {
                                                                                               RelativeSource = RelativeSource.Self
                                                                                           }
                                                                               },
                                                                               new Setter {
                                                                                   Property = BackgroundProperty,
                                                                                   Value = Brushes.DarkRed
                                                                               }
                                                                           }
                                                             }
                                                         }
                                          }
                              };
                ToolTipService.SetShowDuration(editControl, 20000);
                ((TextBox)editControl).SetBinding(TextBox.TextProperty, binding);
            }
            if (type == typeof(Color)) {
                var fillBrush = new SolidColorBrush();
                var colorBinding = new Binding("Color") {
                    Source = definition
                };
                BindingOperations.SetBinding(fillBrush, SolidColorBrush.ColorProperty, colorBinding);
                editControl = new Rectangle {
                                  Width = 30,
                                  Height = 22,
                                  HorizontalAlignment = HorizontalAlignment.Left,
                                  Fill = fillBrush,
                                  Stroke = Brushes.Black,
                                  StrokeThickness = 1,
                                  VerticalAlignment = VerticalAlignment.Center,
                                  Margin = new Thickness(7, 3, 7, 3),
                              };
                editControl.MouseDown += (sender, args) => {
                    var picker = new ColorPicker(OxyColor.FromArgb(definition.Color.A, definition.Color.R, definition.Color.G, definition.Color.B));
                    picker.ShowDialog();
                    definition.Color = picker.SelectedColor.ToColor();
                };
            }

            var commentLabel = new Label {
                                   Foreground = Brushes.PaleGreen,
                                   Background = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0)),
                                   Content = attrib?.Comment,
                               };

            Properties.Children.Add(nameLabel);
            Properties.Children.Add(editControl);
            Properties.Children.Add(commentLabel);
            Grid.SetColumn(nameLabel, 0);
            Grid.SetColumn(editControl, 1);
            Grid.SetColumn(commentLabel, 2);
            Grid.SetRow(nameLabel, Properties.RowDefinitions.Count - 1);
            Grid.SetRow(editControl, Properties.RowDefinitions.Count - 1);
            Grid.SetRow(commentLabel, Properties.RowDefinitions.Count - 1);

            Grid.SetColumnSpan(border, 3);
            Grid.SetRow(border, Properties.RowDefinitions.Count - 1);
            Properties.Children.Add(border);
        }

        private string CamelCaseToText(string camelCase) {
            return System.Text.RegularExpressions.Regex.Replace(camelCase, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        private void CargoDefinitionEditor_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
