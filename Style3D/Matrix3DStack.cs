// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.ThreeD
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Windows.Media.Media3D;

	/// <summary>
	///     Matrix3DStack is a stack of Matrix3Ds.
	/// </summary>
	public class Matrix3DStack : IEnumerable<Matrix3D>, ICollection
	{
		private readonly List<Matrix3D> storage = new List<Matrix3D>();

		public int Count
		{
			get { return this.storage.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return ((ICollection)this.storage).IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return ((ICollection)this.storage).SyncRoot; }
		}

		public Matrix3D Peek()
		{
			return this.storage[this.storage.Count - 1];
		}

		public void Push(Matrix3D item)
		{
			this.storage.Add(item);
		}

		public void Append(Matrix3D item)
		{
			if (this.Count > 0)
			{
				Matrix3D top = this.Peek();
				top.Append(item);
				this.Push(top);
			}
			else
			{
				this.Push(item);
			}
		}

		public void Prepend(Matrix3D item)
		{
			if (this.Count > 0)
			{
				Matrix3D top = this.Peek();
				top.Prepend(item);
				this.Push(top);
			}
			else
			{
				this.Push(item);
			}
		}

		public Matrix3D Pop()
		{
			Matrix3D result = this.Peek();
			this.storage.RemoveAt(this.storage.Count - 1);

			return result;
		}

		public void Clear()
		{
			this.storage.Clear();
		}

		public bool Contains(Matrix3D item)
		{
			return this.storage.Contains(item);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)this.storage).CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Matrix3D>)this).GetEnumerator();
		}

		IEnumerator<Matrix3D> IEnumerable<Matrix3D>.GetEnumerator()
		{
			for (int i = this.storage.Count - 1; i >= 0; i--)
			{
				yield return this.storage[i];
			}
		}
	}
}
