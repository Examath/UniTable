using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Examath.Core.Environment;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UniTable.Editor;
using UniTable.Model;
using WPF.ColorPicker;

namespace UniTable
{
	/// <summary>
	/// Represents information pertaining to the Subject of a <see cref="UniClass"/>
	/// </summary>
	public partial class CourseHeader : ObservableObject
	{
		#region Properties

		private string _Code = "XYZ";
		/// <summary>
		/// Short code for the course
		/// </summary>
		public string Code
		{
			get => _Code;
			set => SetProperty(ref _Code, value);
		}

		private int _Number = 0;
		/// <summary>
		/// Unique 4-digit course number
		/// </summary>
		public int Number
		{
			get => _Number;
			set
			{
				if (SetProperty(ref _Number, value) && string.IsNullOrWhiteSpace(_Url) && value > 1 && !string.IsNullOrWhiteSpace(_Code))
				{
					Url = $"https://adelaideuni.edu.au/study/courses/{Code.ToLowerInvariant()}-{Number}/";
				}
			}
		}

		private string _Name = "Untitled";
		/// <summary>
		/// Longform name of course
		/// </summary>
		public string Name
		{
			get => _Name;
			set => SetProperty(ref _Name, value);
		}

		private string _Url = "";
		/// <summary>
		/// Gets or sets the Url to the webpage with timetabling information
		/// </summary>
		public string Url
		{
			get => _Url;
			set { if (SetProperty(ref _Url, value)) return; }
		}

		private Color _Color = Examath.Core.Utils.HSV.GetRandomColor();
		/// <summary>
		/// Gets or sets the color of the course
		/// </summary>
		public Color Color
		{
			get => _Color;
			set => SetProperty(ref _Color, value);
		}

		private List<ClassType> _ClassTypes = new();
		/// <summary>
		/// List containing various types of classes
		/// </summary>
		public List<ClassType> ClassTypes
		{
			get => _ClassTypes;
			set => SetProperty(ref _ClassTypes, value);
		}

