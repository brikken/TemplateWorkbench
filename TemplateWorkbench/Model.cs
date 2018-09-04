using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TemplateWorkbench
{
    [Serializable]
    public class Model
    {
        [NonSerialized]
        public string DataModel;
        private string DataModelPath;
        public static readonly string defaultProjectExtension = "proj";
        [NonSerialized]
        public string projectPath;
        [NonSerialized]
        public string TemplateCode;
        private string TemplateCodePath;


        public Model() { }

        private void Init(string projectPath)
        {
            this.projectPath = projectPath;
            DataModel = File.ReadAllText(Path.Combine(new FileInfo(projectPath).DirectoryName, DataModelPath));
            TemplateCode = File.ReadAllText(Path.Combine(new FileInfo(projectPath).DirectoryName, TemplateCodePath)); ;
        }

        public static Model LoadProject(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                var model = bf.Deserialize(fs) as Model;
                model?.Init(path);
                return model;
            }
        }

        public void SaveProject()
        {
            var fi = new FileInfo(projectPath);
            DataModelPath = "datamodel.json";
            File.WriteAllText(Path.Combine(fi.DirectoryName, DataModelPath), DataModel);
            TemplateCodePath = "templates.stg";
            File.WriteAllText(Path.Combine(fi.DirectoryName, TemplateCodePath), TemplateCode);
            using (var fs = new FileStream(projectPath, FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }
    }
}
