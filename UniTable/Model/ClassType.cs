using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UniTable
{
    /// <summary>
    /// Represents a single selection of <see cref="UniClass"/>
    /// </summary>
    public partial class ClassType : ObservableObject
    {
        /// <summary>
        /// Gets a longform description of the type of class
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of possible classes the student may enrol in
        /// to fulfill the requirements of this <see cref="ClassType"/>
        /// </summary>
        public List<UniClass> Classes { get; set; } = new();

        private UniClass? _SelectedClass;

        /// <summary>
        /// Sets the single selected <see cref="UniClass"/> for this object
        /// then calls <see cref="UniModel.GetSelected"/> to allow
        /// for updating the display
        /// </summary>
        [XmlIgnore]
        public UniClass? SelectedClass
        {
            get { return _SelectedClass; }
            set
            {
                if (_SelectedClass != value)
				{
					if (_SelectedClass != null)
					{
						_SelectedClass.IsSelected = false;
					}
					_SelectedClass = value;
					if (_SelectedClass != null)
					{
						_SelectedClass.IsSelected = true;
					}
					OnPropertyChanged(nameof(SelectedClass));
				}
            }
        }

        private bool _IsFocused = false;
        /// <summary>
        /// Gets or sets whether this class type is currently being selected by the user through the Ui
        /// </summary>
        [XmlIgnore]
        public bool IsFocused
        {
            get => _IsFocused;
            set => SetProperty(ref _IsFocused, value);
        }

		#region Init

		public ClassType()
        {

        }

		public ClassType(string description)
        {
            Description = description;
        }

		#endregion

		public override string ToString()
        {
            return Description;
        }
    }
}
