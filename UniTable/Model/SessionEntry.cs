using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UniTable
{
    public class SessionEntry
    {
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
            get => $"{StartTime.Hour}-{EndTime.Hour}";
            set
            {
				string[] timeParts = value.Split('-');
				StartTime = TimeOnly.ParseExact(timeParts[0], "%H");
				EndTime = TimeOnly.ParseExact(timeParts[1], "%H");
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
            StartTime = TimeOnly.ParseExact(timeParts[0], "htt");
            EndTime = TimeOnly.ParseExact(timeParts[1], "htt");
        }
    }
}
