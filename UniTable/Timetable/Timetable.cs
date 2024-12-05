using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTable.Timetable
{
	public class Timetable : ObservableObject
	{
		private ObservableCollection<WeekBucket> _Buckets = new();
		/// <summary>
		/// Gets or sets a list of weeks containing all sessions
		/// </summary>
		public ObservableCollection<WeekBucket> Buckets
		{
			get => _Buckets;
			set => SetProperty(ref _Buckets, value);
		}
	}
}
