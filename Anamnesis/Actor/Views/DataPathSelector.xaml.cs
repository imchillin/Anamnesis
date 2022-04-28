// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for DataPathSelector.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class DataPathSelector : UserControl
{
	public static readonly IBind<short> DataPathDp = Binder.Register<short, DataPathSelector>(nameof(DataPath), OnPathChange);
	public static readonly IBind<byte> DataHeadDp = Binder.Register<byte, DataPathSelector>(nameof(DataHead), OnHeadChanged);
	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, DataPathSelector>(nameof(Tribe), OnTribeChanged);

	private DataPathOption? selectedPath;
	private bool supressEvents = false;

	public DataPathSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.SelectedPath = this.GetOption(this.DataPath);

		foreach (ActorModelMemory.DataPaths dataPathVal in Enum.GetValues<ActorModelMemory.DataPaths>())
		{
			string? name = Enum.GetName(dataPathVal);

			if (name == null)
				continue;

			string locName = LocalizationService.GetString($"Character_Data_{name}");
			this.PathOptions.Add(new DataPathOption(locName, dataPathVal));
		}
	}

	public ObservableCollection<DataPathOption> PathOptions { get; set; } = new ObservableCollection<DataPathOption>();

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

	public ActorCustomizeMemory.Tribes Tribe
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
			this.supressEvents = true;
			this.selectedPath = value;
			this.supressEvents = false;
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

	private static void OnTribeChanged(DataPathSelector sender, ActorCustomizeMemory.Tribes value)
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

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.supressEvents)
			return;

		if (this.selectedPath == null)
			return;

		this.DataPath = this.selectedPath.PathValue;
		this.DataHead = this.selectedPath.GetHead(this.Tribe);
	}

	public class DataPathOption
	{
		public DataPathOption(string name, ActorModelMemory.DataPaths path)
		{
			this.Name = name;
			this.PathValue = (short)path;
		}

		public short PathValue { get; private set; }
		public string Name { get; private set; }

		public byte GetHead(ActorCustomizeMemory.Tribes tribe)
		{
			// 1, 3, 5, 7, 9, 11, 13, 15
			if ((int)tribe % 2 != 0)
			{
				// If highlander
				if (this.PathValue == 301 || this.PathValue == 401)
					return 0x65;

				return 0x01;
			}

			// 2, 4, 6, 8, 10
			else if ((int)tribe <= 10)
			{
				// If midlander
				if (this.PathValue == 101 || this.PathValue == 201 || this.PathValue == 104)
					return 0x01;

				return 0x65;
			}

			// 12, 14, 16
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
