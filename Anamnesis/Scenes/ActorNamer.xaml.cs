// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Scenes
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ActorSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ActorNamer : UserControl, IDialog<Dictionary<TargetService.ActorTableActor, SceneFile.SceneActor>>
	{
		public ActorNamer(IEnumerable<TargetService.ActorTableActor> actors)
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			foreach (TargetService.ActorTableActor actor in actors)
			{
				this.Entries.Add(new Entry(actor));
			}
		}

		public event DialogEvent? Close;

		public ObservableCollection<Entry> Entries { get; set; } = new ObservableCollection<Entry>();
		public Dictionary<TargetService.ActorTableActor, SceneFile.SceneActor> Result { get; set; } = new Dictionary<TargetService.ActorTableActor, SceneFile.SceneActor>();

		public static async Task<Dictionary<TargetService.ActorTableActor, SceneFile.SceneActor>> GetActors(IEnumerable<TargetService.ActorTableActor> actors)
		{
			ActorNamer selector = new ActorNamer(actors);
			return await ViewService.ShowDialog<ActorNamer, Dictionary<TargetService.ActorTableActor, SceneFile.SceneActor>>("Name Actors", selector);
		}

		public void Cancel()
		{
			this.Result.Clear();
			this.Close?.Invoke();
		}

		private void OnOkClicked(object sender, RoutedEventArgs e)
		{
			foreach (Entry entry in this.Entries)
			{
				this.Result.Add(entry.TableActor, entry.SceneActor);
			}

			this.Close?.Invoke();
		}

		[AddINotifyPropertyChangedInterface]
		public class Entry
		{
			public Entry(TargetService.ActorTableActor actor)
			{
				this.SceneActor = new SceneFile.SceneActor();
				this.TableActor = actor;
			}

			public SceneFile.SceneActor SceneActor { get; set; }
			public TargetService.ActorTableActor TableActor { get; set; }
		}
	}
}
