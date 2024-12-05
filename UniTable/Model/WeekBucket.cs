using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniTable.Properties;

namespace UniTable
{
    public partial class WeekBucket : ObservableObject
    {
        /// <summary>
        /// Gets or sets the list of all sessions for this week
        /// </summary>
        public List<Session> Sessions { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of labels for each day of this week from Monday to Friday
        /// </summary>
        public List<string> Days { get; private set; } = new();

        /// <summary>
        /// Gets the date of the sunday this week starts from
        /// </summary>
        public DateTime StartOfWeek { get; private set; }

        private string _Statistics = "...";

        /// <summary>
        /// Gets the total duration of all selected classes
        /// for this week, as well as estimated time at uni,
        /// as calculated using <see cref="ComputeStatistics"/>
        /// </summary>
        public string Statistics
        {
            get => _Statistics;
            private set => SetProperty(ref _Statistics, value);
        }

        /// <summary>
        /// Creates a new WeekBucket with the specified start date, and initialises <see cref="Days"/>
        /// </summary>
        /// <param name="startOfWeek"></param>
        public WeekBucket(DateTime startOfWeek)
        {
            StartOfWeek = startOfWeek;

            // For Monday to Friday
            for (int i = 1; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);
                string str = day.Day.ToString();
                if(day.Day == 1) str += " " + day.ToString("MMM");
                Days.Add(str);
            }
        }

        public override string ToString()
        {
            return $"{StartOfWeek:dd/MM}";
        }

        /// <summary>
        /// Calculates <see cref="Statistics"/>,
        /// including total class time and total minimum time commuting and at uni
        /// </summary>
        /// <remarks>
        /// 'Spanner' Algorithm Copyright (c) CEC 2023.
        /// </remarks>
        internal (double, double) ComputeStatistics()
        {
            double totalClassHours = 0.0;
            double totalUniHours = 0.0;
            double totalFare = 0.0;
			Sessions.Sort();

			List<Session> ValidSessions = Sessions.FindAll(
                (session) => (session.UniClass.IsSelected && !session.IsOnline));

            if (ValidSessions.Count > 0)
            {
                Session? firstSessionOfDay = ValidSessions.First();
                for (int i = 0; i < ValidSessions.Count; i++)
                {
                    Session session = ValidSessions[i];
                    totalClassHours += (int)session.Duration.TotalHours;

                    if (i != 0 && session.StartTime.DayOfWeek != firstSessionOfDay.StartTime.DayOfWeek)
                    {
                        ComputeStatisticsForDay(firstSessionOfDay, ValidSessions[i - 1]);
                        firstSessionOfDay = session;
                    }
                    if (i == ValidSessions.Count - 1)
                    {
                        ComputeStatisticsForDay(firstSessionOfDay, session);
                    }
                }
            }

            Statistics = $"{totalClassHours}h, {totalUniHours:#0.#}h\n{totalFare:C}";

            return (totalClassHours, totalUniHours);

            void ComputeStatisticsForDay(Session firstSessionOfDay, Session lastSessionOfDay)
            {
                totalUniHours += (
                    lastSessionOfDay.EndTime
                    - firstSessionOfDay.StartTime
                    + Settings.Default.CommuteTime * 2).TotalHours;

                totalFare += GetFare(firstSessionOfDay.StartTime - Settings.Default.CommuteTime);
                totalFare += GetFare(lastSessionOfDay.EndTime + Settings.Default.CommuteUniToBusTime);
            }

            double GetFare(DateTime dateTime)
            {
                if (dateTime.TimeOfDay > new TimeSpan(9,0,0) && dateTime.TimeOfDay < new TimeSpan(15,0,0))
                {
                    return Settings.Default.FareOffPeak;
                }
                else
                {
                    return Settings.Default.FarePeak;
                }
            }
        }
    }
}
