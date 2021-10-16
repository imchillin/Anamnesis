// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for ModelTypeEditor.xaml.
	/// </summary>
	public partial class ModelTypeEditor : UserControl
	{
		private static Dictionary<int, string>? modelTypeNameLookup;

		public ModelTypeEditor()
		{
			this.InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (modelTypeNameLookup == null)
			{
				modelTypeNameLookup = new Dictionary<int, string>();

				if (GameDataService.Monsters != null)
				{
					foreach (Monster monster in GameDataService.Monsters)
					{
						if (modelTypeNameLookup.ContainsKey(monster.ModelType))
						{
							string str = modelTypeNameLookup[monster.ModelType];
							str += " / " + monster.Name;
							modelTypeNameLookup[monster.ModelType] = str;
						}
						else
						{
							modelTypeNameLookup.Add(monster.ModelType, monster.Name);
						}
					}
				}
			}

			this.GetModelName();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ActorMemory? actorVm = this.DataContext as ActorMemory;

			if (actorVm == null)
				return;

			actorVm.PropertyChanged += this.OnActorVmPropertyChanged;
		}

		private void OnActorVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ActorMemory.ModelType))
			{
				this.GetModelName();
			}
		}

		private void GetModelName()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				ActorMemory? actorVm = this.DataContext as ActorMemory;

				if (actorVm == null)
					return;

				int modelTypeId = actorVm.ModelType;

				this.ModelName.Text = null;

				if (modelTypeNameLookup != null && modelTypeNameLookup.ContainsKey(actorVm.ModelType))
				{
					this.ModelName.Text = modelTypeNameLookup[actorVm.ModelType];
				}
			});
		}
	}
}
