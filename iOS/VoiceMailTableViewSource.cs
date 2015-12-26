using System;
using UIKit;
using System.Collections.Generic;

namespace VoiceMailer.iOS
{
	public class VoiceMailTableViewSource : UITableViewSource
	{
		UIViewController controller;

		public VoiceMailTableViewSource (List<Note> passedNotes, UIViewController _controller)
		{
			notes = passedNotes;
			controller = _controller;
		}
		List<Note> notes;
		string CellIdentifier = "TableCell";

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return notes.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (CellIdentifier);
			var note = notes [indexPath.Row];

			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellIdentifier);
			}

			cell.TextLabel.Text = note.title;
			cell.TextLabel.TextColor = UIColor.White;
			cell.BackgroundColor = UIColor.FromRGB (63, 171, 171);
			return cell;
		}

		public override void RowSelected (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);

			controller.NavigationController.PushViewController (new editVoiceMailViewController (notes [indexPath.Row]), true);
		}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
		{
			switch (editingStyle) {
			case UITableViewCellEditingStyle.Delete:
				Database.deleteNote(notes[indexPath.Row]);
				notes.RemoveAt(indexPath.Row);
				tableView.DeleteRows (new Foundation.NSIndexPath[] {indexPath}, UITableViewRowAnimation.Left);
				break;
			case UITableViewCellEditingStyle.None:
				break;
			}
		}
	}
}

