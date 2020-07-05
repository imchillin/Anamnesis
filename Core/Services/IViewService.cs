// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis;

	public delegate void DrawerEvent();
	public delegate void DialogEvent();

	public enum DrawerDirection
	{
		Left,
		Top,
		Right,
		Bottom,
	}

	public interface IViewService : IService
	{
		/// <summary>
		/// Adds a page to the navigation menu.
		/// </summary>
		/// <typeparam name="T">the type of view to add. Should extend UserControl.</typeparam>
		/// <param name="name">the name of this page.</param>
		/// <param name="icon">the font awesome icon name to use.</param>
		/// <param name="isSupportedCallback">a callback to determine if the given page will appear for a specific actor.</param>
		// Although we could (where T : UserControl) to require correct types, doing so would
		// require the core library to reference the WPF libs, and for simplicity, lets not.
		void AddActorPage<T>(string name, string icon, Func<Actor, bool>? isSupportedCallback = null);

		/// <summary>
		/// Opens a drawer (flyout) menu on the main application window.
		/// </summary>
		/// <typeparam name="T">type of view to place within the drawer.</typeparam>
		/// <param name="title">the title to show at the top of the menu.</param>
		/// <param name="direction">the edge of the window the drawer will appear from.</param>
		Task ShowDrawer<T>(string? title = null, DrawerDirection direction = DrawerDirection.Right);

		/// <summary>
		/// Opens a drawer (flyout) menu on the main application window.
		/// </summary>
		/// <param name="view"> the view to place in the drawer.</param>
		/// <param name="title">the title to show at the top of the menu.</param>
		/// <param name="direction">the edge of the window the drawer will appear from.</param>
		Task ShowDrawer(object view, string? title = null, DrawerDirection direction = DrawerDirection.Right);

		/// <summary>
		/// Opens a blocking dialog with the given view.
		/// </summary>
		/// <typeparam name="TView">the view to place within the dialog.</typeparam>
		/// <typeparam name="TResult">the result the view returns when the dialog is closed.</typeparam>
		/// <param name="title">the title of the dialog.</param>
		/// <returns>the result.</returns>
		Task<TResult> ShowDialog<TView, TResult>(string title)
			where TView : IDialog<TResult>;

		/// <summary>
		/// Opens a blocking dialog with the given view.
		/// </summary>
		/// <typeparam name="TView">the type of view to place within the dialog.</typeparam>
		/// <typeparam name="TResult">the result the view returns when the dialog is closed.</typeparam>
		/// <param name="title">the title of the dialog.</param>
		/// <param name="view">the view to place within the dialog.</param>
		/// <returns>the result.</returns>
		Task<TResult> ShowDialog<TView, TResult>(string title, TView view)
			where TView : IDialog<TResult>;
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
}
