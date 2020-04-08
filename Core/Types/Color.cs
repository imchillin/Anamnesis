// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.ComponentModel;

	public class Color : INotifyPropertyChanged
	{
		private float r = 1;
		private float g = 1;
		private float b = 1;

		public Color()
		{
		}

		public Color(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public float R
		{
			get
			{
				return this.r;
			}

			set
			{
				this.r = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.R)));
			}
		}

		public float G
		{
			get
			{
				return this.g;
			}

			set
			{
				this.g = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.G)));
			}
		}

		public float B
		{
			get
			{
				return this.b;
			}

			set
			{
				this.b = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.B)));
			}
		}
	}
}
