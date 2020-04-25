// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;

	public class Bone : INotifyPropertyChanged, IDisposable
	{
		public readonly SkeletonService.Bone Definition;
		public List<Bone> Children = new List<Bone>();
		public Bone Parent;

		private IMemory<Transform> transformMem;

		public Bone(string name, IMemory<Transform> transformMem, SkeletonService.Bone definition)
		{
			this.Definition = definition;
			this.BoneName = name;
			this.transformMem = transformMem;

			this.Rotation = this.LiveRotation;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string BoneName { get; private set; }
		public bool IsEnabled { get; set; } = true;

		public Quaternion Rotation { get; set; }
		public Vector Scale { get; set; }
		public Vector Position { get; set; }

		public string Tooltip
		{
			get
			{
				return this.BoneName; //// Strings.GetString<UISimplePoseStrings>(this.BoneName + "_Tooltip");
			}
		}

		public Transform Transform
		{
			get
			{
				return this.transformMem.Value;
			}

			set
			{
				this.transformMem.Value = value;
				this.Rotation = this.LiveRotation;
				this.Scale = this.LiveScale;
				this.Position = this.LivePosition;
			}
		}

		public Quaternion LiveRotation
		{
			get
			{
				return this.Transform.Rotation;
			}

			set
			{
				Transform trans = this.Transform;
				trans.Rotation = value;
				this.Transform = trans;
			}
		}

		public Vector LiveScale
		{
			get
			{
				return this.Transform.Scale;
			}

			set
			{
				Transform trans = this.Transform;
				trans.Scale = value;
				this.Transform = trans;
			}
		}

		public Vector LivePosition
		{
			get
			{
				return this.Transform.Position;
			}

			set
			{
				Transform trans = this.Transform;
				trans.Position = value;
				this.Transform = trans;
			}
		}

		public void ApplyTransform()
		{
			if (!this.IsEnabled)
				return;

			// TODO: Build a matrix4x4 and apply it to every bone
			if (this.LiveRotation != this.Rotation)
			{
				Quaternion newRotation = this.Rotation;
				Quaternion oldrotation = this.LiveRotation;
				this.LiveRotation = newRotation;
				Quaternion oldRotationConjugate = oldrotation;
				oldRotationConjugate.Conjugate();

				foreach (Bone child in this.Children)
				{
					child.Rotate(oldRotationConjugate, newRotation);
				}
			}

			/*if (this.LiveScale != this.Scale)
			{
				this.LiveScale = this.Scale;
			}

			if (this.LivePosition != this.Position)
			{
				this.LivePosition = this.Position;
			}*/
		}

		public void Dispose()
		{
			this.transformMem.Dispose();
		}

		private void Rotate(Quaternion sourceOldCnj, Quaternion sourceNew)
		{
			if (!this.IsEnabled)
				return;

			this.Rotation = this.LiveRotation;
			Quaternion newRotation = sourceNew * (sourceOldCnj * this.Rotation);

			if (this.Rotation == newRotation)
				return;

			this.Rotation = newRotation;
			this.LiveRotation = this.Rotation;

			foreach (Bone child in this.Children)
			{
				child.Rotate(sourceOldCnj, sourceNew);
			}
		}
	}
}
