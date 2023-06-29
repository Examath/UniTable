using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTable
{
    /// <summary>
    /// Represents a single selection of <see cref="UniClass"/>
    /// </summary>
    internal partial class ClassType : ObservableObject
    {
        /// <summary>
        /// Gets a longform description of the type of class
        /// </summary>
        public string Description { get; private set; }

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
        public UniClass? SelectedClass
        {
            get { return _SelectedClass; }
            set
            {
                if (_SelectedClass != value)
                {
                    SelectClass(value);
                    Parent.GetSelected();
                }
            }
        }

        /// <summary>
        /// Changes select class without calling <see cref="UniModel.GetSelected"/>
        /// </summary>
        /// <param name="uniClass"></param>
        internal void SelectClass(UniClass? uniClass)
        {
            if (_SelectedClass != null)
            {
                _SelectedClass.IsSelected = false;
            }
            _SelectedClass = uniClass;
            if (_SelectedClass != null)
            {
                _SelectedClass.IsSelected = true;
            }
            OnPropertyChanged(nameof(SelectedClass));
        }

        private bool _IsFocused = false;
        /// <summary>
        /// Gets or sets whether this class type is currently being selected by the user
        /// </summary>
        public bool IsFocused
        {
            get => _IsFocused;
            set => SetProperty(ref _IsFocused, value);
        }

        /// <summary>
        /// Sets the parent that this class will call to update any selection change.
        /// </summary>
        public UniModel Parent { get; set; }

        public ClassType(string description, UniModel parent)
        {
            Description = description;
            Parent = parent;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
