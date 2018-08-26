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
            get { return renderTime?.ToLongTimeString() ?? "N/A"; }
        }

        public ObservableCollection<string> Templates { get; set; } = new ObservableCollection<string>();

        private string templateStatus;
        public string TemplateStatus
        {
            get { return templateStatus; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
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
                JObject json = JObject.Parse(txtDataModel.Text);
                var jsonDict = JsonConvert.DeserializeObject<IDictionary<string, object>>(txtDataModel.Text, new JsonConverter[] { new JsonConverterDictionary(), new JsonConverterCollection() });
                foreach (JProperty child in json.Children())
                {
                    object childValue;
                    jsonDict.TryGetValue(child.Name, out childValue);
                    templateInst.Add(child.Name, childValue);
                }

                // Render
                render = templateInst.Render();

                templateStatus = "OK";
            }
            catch (Exception e)
            {
                templateStatus = e.Message;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TemplateStatus"));

            //render = "Rendering " + PadWithDots(txtTemplate.Text, 40) + " with data " + PadWithDots(txtDataModel.Text, 40);
            renderTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Render"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RenderTime"));
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
            ComboBox cmb = (sender as ComboBox);
            (cmb.ItemsSource as INotifyCollectionChanged).CollectionChanged += (
                (_,__) =>
                    {
                        if ((sender as ComboBox).SelectedIndex == -1 && (sender as ComboBox).Items.Count > 0)
                            (sender as ComboBox).SelectedIndex = 0;
                    }
            );
        }

        private void wnd_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = (MessageBox.Show("Are you sure you want to close?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No);
        }
    }
}
