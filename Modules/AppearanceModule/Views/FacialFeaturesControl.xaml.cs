// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for FacialFeaturesControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FacialFeaturesControl : UserControl
	{
		public static readonly IBind<Appearance.FacialFeature> ValueDp = Binder.Register<Appearance.FacialFeature, FacialFeaturesControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<Appearance.Genders> GenderDp = Binder.Register<Appearance.Genders, FacialFeaturesControl>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Appearance.Tribes> TribeDp = Binder.Register<Appearance.Tribes, FacialFeaturesControl>(nameof(Tribe), OnTribeChanged);
		public static readonly IBind<byte> HeadDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Head), OnHeadChanged);

		private List<Option> features = new List<Option>();

		public FacialFeaturesControl()
		{
			this.InitializeComponent();
			OnValueChanged(this, this.Value);
		}

		public Appearance.Genders Gender
		{
			get => GenderDp.Get(this);
			set => GenderDp.Set(this, value);
		}

		public Appearance.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		public byte Head
		{
			get => HeadDp.Get(this);
			set => HeadDp.Set(this, value);
		}

		public Appearance.FacialFeature Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		private static void OnGenderChanged(FacialFeaturesControl sender, Appearance.Genders value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnTribeChanged(FacialFeaturesControl sender, Appearance.Tribes value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnHeadChanged(FacialFeaturesControl sender, byte value)
		{
			sender.GetFeatures();
			OnValueChanged(sender, sender.Value);
		}

		private static void OnValueChanged(FacialFeaturesControl sender, Appearance.FacialFeature value)
		{
			foreach (Option op in sender.features)
			{
				op.Selected = sender.Value.HasFlag(op.Value);
			}
		}

		private void GetFeatures()
		{
			this.FeaturesList.ItemsSource = null;

			if (this.Tribe == 0)
				return;

			IGameDataService dataService = Services.Get<IGameDataService>();

			IImage[] facialFeatures = null;
			foreach (ICharaMakeType set in dataService.CharacterMakeTypes.All)
			{
				if (set.Tribe != this.Tribe)
					continue;

				if (set.Gender != this.Gender)
					continue;

				facialFeatures = set.FacialFeatures.ToArray();
				break;
			}

			if (facialFeatures == null)
				return;

			this.features.Clear();
			for (byte i = 0; i < 7; i++)
			{
				int id = (this.Head - 1) + (i * 4);

				if (id < 0 || id >= facialFeatures.Length)
					continue;

				Option op = new Option();
				op.Icon = facialFeatures[id];
				op.Value = this.GetValue(i);
				this.features.Add(op);
			}

			Option legacyTattoo = new Option();
			legacyTattoo.Icon = Properties.Resources.LegacyTattoo.ToIImage();
			legacyTattoo.Value = Appearance.FacialFeature.LegacyTattoo;
			this.features.Add(legacyTattoo);

			this.FeaturesList.ItemsSource = this.features;
		}

		private Appearance.FacialFeature GetValue(int index)
		{
			switch (index)
			{
				case 0: return Appearance.FacialFeature.First;
				case 1: return Appearance.FacialFeature.Second;
				case 2: return Appearance.FacialFeature.Third;
				case 3: return Appearance.FacialFeature.Fourth;
				case 4: return Appearance.FacialFeature.Fifth;
				case 5: return Appearance.FacialFeature.Sixth;
				case 6: return Appearance.FacialFeature.Seventh;
			}

			throw new Exception("Invalid index value");
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Appearance.FacialFeature flags = Appearance.FacialFeature.None;
			foreach (Option op in this.FeaturesList.SelectedItems)
			{
				flags |= op.Value;
			}

			this.Value = flags;
		}

		[AddINotifyPropertyChangedInterface]
		private class Option
		{
			public Appearance.FacialFeature Value { get; set; }
			public IImage Icon { get; set; }
			public bool Selected { get; set; }
		}
	}
}
