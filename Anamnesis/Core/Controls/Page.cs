// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using Anamnesis.Keyboard;
using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Windows.Controls;

[AddINotifyPropertyChangedInterface]
public abstract class Page
{
	private UserControl? control;

	public Page(IconChar icon, string context, string name)
	{
		this.Icon = icon;
		this.Name = name;
		this.DisplayNameKey = $"{context}_{name}";
		this.TooltipKey = $"{context}_{name}_Tooltip";

		HotkeyService.RegisterHotkeyHandler($"MainWindow.{name}", () => this.IsActive = true);
	}

	public string Name { get; private set; }
	public int Index { get; set; }
	public string DisplayNameKey { get; private set; }
	public string TooltipKey { get; private set; }

	[DependsOn(nameof(IsActive))]
	public UserControl? Content
	{
		get
		{
			if (this.control == null)
			{
				if (!this.IsActive)
					return null;

				this.control = this.CreateContent();
			}

			return this.control;
		}
	}

	public IconChar Icon { get; private set; }
	public bool IsActive { get; set; }
	public object? DataContext { get; set; }

	protected abstract UserControl CreateContent();
}

public class Page<T>(IconChar icon, string context, string name) : Page(icon, context, name)
	where T : UserControl
{
	protected override UserControl CreateContent()
	{
		UserControl? control = Activator.CreateInstance<T>();
		return control ?? throw new Exception($"Failed to create page content: {typeof(T)}");
	}
}
