using System;

namespace UniTable
{
	/// <summary>
	/// Represents the date, time, duration and location of a single session in university
	/// </summary>
	public class Session : IComparable<Session>
	{
		/// <summary>
		/// Gets the default scale for offsets
		/// </summary>
		private const double PixelsPerHour = 10;

		/// <summary>
		/// Gets the time this session starts
		/// </summary>
		public DateTime StartTime { get; private set; }

		/// <summary>
		/// Gets the time this session ends
		/// </summary>
		public DateTime EndTime { get => StartTime + Duration; }

		/// <summary>
		/// Gets the duration of this session
		/// </summary>
		public TimeSpan Duration { get; private set; }

		/// <summary>
		/// Gets where this session is held
		/// </summary>
		public string Location { get; private set; }

		/// <summary>
		/// Gets whether or not this class is held online
		/// </summary>
		public bool IsOnline { get; private set; }

		/// <summary>
		/// Gets the subjet this session is held for
		/// </summary>
		public CourseHeader CourseHeader { get; private set; }

		/// <summary>
		/// Gets the <see cref="ClassType"/> this session is a part of
		/// </summary>
		public ClassType ClassType { get; private set; }

		/// <summary>
		/// Gets the <see cref="UniClass"/> this session is a part of
		/// </summary>
		public UniClass UniClass { get; private set; }

		/// <summary>
		/// Gets the time from the start of 
		/// the week (including time of day) represented in pixels
		/// </summary>
		/// <remarks>
		/// For compactness, this property is designed to show
		/// <see cref="StartTime"/>s from
		/// Monday to Friday, 6am to 6pm only.
		/// Hence, only 60 of the 168 hours in a week are represented
		/// </remarks>
		public double WeekOffset { get; private set; }

		/// <summary>
		/// Gets the duration of this session represented as length in pixels
		/// </summary>
		public double DurationOffset { get; private set; }

		/// <summary>
		/// Gets the index of this session inside the parent
		/// <see cref="WeekBucket.Sessions"/>
		/// </summary>
		public int InnerIndex { get; internal set; }

		/*// <summary> // Not needed
        /// Gets the index of the parent <see cref="WeekBucket"/>
        /// inside <see cref="UniModel.Buckets"/>
        /// </summary>
        internal int WeekIndex { get; set; }*/

		public Session(
			DateTime startTime,
			TimeSpan duration,
			string location,
			bool isOnline,
			CourseHeader subjectHeader,
			ClassType classType,
			UniClass uniClass
			)
		{
			StartTime = startTime;
			Duration = duration;
			Location = location;
			IsOnline = isOnline;
			CourseHeader = subjectHeader;
			ClassType = classType;
			UniClass = uniClass;

			// 24/7: WeekOffset = ((double)startTime.DayOfWeek) * 24 * PixelsPerHour + startTime.TimeOfDay.TotalHours * PixelsPerHour;
			WeekOffset = ((double)startTime.DayOfWeek - 1) * 12 * PixelsPerHour + (startTime.TimeOfDay.TotalHours - 6) * PixelsPerHour;
			DurationOffset = duration.TotalHours * PixelsPerHour;
		}

		public override string ToString()
		{
			return $"{UniClass.Number}: {StartTime:ddd d MMM, HH:mm} - {StartTime + Duration:HH:mm}\n@ {Location}";
		}

		public int CompareTo(Session? other)
		{
			if (other == null) return 1;
			else return StartTime.CompareTo(other.StartTime);
		}
	}
}
