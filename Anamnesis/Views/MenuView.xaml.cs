// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using FontAwesome.Sharp;

	using TextBlock = Anamnesis.Styles.Controls.TextBlock;

	/// <summary>
	/// Interaction logic for MenuView.xaml.
	/// </summary>
	public partial class MenuView : UserControl
	{
		public static MenuItem File = MenuExtensions.CreateMenuItem("File");
		public static MenuItem Export = File.AddSubItem("Export");

		public MenuView()
		{
			this.InitializeComponent();

			this.Menu.Items.Add(File);

			TargetService.ActorPinned += this.OnActorPinned;
			foreach (TargetService.ActorTableActor actor in TargetService.Instance.PinnedActors)
			{
				this.OnActorPinned(actor);
			}
		}

		private void OnActorPinned(TargetService.ActorTableActor actor)
		{
			MenuItem item = Export.AddSubItem(actor.DisplayName);
			item.AddSubItem("Appearance", null, () => this.ExportAppearance(actor));
			item.AddSubItem("Pose", null, () => this.ExportPose(actor));
		}

		private async void ExportAppearance(TargetService.ActorTableActor actor)
		{
			await CharacterFile.Save(actor.GetViewModel());
		}

		private async void ExportPose(TargetService.ActorTableActor actor)
		{
			ActorViewModel? actorVm = actor.GetViewModel();

			if (actorVm == null)
				return;

			SkeletonVisual3d skeletonVm = await PoseService.GetVisual(actorVm);
			await PoseFile.Save(actorVm, skeletonVm, null, false);
		}
	}

	#pragma warning disable SA1204, SA1402
	public static class MenuExtensions
	{
		public static MenuItem AddSubItem(this MenuItem parent, string headerKey, IconChar? icon = null, Action? callback = null)
		{
			MenuItem item = CreateMenuItem(headerKey, icon, callback);
			parent.Items.Add(item);
			return item;
		}

		public static MenuItem CreateMenuItem(string header, IconChar? icon = null, Action? callback = null)
		{
			MenuItem item = new MenuItem();
			item.Style = Application.Current.FindResource("AnaMenuItem") as Style;
			item.Click += (s, e) => callback?.Invoke();

			TextBlock textBlock = new TextBlock();
			////textBlock.Key = headerKey;
			textBlock.Text = header;
			item.Header = textBlock;

			if (icon != null)
			{
				IconBlock iconBlock = new IconBlock();
				iconBlock.Icon = (IconChar)icon;
				item.Icon = iconBlock;
			}

			return item;
		}
	}
}
