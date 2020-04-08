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
	using ConceptMatrix.Injection.Offsets;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;
	using ConceptMatrix.WpfStyles;
	using FontAwesome.Sharp;
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
		public IconChar InGameIcon { get; set; }

		private void GetEntities()
		{
			try
			{
				Selection.Modes mode = this.selection.GetMode();
				ActorTableOffset actorTableOffset;
				BaseOffset targetOffset;

				// clear the entity list
				this.Entities.Clear();

				if (mode == Selection.Modes.GPose)
				{
					actorTableOffset = Offsets.GposeActorTable;
					targetOffset = Offsets.Gpose;
				}
				else if (mode == Selection.Modes.Overworld)
				{
					actorTableOffset = Offsets.ActorTable;
					targetOffset = Offsets.Target;
				}
				else
				{
					throw new Exception("Unknown selection mode: " + mode);
				}

				byte count = actorTableOffset.GetCount();
				HashSet<string> ids = new HashSet<string>();

				for (byte i = 0; i < count; i++)
				{
					ActorTypes type = actorTableOffset.GetActorValue(i, Offsets.ActorType);
					string name = actorTableOffset.GetActorValue(i, Offsets.Name);

					string id = mode.ToString() + "_" + type + "_" + name;

					if (ids.Contains(id))
						continue;

					ids.Add(id);

					if (string.IsNullOrEmpty(name))
						name = "Unknown";

					PossibleSelection selection = new PossibleSelection(type, targetOffset, id, name, mode);
					selection.IsSelected = !this.selection.UseGameTarget && this.selection.CurrentSelection != null && this.selection.CurrentSelection.Name == name;
					this.Entities.Add(selection);
				}

				this.AutoRadio.IsChecked = this.selection.UseGameTarget;

				if (this.selection.CurrentGameTarget != null)
				{
					this.InGameSelection = this.selection.CurrentGameTarget.Name;
					this.InGameIcon = this.selection.CurrentGameTarget.Type.GetIcon();
				}
				else
				{
					this.InGameSelection = null;
					this.InGameIcon = IconChar.None;
				}
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
			public PossibleSelection(ActorTypes type, IBaseMemoryOffset address, string actorId, string name, Modes mode)
				: base(type, address, actorId, name, mode)
			{
			}

			public bool IsSelected { get; set; }

			public IconChar Icon
			{
				get
				{
					return this.Type.GetIcon();
				}
			}
		}
	}
}
