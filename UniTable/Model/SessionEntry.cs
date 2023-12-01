using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTable
{
    internal class SessionEntry
    {
        /// <summary>
        /// Gets when sessions of this session group start
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Gets when sessions of this session group stop (inclusive)
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Gets the day of the week these sessions are held
        /// </summary>
        public DayOfWeek DayOfWeek { get; private set; }

        /// <summary>
        /// Gets when these sessions start
        /// </summary>
        public TimeOnly StartTime { get; private set; }

        /// <summary>
        /// Gets when these sessions end
        /// </summary>
        public TimeOnly EndTime { get; private set; }

        /// <summary>
        /// Gets where these sessions are held
        /// </summary>
        public string Location { get; private set; }

        public SessionEntry(string[] parts, UniModel uniModel)
        {
            DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), parts[1]);
            Location = parts[3];

            string[] dateParts = parts[0].Split(" - ");
            StartDate = DateTime.ParseExact($"{dateParts[0]} {uniModel.Year}", "d MMM yyyy", CultureInfo.InvariantCulture);
            EndDate = DateTime.ParseExact($"{dateParts[1]} {uniModel.Year}", "d MMM yyyy", CultureInfo.InvariantCulture);

            string[] timeParts = parts[2].Split(" - ");
            StartTime = TimeOnly.ParseExact(timeParts[0], "htt");
            EndTime = TimeOnly.ParseExact(timeParts[1], "htt");
        }
    }
}
