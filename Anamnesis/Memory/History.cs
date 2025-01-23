// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GUI.Dialogs;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf;

/// <summary>History change contexts.</summary>
public enum HistoryContext
{
	Appearance, // Appearance & Equipment
	Posing,     // Posing
	Other,      // Everything else (Default)
}

/// <summary>A history manager.</summary>
[AddINotifyPropertyChangedInterface]
public class History : INotifyPropertyChanged
{
	private const int MaxHistory = 1024 * 1024; // a lot.

	private static readonly TimeSpan TimeTillCommit = TimeSpan.FromMilliseconds(200);

	private readonly ObservableCollection<HistoryEntry> history = new();
	private readonly ReadOnlyObservableCollection<HistoryEntry> readOnlyHistory;
	private HistoryEntry current = new();
	private DateTime lastChangeTime = DateTime.Now;
	private int autoCommitEnabled = 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="History"/> class.
	/// </summary>
	public History()
	{
		this.readOnlyHistory = new ReadOnlyObservableCollection<HistoryEntry>(this.history);
		this.history.CollectionChanged += this.OnHistoryCollectionChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets or sets the current context.</summary>
	public HistoryContext CurrentContext { get; set; } = HistoryContext.Other;

	/// <summary>Gets the current index in the history log.</summary>
	public int CurrentIndex { get; private set; } = -1;

	/// <summary>
	/// Gets a value indicating whether the history log has any entries.
	/// </summary>
	[DependsOn(nameof(this.Entries))]
	public bool HasEntries => this.Entries.Count > 0;

	/// <summary>Gets the collection of active history entries.</summary>
	/// <remarks>
	/// Undone changes are included in the collection until they are overwritten by a new change.
	/// </remarks>
	public ReadOnlyObservableCollection<HistoryEntry> Entries => this.readOnlyHistory;

	/// <summary>Gets or sets a value indicating whether auto commit is enabled.</summary>
	/// <remarks>
	/// When enabled, changes are automatically committed after a certain amount
	/// of time has passed since the first change was recorded.
	/// </remarks>
	public bool AutoCommitEnabled
	{
		get => Interlocked.CompareExchange(ref this.autoCommitEnabled, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.autoCommitEnabled, value ? 1 : 0);
	}

	/// <summary>Gets a value indicating whether undo is possible.</summary>
	[DependsOn(nameof(CurrentIndex))]
	public bool CanUndo => this.CurrentIndex >= 0;

	/// <summary>Gets a value indicating whether redo is possible.</summary>
	[DependsOn(nameof(CurrentIndex), nameof(this.Entries))]
	public bool CanRedo => this.CurrentIndex < this.Entries.Count - 1;

	/// <summary>
	/// Tick must be called periodically to push changes to the history stack when they are old enough.
	/// </summary>
	public async void Tick()
	{
		if (!this.current.HasChanges || !this.AutoCommitEnabled)
			return;

		await Dispatch.MainThread();

		// Commit as soon as the timer is up and the mouse is not being held down
		// The mouse button state check is necessary in case the user is still interacting with the UI
		if (DateTime.Now - this.lastChangeTime > TimeTillCommit && Mouse.LeftButton != MouseButtonState.Pressed)
		{
			this.Commit();
		}
	}

	/// <summary>Steps forward in the history.</summary>
	public async void StepForward()
	{
		if (!this.CanRedo)
			return;

		var nextEntry = this.history[this.CurrentIndex + 1];
		if (nextEntry.Context != this.CurrentContext)
		{
			if (await GenericDialog.ShowLocalizedAsync("History_Context_Change_Confirm", "History_Context_Change", MessageBoxButton.YesNo) != true)
				return;
		}

		this.CurrentIndex++;
		nextEntry.Redo();

		Log.Verbose($"Step Forward: {this.CurrentIndex}");
	}

	/// <summary>Steps back in the history.</summary>
	public async void StepBack()
	{
		// Ensure any pending changes are committed to be undone.
		if (this.current.HasChanges)
			this.Commit();

		if (!this.CanUndo)
			return;

		var currentEntry = this.history[this.CurrentIndex];
		if (currentEntry.Context != this.CurrentContext)
		{
			if (await GenericDialog.ShowLocalizedAsync("History_Context_Change_Confirm", "History_Context_Change", MessageBoxButton.YesNo) != true)
				return;
		}

		currentEntry.Undo();
		this.CurrentIndex--;

		Log.Verbose($"Step Back: {this.CurrentIndex}");
	}

	/// <summary>Commits the set of current changes to history.</summary>
	public async void Commit()
	{
		if (!this.current.HasChanges)
			return;

		Log.Verbose($"Committing change set:\n{this.current}");

		HistoryEntry oldEntry = this.current;
		oldEntry.Name = oldEntry.GetName();
		oldEntry.Context = this.CurrentContext;

		await Dispatch.MainThread();

		// Remove any redo history
		if (this.CurrentIndex < this.history.Count - 1)
		{
			while (this.history.Count > this.CurrentIndex + 1)
			{
				this.history.RemoveAt(this.CurrentIndex + 1);
			}
		}

		this.history.Add(oldEntry);
		this.CurrentIndex = this.history.Count - 1;

		if (this.history.Count > MaxHistory)
		{
			Log.Warning($"History depth exceded max: {MaxHistory}. Flushing");
			this.history.Clear();
			this.CurrentIndex = -1;
		}

		this.current = new();
	}

	/// <summary>Records a change to the latest history entry.</summary>
	/// <param name="change">The property change to record.</param>
	public void Record(PropertyChange change)
	{
		if (!change.ShouldRecord())
			return;

		change.Name ??= change.ToString();
		this.lastChangeTime = DateTime.Now;
		this.current.Record(change);
	}

	/// <summary>Clears the history log and resets the current index.</summary>
	public void Clear()
	{
		this.history.Clear();
		this.CurrentIndex = -1;
		this.current = new();
		this.OnPropertyChanged(nameof(this.HasEntries));
	}

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void OnHistoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		this.OnPropertyChanged(nameof(this.Entries));
	}

