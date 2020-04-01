// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Services;

	public class Bone : INotifyPropertyChanged
	{
		public List<Bone> Children = new List<Bone>();
		public Bone Parent;

		private IMemory<Quaternion> rotationMemory;

		public Bone(string boneName)
		{
			this.BoneName = boneName;

			IInjectionService injection = Module.Services.Get<IInjectionService>();

			string propertyName = boneName + "_X";
			PropertyInfo property = typeof(Offsets.Bones).GetProperty(propertyName);

			if (property == null)
				throw new Exception("Failed to get bone axis: \"" + propertyName + "\"");

			string boneOffset = (string)property.GetValue(injection.Offsets.Character.Body.Bones);
			this.rotationMemory = injection.GetMemory<Quaternion>(Offsets.BaseAddresses.GPose, injection.Offsets.Character.Body.Base, boneOffset);
		}

		#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore

		public string BoneName { get; private set; }
		public bool IsEnabled { get; set; } = true;
		public Quaternion Rotation { get; set; }

		public string Tooltip
		{
			get
			{
				return this.BoneName; //// Strings.GetString<UISimplePoseStrings>(this.BoneName + "_Tooltip");
			}
		}

		public void GetRotation()
		{
			this.Rotation = this.rotationMemory.Get();
		}

		public void SetRotation()
		{
			if (!this.IsEnabled)
				return;

			if (this.rotationMemory.Value == this.Rotation)
				return;

			Quaternion newRotation = this.Rotation;

			Quaternion oldrotation = this.rotationMemory.Get();
			this.rotationMemory.Set(newRotation);
			Quaternion oldRotationConjugate = oldrotation;
			oldRotationConjugate.Conjugate();

			foreach (Bone child in this.Children)
			{
				child.Rotate(oldRotationConjugate, newRotation);
			}
		}

		private void Rotate(Quaternion sourceOldCnj, Quaternion sourceNew)
		{
			if (!this.IsEnabled)
				return;

			this.Rotation = this.rotationMemory.Get();
			Quaternion newRotation = sourceNew * (sourceOldCnj * this.Rotation);

			if (this.Rotation == newRotation)
				return;

			this.Rotation = newRotation;
			this.rotationMemory.Set(this.Rotation);

			foreach (Bone child in this.Children)
			{
				child.Rotate(sourceOldCnj, sourceNew);
			}
		}
	}
}
