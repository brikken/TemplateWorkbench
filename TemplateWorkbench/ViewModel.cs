using Antlr4.StringTemplate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateWorkbench
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string DataModel { get; set; }
        public double FontSize { get; set; } = 12.0;
        public string LastRender { get; private set; }
        private Model model;
        public string Render { get; private set; }
        public DateTime RenderTime { get; private set; }
        public string StatusDataModel { get; private set; }
        public string StatusTemplate { get; private set; }
        protected TemplateGroup stg;
        public string Template { get; set; }
        public ObservableCollection<TemplateListItem> Templates { get; set; } = new ObservableCollection<TemplateListItem>();
        public TemplateListItem TemplateSelected { get; set; }
        private System.Timers.Timer timer = new System.Timers.Timer();
        private readonly int timerDefaultInterval = 1000;

        public ViewModel()
        {
            timer.Elapsed += (_, __) => {
                if (RenderTime != null)
                {
                    RefreshLastRender();
                    timer.Interval = Math.Max(DateTime.Now.Subtract(RenderTime).Milliseconds / 60, timerDefaultInterval);
                }
                else
                {
                    LastRender = "N/A";
                }
            };
        }

        public void CompileTemplates()
        {
            HashSet<string> templateNames = new HashSet<string>();
            try
            {
                stg = new TemplateGroupString(Template);
                templateNames = stg.GetTemplateNames();
                StatusTemplate = "Template(s) compiled";
            }
            catch (Exception e)
            {
                StatusTemplate = $"Error compiling template ({e.Message})";
            }

            // update templates list
            foreach (var t in templateNames)
            {
                if (Templates.Count(tp => tp.Name == t) == 0) Templates.Add(new TemplateListItem() { Name = t });
            }
            // .ToList() forces enumeration before templates are removed, as to not change a collection being enumerated over
            foreach (var tp in Templates.Where(tp => tp.Deleted).ToList())
            {
                Templates.Remove(tp);
            }
            foreach (var tp in Templates.Where(tp => !templateNames.Contains(tp.Name)))
            {
                tp.Deleted = true;
            }

            // if no or invalid selection, select the first valid template if it exists
            if (TemplateSelected == null || TemplateSelected.Deleted)
            {
                TemplateSelected = Templates.FirstOrDefault(t => !t.Deleted);
            }
        }

        public void FormatJson()
        {
            try
            {
                var prettified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(DataModel), Formatting.Indented);
                DataModel = prettified;
            }
            catch (Exception e)
            {
                StatusDataModel = $"JSON format failed ({e.Message})";
            }
        }

        public void Open(string path)
        {
            model = Model.LoadProject(path);
            DataModel = model.DataModel;
            Template = model.TemplateCode;
            CompileTemplates();
        }

        public void PerformRender()
        {
            if (TemplateSelected != null)
            {
                try
                {
                    // get template instance
                    var templateInst = stg.GetInstanceOf(TemplateSelected.Name);

                    // Load data model
                    if (templateInst.GetAttributes() != null)
                    {
                        var jsonDict = JsonConvert.DeserializeObject<IDictionary<string, object>>(DataModel, new JsonConverter[] { new JsonConverterDictionary(), new JsonConverterCollection() });
                        foreach (var elem in jsonDict)
                        {
                            if (templateInst.GetAttributes().ContainsKey(elem.Key))
                            {
                                templateInst.Add(elem.Key, elem.Value);
                            }
                        }
                    }
                    StatusDataModel = "OK";

                    // Render
                    Render = templateInst.Render();

                    StatusTemplate = "OK";
                    RenderTime = DateTime.Now;
                    RefreshLastRender();
                    timer.Interval = timerDefaultInterval;
                    timer.Stop(); timer.Start();
                }
                catch (JsonSerializationException e)
                {
                    StatusDataModel = $"Error reading data model ({e.Message})";
                }
                catch (Exception e)
                {
                    StatusTemplate = e.Message;
                }
            }
            else
            {
                StatusTemplate = "No template selected";
            }
        }

        public void RefreshLastRender()
        {
            LastRender = RenderTime.ToLongTimeString() + " (" + TimeFormatter.TimeAgo(RenderTime) + ")";
        }

        public bool Save()
        {
            if (model != null)
            {
                model.DataModel = DataModel;
                model.TemplateCode = Template;
                model.SaveProject();
                return true;
            }
            return false;
        }

        public void SaveAs(string path)
        {
            model = new Model() { DataModel = DataModel, TemplateCode = Template, projectPath = path };
            model.SaveProject();
        }
    }

    public class TemplateListItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
