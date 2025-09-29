// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using PropertyChanged;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for JobFilterItem.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class RoleFilterItem : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty<Classes> ValueDp = Binder.Register<Classes, RoleFilterItem>(nameof(RoleFilterItem.Value), OnValueChanged);
	public static readonly DependencyProperty<Roles> RoleDp = Binder.Register<Roles, RoleFilterItem>(nameof(RoleFilterItem.Role));

	public RoleFilterItem()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public Classes Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public Roles Role
	{
		get => RoleDp.Get(this);
		set => RoleDp.Set(this, value);
	}

	public string RoleName => this.Role.ToString();

	public bool IsSelected
	{
		get
		{
			foreach (Classes job in this.Role.GetClasses())
			{
				if (!this.Value.HasFlagUnsafe(job))
				{
					return false;
				}
			}

			return true;
		}
	}

	public string ImageSource => $"/Anamnesis;component/Assets/Roles/{this.Role}.png";

	private static void OnValueChanged(RoleFilterItem sender, Classes value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(RoleFilterItem.IsSelected)));
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		bool newState = !this.IsSelected;

		foreach (Classes job in this.Role.GetClasses())
		{
			this.Value = this.Value.SetFlag(job, newState);
		}
	}
}
