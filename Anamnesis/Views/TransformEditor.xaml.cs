// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Controls;

using Anamnesis.Actor.Posing;
using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>Interaction logic for TransformEditor.xaml.</summary>
[AddINotifyPropertyChangedInterface]
public partial class TransformEditor : UserControl, INotifyPropertyChanged
{
	/// <summary>Dependency property for the skeleton entity.</summary>
	public static readonly IBind<SkeletonEntity?> SkeletonDp = Binder.Register<SkeletonEntity?, TransformEditor>(nameof(Skeleton), OnSkeletonChanged, BindMode.OneWay);

	/// <summary>Dependency property for the actor transform.</summary>
	/// <remarks>
	/// If both the skeleton and actor transform properties are set, the actor transform property will be prioritized by the editor.
	/// </remarks>
	public static readonly IBind<ITransform?> ActorTransformDp = Binder.Register<ITransform?, TransformEditor>(nameof(ActorTransform), OnActorTransformChanged, BindMode.TwoWay);

	/// <summary>Dependency property for the translation override.</summary>
	/// <remarks>
	/// If set, the editor will use this value to determine if a transform translation is allowed.
	/// </remarks>
	public static readonly IBind<bool?> CanTranslateOverrideDp = Binder.Register<bool?, TransformEditor>(nameof(CanTranslateOverride), OnCanTranslateChanged, BindMode.OneWay);

	private readonly Dictionary<string, Vector3> initialPositions = [];
	private readonly Dictionary<string, Quaternion> initialRotations = [];
	private readonly Dictionary<string, Vector3> initialScales = [];

	private ActorMemory? currentActor;

	/// <summary>
	/// Initializes a new instance of the <see cref="TransformEditor"/> class.
	/// </summary>
	public TransformEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		if (TargetService.Instance.SelectedActor != null)
		{
			this.currentActor = TargetService.Instance.SelectedActor;
			this.currentActor.PropertyChanged += this.OnActorPropertyChanged;
		}

		TargetService.ActorSelected += this.OnSelectedActorChanged;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets or sets the skeleton entity.</summary>
	public SkeletonEntity? Skeleton
	{
		get => SkeletonDp.Get(this);
		set => SkeletonDp.Set(this, value);
	}

	/// <summary>Gets or sets the actor transform.</summary>
	public ITransform? ActorTransform
	{
		get => ActorTransformDp.Get(this);
		set => ActorTransformDp.Set(this, value);
	}

	/// <summary>Gets or sets the translation override.</summary>
	public bool? CanTranslateOverride
	{
		get => CanTranslateOverrideDp.Get(this);
		set => CanTranslateOverrideDp.Set(this, value);
	}

	public static Settings Settings => SettingsService.Current;

	/// <summary>Gets the selected actor.</summary>
	public ActorMemory? SelectedActor => this.currentActor;

	/// <summary>Gets all selected bones for linked skeleton.</summary>
	/// <remarks>If the skeleton is not set, an empty collection is returned.</remarks>
	public IEnumerable<Bone> SelectedBones => this.Skeleton?.SelectedBones ?? Enumerable.Empty<Bone>();

