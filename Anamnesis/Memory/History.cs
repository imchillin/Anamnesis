// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;
	using XivToolsWpf;

	[AddINotifyPropertyChangedInterface]
	public class History
	{
		private const int MaxHistory = 1024 * 1024; // a lot.

		private static readonly TimeSpan TimeTillCommit = TimeSpan.FromMilliseconds(1000);

		private readonly Stack<HistoryEntry> history = new();
		private HistoryEntry current = new();
		private DateTime lastChangeTime = DateTime.Now;

		public History()
		{
			this.history.Push(new());
		}

		public int Count { get; private set; }
		public int CurrentChangeCount { get; private set; }
		public ObservableCollection<HistoryEntry> Entries { get; private set; } = new();

		/// <summary>
		/// Tick must be called periodically to push changes to the history stack when they are old enough.
		/// </summary>
		public void Tick()
		{
			if (!this.current.HasChanges)
				return;

			if (DateTime.Now - this.lastChangeTime > TimeTillCommit)
			{
				this.Commit();
			}
		}

		public void StepForward()
		{
			throw new NotImplementedException();
		}

		public void StepBack()
		{
			// Ensure any pending changes are comitted to be undone.
			if (this.current.HasChanges)
				this.Commit();

			if (this.history.Count <= 0)
				return;

			HistoryEntry restore = this.history.Pop();
			restore.Restore();

			Task.Run(async () =>
			{
				await Dispatch.MainThread();
				this.Entries.Remove(restore);
			});

			this.Count = this.history.Count;
		}

		public void Commit()
		{
			if (!this.current.HasChanges)
				return;

			Log.Verbose($"Comitting change set:\n{this.current}");

			HistoryEntry oldEntry = this.current;
			oldEntry.Name = oldEntry.GetName();
			this.history.Push(oldEntry);

			Task.Run(async () =>
			{
				await Dispatch.MainThread();
				this.Entries.Add(oldEntry);
			});

			if (this.history.Count > MaxHistory)
			{
				Log.Warning($"History depth exceded max: {MaxHistory}. Flushing");
				this.history.Clear();
			}

			this.current = new();

			this.Count = this.history.Count;
			this.CurrentChangeCount = 0;
		}

		public void Record(PropertyChange change)
		{
			if (!change.ShouldRecord())
				return;

			if (change.Name == null)
				change.Name = change.ToString();

			Log.Verbose($"Recording Change: {change}");

			this.lastChangeTime = DateTime.Now;
			this.current.Record(change);

			this.CurrentChangeCount = this.current.Count;
		}

		public class HistoryEntry
		{
			private const int MaxChanges = 1024;

			private readonly List<PropertyChange> changes = new();

			public bool HasChanges => this.changes.Count > 0;
			public int Count => this.changes.Count;
			public string Name { get; set; } = string.Empty;
			public string ChangeList => this.GetChangeList();

			public void Restore()
			{
				Log.Verbose($"Restoring change set:\n{this}");

				for (int i = this.changes.Count - 1; i >= 0; i--)
				{
					PropertyChange change = this.changes[i];
					if (change.OriginBind is PropertyBindInfo propertyBind)
					{
						propertyBind.Property.SetValue(propertyBind.Memory, change.OldValue);
					}
				}
			}

			public void Record(PropertyChange change)
			{
				// Record the change into the history list
				this.changes.Add(change);

				// Sanity check history depth
				if (this.changes.Count > MaxChanges)
				{
					Log.Warning($"Change depth exceded max: {MaxChanges}. Flushing");
					this.changes.Clear();
				}
			}

			public string GetName()
			{
				HashSet<string> names = new();
				foreach (PropertyChange change in this.changes)
				{
					if (change.Name == null)
						continue;

					names.Add(change.Name);
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

			public string GetChangeList()
			{
				StringBuilder builder = new();

				// Flatten the changes to repeated changes to the same value dont show up
				Dictionary<BindInfo, PropertyChange> flattenedChanges = new();
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

				foreach (PropertyChange change in flattenedChanges.Values)
				{
					if (builder.Length > 0)
						builder.AppendLine();

					builder.Append(change.ToString());
				}

				return builder.ToString();
			}
		}
	}
}
