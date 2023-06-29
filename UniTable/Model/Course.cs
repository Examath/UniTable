using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UniTable
{
    /// <summary>
    /// Represents information pretaining to the Subject of a <see cref="UniClass"/>
    /// </summary>
    internal class SubjectHeader
    {
        /// <summary>
        /// Short code for the subject
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Unique 4-digit subject number
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Longform name of subject
        /// </summary>
        public string Name { get; private set; }

        public Color Color { get; private set; }

        /// <summary>
        /// List containing various types of classes
        /// </summary>
        public List<ClassType> ClassTypes { get; set; } = new();

        /// <summary>
        /// Creates a subject header from the relavent data
        /// </summary>
        /// <param name="param">
        /// In format<c>CODE WORD 1000 - Name Of Subject - #HEXCOLOR</c>
        /// </param>
        public SubjectHeader(string param)
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

        public override string ToString()
        {
            return $"{Code} / {Number} - {Name}";
        }
    }
}
