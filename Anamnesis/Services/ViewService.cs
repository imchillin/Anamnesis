// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Character.Pages;
	using Anamnesis.GUI.Views;
	using Anamnesis.GUI.Windows;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Pages;

#pragma warning disable SA1649

	public delegate void DrawerEvent();
	public delegate void DialogEvent();

	public enum DrawerDirection
	{
		Left,
		Top,
		Right,
		Bottom,
	}

	public interface IDrawer
	{
		event DrawerEvent Close;
	}

	public interface IDialog<TResult> : IDialog
	{
		TResult Result { get; }
	}

	public interface IDialog
	{
		event DialogEvent Close;

		void Cancel();
	}

	public class ViewService : ServiceBase<ViewService>
	{
		private static readonly Dictionary<string, Page> PageLookup = new Dictionary<string, Page>();

		public delegate void PageEvent(Page page);
		public delegate Task DrawerEvent(string? title, UserControl drawer, DrawerDirection direction);

		public static event PageEvent? AddingPage;
		public static event DrawerEvent? ShowingDrawer;

		public static IEnumerable<Page> Pages
		{
			get
			{
				return PageLookup.Values;
			}
		}

		public static void AddPage<T>(string name, string icon, Func<ActorViewModel, bool>? isSupportedCallback = null)
		{
			Page page = new Page();
			page.Icon = icon;
			page.Name = name;
			page.IsSupportedCallback = isSupportedCallback;
			page.Type = typeof(T);

			if (PageLookup.ContainsKey(name))
				throw new Exception($"Page already registered with name: {name}");

			if (!typeof(UserControl).IsAssignableFrom(page.Type))
				throw new Exception($"Page: {page.Type} does not extend from UserControl.");

			PageLookup.Add(name, page);
			AddingPage?.Invoke(page);
		}

		public static Task ShowDrawer<T>(string? title = null, DrawerDirection direction = DrawerDirection.Right)
		{
			UserControl? view = CreateView<T>();

			if (view == null || ShowingDrawer == null)
				return Task.CompletedTask;

			return ShowingDrawer.Invoke(title, view, direction);
		}

		public static Task ShowDrawer(object view, string? title = null, DrawerDirection direction = DrawerDirection.Right)
		{
			if (!(view is UserControl control))
				throw new Exception("Invalid view");

			if (ShowingDrawer == null)
				return Task.CompletedTask;

			return ShowingDrawer.Invoke(title, control, direction);
		}

		public static Page GetPage(string path)
		{
			if (!PageLookup.ContainsKey(path))
				throw new Exception($"View not found for path: {path}");

			return PageLookup[path];
		}

		public static Task<TResult> ShowDialog<TView, TResult>(string title)
			where TView : IDialog<TResult>
		{
			UserControl? userControl = CreateView<TView>();

			if (userControl is TView view)
				return ShowDialog<TView, TResult>(title, view);

			throw new InvalidOperationException();
		}

		public static Task<TResult> ShowDialog<TView, TResult>(string title, TView view)
			where TView : IDialog<TResult>
		{
			Dialog dlg = new Dialog();
			dlg.ContentArea.Content = view;
			dlg.TitleText.Text = title;
			dlg.Owner = App.Current.MainWindow;

			IDialog<TResult>? dialogInterface = dlg.ContentArea.Content as IDialog<TResult>;

			if (dialogInterface == null)
				throw new Exception("Dialog interface not found");

			dialogInterface.Close += () => dlg.Close();

			dlg.ShowDialog();

			return Task.FromResult(dialogInterface.Result);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			AddPage<HomeView>("Home", "Home");
			AddPage<AppearancePage>("Character", "useralt");
			AddPage<PosePage>("Pose", "running");
		}

		private static UserControl? CreateView<T>()
		{
			Type viewType = typeof(T);

			if (!typeof(UserControl).IsAssignableFrom(viewType))
				throw new Exception($"View: {viewType} does not extend from UserControl.");

			UserControl? view;
			try
			{
				view = Activator.CreateInstance(viewType) as UserControl;
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

		public class Page
		{
			public Type? Type;

			public string Name { get; set; } = string.Empty;
			public string Icon { get; set; } = string.Empty;

			public Func<ActorViewModel, bool>? IsSupportedCallback { get; set; }

			public UserControl Create()
			{
				try
				{
					if (this.Type == null)
						throw new Exception();

					UserControl? control = Activator.CreateInstance(this.Type) as UserControl;

					if (control == null)
						throw new Exception("Unknown failure");

					return control;
				}
				catch (TargetInvocationException ex)
				{
					throw new Exception($"Failed to create page: {this.Type}", ex.InnerException);
				}
				catch (Exception ex)
				{
					throw new Exception($"Failed to create page: {this.Type}", ex);
				}
			}

			public bool Supports(ActorViewModel? actor)
			{
				if (this.IsSupportedCallback == null)
					return true;

				if (actor is null)
					return false;

				return this.IsSupportedCallback.Invoke((ActorViewModel)actor);
			}
		}
	}
}
