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
		private Dictionary<string, Type> pages = new Dictionary<string, Type>();

		public delegate void PageEvent(string path, Type pageType);
		public delegate Task DrawerEvent(string title, UserControl drawer, DrawerDirection direction);

		public event PageEvent AddingPage;
		public event DrawerEvent ShowingDrawer;
		public event PageEvent ShowingPage;

		public IEnumerable<string> PagePaths
		{
			get
			{
				return this.pages.Keys;
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

		public void AddPage<T>(string path)
		{
			Type view = typeof(T);

			if (this.pages.ContainsKey(path))
				throw new Exception($"Page already registered at path: {path}");

			if (!typeof(UserControl).IsAssignableFrom(view))
				throw new Exception($"Page: {view} does not extend from UserControl.");

			this.pages.Add(path, view);

			this.AddingPage?.Invoke(path, view);
		}

		public void ShowPage(string path)
		{
			Type page = this.GetPage(path);
			this.ShowingPage?.Invoke(path, page);
		}

		public Task ShowDrawer<T>(string title, DrawerDirection direction)
		{
			UserControl view = this.CreateView<T>();
			return this.ShowingDrawer?.Invoke(title, view, direction);
		}

		public Task ShowDrawer(object view, string title, DrawerDirection direction)
		{
			UserControl control = view as UserControl;

			if (control == null)
				throw new Exception("Invalid view");

			return this.ShowingDrawer?.Invoke(title, control, direction);
		}

		private UserControl CreateView<T>()
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
				return null;
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to create view: {viewType}", ex));
				return null;
			}

			return view;
		}

		private Type GetPage(string path)
		{
			if (!this.pages.ContainsKey(path))
				throw new Exception($"View not found for path: {path}");

			return this.pages[path];
		}
	}
}
