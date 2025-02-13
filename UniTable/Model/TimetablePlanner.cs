using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UniTable.Editor;

namespace UniTable.Model
{
	public partial class TimetablePlanner : ObservableObject
	{
		#region Data

		private int _Year = DateTime.Now.Year;
		/// <summary>
		/// Gets the year this semester is in
		/// </summary>
		public int Year
		{
			get => _Year;
			set { if (SetProperty(ref _Year, value)) DefaultYear = value; }
		}

		public static int DefaultYear { get; private set; } = DateTime.Now.Year;

		private ObservableCollection<CourseHeader> _CourseHeaderList = [];
		/// <summary>
		/// Gets or sets the list of subjects listed
		/// </summary>
		public ObservableCollection<CourseHeader> CourseHeaderList
		{
			get => _CourseHeaderList;
			set => SetProperty(ref _CourseHeaderList, value);
		}
		private void CourseHeader_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ClassType.SelectedClass))
			{
				UpdateSelected();
			}
			OnPropertyChanged(e);
		}

		#endregion

		#region Init

		public void Initialize()
		{
			foreach (CourseHeader courseHeader in CourseHeaderList)
			{
				courseHeader.Initialize();
				courseHeader.PropertyChanged += CourseHeader_PropertyChanged;
			}

			SetSelectedFromQuery(Selection);
		}

		#endregion

		#region Selection

		private string _SelectedComment = "Comment";

		private string _Selection = "0";
		/// <summary>
		/// Gets or sets the selection string
		/// </summary>
		public string Selection
		{
			get => _SelectedComment + _Selection;
			set
			{
				if (!value.StartsWith(_SelectedComment + _Selection))
				{
					SetSelectedFromQuery(value);
				}
			}
		}

		private string _Stats = "Stats";
		/// <summary>
		/// Gets or sets the statistic summary
		/// </summary>
		public string Stats
		{
			get => _Stats;
			set => SetProperty(ref _Stats, value);
		}

		internal void UpdateSelected()
		{
			StringBuilder stringBuilder = new(": ");
			foreach (CourseHeader subjectHeader in CourseHeaderList)
			{
				int[] numbers = new int[subjectHeader.ClassTypes.Count];
				for (int i = 0; i < numbers.Length; i++)
				{
					if (subjectHeader.ClassTypes[i].SelectedClass != null)
					{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
						numbers[i] = subjectHeader.ClassTypes[i].SelectedClass.Number;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
					}
				}
				stringBuilder.AppendJoin(", ", numbers);
				stringBuilder.Append("; ");
			}

			_Selection = stringBuilder.ToString();
			OnPropertyChanged(nameof(Selection));
		}

		internal void SetSelectedFromQuery(string query)
		{
			string[] queries = query.Split(':');
			if (queries.Length >= 2)
			{
				_SelectedComment = queries[0];
				string[] subjectQueries = queries[1].Replace(" ", "").Split(';');
				int lenSh = Math.Min(CourseHeaderList.Count, subjectQueries.Length);
				for (int i = 0; i < lenSh; i++)
				{
					string[] classTypeQueries = subjectQueries[i].Split(',');
					int lenCt = Math.Min(CourseHeaderList[i].ClassTypes.Count, classTypeQueries.Length);
					for (int j = 0; j < lenCt; j++)
					{
						if (
							classTypeQueries[j] != String.Empty &&
							int.TryParse(classTypeQueries[j], out int classQuery))
						{
							ClassType classType = CourseHeaderList[i].ClassTypes[j];
							if (classQuery != 0)
							{
								UniClass? uniClass = classType.Classes.Find((uc) => uc.Number == classQuery);
								if (uniClass != null)
								{
									classType.SelectedClass = uniClass;
								}
							}
							else if (classType.Classes.Count >= 2) // && classQuery = 0
							{
								classType.SelectedClass = null;
							}
						}
					}
				}
			}
			UpdateSelected();
		}

		#endregion

		#region Interpreter

		internal async Task LoadCuacv(string fileName)
		{
			CourseHeader? subjectHeader = null;
			ClassType? classType = null;
			UniClass? uniClass = null;

			string[] lines = await File.ReadAllLinesAsync(fileName);

			int lineNumber = 0;

			foreach (string lineRaw in lines)
			{
				// Cut comments
				string line = lineRaw.Split("//")[0];

				// Skip empty lines
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				string[] parts = line.Split('\t');

				// Skip Table Headers
				if (parts[0] == "Class Nbr")
				{
					continue;
				}

				try
				{

					// Header / Subject Header
					if (parts[0] == "#")
					{
						if (int.TryParse(parts[1], out int semCode)) // Semester Header
						{
							Year = semCode / 10 + 2000;
						}
						else // Subject Header
						{
							subjectHeader = new(parts[1]);
							CourseHeaderList.Add(subjectHeader);
						}
					}

					// Class Type Description
					else if (parts[0].Contains(" Class: ") && subjectHeader != null)
					{
						classType = new(parts[0]);
						subjectHeader.ClassTypes.Add(classType);
					}

					// Class Entry
					else if (int.TryParse(parts[0], out int classNumber) && classType != null)
					{
						string[] uniClassParts = new string[3];
						Array.Copy(parts, 1, uniClassParts, 0, 3);
						uniClass = new(classNumber, uniClassParts);

						if (parts.Length >= 8) // Then a session is provided as well
						{
							string[] firstScheduleParts = new string[4];
							Array.Copy(parts, 4, firstScheduleParts, 0, 4);
							AddSession(firstScheduleParts);
						}

						classType.Classes.Add(uniClass);
					}

					// Class Note
					else if (parts[0].StartsWith("Note: ") && uniClass != null)
					{
						uniClass.Note = parts[0];
					}

					// Additional Session Entry
					else if (uniClass != null)
					{
						AddSession(parts);
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Exception thrown when reading line #{lineNumber}", e);
				}

				lineNumber++;
			}

			void AddSession(string[] parts)
			{
				SessionEntry sessionEntry = new(parts, Year);
				uniClass?.SessionEntries.Add(sessionEntry);
			}
		}

		#endregion

		#region Commands

		[RelayCommand]
		internal void AddCourseHeader()
		{
			CourseHeader courseHeader = new();
			CourseHeaderEditor courseHeaderEditor = new(courseHeader);
			if (courseHeaderEditor.ShowDialog() == true)
			{
				courseHeader.Initialize();
				courseHeader.PropertyChanged += CourseHeader_PropertyChanged;
				CourseHeaderList.Add(courseHeader);

				OnPropertyChanged(nameof(CourseHeader.ClassTypes));
			}
		}

		/// <summary>
		/// Removes a provided course header, after asking for confirmation from user.
		/// </summary>
		/// <param name="courseHeader">The course header to remove</param>
		[RelayCommand]
		internal void RemoveCourseHeader(CourseHeader courseHeader)
		{
			if (Examath.Core.Environment.Messager.Out($"Are you sure you want to remove {courseHeader.Name}?",
				"Remove Course",
				isCancelButtonVisible: true,
				yesButtonText: "Yes") == System.Windows.Forms.DialogResult.Yes)
			{
				courseHeader.PropertyChanged -= CourseHeader_PropertyChanged;
				CourseHeaderList.Remove(courseHeader);
				OnPropertyChanged(nameof(CourseHeader.ClassTypes));
			}
		}

		#endregion
	}
}
