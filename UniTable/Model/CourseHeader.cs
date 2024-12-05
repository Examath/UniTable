using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WPF.ColorPicker;

namespace UniTable
{
    /// <summary>
    /// Represents information pretaining to the Subject of a <see cref="UniClass"/>
    /// </summary>
    public partial class CourseHeader : ObservableObject
    {
        #region Properties

        /// <summary>
        /// Short code for the course
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Unique 4-digit course number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Longform name of course
        /// </summary>
        public string Name { get; set; } = string.Empty;

        private Color _Color = Colors.Black;
        /// <summary>
        /// Gets or sets the color of the course
        /// </summary>
        public Color Color
        {
            get => _Color;
            set => SetProperty(ref _Color, value);
        }

        /// <summary>
        /// List containing various types of classes
        /// </summary>
        public List<ClassType> ClassTypes { get; set; } = new();

        private void ClassType_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ClassType.IsFocused)) OnPropertyChanged(e);
        } 

        #endregion

        #region Init

        /// <summary>
        /// Creates a empty course header
        /// </summary>
        public CourseHeader()
		{

		}

		/// <summary>
		/// Creates a course header from the relavent data
		/// </summary>
		/// <param name="param">
		/// In format<c>CODE WORD 1000 - Name Of Subject - #HEXCOLOR</c>
		/// </param>
		public CourseHeader(string param)
        {
            string[] subParams = param.Split(" - ");
            string[] codeWords = subParams[0].Split(' ');
            Number = int.Parse(codeWords[^1]);
            Code = subParams[0][..(subParams[0].Length - 1 - codeWords[^1].Length)];
            Name = subParams[1];
            if (subParams.Length >= 2 && subParams[2].Contains('#'))
            {
                Color = (Color)ColorConverter.ConvertFromString(subParams[2]);
            }
        }

		public void Initialize()
		{
            foreach (ClassType classType in ClassTypes)
            {
				classType.PropertyChanged += ClassType_PropertyChanged;
            }
		}

        #endregion

        #region Commands

        [RelayCommand]
        internal void ChangeColour()
        {
            Color color = Color;
            bool ok = ColorPickerWindow.ShowDialog(out color);
            if (ok)
            {
                Color = color;
            }
        }

        [RelayCommand]
        internal void UpdateFromClipboard()
        {

        }

        #endregion

        public override string ToString()
        {
            return $"{Code} / {Number} - {Name}";
        }
    }
}
