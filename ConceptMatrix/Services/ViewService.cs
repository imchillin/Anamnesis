// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.Services;

	public class ViewService : IViewService
	{
		private Dictionary<string, Type> views = new Dictionary<string, Type>();

		public delegate void ViewEvent(string path, Type type);
		public delegate void DrawerEvent(string title, UserControl drawer, DrawerDirection direction);

		public event ViewEvent AddingView;
		public event DrawerEvent ShowingDrawer;
		public event ViewEvent ShowingView;

		public IEnumerable<string> ViewPaths
		{
			get
			{
				return this.views.Keys;
			}
		}

		public Task Initialize(IServices services)
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public void AddView<T>(string path)
		{
			Type view = typeof(T);

			if (this.views.ContainsKey(path))
				throw new Exception($"View already registered at path: {path}");

			if (!typeof(UserControl).IsAssignableFrom(view))
				throw new Exception($"View: {view} does not extend from UserControl.");

			this.views.Add(path, view);

			this.AddingView?.Invoke(path, view);
		}

		public void ShowView(string path)
		{
			Type type = this.GetView(path);
			this.ShowingView?.Invoke(path, type);
		}

		public void ShowDrawer<T>(string title, DrawerDirection direction)
		{
			Type viewType = typeof(T);

			if (!typeof(UserControl).IsAssignableFrom(viewType))
				throw new Exception($"View: {viewType} does not extend from UserControl.");

			UserControl view;
			try
			{
				view = (UserControl)Activator.CreateInstance(viewType);
			}
			catch (TargetInvocationException ex)
			{
				Log.Write(new Exception($"Failed to create view: {viewType}", ex.InnerException));
				return;
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to create view: {viewType}", ex));
				return;
			}

			this.ShowingDrawer?.Invoke(title, view, direction);
		}

		public void ShowDrawer(object view, string title, DrawerDirection direction)
		{
			UserControl control = view as UserControl;

			if (control == null)
				throw new Exception("Invalid view");

			this.ShowingDrawer?.Invoke(title, control, direction);
		}

		private Type GetView(string path)
		{
			if (!this.views.ContainsKey(path))
				throw new Exception($"View not found for path: {path}");

			return this.views[path];
		}
	}
}
