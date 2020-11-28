// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using Anamnesis.Styles.DependencyProperties;
	using global::Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AnimationSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AnimationEditor : UserControl
	{
		public static readonly IBind<int> AnimationIdDp = Binder.Register<int, AnimationEditor>(nameof(AnimationId), OnAnimationIdChanged);

		public AnimationEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public int AnimationId
		{
			get => AnimationIdDp.Get(this);
			set => AnimationIdDp.Set(this, value);
		}

		private static void OnAnimationIdChanged(AnimationEditor sender, int value)
		{
			sender.GetName();
		}

		private void OnSelectAnimation(object sender, RoutedEventArgs e)
		{
			Animation? current = null;

			if (GameDataService.Animations.Contains(this.AnimationId))
				current = GameDataService.Animations.Get(this.AnimationId);

			GenericSelector.Show(current, GameDataService.Animations, (v) =>
			{
				if (v == null)
					return;

				this.AnimationId = v.Key;
			});
		}

		private void GetName()
		{
			string? name = null;

			if (GameDataService.Animations.Contains(this.AnimationId))
			{
				Animation anim = GameDataService.Animations.Get(this.AnimationId);
				name = anim.Name;
			}

			Application.Current.Dispatcher.Invoke(() => this.AnimationName.Text = name);
		}
	}
}
