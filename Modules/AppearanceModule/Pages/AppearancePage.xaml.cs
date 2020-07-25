// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.AppearanceModule.Dialogs;
	using ConceptMatrix.AppearanceModule.Files;
	using ConceptMatrix.AppearanceModule.Views;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class AppearancePage : UserControl
	{
		private ISelectionService selectionService;
		private IActorRefreshService refreshService;

		public AppearancePage()
		{
			this.selectionService = Services.Get<ISelectionService>();
			this.refreshService = Services.Get<IActorRefreshService>();

			this.refreshService.RefreshBegin += this.RefreshService_RefreshBegin;
			this.refreshService.RefreshComplete += this.RefreshService_RefreshComplete;

			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public bool IsOverworld { get; private set; }
		public Actor Actor { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged += this.SelectionModeChanged;
			this.OnActorChanged(this.DataContext as Actor);
			this.SelectionModeChanged(this.selectionService.GetMode());
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged -= this.SelectionModeChanged;
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			await this.Load();
		}

		private void OnLoadNpcClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<NpcSelector, INpcResident>("Select NPC", null, (v) => { this.ApplyNpc(v.Appearance); });
		}

		private async void ApplyNpc(INpcBase npc)
		{
			AppearanceFile apFile = npc.ToFile();
			await apFile.Apply(this.Actor, AppearanceFile.SaveModes.All);
		}

		private async Task Load()
		{
			IFileService fileService = Services.Get<IFileService>();
			IViewService viewService = Services.Get<IViewService>();

			FileBase file = await fileService.OpenAny(
				LegacyEquipmentSetFile.FileType,
				LegacyAppearanceFile.AllFileType,
				AppearanceFile.FileType);

			bool advanced = file?.UseAdvancedLoad ?? false;

			if (file is LegacyAppearanceFile legacyAllFile)
				file = legacyAllFile.Upgrade();

			if (file is LegacyEquipmentSetFile legacyEquipmentFile)
				file = legacyEquipmentFile.Upgrade();

			if (file is AppearanceFile apFile)
			{
				AppearanceFile.SaveModes mode = AppearanceFile.SaveModes.All;

				if (advanced)
				{
					mode = await viewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Load Appearance...");

					if (mode == AppearanceFile.SaveModes.None)
					{
						return;
					}
				}

				await apFile.Apply(this.Actor, mode);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();
			IFileService fileService = Services.Get<IFileService>();

			await fileService.Save(
				async (advancedMode) =>
				{
					AppearanceFile.SaveModes mode = AppearanceFile.SaveModes.All;

					if (advancedMode)
						mode = await viewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Save Appearance...");

					AppearanceFile file = new AppearanceFile();

					if (mode == AppearanceFile.SaveModes.None)
						return null;

					file.Read(this.Actor, mode);

					return file;
				},
				AppearanceFile.FileType);
		}

		private void OnActorChanged(Actor actor)
		{
			this.Actor = actor;
			bool hasValidSelection = actor != null && (actor.Type == ActorTypes.Player || actor.Type == ActorTypes.BattleNpc || actor.Type == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}

		private void SelectionModeChanged(Modes mode)
		{
			this.IsOverworld = mode == Modes.Overworld;
		}

		private void RefreshService_RefreshComplete(Actor actor)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;
			});
		}

		private void RefreshService_RefreshBegin(Actor actor)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = false;
			});
		}
	}
}
