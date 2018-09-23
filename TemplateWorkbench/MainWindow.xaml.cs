using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Globalization;

namespace TemplateWorkbench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string fileDialogFilter = $"Project files (*.{Model.defaultProjectExtension})|*.{Model.defaultProjectExtension}|All files (*.*)|*.*";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CopyToClipBoard(string text)
        {
            Clipboard.SetText(text);
        }

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
#if !DEBUG
            e.Cancel = (MessageBox.Show("Are you sure you want to close?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No);
#endif
        }

        private async void CommandRefresh_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            // run compile and render in the background
            await Task.Run(() => {
                viewModel.CompileTemplates();
                viewModel.PerformRender();
            });
        }

        private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandFormatData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewModel.FormatJson();
        }

        private void CommandCopyToClipBoard_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyToClipBoard(viewModel.Render);
        }

        private void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog() { Filter = fileDialogFilter };
            if (dialog.ShowDialog() ?? false)
            {
                viewModel.SaveAs(dialog.FileName);
            }
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!viewModel.Save())
                Commands.SaveAs.Execute(null, this);
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog() { Filter = fileDialogFilter, CheckFileExists = true, Multiselect = false };
            if (dialog.ShowDialog() ?? false)
            {
                viewModel.Open(dialog.FileName);
            }
        }
    }

    public static class Commands
    {
        public static readonly RoutedUICommand Open = new RoutedUICommand(
            "_Open",
            "Open",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Refresh = new RoutedUICommand(
            "_Refresh",
            "Refresh",
            typeof(Commands),
            new InputGestureCollection() {
                new KeyGesture(Key.R, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Save = new RoutedUICommand(
            "_Save",
            "Save",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand SaveAs = new RoutedUICommand(
            "Save _as",
            "Save as",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Exit = new RoutedUICommand(
            "E_xit",
            "Exit",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand FormatData = new RoutedUICommand(
            "_Format data",
            "Format data",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand CopyToClipBoard = new RoutedUICommand(
            "_Copy Render Result",
            "Copy Render Result",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );
    }
}
