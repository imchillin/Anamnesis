// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Scenes
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ActorSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ActorSelector : UserControl, IDialog<Dictionary<SceneFile.SceneActor, TargetService.PinnedActor>>
	{
		public ActorSelector(List<SceneFile.SceneActor> actors)
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			foreach (SceneFile.SceneActor actor in actors)
			{
				this.Entries.Add(new Entry(actor));
			}
		}

		public event DialogEvent? Close;

		public ObservableCollection<Entry> Entries { get; set; } = new ObservableCollection<Entry>();
		public Dictionary<SceneFile.SceneActor, TargetService.PinnedActor> Result { get; set; } = new Dictionary<SceneFile.SceneActor, TargetService.PinnedActor>();

		public static async Task<Dictionary<SceneFile.SceneActor, TargetService.PinnedActor>> GetActors(List<SceneFile.SceneActor> actors)
		{
			ActorSelector selector = new ActorSelector(actors);
			return await ViewService.ShowDialog<ActorSelector, Dictionary<SceneFile.SceneActor, TargetService.PinnedActor>>("Select Actors", selector);
		}

		public void Cancel()
		{
			this.Result.Clear();
			this.Close?.Invoke();
		}

		private void OnOkClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			foreach (Entry entry in this.Entries)
			{
				if (entry.Actor == null)
					continue;

				this.Result.Add(entry.SceneActor, entry.Actor);
			}

			this.Close?.Invoke();
		}

		[AddINotifyPropertyChangedInterface]
		public class Entry
		{
			public readonly SceneFile.SceneActor SceneActor;

			public Entry(SceneFile.SceneActor scene)
			{
				this.SceneActor = scene;
			}

			public string? Identifier => this.SceneActor.Identifier;
			public List<ActorBasicViewModel> AllActors => TargetService.GetAllActors();

			public TargetService.PinnedActor? Actor { get; set; }
		}
	}
}
