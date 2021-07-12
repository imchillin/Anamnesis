// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;
	using XivToolsWpf.DependencyProperties;

	/// <summary>
	/// Interaction logic for FacialFeaturesControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FacialFeaturesControl : UserControl
	{
		public static readonly IBind<Customize.FacialFeature> ValueDp = Binder.Register<Customize.FacialFeature, FacialFeaturesControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<Customize.Genders> GenderDp = Binder.Register<Customize.Genders, FacialFeaturesControl>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Customize.Tribes> TribeDp = Binder.Register<Customize.Tribes, FacialFeaturesControl>(nameof(Tribe), OnTribeChanged);
		public static readonly IBind<byte> HeadDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Head), OnHeadChanged);

		private readonly List<Option> features = new List<Option>();
		private bool locked = false;

		public FacialFeaturesControl()
		{
			this.InitializeComponent();
			OnValueChanged(this, this.Value);
		}

		public Customize.Genders Gender
		{
			get => GenderDp.Get(this);
			set => GenderDp.Set(this, value);
		}

		public Customize.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		public byte Head
		{
			get => HeadDp.Get(this);
			set => HeadDp.Set(this, value);
		}

		public Customize.FacialFeature Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		private static void OnGenderChanged(FacialFeaturesControl sender, Customize.Genders value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnTribeChanged(FacialFeaturesControl sender, Customize.Tribes value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnHeadChanged(FacialFeaturesControl sender, byte value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnValueChanged(FacialFeaturesControl sender, Customize.FacialFeature value)
		{
			sender.locked = true;
			foreach (Option op in sender.features)
			{
				op.Selected = sender.Value.HasFlag(op.Value);
			}

			sender.locked = false;
		}

		private void GetFeatures()
		{
			this.locked = true;
			this.FeaturesList.ItemsSource = null;

			if (this.Tribe == 0)
				return;

			ImageSource[]? facialFeatures = null;
			if (GameDataService.CharacterMakeTypes != null)
			{
				foreach (ICharaMakeType set in GameDataService.CharacterMakeTypes)
				{
					if (set.Tribe != this.Tribe)
						continue;

					if (set.Gender != this.Gender)
						continue;

					facialFeatures = set.FacialFeatures.ToArray();
					break;
				}
			}

			if (facialFeatures == null)
				return;

			this.features.Clear();
			for (byte i = 0; i < 7; i++)
			{
				int id = (this.Head - 1) + (8 * i);

				if (id < 0 || id >= facialFeatures.Length)
					continue;

				Option op = new Option();
				op.Icon = facialFeatures[id];
				op.Value = this.GetValue(i);
				op.Index = id;
				this.features.Add(op);
			}

			Option legacyTattoo = new Option();
			legacyTattoo.Value = Customize.FacialFeature.LegacyTattoo;
			this.features.Add(legacyTattoo);

			this.FeaturesList.ItemsSource = this.features;
			this.locked = false;
		}

		private Customize.FacialFeature GetValue(int index)
		{
			switch (index)
			{
				case 0: return Customize.FacialFeature.First;
				case 1: return Customize.FacialFeature.Second;
				case 2: return Customize.FacialFeature.Third;
				case 3: return Customize.FacialFeature.Fourth;
				case 4: return Customize.FacialFeature.Fifth;
				case 5: return Customize.FacialFeature.Sixth;
				case 6: return Customize.FacialFeature.Seventh;
			}

			throw new Exception("Invalid index value");
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.locked)
				return;

			Customize.FacialFeature flags = Customize.FacialFeature.None;
			foreach (Option? op in this.FeaturesList.SelectedItems)
			{
				if (op == null)
					continue;

				flags |= op.Value;
			}

			this.Value = flags;
		}

		[AddINotifyPropertyChangedInterface]
		private class Option
		{
			public Customize.FacialFeature Value { get; set; }
			public ImageSource? Icon { get; set; }
			public int Index { get; set; }
			public bool Selected { get; set; }
		}
	}
}
