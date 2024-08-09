using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace Electron
{
    public partial class MainWindow : Window
        
    {
        private Settings _Settings;
        public MainWindow()
        {
            InitializeComponent();
            _Settings = new Settings();

            PositionSettings();
            this.LocationChanged += MainWindow_LocationChanged;
            this.SizeChanged += MainWindow_SizeChanged;

            Editor.Navigate(new Uri(string.Format("file:///{0}/monaco/index.html", Directory.GetCurrentDirectory())));

            foreach (FileInfo S in new DirectoryInfo("./scripts").GetFiles("*.txt"))
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = S.Name,
                    Style = (Style)FindResource("CustomListBoxItemStyle")
                };
                Scripts.Items.Add(item);
            }

            foreach (FileInfo S in new DirectoryInfo("./scripts").GetFiles("*.lua"))
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = S.Name,
                    Style = (Style)FindResource("CustomListBoxItemStyle")
                };
                Scripts.Items.Add(item);
            }
        }

        private void Scripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Scripts.SelectedItem is ListBoxItem selectedItem)
            {
                string fileName = selectedItem.Content.ToString();
                string script = File.ReadAllText(Path.Combine("./scripts", fileName));

                Editor.InvokeScript("setValue", script);
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            PositionSettings();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionSettings();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Editor.InvokeScript("setValue", "");
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Txt files (*.txt)|*.txt|Lua Files (*.lua)|*.lua|All Files (*.*)|*.*";

            if (Dialog.ShowDialog() == true)
            {
                string Script = File.ReadAllText(Dialog.FileName);
                Editor.InvokeScript("setValue", Script);
            }
        }

        private void Attach_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = Directory.GetCurrentDirectory();

            try
            {
                var files = Directory.GetFiles(directoryPath, "*.exe");

                string robloxProcessName = "RobloxPlayerBeta";

                try
                {
                    var robloxProcesses = Process.GetProcessesByName(robloxProcessName);

                    if (robloxProcesses.Length == 0)
                    {
                        MessageBox.Show("Roblox was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                foreach (var file in files)
                {
                    if (!System.IO.Path.GetFileName(file).Equals("electron.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        Process.Start(file);
                        return;
                    }
                }

                MessageBox.Show("Synapse Z was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        async private void Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string luaScriptContent = Editor.InvokeScript("getValue").ToString();
                await ExecuteScript(luaScriptContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error trying to execute script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteScript(string script, string pid = null)
        {
            string uniqueId = Guid.NewGuid().ToString();

            await Task.Delay(1);

            try
            {
                string schedulerPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "scheduler");
                if (!Directory.Exists(schedulerPath))
                {
                    Directory.CreateDirectory(schedulerPath);
                }

                string fileName = pid != null ? $"PID{pid}_{uniqueId}.lua" : $"{uniqueId}.lua";
                string filePath = System.IO.Path.Combine(schedulerPath, fileName);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await writer.WriteLineAsync(script + "@@FileFullyWritten@@");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PositionSettings()
        {
            _Settings.Left = this.Left + this.Width + 10;
            _Settings.Top = this.Top;
            _Settings.Height = this.Height;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_Settings.IsVisible)
            {
                _Settings.Hide();
            }
            else
            {
                PositionSettings();
                _Settings.Show();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _Settings.Close();
        }
    }
}
