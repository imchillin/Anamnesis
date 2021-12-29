// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public partial class AnimationEditor : UserControl
	{
		public AnimationEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public ActorMemory? Actor { get; private set; }
		public AnimationService AnimationService => AnimationService.Instance;
		public GposeService GPoseService => GposeService.Instance;

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
	}
}
