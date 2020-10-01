// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Scenes
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for ActorSelector.xaml.
	/// </summary>
	public partial class ActorSelector : UserControl, IDialog<Dictionary<SceneFile.SceneActor, ActorViewModel>>
	{
		public ActorSelector(List<SceneFile.SceneActor> actors)
		{
			this.InitializeComponent();
			this.Result = new Dictionary<SceneFile.SceneActor, ActorViewModel>();
		}

		public event DialogEvent? Close;

		public Dictionary<SceneFile.SceneActor, ActorViewModel> Result { get; set; }

		public static async Task<Dictionary<SceneFile.SceneActor, ActorViewModel>> GetActors(List<SceneFile.SceneActor> actors)
		{
			ActorSelector selector = new ActorSelector(actors);
			return await ViewService.ShowDialog<ActorSelector, Dictionary<SceneFile.SceneActor, ActorViewModel>>("Select Actors", selector);
		}

		public void Cancel()
		{
			this.Result.Clear();
			this.Close?.Invoke();
		}
	}
}
