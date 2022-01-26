// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
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
		public static readonly IBind<Types> TypeDp = Binder.Register<Types, SubActorEditor>(nameof(SubActorType), OnTypeChanged);

		public SubActorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			OnTypeChanged(this, this.SubActorType);
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
		public ImageSource? IconSource { get; set; }
		public string TypeKey => "SubActor_" + this.SubActorType;

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

		private static void OnTypeChanged(SubActorEditor sender, Types value)
		{
			try
			{
				BitmapImage logo = new BitmapImage();
				logo.BeginInit();
				logo.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
				logo.UriSource = new Uri("pack://application:,,,/Anamnesis;component/Assets/Slots/" + value.ToString() + ".png");
				logo.EndInit();
				sender.IconSource = logo;
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get icon for sub actor type: {value}", ex);
			}
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
		}
	}
}