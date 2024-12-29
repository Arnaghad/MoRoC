using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Layout;
using MoRoC.Classes;
using MoRoC.ViewModels;

namespace MoRoC.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iconPath = Path.Combine(executableDirectory, "MoRoC.png");
            DataContext = new MainWindowViewModel();
            this.Icon = new WindowIcon(iconPath);
            this.Title = "MoRoC";
            this.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;

            var tabControl = new TabControl
            {
                Margin = new Thickness(5),
                TabStripPlacement = Dock.Left
            };
            
            tabControl.Items.Add(CreateGeneralTab());
            tabControl.Items.Add(CreateCpuTab());
            tabControl.Items.Add(CreateGpuTab());
            tabControl.Items.Add(CreateMotherBoardTab());
            tabControl.Items.Add(CreateRamTab());

            Content = tabControl;
        }

        private TabItem CreateGeneralTab()
        {
            var grid = new Grid();
            
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var detailsColumn = CreateDetailsColumn();
            Grid.SetColumn(detailsColumn, 0);
            grid.Children.Add(detailsColumn);

            var splitter1 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter1, 1);
            grid.Children.Add(splitter1);

            var temperatureColumn = CreateTemperatureColumn();
            Grid.SetColumn(temperatureColumn, 2);
            grid.Children.Add(temperatureColumn);

            var splitter2 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter2, 3);
            grid.Children.Add(splitter2);

            var fanSpeedColumn = CreateFanSpeedColumn();
            Grid.SetColumn(fanSpeedColumn, 4);
            grid.Children.Add(fanSpeedColumn);

            return new TabItem { Header = "General", Content = grid };
        }

        private Grid CreateDetailsColumn()
        {
            var grid = new Grid();

            // Add row definitions
            for (int i = 0; i < 17; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(i % 2 == 1 ? GridLength.Auto : GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("Details"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("CPU"), 2);
            AddToGrid(grid, CreateBindingBlock("CpuName"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("GPU"), 5);
            AddToGrid(grid, CreateBindingBlock("GpuName"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Motherboard"), 8);
            AddToGrid(grid, CreateBindingBlock("MbName"), 9);
            AddToGrid(grid, CreateSplitter(), 10);
            AddToGrid(grid, CreateLabelBlock("Storage"), 11);
            AddToGrid(grid, CreateBindingBlock("StorageNames"), 12);
            AddToGrid(grid, CreateSplitter(), 13);
            AddToGrid(grid, CreateLabelBlock("RAM"), 14);
            AddToGrid(grid, CreateBindingBlock("TotalRamVolume"), 15);
            AddToGrid(grid, CreateSplitter(), 16);

            return grid;
        }

        private Grid CreateTemperatureColumn()
        {
            var grid = new Grid();

            // Add row definitions
            for (int i = 0; i < 14; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(i % 2 == 1 ? GridLength.Auto : GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("Temperature"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("CPU"), 2);
            AddToGrid(grid, CreateBindingBlock("CpuTemp.Output.Value"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("GPU"), 5);
            AddToGrid(grid, CreateBindingBlock("GpuTemp.Output.Value"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Motherboard"), 8);
            AddToGrid(grid, CreateBindingBlock("MbTemp.Output.Value"), 9);
            AddToGrid(grid, CreateSplitter(), 10);
            AddToGrid(grid, CreateLabelBlock("Storage"), 11);
            AddToGrid(grid, CreateBindingBlock("StorageTemp.Output.Value"), 12);
            AddToGrid(grid, CreateSplitter(), 13);

            return grid;
        }

        private Grid CreateFanSpeedColumn()
        {
            var grid = new Grid();

            // Add row definitions
            for (int i = 0; i < 11; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(i % 2 == 1 ? GridLength.Auto : GridLength.Auto));
            }
            
            AddToGrid(grid, CreateHeaderBlock("Fan Speed"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("CPU"), 2);
            AddToGrid(grid, CreateBindingBlock("CpuFanSpeed.Output.Value"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("GPU"), 5);
            AddToGrid(grid, CreateBindingBlock("GpuFanSpeed.Output.Value"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Motherboard"), 8);
            AddToGrid(grid, CreateBindingBlock("MbFanSpeed.Output.Value"), 9);
            AddToGrid(grid, CreateSplitter(), 10);

            return grid;
        }
        
         private TabItem CreateCpuTab()
        {
            var grid = new Grid();
            
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var infoColumn = CreateCpuInfoColumn();
            Grid.SetColumn(infoColumn, 0);
            grid.Children.Add(infoColumn);

            var splitter1 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter1, 1);
            grid.Children.Add(splitter1);

            var ClockSpeedsColumn = CreateClockSpeedsColumn();
            Grid.SetColumn(ClockSpeedsColumn, 2);
            grid.Children.Add(ClockSpeedsColumn);

            var splitter2 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter2, 3);
            grid.Children.Add(splitter2);

            var clockSpeedsColumn = CreateCpuClockSpeedsColumn();
            Grid.SetColumn(clockSpeedsColumn, 4);
            grid.Children.Add(clockSpeedsColumn);

            return new TabItem { Header = "CPU", Content = grid };
        }

        private Grid CreateCpuInfoColumn()
        {
            var grid = new Grid();

            for (int i = 0; i < 26; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("CPU Information"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("Name"), 2);
            AddToGrid(grid, CreateBindingBlock("CpuName"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("Temperature"), 5);
            AddToGrid(grid, CreateBindingBlock("CpuTemp.Output.Value"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Physical Cores"), 8);
            AddToGrid(grid, CreateBindingBlock("PhysicalCores"), 9);
            AddToGrid(grid, CreateSplitter(), 10);
            AddToGrid(grid, CreateLabelBlock("Logical Cores"), 11);
            AddToGrid(grid, CreateBindingBlock("LogicalCores"), 12);
            AddToGrid(grid, CreateSplitter(), 13);
            AddToGrid(grid, CreateLabelBlock("Load of Processor"), 14);
            AddToGrid(grid, CreateBindingBlock("CpuUsage.Output.Value"), 15);
            AddToGrid(grid, CreateSplitter(), 16);
            AddToGrid(grid, CreateLabelBlock("Power Usage"), 17);
            AddToGrid(grid, CreateBindingBlock("CpuPowerUsage.Output.Value"), 18);
            AddToGrid(grid, CreateSplitter(), 19);
            AddToGrid(grid, CreateLabelBlock("Clock Speed"), 20);
            AddToGrid(grid, CreateBindingBlock("CpuClockSpeed.Output.Value"), 21);
            AddToGrid(grid, CreateSplitter(), 22);
            AddToGrid(grid, CreateLabelBlock("Fan Speed"), 23);
            AddToGrid(grid, CreateBindingBlock("CpuFanSpeed.Output.Value"), 24);
            AddToGrid(grid, CreateSplitter(), 25);
            return grid;
        }

        private Grid CreateClockSpeedsColumn()
        {
            var grid = new Grid();

            for (int i = 0; i < 5; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("Clock Speed of each core"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateBindingBlock("CoresClockSpeed.Output.Value"), 3);
            AddToGrid(grid, CreateSplitter(), 4);

            return grid;
        }

        private Grid CreateCpuClockSpeedsColumn()
        {
            var grid = new Grid();
            var cpuSpeedChart = new CpuClockSpeedsGraph();
            var cpuTempreatureChart = new CpuTemperatureGraph();
            for (int i = 0; i < 6; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("CPU Charts"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, cpuSpeedChart, 2); 
            AddToGrid(grid, CreateSplitter(), 3);
            AddToGrid(grid, cpuTempreatureChart, 4);
            AddToGrid(grid, CreateSplitter(), 5);
            return grid;
        }
        private TabItem CreateGpuTab()
        {
            var grid = new Grid();
            
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var infoColumn = CreateGpuInfoColumn();
            Grid.SetColumn(infoColumn, 0);
            grid.Children.Add(infoColumn);

            var splitter1 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter1, 1);
            grid.Children.Add(splitter1);

            var chartsColumn = CreateGpuChartsColumn();
            Grid.SetColumn(chartsColumn, 2);
            grid.Children.Add(chartsColumn);

            return new TabItem { Header = "GPU", Content = grid };
        }

        private Grid CreateGpuInfoColumn()
        {
            var grid = new Grid();

            for (int i = 0; i < 23; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("GPU Information"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("Name"), 2);
            AddToGrid(grid, CreateBindingBlock("GpuName"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("Temperature"), 5);
            AddToGrid(grid, CreateBindingBlock("GpuTemp.Output.Value"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Core Clock"), 8);
            AddToGrid(grid, CreateBindingBlock("GpuClockSpeed.Output.Value"), 9);
            AddToGrid(grid, CreateSplitter(), 10);
            AddToGrid(grid, CreateLabelBlock("Memory Clock"), 11);
            AddToGrid(grid, CreateBindingBlock("GpuMemoryClockSpeed.Output.Value"), 12);
            AddToGrid(grid, CreateSplitter(), 13);
            AddToGrid(grid, CreateLabelBlock("GPU Load"), 14);
            AddToGrid(grid, CreateBindingBlock("GpuLoad.Output.Value"), 15);
            AddToGrid(grid, CreateSplitter(), 16);
            AddToGrid(grid, CreateLabelBlock("GPU Power Load"), 17);
            AddToGrid(grid, CreateBindingBlock("GpuPowerLoad.Output.Value"), 18);
            AddToGrid(grid, CreateSplitter(), 19);
            AddToGrid(grid, CreateLabelBlock("Fan Speed"), 20);
            AddToGrid(grid, CreateBindingBlock("GpuFanSpeed.Output.Value"), 21);
            AddToGrid(grid, CreateSplitter(), 22);

            return grid;
        }
    
        private Grid CreateGpuChartsColumn()
        {
            var grid = new Grid();
            var gpuClockChart = new GpuClockSpeedsGraph();
            var gpuTemperatureChart = new GpuTemperatureGraph();

            for (int i = 0; i < 6; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("GPU Charts"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, gpuClockChart, 2);
            AddToGrid(grid, CreateSplitter(), 3);
            AddToGrid(grid, gpuTemperatureChart, 4);
            AddToGrid(grid, CreateSplitter(), 5);

            return grid;
        }
        
        private TabItem CreateMotherBoardTab()
        {
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var infoColumn = CreateMotherBoardInfoColumn();
            Grid.SetColumn(infoColumn, 0);
            grid.Children.Add(infoColumn);

            var splitter1 = new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
            Grid.SetColumn(splitter1, 1);
            grid.Children.Add(splitter1);

            var chartsColumn = CreateMotherBoardChartsColumn();
            Grid.SetColumn(chartsColumn, 2);
            grid.Children.Add(chartsColumn);

            return new TabItem { Header = "Motherboard", Content = grid };
        }

        private Grid CreateMotherBoardInfoColumn()
        {
            var grid = new Grid();

            for (int i = 0; i < 20; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("Motherboard Information"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, CreateLabelBlock("Name"), 2);
            AddToGrid(grid, CreateBindingBlock("MbName"), 3);
            AddToGrid(grid, CreateSplitter(), 4);
            AddToGrid(grid, CreateLabelBlock("Manufacturer"), 5);
            AddToGrid(grid, CreateBindingBlock("MbManufacturer"), 6);
            AddToGrid(grid, CreateSplitter(), 7);
            AddToGrid(grid, CreateLabelBlock("Temperature"), 8);
            AddToGrid(grid, CreateBindingBlock("MbTemp.Output.Value"), 9);
            AddToGrid(grid, CreateSplitter(), 10);
            AddToGrid(grid, CreateLabelBlock("BIOS Version"), 11);
            AddToGrid(grid, CreateBindingBlock("MbBiosName"), 12);
            AddToGrid(grid, CreateSplitter(), 13);
            AddToGrid(grid, CreateLabelBlock("BIOS Manufacturer"), 14);
            AddToGrid(grid, CreateBindingBlock("MbManufacturerBiosName"), 15);
            AddToGrid(grid, CreateSplitter(), 16);
            AddToGrid(grid, CreateLabelBlock("Fan Speed"), 17);
            AddToGrid(grid, CreateBindingBlock("MbFanSpeed.Output.Value"), 18);
            AddToGrid(grid, CreateSplitter(), 19);
            return grid;
        }

        private Grid CreateMotherBoardChartsColumn()
        {
            var grid = new Grid();
            var MotherboardTempreatureGraph = new MotherboardTemperatureGraph();
            for (int i = 0; i < 4; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            AddToGrid(grid, CreateHeaderBlock("Motherboard Charts"), 0);
            AddToGrid(grid, CreateSplitter(), 1);
            AddToGrid(grid, MotherboardTempreatureGraph, 2);
            AddToGrid(grid, CreateSplitter(), 3);

            return grid;
        }

    private TabItem CreateRamTab()
    {
        var grid = new Grid();
        
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var infoColumn = CreateRamInfoColumn();
        Grid.SetColumn(infoColumn, 0);
        grid.Children.Add(infoColumn);

        var splitter1 = new GridSplitter
        {
            Background = new SolidColorBrush(Colors.Gray),
            IsEnabled = false
        };
        Grid.SetColumn(splitter1, 1);
        grid.Children.Add(splitter1);

        var detailsColumn = CreateRamDetailsColumn();
        Grid.SetColumn(detailsColumn, 2);
        grid.Children.Add(detailsColumn);

        return new TabItem { Header = "RAM", Content = grid };
    }

    private Grid CreateRamInfoColumn()
    {
        var grid = new Grid();

        for (int i = 0; i < 8; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        AddToGrid(grid, CreateHeaderBlock("RAM Selection"), 0);
        AddToGrid(grid, CreateSplitter(), 1);

        var comboBox = new ComboBox
        {
            Margin = new Thickness(10),
            FontSize = 16,
            ItemTemplate = new FuncDataTemplate<RamSlotInfo>((item, _) =>
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                var nameBlock = new TextBlock();
                nameBlock.Bind(TextBlock.TextProperty, new Binding(nameof(RamSlotInfo.Slot)));
                stackPanel.Children.Add(nameBlock);

                var capacityBlock = new TextBlock
                {
                    Margin = new Thickness(10, 0, 0, 0)
                };
                capacityBlock.Bind(TextBlock.TextProperty, new Binding(nameof(RamSlotInfo.Capacity)));
                stackPanel.Children.Add(capacityBlock);

                var speedBlock = new TextBlock
                {
                    Margin = new Thickness(10, 0, 0, 0)
                };
                speedBlock.Bind(TextBlock.TextProperty, new Binding(nameof(RamSlotInfo.Speed)));
                stackPanel.Children.Add(speedBlock);

                return stackPanel;
            })
        };
            comboBox.Bind(ComboBox.ItemsSourceProperty, new Binding("RamSlots"));
            comboBox.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedRamSlot"));

            AddToGrid(grid, comboBox, 2);
            AddToGrid(grid, CreateSplitter(), 3);
            
            AddToGrid(grid, CreateLabelBlock("Total RAM"), 4);
            AddToGrid(grid, CreateBindingBlock("TotalRamVolume"), 5);
            AddToGrid(grid, CreateSplitter(), 6);

            return grid;
        }

    private Grid CreateRamDetailsColumn()
    {
        var grid = new Grid();

        for (int i = 0; i < 17; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        AddToGrid(grid, CreateHeaderBlock("RAM Information"), 0);
        AddToGrid(grid, CreateSplitter(), 1);
        AddToGrid(grid, CreateLabelBlock("Slot"), 2);
        AddToGrid(grid, CreateBindingBlock("SelectedRamSlot.Slot"), 3);
        AddToGrid(grid, CreateSplitter(), 4);
        AddToGrid(grid, CreateLabelBlock("Manufacturer"), 5);
        AddToGrid(grid, CreateBindingBlock("SelectedRamSlot.Manufacturer"), 6);
        AddToGrid(grid, CreateSplitter(), 7);
        AddToGrid(grid, CreateLabelBlock("Part Number"), 8);
        AddToGrid(grid, CreateBindingBlock("SelectedRamSlot.PartNumber"), 9);
        AddToGrid(grid, CreateSplitter(), 10);
        AddToGrid(grid, CreateLabelBlock("Capacity"), 11);
        AddToGrid(grid, CreateBindingBlock("SelectedRamSlot.Capacity"), 12);
        AddToGrid(grid, CreateSplitter(), 13);
        AddToGrid(grid, CreateLabelBlock("Speed"), 14);
        AddToGrid(grid, CreateBindingBlock("SelectedRamSlot.Speed"), 15);
        AddToGrid(grid, CreateSplitter(), 16);

        return grid;
    }

        private void AddToGrid(Grid grid, Control control, int row)
        {
            Grid.SetRow(control, row);
            grid.Children.Add(control);
        }

        private TextBlock CreateHeaderBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 25,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(10)
            };
        }

        private TextBlock CreateLabelBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(10, 1, 0, 0)
            };
        }

        private TextBlock CreateBindingBlock(string path)
        {
            var textBlock = new TextBlock
            {
                FontSize = 20,
                Margin = new Thickness(10, 1, 0, 10)
            };
            
            textBlock.Bind(TextBlock.TextProperty, new Binding(path));
            return textBlock;
        }

        private GridSplitter CreateSplitter()
        {
            return new GridSplitter
            {
                Background = new SolidColorBrush(Colors.Gray),
                IsEnabled = false
            };
        }
    }
}