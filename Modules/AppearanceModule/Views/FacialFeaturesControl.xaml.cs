// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.Services;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for FacialFeaturesControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FacialFeaturesControl : UserControl
	{
		public static readonly IBind<byte> ValueDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<Appearance.Genders> GenderDp = Binder.Register<Appearance.Genders, FacialFeaturesControl>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Appearance.Tribes> TribeDp = Binder.Register<Appearance.Tribes, FacialFeaturesControl>(nameof(Tribe), OnTribeChanged);
		public static readonly IBind<byte> HeadDp = Binder.Register<byte, FacialFeaturesControl>(nameof(Head), OnHeadChanged);

		public FacialFeaturesControl()
		{
			this.InitializeComponent();
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

		public byte Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		private static void OnGenderChanged(FacialFeaturesControl sender, Appearance.Genders value)
		{
			sender.GetFeatures();
		}

		private static void OnTribeChanged(FacialFeaturesControl sender, Appearance.Tribes value)
		{
			sender.GetFeatures();
		}

		private static void OnHeadChanged(FacialFeaturesControl sender, byte value)
		{
			sender.GetFeatures();
		}

		private static void OnValueChanged(FacialFeaturesControl sender, byte value)
		{
			sender.GetFeatures();
		}

		private void GetFeatures()
		{
			if (this.Tribe == 0)
				return;

			IGameDataService dataService = Module.Services.Get<IGameDataService>();

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
				throw new Exception("No facial features found");

			List<Option> features = new List<Option>();
			for (byte i = 0; i < 7; i++)
			{
				int id = this.Head + (i * 4);

				if (id < 0 || id > facialFeatures.Length)
					continue;

				Option op = new Option();
				op.Icon = facialFeatures[id];
				op.Value = this.GetValue(i);
				features.Add(op);
			}

			Option legacyTattoo = new Option();
			legacyTattoo.Icon = Properties.Resources.LegacyTattoo.ToIImage();
			legacyTattoo.Value = 128;
			features.Add(legacyTattoo);

			this.FeaturesList.ItemsSource = features;
		}

		private byte GetValue(int index)
		{
			switch (index)
			{
				case 0: return 0;
				case 1: return 1;
				case 2: return 2;
				case 3: return 4;
				case 4: return 8;
				case 5: return 16;
				case 6: return 32;
				case 7: return 64;
			}

			throw new Exception("Invalid index value");
		}

		private class Option
		{
			public byte Value { get; set; }
			public IImage Icon { get; set; }
		}
	}
}
