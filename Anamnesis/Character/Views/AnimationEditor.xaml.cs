// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using Serilog;

	[AddINotifyPropertyChangedInterface]
	public partial class AnimationEditor : UserControl
	{
		private bool slowMotion;
		private uint animationId = 8047;

		public AnimationEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public ActorMemory? Actor { get; private set; }
		public AnimationService AnimationService => AnimationService.Instance;
		public GposeService GposeService => GposeService.Instance;

		public int RepeatTimer { get; set; } = 0;

		public uint AnimationId
		{
			get
			{
				return this.animationId;
			}
			set
			{
				this.animationId = value;
				////this.OnPlayClicked(null, null);
			}
		}

		public bool SlowMotion
		{
			get
			{
				return this.slowMotion;
			}
			set
			{
				this.slowMotion = value;
				this.OnPlayClicked(null, null);
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.DataContext is ActorMemory actor)
			{
				this.Actor = actor;
			}
			else
			{
				this.Actor = null;
			}
		}

		private void OnSearchClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<AnimationSelector, ActionTimeline>(null, (action) =>
			{
				if (action == null)
					return;

				this.AnimationId = action.RowId;
				this.OnPlayClicked(null, null);
			});
		}

		private void OnDrawWeaponClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			this.AnimationService.AnimateActor(this.Actor, 190);
		}

		private void OnPlayClicked(object? sender, RoutedEventArgs? e)
		{
			if (this.Actor == null)
				return;

			ActorMemory.AnimationModes mode = this.SlowMotion ? ActorMemory.AnimationModes.SlowMotion : ActorMemory.AnimationModes.Normal;
			this.AnimationService.AnimateActor(this.Actor, this.AnimationId, mode, this.RepeatTimer);
		}

		private void OnResetClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			this.AnimationService.AnimateActor(this.Actor, 3);
		}
	}
}