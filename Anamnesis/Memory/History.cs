// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Serilog;

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

		public void Commit()
		{
			if (!this.current.HasChanges)
				return;

			Log.Verbose($"Comitting change set:\n{this.current}");

			this.history.Push(this.current);
			this.current = new();

			if (this.history.Count > MaxHistory)
			{
				Log.Warning($"History depth exceded max: {MaxHistory}. Flushing");
				this.history.Clear();
			}
		}

		public void Record(PropertyChange change)
		{
			if (!change.ShouldRecord())
				return;

			Log.Verbose($"Recording Change: {change}");

			this.lastChangeTime = DateTime.Now;
			this.current.Record(change);
		}

		public class HistoryEntry
		{
			private const int MaxChanges = 1024;

			private readonly List<PropertyChange> changes = new();

			public bool HasChanges => this.changes.Count > 0;

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
