﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTable
{
    //Format
    //Class Nbr	Section	Size	Available	Dates	Days	Time	Location
    //19009	PA01	370	335	4 Apr - 4 Apr	Tuesday	1pm - 3pm	MyUni, OL, Online Class

    /// <summary>
    /// Represents one of the timetabled classes that may be chosen when entering university
    /// </summary>
    internal partial class UniClass : ObservableObject
    {
        /// <summary>
        /// Unique 5-digit class number
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Alphanumeric code for this class withing the Subject
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Number of seats in this class
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Number of seats available
        /// </summary>
        public int Available { get; private set; }

        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// List of sessions this class has
        /// </summary>
        public List<SessionEntry> SessionEntries { get; set; } = new();

        private bool _Selected = false;
        /// <summary>
        /// Gets or sets whether this class is chosen
        /// </summary>
        public bool IsSelected
        {
            get => _Selected;
            internal set => SetProperty(ref _Selected, value);
        }

        private int _SessionCount = 0;

        /// <summary>
        /// Gets the number of sessions associated with this class
        /// </summary>
        public int SessionCount
        {
            get => _SessionCount;
            internal set => SetProperty(ref _SessionCount, value);
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

        internal void AddNote(string note)
        {
            Note = note;
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
                SessionEntry firstEntry = SessionEntries.First();
                bool Variant = false;
                foreach (SessionEntry sessionEntry in SessionEntries)
                {
                    if (
                        firstEntry.DayOfWeek != sessionEntry.DayOfWeek ||
                        firstEntry.StartTime != sessionEntry.StartTime ||
                        firstEntry.EndTime != sessionEntry.EndTime ||
                        firstEntry.Location != sessionEntry.Location)
                    {
                        Variant = true;
                        break;
                    }
                }
                if (Variant)
                {
                    Timings = "Variant";
                }
                else
                {
                    Timings = $"{firstEntry.DayOfWeek} {firstEntry.StartTime:htt} - {firstEntry.EndTime:htt} @ {firstEntry.Location}";
                }
            }
        }
    }
}