		private void ClassType_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(ClassType.IsFocused)) OnPropertyChanged(e);
		}

		private bool _IsEnabled = true;
		/// <summary>
		/// Gets or sets whether the course is enabled (and bucketed)
		/// </summary>
		public bool IsEnabled
		{
			get => _IsEnabled;
			set => SetProperty(ref _IsEnabled, value);
		}

		private string _UpdateStatus = "";
		/// <summary>
		/// Gets or sets the status text used for reporting loading
		/// </summary>
		public string UpdateStatus
		{
			get => _UpdateStatus;
			set => SetProperty(ref _UpdateStatus, value);
		}

		#endregion

		#region Init

		/// <summary>
		/// Creates a empty course header
		/// </summary>
		public CourseHeader()
		{

		}

		/// <summary>
		/// Creates a course header from the relavent data
		/// </summary>
		/// <param name="param">
		/// In format<c>CODE WORD 1000 - Name Of Subject - #HEXCOLOR</c>
		/// </param>
		public CourseHeader(string param)
		{
			string[] subParams = param.Split(" - ");
			string[] codeWords = subParams[0].Split(' ');
			Number = int.Parse(codeWords[^1]);
			Code = subParams[0][..(subParams[0].Length - 1 - codeWords[^1].Length)];
			Name = subParams[1];
			if (subParams.Length >= 3 && subParams[2].Contains('#'))
			{
				Color = (Color)ColorConverter.ConvertFromString(subParams[2]);
			}
		}

		public void Initialize()
		{
			foreach (ClassType classType in ClassTypes)
			{
				classType.Initialize();
				classType.PropertyChanged += ClassType_PropertyChanged;
			}
		}

		#endregion

		#region Commands

		[RelayCommand]
		internal void ChangeColour()
		{
			Color color = Color;
			bool ok = ColorPickerWindow.ShowDialog(out color);
			if (ok)
			{
				Color = color;
			}
		}

		/// <summary>
		/// Loads or updates timetable information for a course from <see cref="Url"/>.
		/// </summary>
		/// <remarks>
		/// This method fetches the webpage at tpe provided location, 
		/// and interprets the webpage to see if timetable information is available.
		/// The user is prompted to confirm semester and campus location.
		/// Then the <see cref="ClassTypes"/>, <see cref="UniClass"/>s and <see cref="Session"/>s
		/// are added as required.
		/// </remarks>
		[RelayCommand]
		internal async Task FetchFromUrl()
		{
			//// HTTP fetch

			UpdateStatus = $"Fetching {Url}...";

			HtmlDocument document = new HtmlDocument();
			try
			{
				using HttpClient client = new();
				if (string.IsNullOrWhiteSpace(Url))
				{
					Messager.Out("Provide a course URL before trying to import.", "No URL provided", messageStyle: ConsoleStyle.FormatBlockStyle);
					return;
				}

				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36 Edg/143.0.3650.80");
				client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

				using var stream = await client.GetStreamAsync(Url);

				document = new HtmlDocument();
				document.Load(stream);

				UpdateStatus = $"Fetched from {Url} at {DateTime.Now:g}";
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				Messager.OutException(e, "Fetching course webpage");
#pragma warning restore CS0162 // Unreachable code detected
				return;
			}

			try
			{
				LoadFromHtml(document);
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				Messager.OutException(e, $"Interpreting HTML");
				return;
#pragma warning restore CS0162 // Unreachable code detected
			}
		}

		/// <summary>
		/// Loads or updates timetable information for a course from a file.
		/// </summary>
		/// <remarks>
		/// This loads a html document from the provided location
		/// and interprets the webpage to see if timetable information is available.
		/// The user is prompted to confirm semester and campus location.
		/// Then the <see cref="ClassTypes"/>, <see cref="UniClass"/>s and <see cref="Session"/>s
		/// are added as required.
		/// </remarks>
		[RelayCommand]
		internal void FetchFromFile()
		{
			//// HTTP fetch
			string filePath = @"C:\Users\paulj\Downloads\sample.txt";

			UpdateStatus = $"Loading {filePath}...";

			HtmlDocument document = new HtmlDocument();
			try
			{
				if (!System.IO.Path.Exists(filePath))
				{
					Messager.Out("Please provide a file that exists", "File does not exist", messageStyle: ConsoleStyle.FormatBlockStyle);
					return;
				}

				using var stream = new System.IO.FileStream(filePath,
					System.IO.FileMode.Open, System.IO.FileAccess.Read);

				document = new HtmlDocument();
				document.Load(stream);

				UpdateStatus = $"Loaded from {filePath} at {DateTime.Now:g}";
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				Messager.OutException(e, "Loading course html");
#pragma warning restore CS0162 // Unreachable code detected
				return;
			}

			try
			{
				LoadFromHtml(document);
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				Messager.OutException(e, $"Interpreting HTML");
				return;
#pragma warning restore CS0162 // Unreachable code detected
			}
		}

		private readonly string _UNI_DATE_FORMAT = "d MMM yyyy";
		private readonly string[] _UNI_TIME_FORMAT = { "htt", "h:mmtt" };

		private void LoadFromHtml(HtmlDocument document)
		{
			//// Availability Identification

			// Find the Availability section
			HtmlNode? availabilitySection = document.DocumentNode
				.SelectSingleNode("//section[@id='availability']");

			if (availabilitySection == null)
			{
				Messager.Out(
					"The page does not contain an Availability section.",
					"No Availability Found",
					messageStyle: ConsoleStyle.FormatBlockStyle
				);
				return;
			}

			//// Select semester

			HtmlNodeCollection? semesterNodes = availabilitySection.SelectNodes(".//div[contains(@class,'cmp-course-accordion--container')]")
				?? throw new Exception("Could not find semester sections within");

			HtmlNode? semesterNode = null;

			foreach (var semNode in semesterNodes)
			{
				// Find the semester title node: <span class="cmp-course-accordion__title">
				HtmlNode semTitleNode = semNode.SelectSingleNode(".//span[contains(@class,'cmp-course-accordion__title')]");
				if (semTitleNode != null
					&& HtmlEntity.DeEntitize(semTitleNode.InnerText.Trim())
					.Equals(TimetablePlanner.DefaultSemester, StringComparison.CurrentCultureIgnoreCase)) 
				{
					semesterNode = semNode;
				}
			}

			if (semesterNode == null)
			{
				Messager.Out(
					$"This course does not appear to have a timetable for '{TimetablePlanner.DefaultSemester}'. Please check settings and course page.",
					"Semester Not Found",
					messageStyle: ConsoleStyle.FormatBlockStyle
				);
				return;
			}

			// TODO Campus selection TODO
			string campus = "Adelaide City Campus East";

			// Status
			UpdateStatus += $"\nLoading {TimetablePlanner.DefaultSemester}, {TimetablePlanner.DefaultYear}, {campus}";

			//// Class Loading

			List<ClassType> newClassTypes = [];

			// Load class types
			HtmlNodeCollection classTypeNodes = semesterNode.SelectNodes("./div/div/div/div[contains(@class,'cmp-course-accordion--container')]")
				?? throw new Exception("Class type nodes not found");
			foreach (HtmlNode classTypeNode in classTypeNodes)
			{
				ClassType classType = new();
				bool isClassTypeValid = true;

				// Get class type description
				var classTypeTitleNode = classTypeNode.SelectSingleNode(".//span[contains(@class,'cmp-course-accordion__title')]")
					?? throw new Exception("Class type description node not found");
				classType.Description = HtmlEntity.DeEntitize(classTypeTitleNode.InnerText.Trim());

				// Load classes
				HtmlNodeCollection classNodes = classTypeNode.SelectNodes(".//div[contains(@class,'cmp-course-accordion--container-session')]")
					?? throw new Exception("Class nodes not found");
				foreach (HtmlNode classNode in classNodes)
				{
					UniClass uniClass = new();

					// Get properties
					HtmlNodeCollection? propertyNodes = classNode.SelectNodes(".//h6[contains(@class,'cmp-course-accordion--card-text')]")
						?? throw new Exception("Class property nodes not found");
					foreach (HtmlNode propertyNode in propertyNodes)
					{
						string[] property = HtmlEntity.DeEntitize(propertyNode.InnerText.Trim()).Split(' ');

						string propertyValue = property.Last();

						switch (string.Join(" ", property[..^1]).Trim().ToLower())
						{
							case "class number":
								if (int.TryParse(propertyValue, out int classNbr))
									uniClass.Number = classNbr;
								break;

							case "section":
								uniClass.Section = propertyValue;
								break;

							case "size":
								if (int.TryParse(propertyValue, out int size))
									uniClass.Size = size;
								break;

							case "available":
								if (int.TryParse(propertyValue, out int avail))
									uniClass.Available = avail;
								break;
						}
					}

					// Get sessions
					HtmlNodeCollection? sessionNodes = classNode.SelectNodes(".//tr[not(contains(@class,'cmp-data-table__heading'))]")
						?? throw new Exception("Session nodes not found");
					foreach (HtmlNode sessionNode in sessionNodes)
					{
						// Filter campus
						if (!sessionNode.SelectSingleNode("./td[4]").InnerText.Trim().Equals(campus, StringComparison.CurrentCultureIgnoreCase))
						{
							isClassTypeValid = false;
							break;
						}

						SessionEntry sessionEntry = new();

						// Read properties
						//HtmlNodeCollection sessionPropertyNodes = sessionNode.SelectNodes("./td");
						//foreach (HtmlNode sessionPropertyNode in sessionPropertyNodes)
						//{
						// Property name & value
						//string propertyName = sessionNode.SelectSingleNode("./div[contains(@class,'table-header-content')]").InnerText.Trim()
						//	?? throw new Exception("Session property name node not found");
						//string propertyValue = sessionNode.SelectSingleNode("./div[contains(@class,'table-content')]").InnerText.Trim()
						//	?? throw new Exception("Session property name node not found");

						// Date
						string[] dateParts = sessionNode.SelectSingleNode("./td[1]").InnerText.Split(" - ");
						sessionEntry.StartDate = DateTime.ParseExact($"{dateParts[0]} {TimetablePlanner.DefaultYear}", _UNI_DATE_FORMAT, CultureInfo.InvariantCulture);
						sessionEntry.EndDate = DateTime.ParseExact($"{dateParts[1]} {TimetablePlanner.DefaultYear}", _UNI_DATE_FORMAT, CultureInfo.InvariantCulture);

						// Day
						sessionEntry.DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), sessionNode.SelectSingleNode("./td[2]").InnerText);

						// Time
						string[] timeParts = sessionNode.SelectSingleNode("./td[3]").InnerText.Split(" - ");
						sessionEntry.StartTime = TimeOnly.ParseExact(timeParts[0], _UNI_TIME_FORMAT);
						sessionEntry.EndTime = TimeOnly.ParseExact(timeParts[1], _UNI_TIME_FORMAT);

						// Location
						sessionEntry.Location = sessionNode.SelectSingleNode("./td[5]").InnerText;

						// Load
						uniClass.SessionEntries.Add(sessionEntry);
					}

					// Load 
					classType.Classes.Add(uniClass);
				}

				if (isClassTypeValid) newClassTypes.Add(classType);
			}

			if (newClassTypes.Count <= 0)
			{
				Messager.Out(
					$"This course does not appear to have any classes at '{campus}'. Please check settings and course page.",
					"Empty Class Types",
					messageStyle: ConsoleStyle.FormatBlockStyle
				);
				return;
			}

			//// Final change set

			foreach (ClassType c in ClassTypes) c.PropertyChanged -= ClassType_PropertyChanged;
			foreach (ClassType c in newClassTypes)
			{
				c.Initialize();
				c.PropertyChanged += ClassType_PropertyChanged;
			}
			ClassTypes = newClassTypes;
		}

		/// <summary>
		/// Updates the timetable data by importing and parsing class information from the clipboard.
		/// </summary>
		/// <remarks>This method reads CAUCV text data from the clipboard, interprets it as timetable information, and
		/// updates the collection of class types accordingly. Existing class type event handlers are detached before the
		/// update, and new event handlers are attached to the imported class types. If the clipboard does not contain valid
		/// timetable data, a message is displayed to the user.</remarks>
		/// <exception cref="Exception">Error pharsing CAUCV</exception>
		[RelayCommand]
		internal void UpdateFromClipboard()
		{
			ClassType? classType = null;
			UniClass? uniClass = null;

			string[] lines = [];
			List<ClassType> newClassTypes = [];

			// Get text from clipboard
			try
			{
				if (Clipboard.ContainsText())
				{
					lines = SplitLines().Split(Clipboard.GetText());
				}
				else
				{
					Messager.Out("Copy the timetable data", "Nothing to paste",
						messageStyle: ConsoleStyle.FormatBlockStyle);
				}
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				Messager.OutException(e, "Pasting Data");
				return;
#pragma warning restore CS0162 // Unreachable code detected
			}

			// Interptet

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
					// Removed

					// Class Type Description
					if (parts[0].Contains(" Class: "))
					{
						classType = new(parts[0]);
						newClassTypes.Add(classType);
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
#if DEBUG
					throw new Exception($"Exception thrown when reading line #{lineNumber}", e);
#endif
#pragma warning disable CS0162 // Unreachable code detected
					Messager.OutException(e, $"Interpreting line #{lineNumber}");
					return;
#pragma warning restore CS0162 // Unreachable code detected
				}

				lineNumber++;
			}

			//Finally set changes
			foreach (ClassType c in ClassTypes) c.PropertyChanged -= ClassType_PropertyChanged;
			foreach (ClassType c in newClassTypes)
			{
				c.Initialize();
				c.PropertyChanged += ClassType_PropertyChanged;
			}
			ClassTypes = newClassTypes;

			void AddSession(string[] parts)
			{
				SessionEntry sessionEntry = new(parts, TimetablePlanner.DefaultYear);
				uniClass?.SessionEntries.Add(sessionEntry);
			}
		}

		[RelayCommand]
		internal void Edit()
		{
			CourseHeaderEditor courseHeaderEditor = new CourseHeaderEditor(this);
			courseHeaderEditor.ShowDialog();
		}

		#endregion

		public override string ToString()
		{
			return $"{Code} / {Number} - {Name}";
		}

		[GeneratedRegex("\r\n|\r|\n")]
		private static partial Regex SplitLines();
	}
}
