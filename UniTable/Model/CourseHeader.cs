using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Examath.Core.Environment;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using UniTable.Editor;
using UniTable.Model;
using WPF.ColorPicker;

namespace UniTable
{
	/// <summary>
	/// Represents information pretaining to the Subject of a <see cref="UniClass"/>
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
			set => SetProperty(ref _Number, value);
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
