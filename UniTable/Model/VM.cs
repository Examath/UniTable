using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UniTable.Properties;

namespace UniTable
{
    internal partial class UniModel : ObservableObject
    {
        #region Collections

        private ObservableCollection<SubjectHeader> _SubjectHeaderList = new();
        /// <summary>
        /// Gets or sets the list of subjects listed
        /// </summary>
        public ObservableCollection<SubjectHeader> SubjectHeaderList
        {
            get => _SubjectHeaderList;
            set => SetProperty(ref _SubjectHeaderList, value);
        }

        private ObservableCollection<WeekBucket> _Buckets = new();
        /// <summary>
        /// Gets or sets a list of weeks containing all sessions
        /// </summary>
        public ObservableCollection<WeekBucket> Buckets
        {
            get => _Buckets;
            set => SetProperty(ref _Buckets, value);
        }

        #endregion

        #region Temporial Bounds

        /// <summary>
        /// Gets the date of the earliest session
        /// </summary>
        public DateTime SessionsStartDate { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// Gets the date of the latest session
        /// </summary>
        public DateTime SessionsEndDate { get; private set; } = DateTime.MinValue;

        #endregion

        #region AutoLoader and Manual Command

        public UniModel()
        {
            //LoadAsync();
        }

        bool isLoaded = false;

        bool CanLoadUniTable => !isLoaded;

        #endregion

        #region LoadUniTable()

        //[RelayCommand(CanExecute = nameof(CanLoadUniTable))]
        public async void LoadUniTable(string fileName)
        {
            SubjectHeaderList.Clear();
            SubjectHeader? subjectHeader = null;
            ClassType? classType = null;
            UniClass? uniClass = null;

            string[] lines = await File.ReadAllLinesAsync(fileName);

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

                // Subject Header
                if (parts[0] == "#")
                {
                    subjectHeader = new(parts[1]);
                    SubjectHeaderList.Add(subjectHeader);
                }

                // Class Type Description
                else if (parts[0].Contains(" Class: ") && subjectHeader != null)
                {
                    classType = new(parts[0], this);
                    subjectHeader.ClassTypes.Add(classType);
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
                    uniClass.AddNote(parts[0]);
                }

                // Additional Session Entry
                else if (uniClass != null)
                {
                    AddSession(parts);
                }
            }

            // Create week buckets

            // Find Bounds
            SessionsStartDate = SessionsStartDate.AddDays(-(double)SessionsStartDate.DayOfWeek);
            int NumberOfWeeks = (int)Math.Ceiling((SessionsEndDate - SessionsStartDate).TotalDays / 7);
            // Initialise each bucket
            Buckets.Clear();
            for (int i = 0; i < NumberOfWeeks; i++)
            {
                Buckets.Add(new(SessionsStartDate.AddDays(i * 7)));
            }

            // Add each session to respective bucket
            foreach (SubjectHeader sh in SubjectHeaderList)
            {
                foreach (ClassType ct in sh.ClassTypes)
                {
                    foreach (UniClass uc in ct.Classes)
                    {
                        int sessionCount = 0;
                        foreach (SessionEntry se in uc.SessionEntries)
                        {
                            TimeSpan duration = se.EndTime - se.StartTime;
                            bool isOnline = se.Location.Contains("Online Class");
                            for (DateTime i = se.StartDate; i <= se.EndDate; i = i.AddDays(7))
                            {
                                Session session = new(
                                    i + se.StartTime.ToTimeSpan(),
                                    duration,
                                    se.Location,
                                    isOnline,
                                    sh,
                                    ct,
                                    uc
                                    );

                                // Positioning & more
                                int weekIndex = (int)Math.Floor((i - SessionsStartDate).TotalDays / 7);
                                //session.WeekIndex = weekIndex;
                                session.InnerIndex = Buckets[weekIndex].Sessions.Count;
                                Buckets[weekIndex].Sessions.Add(session);
                                sessionCount++;
                            }
                        }

                        uc.SessionCount = sessionCount;
                        uc.ComputeTimings();
                    }

                    if (ct.Classes.Count == 1) // Only one option, then select
                    {
                        ct.SelectClass(ct.Classes[0]);
                    }
                }
            }

            GetSelected();
            isLoaded = true;
            //LoadUniTableCommand.NotifyCanExecuteChanged();

            void AddSession(string[] parts)
            {
                SessionEntry sessionEntry = new(parts);
                if (sessionEntry.StartDate < SessionsStartDate) SessionsStartDate = sessionEntry.StartDate;
                if (sessionEntry.EndDate > SessionsEndDate) SessionsEndDate = sessionEntry.EndDate;
                if (uniClass != null) uniClass.SessionEntries.Add(sessionEntry);
            }
        }