	/// <summary>Gets a value indicating whether translation is allowed.</summary>
	public bool CanTranslate => this.CanTranslateOverride ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanTranslate) && TargetService.Instance.SelectedActor != null && (TargetService.Instance.SelectedActor.IsMotionDisabled || PoseService.Instance.FreezeWorldPosition));

	/// <summary>Gets a value indicating whether rotation is allowed.</summary>
	public bool CanRotate => this.ActorTransform?.CanRotate ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanRotate));

	/// <summary>Gets a value indicating whether scaling is allowed.</summary>
	public bool CanScale => this.ActorTransform?.CanScale ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanScale));

	/// <summary>Gets a value indicating whether linked scaling is allowed.</summary>
	public bool CanLinkScale => this.ActorTransform?.CanLinkScale ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.CanLinkScale));

	/// <summary>Gets a value indicating whether scaling is linked.</summary>
	public bool ScaleLinked => this.ActorTransform?.ScaleLinked ?? (this.Skeleton != null && this.Skeleton.SelectedBones != null && this.Skeleton.SelectedBones.All(b => b.ScaleLinked));

	/// <summary>Gets or sets the position.</summary>
	[DoNotNotify]
	public Vector3 Position
	{
		get
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
				return this.ActorTransform.Position;

			// If skeleton is not set, return default value
			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Vector3.Zero;

			// If there is no selection, use the actor transform
			if (!this.SelectedBones.Any())
				return this.SelectedActor?.ModelObject?.Transform?.Position ?? Vector3.Zero;

			// If there is a single bone selected, use its position
			// For multiple bones, return the deviation from the initial position
			return this.Skeleton.SelectedBones.Count() == 1
				? this.Skeleton.SelectedBones.First().Position
				: this.DeviationPosition;
		}
		set
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
			{
				this.ActorTransform.Position = value;
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
				Vector3 deviation = value - Vector3.Zero;
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					if (this.initialPositions.TryGetValue(bone.Name, out Vector3 initialPos))
					{
						bone.Position = initialPos + deviation;
					}
				}
			}
		}
	}

	/// <summary>Gets or sets the rotation.</summary>
	[DoNotNotify]
	public Quaternion Rotation
	{
		get
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
				return this.ActorTransform.Rotation;

			// If skeleton is not set, return default value
			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Quaternion.Identity;

			// If there is no selection, use the actor transform
			if (!this.SelectedBones.Any())
				return this.SelectedActor?.ModelObject?.Transform?.Rotation ?? Quaternion.Identity;

			// If there is a single bone selected, use its rotation
			// For multiple bones, return the deviation from the initial rotation
			return this.Skeleton.SelectedBones.Count() == 1
				? this.Skeleton.SelectedBones.First().Rotation
				: this.DeviationRotation;
		}
		set
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
			{
				this.ActorTransform.Rotation = value;
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
				Quaternion deviation = value * Quaternion.Inverse(Quaternion.Identity);
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					if (this.initialRotations.TryGetValue(bone.Name, out Quaternion initialRotation))
					{
						bone.Rotation = initialRotation * deviation;
					}
				}
			}
		}
	}

	/// <summary>Gets the root rotation.</summary>
	public Quaternion RootRotation
	{
		get
		{
			if (this.Skeleton != null)
			{
				if (this.Skeleton.SelectedBones?.Count() == 1)
					return this.Skeleton.SelectedBones.First().RootRotation;
				else if (this.Skeleton.SelectedBones?.Count() > 1)
					return Quaternion.Multiply(this.Skeleton.RootRotation, Quaternion.Multiply(Quaternion.CreateFromAxisAngle(-Vector3.UnitX, MathF.PI / 2), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2)));
			}

			return Quaternion.Identity;
		}
	}

	/// <summary>Gets or sets the scale.</summary>
	[DoNotNotify]
	public Vector3 Scale
	{
		get
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
				return this.ActorTransform.Scale;

			// If skeleton is not set, return default value
			if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
				return Vector3.Zero;

			// If there is no selection, use the actor transform
			if (!this.SelectedBones.Any())
				return this.SelectedActor?.ModelObject?.Transform?.Scale ?? Vector3.Zero;

			// If there is a single bone selected, use its scale
			// For multiple bones, return the deviation from the initial scale
			return this.Skeleton.SelectedBones.Count() == 1
				? this.Skeleton.SelectedBones.First().Scale
				: this.DeviationScale;
		}
		set
		{
			// If present, prioritize actor transform
			if (this.ActorTransform != null)
			{
				this.ActorTransform.Scale = value;
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
				Vector3 deviation = value - Vector3.Zero;
				foreach (Bone bone in this.Skeleton.SelectedBones)
				{
					if (this.initialScales.TryGetValue(bone.Name, out Vector3 initialScale))
					{
						bone.Scale = initialScale + deviation;
					}
				}
			}
		}
	}

	/// <summary>Gets the deviation position for multi-bone selections.</summary>
	private Vector3 DeviationPosition
	{
		get
		{
			// For any selection, use the first bone as reference point
			if (this.Skeleton?.SelectedBones?.FirstOrDefault() is Bone bone && this.initialPositions.TryGetValue(bone.Name, out Vector3 initialPosition))
				return bone.Position - initialPosition;

			return Vector3.Zero;
		}
	}

	/// <summary>Gets the deviation rotation for multi-bone selections.</summary>
	private Quaternion DeviationRotation
	{
		get
		{
			// For any selection, use the first bone as reference point
			if (this.Skeleton?.SelectedBones?.FirstOrDefault() is Bone bone && this.initialRotations.TryGetValue(bone.Name, out Quaternion initialRotation))
				return Quaternion.Multiply(bone.Rotation, Quaternion.Inverse(initialRotation));

			return Quaternion.Identity;
		}
	}

	/// <summary>Gets the deviation scale for multi-bone selections.</summary>
	private Vector3 DeviationScale
	{
		get
		{
			// For any selection, use the first bone as reference point
			if (this.Skeleton?.SelectedBones?.FirstOrDefault() is Bone bone && this.initialScales.TryGetValue(bone.Name, out Vector3 initialScale))
				return bone.Scale - initialScale;

			return Vector3.Zero;
		}
	}

	/// <summary>Handles changes to the skeleton dependency property.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
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

		Log.Verbose("[TransformEditor] Skeleton was updated");
	}

	/// <summary>Handles changes to the actor transform dependency property.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnActorTransformChanged(TransformEditor sender, ITransform? value)
	{
		sender.RaisePropertyChanged(string.Empty);
	}

	/// <summary>Handles changes to the translation override dependency property.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnCanTranslateChanged(TransformEditor sender, bool? value)
	{
		sender.RaisePropertyChanged(nameof(sender.CanTranslate));
	}

	/// <summary>Sets internally the initial values for the selected bones.</summary>
	/// <remarks>
	/// The initial values are used to calculate the deviation from the original position, rotation, and scale.
	/// </remarks>
	private void SetInitialValues()
	{
		if (this.Skeleton == null || this.Skeleton.SelectedBones == null)
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

	/// <summary>Raises the property changed event.</summary>
	/// <param name="propertyName">The name of the property that changed.</param>
	private void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>Handles property changes for the skeleton.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnSkeletonPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones))
		{
			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.SetInitialValues();
				this.ScaleVectorEditor.Minimum = this.Skeleton != null && this.Skeleton.SelectedBones.Count() > 1 ? null : "0";
				this.RaisePropertyChanged(string.Empty);
			});
		}
	}

	/// <summary>Handles the history applied event.</summary>
	private void OnHistoryApplied()
	{
		this.RaisePropertyChanged(string.Empty);
	}

	/// <summary>Handles the loaded event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		HistoryService.OnHistoryApplied += this.OnHistoryApplied;
	}

	/// <summary>Handles the unloaded event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		HistoryService.OnHistoryApplied -= this.OnHistoryApplied;

		if (this.Skeleton != null)
		{
			this.Skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;
		}
	}

	/// <summary>
	/// Handles changes to the selected actor.
	/// </summary>
	/// <param name="actor">The new selected actor.</param>
	private void OnSelectedActorChanged(ActorMemory? actor)
	{
		// We already track skeleton changes so this is only used to keep track of actor property changes
		if (this.currentActor != null)
		{
			this.currentActor.PropertyChanged -= this.OnActorPropertyChanged;
		}

		this.currentActor = actor;
		if (this.currentActor != null)
		{
			this.currentActor.PropertyChanged += this.OnActorPropertyChanged;
		}

		// Raise property changed for the entire TransformEditor
		this.RaisePropertyChanged(string.Empty);
	}

	/// <summary>
	/// Handles property changes for the selectod actor's properties.
	/// </summary>
	/// <param name="sender"> The sender.</param>
	/// <param name="e"> The event arguments.</param>
	private void OnActorPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ActorMemory.IsMotionDisabled))
		{
			this.RaisePropertyChanged(nameof(this.CanTranslate));
		}
	}
}
