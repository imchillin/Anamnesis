// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.WpfStyles;
	using FontAwesome.Sharp;
	using Microsoft.VisualBasic;

	/// <summary>
	/// Interaction logic for TargetSelectorView.xaml.
	/// </summary>
	public partial class TargetSelectorView : UserControl, IDrawer
	{
		public Actor Actor;

		private SelectionService selection;
		private IInjectionService injection;
		private bool lockSelection = false;

		public TargetSelectorView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.selection = App.Services.Get<SelectionService>();
			this.injection = App.Services.Get<IInjectionService>();

			this.DataContext = this;

			this.GetEntities();
		}

		public event DrawerEvent Close;

		public ObservableCollection<ActorEx> Entities { get; set; } = new ObservableCollection<ActorEx>();

		private void GetEntities()
		{
			try
			{
				this.lockSelection = true;

				this.Entities.Clear();

				Dictionary<string, Actor> actors = this.selection.GetSelectableActors();
				foreach (Actor actor in actors.Values)
				{
					this.Entities.Add(new ActorEx(actor));
				}

				this.lockSelection = false;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void OnSelected(object sender, RoutedEventArgs e)
		{
			if (this.lockSelection)
				return;

			if (sender is RadioButton btn)
			{
				ActorEx selection = btn.DataContext as ActorEx;
				this.Actor = selection.Actor;
				this.selection.SelectActor(this.Actor);

				this.Close?.Invoke();
			}
		}

		public class ActorEx
		{
			public readonly Actor Actor;

			public ActorEx(Actor actor)
			{
				this.Actor = actor;
			}

			public IconChar Icon
			{
				get
				{
					return this.Actor.Type.GetIcon();
				}
			}

			public string Name
			{
				get
				{
					return this.Actor.Name;
				}
			}
		}
	}
}
