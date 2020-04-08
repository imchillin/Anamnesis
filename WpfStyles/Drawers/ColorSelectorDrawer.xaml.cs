// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Drawers
{
	using System.Reflection;
	using System.Windows.Controls;
	using System.Windows.Media;
	using ConceptMatrix.Services;

	using CmColor = ConceptMatrix.Color;

	/// <summary>
	/// Interaction logic for ColorSelectorDrawer.xaml.
	/// </summary>
	public partial class ColorSelectorDrawer : UserControl, IDrawer
	{
		public ColorSelectorDrawer()
		{
			this.InitializeComponent();

			PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo property in properties)
			{
				Color c = (Color)property.GetValue(null);

				if (c.A <= 0)
					continue;

				ColorOption op = new ColorOption();
				op.Name = property.Name;
				op.Color = c;

				this.Selector.Items.Add(op);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;

		public CmColor Value
		{
			get
			{
				ColorOption selected = this.Selector.Value as ColorOption;

				if (selected == null)
					return null;

				return selected.AsColor();
			}

			set
			{
			}
		}

		private void OnSelectorClose()
		{
			this.Close?.Invoke();
		}

		private bool OnFilter(object obj, string[] search = null)
		{
			if (obj is ColorOption color)
			{
				return SearchUtility.Matches(color.Name, search);
			}

			return false;
		}

		private class ColorOption
		{
			public string Name { get; set; }
			public Color Color { get; set; }

			public CmColor AsColor()
			{
				return new CmColor(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f);
			}
		}
	}
}
