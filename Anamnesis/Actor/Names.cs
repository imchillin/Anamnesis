// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public class Names
{
	private readonly ActorBasicMemory actor;
	private string? nickname;

	public Names(ActorBasicMemory owner)
	{
		this.actor = owner;
		this.actor.PropertyChanged += this.OnActorPropertyChanged;

		this.Update();
	}

	public enum DisplayModes
	{
		FullName,
		Initials,
		SurnameAbreviated,
		ForenameAbreviated,
	}

	public string? Nickname
	{
		get => this.nickname;
		set
		{
			this.nickname = value;
			this.Update();
		}
	}

	public string FullName { get; private set; } = string.Empty;
	public string DisplayName => this.Nickname ?? this.FullName;

	public string Initials { get; private set; } = string.Empty;
	public string SurnameAbreviated { get; private set; } = string.Empty;
	public string ForenameAbreviated { get; private set; } = string.Empty;

	public string Text
	{
		get
		{
			switch (this.DisplayMode)
			{
				case DisplayModes.FullName: return this.DisplayName;
				case DisplayModes.Initials: return this.Initials;
				case DisplayModes.SurnameAbreviated: return this.SurnameAbreviated;
				case DisplayModes.ForenameAbreviated: return this.ForenameAbreviated;
			}

			throw new NotImplementedException();
		}
	}

	// Should be a setting.
	private DisplayModes DisplayMode => DisplayModes.SurnameAbreviated;

	public void Update()
	{
		this.FullName = this.actor.NameBytes.ToString();

		this.Initials = string.Empty;
		this.SurnameAbreviated = string.Empty;
		this.ForenameAbreviated = string.Empty;

		if (string.IsNullOrEmpty(this.FullName) && string.IsNullOrEmpty(this.Nickname))
			return;

		string[] spaceSeperatedParts;

		if (this.Nickname == null)
		{
			// Remove anything after '(' for game names.
			string[] parts = this.FullName.Split('(', StringSplitOptions.RemoveEmptyEntries);
			spaceSeperatedParts = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
		}
		else
		{
			spaceSeperatedParts = this.Nickname.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		}

		for (int i = 0; i < spaceSeperatedParts.Length; i++)
		{
			if (spaceSeperatedParts[i].Length <= 0 || string.IsNullOrWhiteSpace(spaceSeperatedParts[i]))
				continue;

			char initial = spaceSeperatedParts[i][0];

			this.Initials += " ." + initial;

			if (i == 0)
			{
				this.SurnameAbreviated += " ." + spaceSeperatedParts[i];
			}
			else
			{
				this.SurnameAbreviated += " ." + initial;
			}

			if (i == spaceSeperatedParts.Length - 1)
			{
				this.ForenameAbreviated += ". " + spaceSeperatedParts[i];
			}
			else
			{
				this.ForenameAbreviated += " ." + initial;
			}
		}

		this.Initials = this.Initials.Trim(' ').Trim('.');
		this.SurnameAbreviated = this.SurnameAbreviated.Trim(' ').Trim('.');
		this.ForenameAbreviated = this.ForenameAbreviated.Trim(' ').Trim('.');
	}

	private void OnActorPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(ActorBasicMemory.NameBytes))
			return;

		this.Update();
	}
}
