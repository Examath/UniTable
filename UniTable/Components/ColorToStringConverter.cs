using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UniTable.Components
{
	public class ColorToStringConverter : IValueConverter
	{
		// Color → string
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color c)
			{
				// Default hex format: "#RRGGBB" if fully opaque, "#AARRGGBB" otherwise
				return c.A == 255
					? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
					: $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
			}

			return "";
		}

		// string → Color
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && !string.IsNullOrWhiteSpace(s))
			{
				try
				{
					// ColorConverter handles named colors and hex (#RGB, #ARGB, etc.)
					var obj = ColorConverter.ConvertFromString(s);
					if (obj is Color color)
						return color;
				}
				catch
				{
					// fall through to default below
				}
			}

			// fallback: Black
			return Colors.Black;
		}
	}
}