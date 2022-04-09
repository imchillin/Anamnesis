// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using PropertyChanged;
	using Serilog;

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

			this.Count = this.history.Count;
		}

		public void Commit()
		{
			if (!this.current.HasChanges)
				return;

			Log.Verbose($"Comitting change set:\n{this.current}");

			this.history.Push(this.current);

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

			public override string ToString()
			{
				StringBuilder builder = new();
				foreach (PropertyChange change in this.changes)
				{
					builder.AppendLine(change.ToString());
				}

				return builder.ToString();
			}
		}
	}
}
