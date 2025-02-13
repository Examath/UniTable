using Examath.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace UniTable.Model
{
	public partial class VM : XMLFileObject<TimetablePlanner>
	{
		public VM() : base(new FileFilter("UniTable TimetablePlanner Planner", "*.uttp"), new FileFilter("Old format", "*.cuacv"))
		{

		}

		public override void CreateFile()
		{
			Data = new();
		}

		#region Loading

		/// <summary>
		/// Determines the type of file, and runs an importer for .cuacv, or
		/// deserializes XML data from the file at <see cref="FileLocation"/> to <see cref="Data"/> asynchronously.
		/// </summary>
		public override async Task LoadFileAsync()
		{
			if (FileLocation != null)
			{
				if (Path.GetExtension(FileLocation) == ".cuacv")
				{
					TimetablePlanner timetablePlanner = new();
					await timetablePlanner.LoadCuacv(FileLocation);
					FileLocation = null;
					Data = timetablePlanner;
				}
				else
				{
					await base.LoadFileAsync();
				}
			}
		}
		public override void InitialiseData()
		{
			Data?.Initialize();
			UpdateBuckets();
			ComputeStatistics();
		}

		#endregion

		#region Buckets

		/// <summary>
		/// Gets the date of the earliest session
		/// </summary>
		public DateTime SessionsStartDate { get; private set; } = DateTime.MaxValue;

		/// <summary>
		/// Gets the date of the latest session
		/// </summary>
		public DateTime SessionsEndDate { get; private set; } = DateTime.MinValue;

		/// <summary>
		/// Gets or sets a list of weeks containing all sessions
		/// </summary>
		public WeekBucket[] Buckets { get; set; }

		/// <summary>
		/// Updates the bucketing of the sessions (performance intensive)
		/// </summary>
		public void UpdateBuckets()
		{
			if (Data == null) return;

			// Reset Bounds
			SessionsStartDate = DateTime.MaxValue;
			SessionsEndDate = DateTime.MinValue;

			// Generation Loop
			List<Session> sessions = [];

			foreach (CourseHeader courseHeader in Data.CourseHeaderList)
			{
				if (!courseHeader.IsEnabled) continue;
				foreach (ClassType classType in courseHeader.ClassTypes)
				{
					foreach (UniClass uniClass in classType.Classes)
					{
						int sessionCount = 0;
						foreach (SessionEntry sessionEntry in uniClass.SessionEntries)
						{
							TimeSpan duration = sessionEntry.EndTime - sessionEntry.StartTime;
							bool isOnline = sessionEntry.Location.Contains("Online Class");
							// Bounds
							if (sessionEntry.StartDate < SessionsStartDate) SessionsStartDate = sessionEntry.StartDate;
							if (sessionEntry.EndDate > SessionsEndDate) SessionsEndDate = sessionEntry.EndDate;

							// Generate
							for (DateTime i = sessionEntry.StartDate; i <= sessionEntry.EndDate; i = i.AddDays(7))
							{
								Session session = new(
									i + sessionEntry.StartTime.ToTimeSpan(),
									duration,
									sessionEntry.Location,
									isOnline,
									courseHeader,
									classType,
									uniClass
									);
								sessions.Add(session);
								sessionCount++;
							}
						}

						uniClass.SessionCount = sessionCount;
						//TODO: move to init
						uniClass.ComputeTimings();
					}
				}
			}

			// Check if any exist
			if (sessions.Count < 1) return;

			// Prepare Bounds
			SessionsStartDate = SessionsStartDate.AddDays(-(double)SessionsStartDate.DayOfWeek);
			int NumberOfWeeks = (int)Math.Ceiling((SessionsEndDate - SessionsStartDate).TotalDays / 7);

			// Initialize each bucket

			Buckets = new WeekBucket[NumberOfWeeks];
			for (int i = 0; i < NumberOfWeeks; i++)
			{
				Buckets[i] = (new(SessionsStartDate.AddDays(i * 7)));
			}

			// Add each session to respective bucket

			foreach (Session session in sessions)
			{
				// Positioning & more
				int weekIndex = (int)Math.Floor((session.StartTime - SessionsStartDate).TotalDays / 7);
				//session.WeekIndex = weekIndex;
				session.InnerIndex = Buckets[weekIndex].Sessions.Count;
				Buckets[weekIndex].Sessions.Add(session);
			}

			OnPropertyChanged(nameof(Buckets));
			//UpdateSelected();
		}

		/// <summary>
		/// Calls <see cref="WeekBucket.ComputeStatistics"/> on each
		/// weekbucket in <see cref="Buckets"/>
		/// </summary>
		public void ComputeStatistics()
		{
			if (Data == null || Buckets == null) return;
			(double, double) sum = (0.0, 0);
			foreach (WeekBucket weekBucket in Buckets)
			{
				(double, double) result = weekBucket.ComputeStatistics();
				sum = (sum.Item1 + result.Item1, sum.Item2 + result.Item2);
			}
			Data.Stats = $" : Total Classtime: {sum.Item1}h | Total Commitment: {sum.Item2:#0.#}h";
		}

		protected override void Data_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			base.Data_PropertyChanged(sender, e);
			if (e.PropertyName == nameof(CourseHeader.ClassTypes))
			{
				UpdateBuckets();
				Data?.SetSelectedFromQuery(Data.Selection);
			}
			else if (e.PropertyName == nameof(CourseHeader.IsEnabled))
			{
				UpdateBuckets();
			}
			ComputeStatistics();
		}

		#endregion
	}
}
