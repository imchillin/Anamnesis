// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Media.Media3D;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for TargetSelectorView.xaml.
	/// </summary>
	public partial class TargetSelectorView : UserControl
	{
		private SelectionService selection;
		private IInjectionService injection;

		public TargetSelectorView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.selection = App.Services.Get<SelectionService>();
			this.injection = App.Services.Get<IInjectionService>();

			this.DataContext = this;

			this.GetEntities();
		}

		public ObservableCollection<PossibleSelection> Entities { get; set; } = new ObservableCollection<PossibleSelection>();
		public string InGameSelection { get; set; }

		private void GetEntities()
		{
			try
			{
				bool isGpose = true;

				// clear the entity list
				this.Entities.Clear();

				if (isGpose)
				{
					IMemory<int> countMem = this.injection.GetMemory<int>(BaseAddresses.GPoseEntity);
					int count = countMem.Get();

					for (int i = 0; i < count; i++)
					{
						// ew
						string addr = (long.Parse(this.injection.Offsets.GposeEntityOffset, NumberStyles.HexNumber) + long.Parse(((i + 1) * 8).ToString("X"), NumberStyles.HexNumber)).ToString("X");

						////IMemory<Vector3D> positionMem = this.injection.GetMemory<Vector3D>(addr, this.injection.Offsets.Character.Body.Base, this.injection.Offsets.Character.Body.Position.X)

						IMemory<string> actorIdMem = this.injection.GetMemory<string>(BaseAddresses.GPose, this.injection.Offsets.Character.ActorID);
						IMemory<string> nameMem = this.injection.GetMemory<string>(addr, this.injection.Offsets.Character.Name);

						string name = nameMem.Get();
						string actorId = actorIdMem.Get();

						PossibleSelection selection = new PossibleSelection(Selection.Types.Character, BaseAddresses.GPose, actorId, name);
						selection.IsSelected = !this.selection.UseGameTarget && this.selection.CurrentSelection != null && this.selection.CurrentSelection.Name == name;
						this.Entities.Add(selection);
					}
				}
				else
				{
					throw new NotImplementedException();
				}

				this.AutoRadio.IsChecked = this.selection.UseGameTarget;
				this.InGameSelection = this.selection.CurrentGameTarget?.Name;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void OnAutoSelected(object sender, RoutedEventArgs e)
		{
			this.selection.UseGameTarget = true;
			this.selection.CurrentSelection = null;
		}

		private void OnSelected(object sender, RoutedEventArgs e)
		{
			if (sender is RadioButton btn)
			{
				PossibleSelection selection = btn.DataContext as PossibleSelection;
				this.selection.UseGameTarget = false;
				this.selection.CurrentSelection = selection;
			}
		}

		public class PossibleSelection : Selection
		{
			public PossibleSelection(Types type, BaseAddresses address, string actorId, string name)
				: base(type, address, actorId, name)
			{
			}

			public bool IsSelected { get; set; }
		}
	}
}
