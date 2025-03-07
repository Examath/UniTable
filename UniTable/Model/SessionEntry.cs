﻿using System;
using System.Globalization;
using System.Xml.Serialization;

namespace UniTable
{
	public class SessionEntry
	{
		private readonly string[] _UTTP_TIME_FORMAT = { "%H", "H:mm" };
		private readonly string[] _UNI_TIME_FORMAT = { "htt", "h:mmtt" };

		/// <summary>
		/// Gets when sessions of this session group start
		/// </summary>
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets when sessions of this session group stop (inclusive)
		/// </summary>
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Gets the day of the week these sessions are held
		/// </summary>
		public DayOfWeek DayOfWeek { get; set; }

		/// <summary>
		/// Gets when these sessions start
		/// </summary>
		[XmlIgnore]
		public TimeOnly StartTime { get; private set; }

		/// <summary>
		/// Gets when these sessions end
		/// </summary>
		[XmlIgnore]
		public TimeOnly EndTime { get; private set; }

		public string Timestring
		{
			get => $"{StartTime:H:mm}-{EndTime:H:mm}";
			set
			{
				string[] timeParts = value.Split('-');
				StartTime = TimeOnly.ParseExact(timeParts[0], _UTTP_TIME_FORMAT);
				EndTime = TimeOnly.ParseExact(timeParts[1], _UTTP_TIME_FORMAT);
			}
		}

		/// <summary>
		/// Gets where these sessions are held
		/// </summary>
		public string Location { get; set; } = string.Empty;

		public SessionEntry()
		{

		}

		public SessionEntry(string[] parts, int year)
		{
			DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), parts[1]);
			Location = parts[3];

			string[] dateParts = parts[0].Split(" - ");
			StartDate = DateTime.ParseExact($"{dateParts[0]} {year}", "d MMM yyyy", CultureInfo.InvariantCulture);
			EndDate = DateTime.ParseExact($"{dateParts[1]} {year}", "d MMM yyyy", CultureInfo.InvariantCulture);

			string[] timeParts = parts[2].Split(" - ");
			StartTime = TimeOnly.ParseExact(timeParts[0], _UNI_TIME_FORMAT);
			EndTime = TimeOnly.ParseExact(timeParts[1], _UNI_TIME_FORMAT);
		}
	}
}
