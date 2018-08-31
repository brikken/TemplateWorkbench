using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TemplateWorkbench
{
    public class ModelBase : INotifyPropertyChanged
    {
        private Dictionary<string, object> variableDic = new Dictionary<string, object>();

        protected Observable<T> getVar<T>(string name) //GetVariable
        {
            if (!variableDic.ContainsKey(name))
                variableDic.Add(name, new Observable<T>(new Action(() => { NotifyPropertyChanged(name); })));
            return (Observable<T>)variableDic[name];
        }

        protected void setVar<T>(string name, Observable<T> value)
        {
			if (!variableDic.ContainsKey(name))
			    variableDic.Add(name, new Observable<T>(new Action(() => { NotifyPropertyChanged(name); })));
			((Observable<T>)variableDic[name]).Value = value.Value;
		}

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class Person : ModelBase
    {
        public Observable<string> LastName { get { return getVar<string>("LastName"); } }
        public Observable<int> Age
        {
            get { return getVar<int>("Age"); }
            set { setVar("Age", value); }
        }
    }

    public class ObservableConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            object valueValue = value.GetType().GetProperty("Value").GetValue(value, null);
            return base.ConvertTo(context, culture, valueValue, destinationType);
        }
    }

    [TypeConverter(typeof(ObservableConverter))]
    public class Observable<T> : INotifyPropertyChanged
    {
        T _value;
        Action _updateAction;

        public Observable(Action updateAction)
        {
            _updateAction = updateAction;
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (value == null || !value.Equals(_value))
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                    if (_updateAction != null)
                        _updateAction.Invoke();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public static implicit operator Observable<T>(T value)
        {
            return new Observable<T>(null) { Value = value };
        }
    }
}
