using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace UniTable.Components
{	
	public class HorizontalRules : FrameworkElement
	{
		private readonly List<string> Days = new List<string> { "MON", "TUE", "WED", "THU", "FRI" };
		private const double TOP_DIVIDER_HEIGHT = 4;

		public double PixelsPerHour
		{
			get { return (double)GetValue(PixelsPerHourProperty); }
			set { SetValue(PixelsPerHourProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelsPerHour.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelsPerHourProperty =
			DependencyProperty.Register("PixelsPerHour", typeof(double), typeof(HorizontalRules), 
				new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPixelsPerHourChanged));

		private static void OnPixelsPerHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as HorizontalRules;
			control?.InvalidateVisual();
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			Brush dividerBrush = (Brush)FindResource("Divider");
			Brush lowBrush = (Brush)FindResource("GridBrush");
			Brush highBrush = Brushes.White;
			Typeface typeface = new("Segoe UI");

			double headerHeight = (double)FindResource("HeaderHeight");

			// Draw background and top divider
			double y = headerHeight;
			dc.DrawRectangle(lowBrush, null, new Rect(0, y, ActualWidth, ActualHeight - y));
			dc.DrawRectangle(dividerBrush, null, new Rect(0, y, ActualWidth, TOP_DIVIDER_HEIGHT));

			// Foreach day:
			foreach (var day in Days)
			{
				// Draw white band
				dc.DrawRectangle(highBrush, null, new Rect(0, y + 3 * PixelsPerHour, ActualWidth, 6 * PixelsPerHour));

				// Draw lines for each hour
				drawLine(highBrush, 1);
				drawLine(highBrush, 2);
				drawLine(lowBrush, 4);
				drawLine(lowBrush, 5);
				drawLine(lowBrush, 6, 2);
				drawLine(lowBrush, 7);
				drawLine(lowBrush, 8);
				drawLine(highBrush, 10);
				drawLine(highBrush, 11);
				drawLine(dividerBrush, 12, 2);

				// Draw Day Label
				FormattedText formattedText = new(day, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 16, dividerBrush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
				dc.PushTransform(new RotateTransform(-90, 0, y + 6 * PixelsPerHour));
				dc.DrawText(formattedText, new(-formattedText.Width / 2, y + 6 * PixelsPerHour - 2));
				dc.Pop();

				y += 12 * PixelsPerHour;

			}

			void drawLine(Brush b, double h, double t = 1)
			{
				double ay = y + h * PixelsPerHour;
				dc.DrawRectangle(b, null, new Rect(0, ay - t/2, ActualWidth, t));
			}
		}
	}
}
