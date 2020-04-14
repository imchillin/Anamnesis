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

	public class ViewService : IViewService
	{
		private Dictionary<string, Type> pendingPages = new Dictionary<string, Type>();
		private Dictionary<string, UserControl> pages = new Dictionary<string, UserControl>();
		private bool isStarted = false;

		public delegate void PageEvent(string path, UserControl page);
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

		public Task Initialize()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			foreach ((string path, Type pageType) in this.pendingPages)
			{
				this.CreatePage(path, pageType);
			}

			this.pendingPages.Clear();

			this.isStarted = true;
			return Task.CompletedTask;
		}

		public void AddPage<T>(string path)
		{
			Type pageType = typeof(T);

			if (this.pages.ContainsKey(path))
				throw new Exception($"Page already registered at path: {path}");

			if (!typeof(UserControl).IsAssignableFrom(pageType))
				throw new Exception($"Page: {pageType} does not extend from UserControl.");

			if (!this.isStarted)
			{
				this.pendingPages.Add(path, pageType);
			}
			else
			{
				this.CreatePage(path, pageType);
			}
		}

		public void ShowPage(string path)
		{
			UserControl page = this.GetPage(path);
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

		private void CreatePage(string path, Type type)
		{
			UserControl page = null;

			try
			{
				App.Current.Dispatcher.Invoke(() =>
				{
					page = Activator.CreateInstance(type) as UserControl;
				});
			}
			catch (TargetInvocationException ex)
			{
				Log.Write(new Exception($"Failed to create page: {type}", ex.InnerException));
				return;
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to create page: {type}", ex));
				return;
			}

			this.pages.Add(path, page);
			this.AddingPage?.Invoke(path, page);
		}

		private UserControl GetPage(string path)
		{
			if (!this.pages.ContainsKey(path))
				throw new Exception($"View not found for path: {path}");

			return this.pages[path];
		}
	}
}
