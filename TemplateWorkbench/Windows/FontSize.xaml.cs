using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TemplateWorkbench.Windows
{
    /// <summary>
    /// Interaction logic for FontSize.xaml
    /// </summary>
    public partial class FontSize : Window
    {
        public FontSize()
        {
            InitializeComponent();
        }

        public FontSize(ViewModel vm) : this()
        {
            DataContext = vm;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            TxtFontSize.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            Close();
        }
    }
}
