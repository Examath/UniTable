using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace UniTable
{
	//Format
	//Class Nbr	Section	Size	Available	Dates	Days	Time	Location
	//19009	PA01	370	335	4 Apr - 4 Apr	Tuesday	1pm - 3pm	MyUni, OL, Online Class

	/// <summary>
	/// Represents one of the timetabled classes that may be chosen when entering university
	/// </summary>
	public partial class UniClass : ObservableObject
	{
		/// <summary>
		/// Unique 5-digit class number
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// Alphanumeric code for this class within the Subject
		/// </summary>
		public string Section { get; set; } = string.Empty;

		/// <summary>
		/// Number of seats in this class
		/// </summary>
		public int Size { get; set; }

		/// <summary>
		/// Number of seats available
		/// </summary>
		public int Available { get; set; }

		private string _Note = "";
		/// <summary>
		/// Gets or sets the note for this class
		/// </summary>
		public string Note
		{
			get => _Note;
			set => SetProperty(ref _Note, value);
		}

		/// <summary>
		/// List of sessions this class has
		/// </summary>
		public List<SessionEntry> SessionEntries { get; set; } = new();

		private bool _Selected = false;
		/// <summary>
		/// Gets or sets whether this class is chosen
		/// </summary>
		[XmlIgnore]
		public bool IsSelected
		{
			get => _Selected;
			internal set => SetProperty(ref _Selected, value);
		}

		private int _SessionCount = 0;

		/// <summary>
		/// Gets the number of sessions associated with this class
		/// </summary>
		[XmlIgnore]
		public int SessionCount
		{
			get => _SessionCount;
			internal set => SetProperty(ref _SessionCount, value);
		}

		private bool _IsMouseOver = false;
		/// <summary>
		/// Gets or sets whether the user has their mosue over the UI for this class
		/// </summary>
		[XmlIgnore]
		public bool IsMouseOver
		{
			get => _IsMouseOver;
			set => SetProperty(ref _IsMouseOver, value);
		}

		public UniClass()
		{

		}

		/// <summary>
		/// Creates a university class entry from relavent data
		/// </summary>
		/// <param name="classNumber">Unique 5-digit class number</param>
		/// <param name="parts">
		///     Array of three strings in format
		///     <c>[Section, Size, Available]</c>
		/// </param>
		public UniClass(int classNumber, string[] parts)
		{
			Number = classNumber;
			Section = parts[0];
			Size = int.Parse(parts[1]);
			Available = (parts[2] == "FULL") ? 0 : int.Parse(parts[2]);
		}

		public override string ToString()
		{
			return $"{Section}";
		}

		/// <summary>
		/// Gets the number of seats taken out of the total size of the class
		/// </summary>
		public string Space
		{
			get => $"{Size - Available}/{Size}";
		}

		private string _Timings = "Not Computed";
		/// <summary>
		/// Gets the time and location the sessions of this class are held, if any
		/// </summary>
		[XmlIgnore]
		public string Timings
		{
			get => _Timings;
			private set => SetProperty(ref _Timings, value);
		}

		/// <summary>
		/// Calculates <see cref="Timings"/> by looping through all session entries
		/// </summary>
		internal void ComputeTimings()
		{
			if (SessionEntries.Count < 1)
			{
				Timings = "Empty";
			}
			else
			{
				Dictionary<string, int> TimingsL = new Dictionary<string, int>();
				Dictionary<string, int> LocationsL = new Dictionary<string, int>();

				foreach (SessionEntry sessionEntry in SessionEntries)
				{
					//if (
					//    firstEntry.DayOfWeek != sessionEntry.DayOfWeek ||
					//    firstEntry.StartTime != sessionEntry.StartTime ||
					//    firstEntry.EndTime != sessionEntry.EndTime ||
					//    firstEntry.Location != sessionEntry.Location)
					//{
					//    break;
					//}

					string timing = $"{sessionEntry.DayOfWeek.ToString()[..2]} {sessionEntry.StartTime:HH}-{sessionEntry.EndTime:HH}";

					if (TimingsL.ContainsKey(timing))
					{
						TimingsL[timing]++;
					}
					else
					{
						TimingsL.Add(timing, 1);
					}

					if (LocationsL.ContainsKey(sessionEntry.Location))
					{
						LocationsL[sessionEntry.Location]++;
					}
					else
					{
						LocationsL.Add(sessionEntry.Location, 1);
					}
				}

				IEnumerable<string> timings = TimingsL.OrderByDescending((d) => d.Value).Select(x => x.Key);
				IEnumerable<string> locations = LocationsL.OrderByDescending((d) => d.Value).Select(x => x.Key);
				IEnumerable<string> compactLocations = locations.Select(location =>
				{
					var match = LocationCodeRegex().Match(location);
					return match.Success ? match.Value : location;
				});

				if (TimingsL.Count > 1 && LocationsL.Count > 1) // Super Variant
				{
					Timings = $"Various times @ {string.Join(", ", compactLocations)}    ({string.Join(" | ", locations)})";
				}
				else
				{
					Timings = $"{string.Join(", ", timings)} @ {string.Join(", ", compactLocations)}    ({string.Join(" | ", locations)})";
				}
			}
		}

		[GeneratedRegex(@"(?<=, )\w{2,5}(?=,)")]
		private static partial Regex LocationCodeRegex();
	}
}
