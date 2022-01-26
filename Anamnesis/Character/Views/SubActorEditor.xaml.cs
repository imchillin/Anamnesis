// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;
	using XivToolsWpf.DependencyProperties;

	/// <summary>
	/// Interaction logic for SubActorEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SubActorEditor : UserControl
	{
		public static readonly IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, SubActorEditor>(nameof(Actor), BindMode.OneWay);
		public static readonly IBind<ActorMemory?> SubActorDp = Binder.Register<ActorMemory?, SubActorEditor>(nameof(SubActor), OnChanged, BindMode.TwoWay);
		public static readonly IBind<Types> TypeDp = Binder.Register<Types, SubActorEditor>(nameof(SubActorType));

		public SubActorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public enum Types
		{
			Mount,
			Companion,
			Ornament,
		}

		public ActorMemory? Actor
		{
			get => ActorDp.Get(this);
			set => ActorDp.Set(this, value);
		}

		public ActorMemory? SubActor
		{
			get => SubActorDp.Get(this);
			set => SubActorDp.Set(this, value);
		}

		public Types SubActorType
		{
			get => TypeDp.Get(this);
			set => TypeDp.Set(this, value);
		}

		public INpcBase? Npc { get; set; }

		private static void OnChanged(SubActorEditor sender, ActorMemory? value)
		{
			if (value == null || sender.Actor == null)
			{
				sender.Npc = null;
				return;
			}

			if (sender.SubActorType == Types.Companion)
			{
				sender.Npc = GameDataService.Companions.GetRow(value.DataId);
			}
			else if (sender.SubActorType == Types.Mount)
			{
				sender.Npc = GameDataService.Mounts.GetRow(sender.Actor.MountId);
			}
			else if (sender.SubActorType == Types.Ornament)
			{
				sender.Npc = GameDataService.Ornaments.GetRow(sender.Actor.CharacterModeInput);
			}
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
		}
	}
}