// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.ThreeD
{
	using System;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;

	public static class MathUtils
	{
		public static readonly Matrix3D ZeroMatrix = new Matrix3D(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		public static readonly Vector3D XAxis = new Vector3D(1, 0, 0);
		public static readonly Vector3D YAxis = new Vector3D(0, 1, 0);
		public static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);

		public static double GetAspectRatio(Size size)
		{
			return size.Width / size.Height;
		}

		public static double DegreesToRadians(double degrees)
		{
			return degrees * (Math.PI / 180.0);
		}

		public static double RadiansToDegrees(double radians)
		{
			return radians * (180 / Math.PI);
		}

		/// <summary>
		///     Computes the effective view matrix for the given
		///     camera.
		/// </summary>
		public static Matrix3D GetViewMatrix(Camera camera)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}

			ProjectionCamera projectionCamera = camera as ProjectionCamera;

			if (projectionCamera != null)
			{
				return GetViewMatrix(projectionCamera);
			}

			MatrixCamera matrixCamera = camera as MatrixCamera;

			if (matrixCamera != null)
			{
				return matrixCamera.ViewMatrix;
			}

			throw new ArgumentException(string.Format("Unsupported camera type '{0}'.", camera.GetType().FullName), "camera");
		}

		/// <summary>
		///     Computes the effective projection matrix for the given
		///     camera.
		/// </summary>
		public static Matrix3D GetProjectionMatrix(Camera camera, double aspectRatio)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}

			PerspectiveCamera perspectiveCamera = camera as PerspectiveCamera;

			if (perspectiveCamera != null)
			{
				return GetProjectionMatrix(perspectiveCamera, aspectRatio);
			}

			OrthographicCamera orthographicCamera = camera as OrthographicCamera;

			if (orthographicCamera != null)
			{
				return GetProjectionMatrix(orthographicCamera, aspectRatio);
			}

			MatrixCamera matrixCamera = camera as MatrixCamera;

			if (matrixCamera != null)
			{
				return matrixCamera.ProjectionMatrix;
			}

			throw new ArgumentException(string.Format("Unsupported camera type '{0}'.", camera.GetType().FullName), "camera");
		}

		public static bool ToViewportTransform(DependencyObject visual, out Matrix3D matrix)
		{
			matrix = Matrix3D.Identity;

			Viewport3DVisual viewportVisual;
			Matrix3D toWorld = GetWorldTransformationMatrix(visual, out viewportVisual);

			bool success;
			Matrix3D toViewport = TryWorldToViewportTransform(viewportVisual, out success);

			if (!success)
				return false;

			toWorld.Append(toViewport);
			matrix = toWorld;
			return true;
		}

		/// <summary>
		///     Computes the transform from world space to the Viewport3DVisual's
		///     inner 2D space.
		///     This method can fail if Camera.Transform is non-invertable
		///     in which case the camera clip planes will be coincident and
		///     nothing will render.  In this case success will be false.
		/// </summary>
		public static Matrix3D TryWorldToViewportTransform(Viewport3DVisual visual, out bool success)
		{
			success = false;
			Matrix3D result = TryWorldToCameraTransform(visual, out success);

			if (success)
			{
				result.Append(GetProjectionMatrix(visual.Camera, MathUtils.GetAspectRatio(visual.Viewport.Size)));
				result.Append(GetHomogeneousToViewportTransform(visual.Viewport));
				success = true;
			}

			return result;
		}

		/// <summary>
		///     Computes the transform from world space to camera space
		///     This method can fail if Camera.Transform is non-invertable
		///     in which case the camera clip planes will be coincident and
		///     nothing will render.  In this case success will be false.
		/// </summary>
		public static Matrix3D TryWorldToCameraTransform(Viewport3DVisual visual, out bool success)
		{
			success = false;
			Matrix3D result = Matrix3D.Identity;

			Camera camera = visual.Camera;

			if (camera == null)
			{
				return ZeroMatrix;
			}

			Rect viewport = visual.Viewport;

			if (viewport == Rect.Empty)
			{
				return ZeroMatrix;
			}

			Transform3D cameraTransform = camera.Transform;

			if (cameraTransform != null)
			{
				Matrix3D m = cameraTransform.Value;

				if (!m.HasInverse)
				{
					return ZeroMatrix;
				}

				m.Invert();
				result.Append(m);
			}

			result.Append(GetViewMatrix(camera));

			success = true;
			return result;
		}

		/// <summary>
		///     Computes the transform from the inner space of the given
		///     Visual3D to the 2D space of the Viewport3DVisual which
		///     contains it.
		///     The result will contain the transform of the given visual.
		///     This method can fail if Camera.Transform is non-invertable
		///     in which case the camera clip planes will be coincident and
		///     nothing will render.  In this case success will be false.
		/// </summary>
		public static Matrix3D TryTransformTo2DAncestor(DependencyObject visual, out Viewport3DVisual viewport, out bool success)
		{
			Matrix3D to2D = GetWorldTransformationMatrix(visual, out viewport);
			to2D.Append(MathUtils.TryWorldToViewportTransform(viewport, out success));

			if (!success)
			{
				return ZeroMatrix;
			}

			return to2D;
		}

		/// <summary>
		///     Computes the transform from the inner space of the given
		///     Visual3D to the camera coordinate space
		///     The result will contain the transform of the given visual.
		///     This method can fail if Camera.Transform is non-invertable
		///     in which case the camera clip planes will be coincident and
		///     nothing will render.  In this case success will be false.
		/// </summary>
		public static Matrix3D TryTransformToCameraSpace(DependencyObject visual, out Viewport3DVisual viewport, out bool success)
		{
			Matrix3D toViewSpace = GetWorldTransformationMatrix(visual, out viewport);
			toViewSpace.Append(MathUtils.TryWorldToCameraTransform(viewport, out success));

			if (!success)
			{
				return ZeroMatrix;
			}

			return toViewSpace;
		}

		/// <summary>
		///     Transforms the axis-aligned bounding box 'bounds' by
		///     'transform'.
		/// </summary>
		/// <param name="bounds">The AABB to transform.</param>
		/// <param name="transform">the transform.</param>
		/// <returns>Transformed AABB.</returns>
		public static Rect3D TransformBounds(Rect3D bounds, Matrix3D transform)
		{
			double x1 = bounds.X;
			double y1 = bounds.Y;
			double z1 = bounds.Z;
			double x2 = bounds.X + bounds.SizeX;
			double y2 = bounds.Y + bounds.SizeY;
			double z2 = bounds.Z + bounds.SizeZ;

			Point3D[] points = new Point3D[]
			{
				new Point3D(x1, y1, z1),
				new Point3D(x1, y1, z2),
				new Point3D(x1, y2, z1),
				new Point3D(x1, y2, z2),
				new Point3D(x2, y1, z1),
				new Point3D(x2, y1, z2),
				new Point3D(x2, y2, z1),
				new Point3D(x2, y2, z2),
			};

			transform.Transform(points);

			// reuse the 1 and 2 variables to stand for smallest and largest
			Point3D p = points[0];
			x1 = x2 = p.X;
			y1 = y2 = p.Y;
			z1 = z2 = p.Z;

			for (int i = 1; i < points.Length; i++)
			{
				p = points[i];

				x1 = Math.Min(x1, p.X);
				y1 = Math.Min(y1, p.Y);
				z1 = Math.Min(z1, p.Z);
				x2 = Math.Max(x2, p.X);
				y2 = Math.Max(y2, p.Y);
				z2 = Math.Max(z2, p.Z);
			}

			return new Rect3D(x1, y1, z1, x2 - x1, y2 - y1, z2 - z1);
		}

		/// <summary>
		///     Normalizes v if |v| > 0.
		///     This normalization is slightly different from Vector3D.Normalize. Here
		///     we just divide by the length but Vector3D.Normalize tries to avoid
		///     overflow when finding the length.
		/// </summary>
		/// <param name="v">The vector to normalize.</param>
		/// <returns>'true' if v was normalized.</returns>
		public static bool TryNormalize(ref Vector3D v)
		{
			double length = v.Length;

			if (length != 0)
			{
				v /= length;
				return true;
			}

			return false;
		}

		/// <summary>
		///     Computes the center of 'box'.
		/// </summary>
		/// <param name="box">The Rect3D we want the center of.</param>
		/// <returns>The center point.</returns>
		public static Point3D GetCenter(Rect3D box)
		{
			return new Point3D(box.X + (box.SizeX / 2), box.Y + (box.SizeY / 2), box.Z + (box.SizeZ / 2));
		}

		public static Point3D NearestPointOnRay(Point3D rayOrigin, Vector3D rayDirection, Point3D pnt)
		{
			rayDirection.Normalize();
			Vector3D v = pnt - rayOrigin;
			double d = Vector3D.DotProduct(v, rayDirection);
			return rayOrigin + (rayDirection * d);
		}

		private static Matrix3D GetViewMatrix(ProjectionCamera camera)
		{
			Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");

			// This math is identical to what you find documented for
			// D3DXMatrixLookAtRH with the exception that WPF uses a
			// LookDirection vector rather than a LookAt point.
			Vector3D zAxis = -camera.LookDirection;
			zAxis.Normalize();

			Vector3D xAxis = Vector3D.CrossProduct(camera.UpDirection, zAxis);
			xAxis.Normalize();

			Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);

			Vector3D position = (Vector3D)camera.Position;
			double offsetX = -Vector3D.DotProduct(xAxis, position);
			double offsetY = -Vector3D.DotProduct(yAxis, position);
			double offsetZ = -Vector3D.DotProduct(zAxis, position);

			return new Matrix3D(xAxis.X, yAxis.X, zAxis.X, 0, xAxis.Y, yAxis.Y, zAxis.Y, 0, xAxis.Z, yAxis.Z, zAxis.Z, 0, offsetX, offsetY, offsetZ, 1);
		}

		private static Matrix3D GetProjectionMatrix(OrthographicCamera camera, double aspectRatio)
		{
			Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");

			// This math is identical to what you find documented for
			// D3DXMatrixOrthoRH with the exception that in WPF only
			// the camera's width is specified.  Height is calculated
			// from width and the aspect ratio.
			double w = camera.Width;
			double h = w / aspectRatio;
			double zn = camera.NearPlaneDistance;
			double zf = camera.FarPlaneDistance;

			double m33 = 1 / (zn - zf);
			double m43 = zn * m33;

			return new Matrix3D(2 / w, 0, 0, 0, 0, 2 / h, 0, 0, 0, 0, m33, 0, 0, 0, m43, 1);
		}

		private static Matrix3D GetProjectionMatrix(PerspectiveCamera camera, double aspectRatio)
		{
			Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");

			// This math is identical to what you find documented for
			// D3DXMatrixPerspectiveFovRH with the exception that in
			// WPF the camera's horizontal rather the vertical
			// field-of-view is specified.
			double hFoV = MathUtils.DegreesToRadians(camera.FieldOfView);
			double zn = camera.NearPlaneDistance;
			double zf = camera.FarPlaneDistance;

			double xScale = 1 / Math.Tan(hFoV / 2);
			double yScale = aspectRatio * xScale;
			double m33 = (zf == double.PositiveInfinity) ? -1 : (zf / (zn - zf));
			double m43 = zn * m33;

			return new Matrix3D(xScale, 0, 0, 0, 0, yScale, 0, 0, 0, 0, m33, -1, 0, 0, m43, 0);
		}

		private static Matrix3D GetHomogeneousToViewportTransform(Rect viewport)
		{
			double scaleX = viewport.Width / 2;
			double scaleY = viewport.Height / 2;
			double offsetX = viewport.X + scaleX;
			double offsetY = viewport.Y + scaleY;

			return new Matrix3D(scaleX, 0, 0, 0, 0, -scaleY, 0, 0, 0, 0, 1, 0, offsetX, offsetY, 0, 1);
		}

		/// <summary>
		/// Gets the object space to world space transformation for the given DependencyObject.
		/// </summary>
		/// <param name="visual">The visual whose world space transform should be found.</param>
		/// <param name="viewport">The Viewport3DVisual the Visual is contained within.</param>
		/// <returns>The world space transformation.</returns>
		private static Matrix3D GetWorldTransformationMatrix(DependencyObject visual, out Viewport3DVisual viewport)
		{
			Matrix3D worldTransform = Matrix3D.Identity;
			viewport = null;

			if (!(visual is Visual3D))
			{
				throw new ArgumentException("Must be of type Visual3D.", "visual");
			}

			while (visual != null)
			{
				if (!(visual is ModelVisual3D))
				{
					break;
				}

				Transform3D transform = (Transform3D)visual.GetValue(ModelVisual3D.TransformProperty);

				if (transform != null)
				{
					worldTransform.Append(transform.Value);
				}

				visual = VisualTreeHelper.GetParent(visual);
			}

			viewport = visual as Viewport3DVisual;

			if (viewport == null)
			{
				if (visual != null)
				{
					// In WPF 3D v1 the only possible configuration is a chain of
					// ModelVisual3Ds leading up to a Viewport3DVisual.
					throw new ApplicationException(string.Format("Unsupported type: '{0}'.  Expected tree of ModelVisual3Ds leading up to a Viewport3DVisual.", visual.GetType().FullName));
				}

				return ZeroMatrix;
			}

			return worldTransform;
		}
	}
}
