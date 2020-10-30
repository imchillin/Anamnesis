// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Styles.DependencyProperties;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for DataPathSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class DataPathSelector : UserControl
	{
		public static readonly IBind<short> DataPathDp = Binder.Register<short, DataPathSelector>(nameof(DataPath), OnPathChange);
		public static readonly IBind<byte> DataHeadDp = Binder.Register<byte, DataPathSelector>(nameof(DataHead), OnHeadChanged);
		public static readonly IBind<Appearance.Tribes> TribeDp = Binder.Register<Appearance.Tribes, DataPathSelector>(nameof(Tribe), OnTribeChanged);

		private DataPathOption? selectedPath;

		public DataPathSelector()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.SelectedPath = this.GetOption(this.DataPath);
		}

		public ObservableCollection<DataPathOption> PathOptions { get; set; } = new ObservableCollection<DataPathOption>()
		{
			new DataPathOption("Midlander Masculine", 101),
			new DataPathOption("Midlander Masculine Child", 104),
			new DataPathOption("Midlander Feminine", 201),
			new DataPathOption("Midlander Feminine Child", 204),
			new DataPathOption("Highlander Masculine", 301),
			new DataPathOption("Highlander Feminine", 401),
			new DataPathOption("Elezen Masculine", 501),
			new DataPathOption("Elezen Masculine Child", 504),
			new DataPathOption("Elezen Feminine", 601),
			new DataPathOption("Elezen Feminine Child", 604),
			new DataPathOption("Miqote Masculine", 701),
			new DataPathOption("Miqote Masculine Child", 704),
			new DataPathOption("Miqote Feminine", 801),
			new DataPathOption("Miqote Feminine Child", 804),
			new DataPathOption("Roegadyn Masculine", 901),
			new DataPathOption("Roegadyn Feminine", 1001),
			new DataPathOption("Lalafell Masculine", 1101),
			new DataPathOption("Lalafell Feminine", 1201),
			new DataPathOption("AuRa Masculine", 1301),
			new DataPathOption("AuRa Feminine", 1401),
			new DataPathOption("Hrothgar", 1501),
			new DataPathOption("Viera", 1801),
			new DataPathOption("Padjal Masculine", 9104),
			new DataPathOption("Padjal Feminine", 9204),
		};

		public short DataPath
		{
			get => DataPathDp.Get(this);
			set => DataPathDp.Set(this, value);
		}

		public byte DataHead
		{
			get => DataHeadDp.Get(this);
			set => DataHeadDp.Set(this, value);
		}

		public Appearance.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		public DataPathOption? SelectedPath
		{
			get
			{
				return this.selectedPath;
			}
			set
			{
				this.selectedPath = value;

				if (value != null && value.PathValue != 0)
				{
					this.DataPath = value.PathValue;
					this.DataHead = value.GetHead(this.Tribe);
				}
			}
		}

		private static void OnPathChange(DataPathSelector sender, short value)
		{
			sender.SelectedPath = sender.GetOption(sender.DataPath);
		}

		private static void OnHeadChanged(DataPathSelector sender, byte value)
		{
			sender.SelectedPath = sender.GetOption(sender.DataPath);
		}

		private static void OnTribeChanged(DataPathSelector sender, Appearance.Tribes value)
		{
			sender.SelectedPath = sender.GetOption(sender.DataPath);
		}

		private DataPathOption? GetOption(short dataPath)
		{
			foreach (DataPathOption option in this.PathOptions)
			{
				if (option.PathValue == dataPath)
				{
					return option;
				}
			}

			return null;
		}

		public class DataPathOption
		{
			public DataPathOption(string name, short path)
			{
				this.Name = name;
				this.PathValue = path;
			}

			public short PathValue { get; private set; }
			public string Name { get; private set; }

			public byte GetHead(Appearance.Tribes tribe)
			{
				if ((int)tribe % 2 != 0)
				{
					// If highlander
					if (this.PathValue == 301 || this.PathValue == 401)
						return 0x65;

					return 0x01;
				}
				else if ((int)tribe <= 10)
				{
					// If midlander
					if (this.PathValue == 101 || this.PathValue == 201 || this.PathValue == 104)
						return 0x01;

					return 0x65;
				}
				else
				{
					// If highlander
					if (this.PathValue == 301 || this.PathValue == 401)
						return 0xC9;

					return 0x65;
				}
			}
		}
	}
}
