using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Misc;

namespace TemplateWorkbench
{
    class AccumulatingTemplateErrorListener : ITemplateErrorListener, INotifyPropertyChanged
    {
        public string Error { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AppendError(string msg)
        {
            Error += msg + Environment.NewLine;
        }

        public void CompiletimeError(TemplateMessage msg)
        {
            AppendError(msg.ToString());
        }

        public void InternalError(TemplateMessage msg)
        {
            AppendError(msg.ToString());
        }

        public void IOError(TemplateMessage msg)
        {
            AppendError(msg.ToString());
        }

        public void RuntimeError(TemplateMessage msg)
        {
            AppendError(msg.ToString());
        }
    }
}