	/// <summary>Represents a history entry.</summary>
	public class HistoryEntry
	{
		private const int MaxChanges = 1024;

		private readonly List<PropertyChange> changes = new();

		/// <summary>Gets a value indicating whether the entry has changes.</summary>
		public bool HasChanges
		{
			get
			{
				lock (this.changes)
				{
					return this.changes.Count > 0;
				}
			}
		}

		/// <summary>Gets the count of changes in the entry.</summary>
		public int Count
		{
			get
			{
				lock (this.changes)
				{
					return this.changes.Count;
				}
			}
		}

		/// <summary>Gets or sets the name of the entry.</summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>Gets or sets the context of the entry.</summary>
		public HistoryContext Context { get; set; } = HistoryContext.Other;

		/// <summary>Gets the list of changes as a string.</summary>
		public string ChangeList => this.GetChangeList();

		/// <summary>Undoes the changes in the entry.</summary>
		public void Undo()
		{
			Log.Verbose($"Restoring change set:\n{this}");

			lock (this.changes)
			{
				// Apply changes in reverse order for undo
				for (int i = this.changes.Count - 1; i >= 0; i--)
				{
					PropertyChange change = this.changes[i];
					if (change.OriginBind is PropertyBindInfo propertyBind)
					{
						propertyBind.Property.SetValue(propertyBind.Memory, change.OldValue);
					}
				}
			}
		}

		/// <summary>Redoes the changes in the entry.</summary>
		public void Redo()
		{
			Log.Verbose($"Applying back change set:\n{this}");

			lock (this.changes)
			{
				// Apply changes in recorded order for redo
				foreach (PropertyChange change in this.changes)
				{
					if (change.OriginBind is PropertyBindInfo propertyBind)
					{
						propertyBind.Property.SetValue(propertyBind.Memory, change.NewValue);
					}
				}
			}
		}

		/// <summary>Records a property change in the entry.</summary>
		/// <param name="change">The property change to record.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Record(PropertyChange change)
		{
			if (!change.ShouldRecord())
				return;

			lock (this.changes)
			{
				// Keep only the latest change for each bind to minimize stored changes and recovery steps
				for (int i = 0; i < this.changes.Count; i++)
				{
					if (this.changes[i].Path == change.Path)
					{
						// Validate the existing change's OldValue
						if (IsValidOldValue(this.changes[i].OldValue))
						{
							// Transfer the old value of the existing change to the new change if it is valid
							change.OldValue = this.changes[i].OldValue;
						}

						// Remove the existing change
						this.changes.RemoveAt(i);
						break;
					}
				}

				// Add the latest change into the history entry's changes list
				this.changes.Add(change);

				// Sanity check history depth
				if (this.changes.Count > MaxChanges)
				{
					Log.Warning($"Change depth exceded max: {MaxChanges}. Flushing");
					this.changes.Clear();
				}
			}
		}

		/// <summary>Gets the name of the entry.</summary>
		/// <returns>The name of the entry.</returns>
		public string GetName()
		{
			HashSet<string> names = new();
			lock (this.changes)
			{
				foreach (PropertyChange change in this.changes)
				{
					if (change.Name == null)
						continue;

					names.Add(change.Name);
				}
			}

			StringBuilder builder = new();
			foreach (string name in names)
			{
				if (builder.Length > 0)
					builder.Append(", ");

				builder.Append(name);
			}

			return builder.ToString();
		}

		/// <summary>Gets the list of changes as a string.</summary>
		/// <returns>The list of changes in string form.</returns>
		public string GetChangeList()
		{
			StringBuilder builder = new();

			// Flatten the changes so repeated changes to the same value dont show up
			Dictionary<BindInfo, PropertyChange> flattenedChanges = new();
			lock (this.changes)
			{
				foreach (PropertyChange change in this.changes)
				{
					if (!flattenedChanges.ContainsKey(change.OriginBind))
					{
						flattenedChanges.Add(change.OriginBind, new(change));
					}
					else
					{
						PropertyChange existingChange = flattenedChanges[change.OriginBind];

						existingChange.NewValue = change.NewValue;
						flattenedChanges[change.OriginBind] = existingChange;
					}
				}
			}

			foreach (PropertyChange change in flattenedChanges.Values)
			{
				if (builder.Length > 0)
					builder.AppendLine();

				builder.Append(change.ToString());
			}

			return builder.ToString();
		}

		/// <summary>Determines whether the old value is valid.</summary>
		/// <param name="oldValue">The old value to validate.</param>
		/// <returns>True if the old value is valid; otherwise, false.</returns>
		private static bool IsValidOldValue(object? oldValue)
		{
			return oldValue switch
			{
				Vector2 vector => !float.IsNaN(vector.X) && !float.IsNaN(vector.Y),
				Vector3 vector => !float.IsNaN(vector.X) && !float.IsNaN(vector.Y) && !float.IsNaN(vector.Z),
				Quaternion quaternion => !float.IsNaN(quaternion.X) && !float.IsNaN(quaternion.Y) && !float.IsNaN(quaternion.Z) && !float.IsNaN(quaternion.W),
				null => false,
				_ => true
			};
		}
	}
}
