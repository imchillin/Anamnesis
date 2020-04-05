// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.ComponentModel;

	public class Color : INotifyPropertyChanged
	{
		private double r = 1;
		private double g = 1;
		private double b = 1;

		public Color()
		{
		}

		public Color(double r, double g, double b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double R
		{
			get
			{
				return this.r;
			}

			set
			{
				this.r = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color.R)));
			}
		}

		public double G
		{
			get
			{
				return this.g;
			}

			set
			{
				this.g = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color.G)));
			}
		}

		public double B
		{
			get
			{
				return this.b;
			}

			set
			{
				this.b = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color.B)));
			}
		}
	}
}
