// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
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
		/// Adds a view of type T to the navigation menu at path.
		/// </summary>
		/// <typeparam name="T">the type of view to add. Should extend UserControl.</typeparam>
		/// <param name="path">the path to add the menu item in the navigation sidebar.</param>
		// Although we could (where T : UserControl) to require correct types, doing so would
		// require the core library to reference the WPF libs, and for simplicity, lets not.
		void AddView<T>(string path);

		/// <summary>
		/// Opens a drawer (flyout) menu on the main application window.
		/// </summary>
		/// <typeparam name="T">type of view to place within the drawer.</typeparam>
		/// <param name="title">the title to show at the top of the menu.</param>
		/// <param name="direction">the edge of the window the drawer will appear from.</param>
		void ShowDrawer<T>(string title = null, DrawerDirection direction = DrawerDirection.Right);
	}
}
