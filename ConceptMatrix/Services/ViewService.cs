// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.Services;

	public class ViewService : IViewService
	{
		private Dictionary<string, Type> views = new Dictionary<string, Type>();

		public delegate void ViewEvent(string path);
		public delegate void DrawerEvent(string title, Type drawerType, DrawerDirection direction);

		public event ViewEvent AddingView;
		public event DrawerEvent ShowingDrawer;

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

			this.AddingView?.Invoke(path);
		}

		public Type GetView(string path)
		{
			if (!this.views.ContainsKey(path))
				throw new Exception($"View not found for path: {path}");

			return this.views[path];
		}

		public void ShowDrawer<T>(string title, DrawerDirection direction)
		{
			Type view = typeof(T);

			if (!typeof(UserControl).IsAssignableFrom(view))
				throw new Exception($"View: {view} does not extend from UserControl.");

			this.ShowingDrawer?.Invoke(title, typeof(T), direction);
		}
	}
}
