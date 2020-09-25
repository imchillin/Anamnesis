// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for TargetSelectorView.xaml.
	/// </summary>
	public partial class TargetSelectorView : UserControl, IDrawer
	{
		public TargetSelectorView()
		{
			this.InitializeComponent();
		}

		public event DrawerEvent? Close;

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			foreach (TargetService.ActorTableActor? actor in TargetService.Instance.AllActors)
			{
				this.Selector.Items.Add(actor);
			}

			this.Selector.FilterItems();
		}

		private void OnCloseClicked(object sender, RoutedEventArgs e)
		{
			this.Close?.Invoke();
		}

		private void OnClose()
		{
			this.Close?.Invoke();

			TargetService.ActorTableActor? actor = this.Selector.Value as TargetService.ActorTableActor;

			if (actor == null)
				return;

			TargetService.AddActor(actor);
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is TargetService.ActorTableActor item)
			{
				if (!SearchUtility.Matches(item.Name, search))
					return false;

				if (TargetService.Instance.Actors.Contains(item))
					return false;

				return true;
			}

			return false;
		}
	}
}
