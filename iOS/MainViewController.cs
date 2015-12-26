// Coded by Brandon Bosse
// Dec 2015
// Using Xamarin Studio

using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Collections.Generic;

namespace VoiceMailer.iOS
{
	partial class MainViewController : UIViewController
	{
		List<Note> notes;
		UITableView table;

		public MainViewController (IntPtr handle) : base (handle)
		{
			notes = new List<Note> ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Create graphical elements here
			this.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(51,51,51);
			this.Title = "Voice Memo";
			this.NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes () { ForegroundColor = UIColor.White };
			this.View.BackgroundColor = UIColor.FromRGB (63, 171, 171);

			table = new UITableView () {
				Frame = new CoreGraphics.CGRect (0, 0, this.View.Bounds.Width, this.View.Bounds.Height - this.NavigationController.NavigationBar.Frame.Height),
				BackgroundColor = UIColor.FromRGB (63, 171, 171)
			};
			this.View.Add (table);

			var addButton = new UIBarButtonItem (UIBarButtonSystemItem.Add);
			addButton.TintColor = UIColor.White;

			this.NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { addButton };

			addButton.Clicked += (sender, e) => {
				Console.WriteLine("Button clicked!");
				// Open up a new screen to add a new voice mail
				this.NavigationController.PushViewController(new NewVoiceMailViewController(), true);
			};
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			notes = Database.getNotes ();
			table.Source = new VoiceMailTableViewSource (notes, this);
		}
	}
}