        #endregion

        #region Selected

        internal void GetSelected()
        {
            StringBuilder stringBuilder = new(": ");
            foreach (SubjectHeader subjectHeader in SubjectHeaderList)
            {
                int[] numbers = new int[subjectHeader.ClassTypes.Count];
                for (int i = 0; i < numbers.Length; i++)
                {
                    if (subjectHeader.ClassTypes[i].SelectedClass != null)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        numbers[i] = subjectHeader.ClassTypes[i].SelectedClass.Number;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                }
                stringBuilder.AppendJoin(", ", numbers);
                stringBuilder.Append("; ");
            }

            foreach (WeekBucket weekBucket in Buckets)
            {
                weekBucket.Sessions.Sort();
            }
            ComputeStatistics();

            _SelectedData = stringBuilder.ToString();
            OnPropertyChanged(nameof(Selected));
        }

        private void SetSelected(string query)
        {
            string[] queries = query.Split(':');
            if (query.Length >= 2)
            {
                _SelectedComment = queries[0];
                string[] subjectQueries = queries[1].Replace(" ", "").Split(';');
                int lenSh = Math.Min(SubjectHeaderList.Count, subjectQueries.Length);
                for (int i = 0; i < lenSh; i++)
                {
                    string[] classTypeQueries = subjectQueries[i].Split(',');
                    int lenCt = Math.Min(SubjectHeaderList[i].ClassTypes.Count, classTypeQueries.Length);
                    for (int j = 0; j < lenCt; j++)
                    {
                        if (
                            classTypeQueries[j] != String.Empty &&
                            int.TryParse(classTypeQueries[j], out int classQuery))
                        {
                            ClassType classType = SubjectHeaderList[i].ClassTypes[j];
                            if (classQuery != 0)
                            {
                                UniClass? uniClass = classType.Classes.Find((uc) => uc.Number == classQuery);
                                if (uniClass != null)
                                {
                                    classType.SelectClass(uniClass);
                                }
                            }
                            else if (classType.Classes.Count >= 2) // && classQuery = 0
                            {
                                classType.SelectClass(null);
                            }
                        }
                    }
                }
            }
            GetSelected();
        }

        private string _SelectedComment = "Selected";
        private string _SelectedData = "Selected";
        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string Selected
        {
            get => _SelectedComment + _SelectedData + _Sum;
            set
            {
                if (!value.StartsWith(_SelectedComment + _SelectedData))
                    SetSelected(value);
            }
        }

        #endregion

        #region Statistics

        private string _Sum = " : ...";

        /// <summary>
        /// Gets or sets the estimated time it takes to commute to uni
        /// </summary>
        public TimeSpan CommuteTime
        {
            get => Settings.Default.CommuteTime;
            set
            {
                if (Settings.Default.CommuteTime != value)
                {
                    Settings.Default.CommuteTime = value;
                    Settings.Default.Save();
                    ComputeStatistics();
                    OnPropertyChanged(nameof(CommuteTime));
                }
            }
        }


        /// <summary>
        /// Calls <see cref="WeekBucket.ComputeStatistics"/> on each
        /// weekbucket in <see cref="Buckets"/>
        /// </summary>
        private void ComputeStatistics()
        {
            (double, double) sum = (0.0, 0);
            foreach (WeekBucket weekBucket in Buckets)
            {
                (double,double) result = weekBucket.ComputeStatistics(CommuteTime.TotalHours);
                sum = (sum.Item1 + result.Item1, sum.Item2 + result.Item2);
            }
            _Sum = $" : Total Classtime: {sum.Item1}h | Total Commitment: {sum.Item2:#0.#}h";
        }

        #endregion
    }
}
