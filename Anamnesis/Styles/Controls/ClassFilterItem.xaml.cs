// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;
using System.ComponentModel;
using System.Windows.Controls;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for ClassFilterItem.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ClassFilterItem : UserControl, INotifyPropertyChanged
{
	public static DependencyProperty<Classes> ValueDp = Binder.Register<Classes, ClassFilterItem>(nameof(ClassFilterItem.Value), OnValueChanged);
	public static DependencyProperty<Classes> ClassDp = Binder.Register<Classes, ClassFilterItem>(nameof(ClassFilterItem.Class), OnClassChanged);

	public ClassFilterItem()
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

	public Classes Class
	{
		get => ClassDp.Get(this);
		set => ClassDp.Set(this, value);
	}

	public string? ClassName
	{
		get
		{
			return this.Class.GetName();
		}
	}

	public bool IsSelected
	{
		get
		{
			return this.Value.HasFlag(this.Class);
		}

		set
		{
			this.Value = this.Value.SetFlag(this.Class, value);
		}
	}

	public ImageReference? Image { get; set; }
	////return "/Anamnesis;component/Assets/Classes/" + this.Class.ToString() + ".png";

	private static void OnValueChanged(ClassFilterItem sender, Classes value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(RoleFilterItem.IsSelected)));
	}

	private static void OnClassChanged(ClassFilterItem sender, Classes value)
	{
		sender.Image = value.GetIcon();
	}
}
