// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Controls;

using Anamnesis.Actor.Posing;
using Anamnesis.Core;
using Anamnesis.Memory;
using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

// TODO: Posing of multiple selected bones does not currently work. Left (_l) and right (_r) bone counterparts need to mirrored (on some axises).
// Or perhaps the rotation and position need to move in model space and not local space? for the multi-selected bone transforms?

/// <summary>
/// Interaction logic for TransformEditor.xaml.
/// </summary>
/// <remarks>
/// If both both the skeleton and value properties are set, the value property will be prioritized by the editor.
/// </remarks>
[AddINotifyPropertyChangedInterface]
public partial class TransformEditor : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<SkeletonEntity?> SkeletonDp = Binder.Register<SkeletonEntity?, TransformEditor>(nameof(Skeleton), OnSkeletonChanged, BindMode.OneWay);
	public static readonly IBind<ITransform?> ValueDp = Binder.Register<ITransform?, TransformEditor>(nameof(Value), OnTransformChanged, BindMode.TwoWay);
	public static readonly IBind<bool?> CanTranslateDp = Binder.Register<bool?, TransformEditor>(nameof(CanTranslateProperty), OnCanTranslateChanged, BindMode.OneWay);

	private readonly Dictionary<string, Vector3> initialPositions = new();
	private readonly Dictionary<string, Quaternion> initialRotations = new();
	private readonly Dictionary<string, Vector3> initialScales = new();

	public TransformEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public SkeletonEntity? Skeleton
	{
		get => SkeletonDp.Get(this);
		set => SkeletonDp.Set(this, value);
	}

	public ITransform? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public bool? CanTranslateProperty
	{
		get => CanTranslateDp.Get(this);
		set => CanTranslateDp.Set(this, value);
	}

	public ActorMemory? SelectedActor => TargetService.Instance.SelectedActor;

	public IEnumerable<Bone> SelectedBones => this.Skeleton?.SelectedBones ?? Enumerable.Empty<Bone>();

	public bool CanTranslate => this.CanTranslateProperty ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanTranslate) && TargetService.Instance.SelectedActor != null && (TargetService.Instance.SelectedActor.IsMotionDisabled || PoseService.Instance.FreezeWorldPosition));

	public bool CanRotate => this.Value?.CanRotate ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanRotate));

	public bool CanScale => this.Value?.CanScale ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanScale));

	public bool CanLinkScale => this.Value?.CanLinkScale ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanLinkScale));

	public bool ScaleLinked => this.Value?.ScaleLinked ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.ScaleLinked));
	public Vector3 Position
	{
		get
		{
			if (this.Value != null)
				return this.Value.Position;

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Vector3.Zero;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					return this.SelectedActor.ModelObject.Transform.Position;
				}
			}

			if (this.Skeleton.SelectedBones.Count() == 1)
				return this.Skeleton.SelectedBones.First().Position;

			return this.DeviationPosition;
		}
		set
		{
			if (this.Value != null)
			{
				this.Value.Position = value;
				return;
			}

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					this.SelectedActor.ModelObject.Transform.Position = value;
				}
			}
			else if (this.Skeleton.SelectedBones.Count() == 1)
			{
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					bone.Position = value;
				}
			}
			else
			{
				Vector3 deviation = value - this.DeviationPosition;
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					bone.Position += deviation;
				}
			}
		}
	}

	public Quaternion Rotation
	{
		get
		{
			if (this.Value != null)
				return this.Value.Rotation;

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Quaternion.Identity;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					return this.SelectedActor.ModelObject.Transform.Rotation;
				}
			}

			if (this.Skeleton.SelectedBones.Count() == 1)
				return this.Skeleton.SelectedBones.First().Rotation;

			return this.DeviationRotation;
		}
		set
		{
			if (this.Value != null)
			{
				this.Value.Rotation = value;
				return;
			}

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					this.SelectedActor.ModelObject.Transform.Rotation = value;
				}
			}
			else if (this.Skeleton.SelectedBones.Count() == 1)
			{
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					bone.Rotation = value;
				}
			}
			else
			{
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					if (this.initialRotations.TryGetValue(bone.Name, out Quaternion initialRotation))
					{
						Quaternion deviation = value * Quaternion.Inverse(this.DeviationRotation);
						bone.Rotation *= deviation;
					}
				}
			}
		}
	}

	public Quaternion RootRotation
	{
		get
		{
			if (this.Value != null)
				return Quaternion.Identity;

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Quaternion.Identity;

			if (this.Skeleton.SelectedBones.Count() == 1)
				return this.Skeleton.SelectedBones.First().RootRotation;

			return this.Skeleton.RootRotation;
		}
	}

	public Vector3 Scale
	{
		get
		{
			if (this.Value != null)
				return this.Value.Scale;

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Vector3.Zero;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					return this.SelectedActor.ModelObject.Transform.Scale;
				}
			}

			if (this.Skeleton.SelectedBones.Count() == 1)
				return this.Skeleton.SelectedBones.First().Scale;

			return this.DeviationScale;
		}
		set
		{
			if (this.Value != null)
			{
				this.Value.Scale = value;
				return;
			}

			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return;

			if (!this.SelectedBones.Any())
			{
				if (this.SelectedActor?.ModelObject?.Transform != null)
				{
					this.SelectedActor.ModelObject.Transform.Scale = value;
				}
			}
			else if (this.Skeleton.SelectedBones.Count() == 1)
			{
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					bone.Scale = value;
				}
			}
			else
			{
				Vector3 deviation = value - this.DeviationScale;
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					bone.Scale += deviation;
				}
			}
		}
	}

	private Vector3 DeviationPosition
	{
		get
		{
			if (this.Skeleton?.SelectedBones != null && this.Skeleton.SelectedBones.Any())
			{
				var firstBone = this.Skeleton.SelectedBones.First();
				if (this.initialPositions.TryGetValue(firstBone.Name, out Vector3 initialPosition))
				{
					return firstBone.Position - initialPosition;
				}
			}

			return Vector3.Zero;
		}
	}

	private Quaternion DeviationRotation
	{
		get
		{
			if (this.Skeleton?.SelectedBones != null && this.Skeleton.SelectedBones.Any())
			{
				var firstBone = this.Skeleton.SelectedBones.First();
				if (this.initialRotations.TryGetValue(firstBone.Name, out Quaternion initialRotation))
				{
					return Quaternion.Multiply(firstBone.Rotation, Quaternion.Inverse(initialRotation));
				}
			}

			return Quaternion.Identity;
		}
	}

	private Vector3 DeviationScale
	{
		get
		{
			if (this.Skeleton?.SelectedBones != null && this.Skeleton.SelectedBones.Any())
			{
				var firstBone = this.Skeleton.SelectedBones.First();
				if (this.initialScales.TryGetValue(firstBone.Name, out Vector3 initialScale))
				{
					return firstBone.Scale - initialScale;
				}
			}

			return Vector3.Zero;
		}
	}

	private static void OnSkeletonChanged(TransformEditor sender, SkeletonEntity? value)
	{
		if (sender.Skeleton != null)
			sender.Skeleton.PropertyChanged -= sender.OnSkeletonPropertyChanged;

		sender.Skeleton = value;

		if (sender.Skeleton != null)
		{
			sender.Skeleton.PropertyChanged += sender.OnSkeletonPropertyChanged;

			sender.SetInitialValues();
			sender.RaisePropertyChanged(string.Empty);
		}
	}

	private static void OnTransformChanged(TransformEditor sender, ITransform? value)
	{
		sender.RaisePropertyChanged(string.Empty);
	}

	private static void OnCanTranslateChanged(TransformEditor sender, bool? value)
	{
		sender.RaisePropertyChanged(nameof(sender.CanTranslate));
	}

	private void SetInitialValues()
	{
		if (this.Skeleton == null || this.Skeleton.SelectedBones == null || !this.Skeleton.SelectedBones.Any())
			return;

		this.initialPositions.Clear();
		this.initialRotations.Clear();
		this.initialScales.Clear();

		foreach (var bone in this.Skeleton.SelectedBones)
		{
			this.initialPositions[bone.Name] = bone.Position;
			this.initialRotations[bone.Name] = bone.Rotation;
			this.initialScales[bone.Name] = bone.Scale;
		}
	}

	private void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void OnSkeletonPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones))
		{
			this.RaisePropertyChanged(string.Empty);
		}
	}
}
