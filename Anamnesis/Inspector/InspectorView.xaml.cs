// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Inspector;

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;
using PropertyChanged;

using Binder = XivToolsWpf.DependencyProperties.Binder;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public partial class InspectorView : UserControl
{
	public static readonly IBind<object?> TargetDp = Binder.Register<object?, InspectorView>(nameof(Target), OnTargetChanged, BindMode.OneWay);

	public InspectorView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		OnTargetChanged(this, this.Target);
	}

	public ObservableCollection<Entry> Entries { get; set; } = new();

	public object? Target
	{
		get => TargetDp.Get(this);
		set => TargetDp.Set(this, value);
	}

	private static void OnTargetChanged(InspectorView sender, object? value)
	{
		sender.Entries.Clear();

		if (value == null)
			return;

		Type targetType = value.GetType();
		PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

		foreach (PropertyInfo property in properties)
		{
			sender.Entries.Add(new(value, property));
		}
	}

	[AddINotifyPropertyChangedInterface]
	public class Entry : INotifyPropertyChanged
	{
		public Entry(object target, PropertyInfo property)
		{
			this.Property = property;
			this.Target = target;

			if (this.Target is INotifyPropertyChanged changed)
			{
				changed.PropertyChanged += this.OnTargetPropertyChanged;
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public object Target { get; private set; }
		public PropertyInfo Property { get; private set; }

		public string Name => this.Property.Name;
		public Type Type => this.Property.PropertyType;

		public object? Value
		{
			get => this.Property.GetValue(this.Target);
			set => this.Property.SetValue(this.Target, value);
		}

		private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			this.PropertyChanged?.Invoke(this, new(nameof(this.Value)));
		}
	}
}
