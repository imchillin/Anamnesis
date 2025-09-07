// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for GlassesItemView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class GlassesItemView : UserControl
{
	public static readonly IBind<GlassesMemory?> GlassesDp = Binder.Register<GlassesMemory?, GlassesItemView>(nameof(GlassesModel), OnItemModelChanged, BindMode.TwoWay);

	private readonly bool lockViewModel = false;
	public GlassesItemView()
	{
		this.InitializeComponent();

		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.ContentArea.DataContext = this;
	}

	public Glasses? Glasses { get; set; }
	public ImageSource? IconSource { get; set; }
	public bool IsLoading { get; set; }

	public GlassesMemory? GlassesModel
	{
		get => GlassesDp.Get(this);
		set => GlassesDp.Set(this, value);
	}

	public ActorMemory? Actor { get; private set; }

	private static void OnItemModelChanged(GlassesItemView sender, GlassesMemory? value)
	{
		if (sender.GlassesModel != null)
			sender.GlassesModel.PropertyChanged -= sender.OnViewModelPropertyChanged;

		if (sender.GlassesModel == null)
			return;

		sender.IconSource = ItemSlots.Glasses.GetIcon();
		sender.GlassesModel.PropertyChanged += sender.OnViewModelPropertyChanged;

		sender.OnViewModelPropertyChanged(null, null);
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.CanRefresh != true)
			return;

		SelectorDrawer.Show<GlassesSelector, Glasses>(this.Glasses, (pickedGlasses) =>
		{
			if (this.GlassesModel == null)
				return;

			this.Glasses = pickedGlasses;
			this.GlassesModel.GlassesId = (ushort)pickedGlasses.RowId;
		});
	}

	private void OnSlotMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Actor?.CanRefresh != true)
			return;

		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.GlassesModel?.Clear();
		}
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? e)
	{
		if (this.lockViewModel)
			return;

		Task.Run(async () =>
		{
			await Task.Yield();
			await Dispatch.MainThread();
			if (this.GlassesModel == null || GameDataService.Glasses == null)
				return;

			this.IsLoading = true;

			try
			{
				GlassesMemory? valueVm = this.GlassesModel;
				await Dispatch.NonUiThread();

				if (this.Actor == null)
					throw new Exception("No Actor in item view");

				await Dispatch.MainThread();

				this.Glasses = GameDataService.Glasses.GetRow(valueVm.GlassesId);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to update item");
			}

			this.IsLoading = false;
		});
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.Actor = this.DataContext as ActorMemory;
	}
}
