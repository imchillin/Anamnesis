// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.MemoryBinds.Converters;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for WorldView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class WorldView : UserControl
	{
		private IMemory<int> timeMem;

		private int time = 0;
		private int moon = 0;

		public WorldView()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public int Time
		{
			get
			{
				return this.time;
			}

			set
			{
				this.time = value;
				this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
			}
		}

		public int Moon
		{
			get
			{
				return this.moon;
			}

			set
			{
				this.moon = value;
				this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			IInjectionService injection = ConceptMatrix.Services.Get<IInjectionService>();
			this.timeMem = injection.GetMemory(Offsets.Main.Time, Offsets.Main.TimeControl);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.timeMem.Dispose();
		}
	}
}
