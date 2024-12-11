using System.Windows;

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
