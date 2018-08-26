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
using Antlr4.StringTemplate;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;

namespace TemplateWorkbench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string render = "";
        public string Render {
            get { return render; }
        }

        private DateTime? renderTime;
        public string RenderTime
        {
            get { return renderTime.HasValue ? renderTime?.ToLongTimeString() + " (" + TimeFormatter.TimeAgo(renderTime.Value) + ")" : "N/A"; }
        }

        public ObservableCollection<string> Templates { get; set; } = new ObservableCollection<string>();

        private string templateStatus;
        public string TemplateStatus
        {
            get { return templateStatus; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private System.Timers.Timer timer = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();

            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RenderTime")); };
            timer.Interval = 10000;
            timer.Enabled = true;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R && (
                e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                // CTRL+R
                PerformRender();
            }
            else if (e.Key == Key.F && (
                e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                // CTRL+F
                FormatJson();
            }
        }

        private void FormatJson()
        {
            try
            {
                var prettified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(txtDataModel.Text), Formatting.Indented);
                txtDataModel.Text = prettified;
            }
            catch (Exception)
            {
            }
        }

        private void PerformRender()
        {
            try
            {
                // Load template
                TemplateGroupString stg = new TemplateGroupString(txtTemplate.Text);
                var templateNames = stg.GetTemplateNames();
                templateNames.ToList().ForEach(t => {
                    if (!Templates.Contains(t)) Templates.Add(t);
                });
                Templates.ToList().ForEach(t =>
                {
                    if (!templateNames.Contains(t)) Templates.Remove(t);
                });
                var templateInst = stg.GetInstanceOf(lbTemplates.SelectedValue as string);

                // Load data model
                if (templateInst.GetAttributes() != null)
                {
                    var jsonDict = JsonConvert.DeserializeObject<IDictionary<string, object>>(txtDataModel.Text, new JsonConverter[] { new JsonConverterDictionary(), new JsonConverterCollection() });
                    foreach (var elem in jsonDict)
                    {
                        if (templateInst.GetAttributes().ContainsKey(elem.Key)) {
                            templateInst.Add(elem.Key, elem.Value);
                        }
                    }
                }

                // Render
                render = templateInst.Render();

                templateStatus = "OK";
                renderTime = DateTime.Now;
                timer.Stop(); timer.Start();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Render"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RenderTime"));
            }
            catch (Exception e)
            {
                templateStatus = e.Message;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TemplateStatus"));
        }

        private string PadWithDots(string text, int length)
        {
            if (text.Length > length)
                return text.Substring(0, length - 5) + "[...]";
            else
                return text;
        }

        private void lbTemplates_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            ListBox cmb = (sender as ListBox);
            (cmb.ItemsSource as INotifyCollectionChanged).CollectionChanged += (
                (_,__) =>
                    {
                        if ((sender as ListBox).SelectedIndex == -1 && (sender as ListBox).Items.Count > 0)
                            (sender as ListBox).SelectedIndex = 0;
                    }
            );
        }

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
#if !DEBUG
            e.Cancel = (MessageBox.Show("Are you sure you want to close?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No);
#endif
        }
    }
}
