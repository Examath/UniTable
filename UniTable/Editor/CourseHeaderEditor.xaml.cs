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

namespace UniTable.Editor
{
    /// <summary>
    /// Interaction logic for CourseHeaderEditor.xaml
    /// </summary>
    public partial class CourseHeaderEditor : Window
    {
        public CourseHeaderEditor(CourseHeader courseHeader)
        {
            DataContext = courseHeader;
            InitializeComponent();
        }

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
            this.DialogResult = true;
        }
    }
}
