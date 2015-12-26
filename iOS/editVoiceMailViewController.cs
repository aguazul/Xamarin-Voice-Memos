// Coded by Brandon Bosse
// Dec 2015
// Using Xamarin Studio

using System;
using UIKit;
using AVFoundation;
using Foundation;
using System.Diagnostics;
using AudioToolbox;
using System.IO;

namespace VoiceMailer.iOS
{
	public class editVoiceMailViewController : UIViewController
	{
		Note note;

		// Declare Audio Recording components
		AVPlayer player;
		NSUrl audioFilePath = null;
		NSObject observer;

		public editVoiceMailViewController (Note _note)
		{
			note = _note;
			audioFilePath = new NSUrl (note.audioFileURL);
			AudioSession.Initialize ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Create graphical elements here
			this.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(51,51,51);
			this.Title = "Edit Voice Memo";
			this.NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes () { ForegroundColor = UIColor.White };

			this.NavigationController.NavigationBar.TintColor = UIColor.White;
			this.View.BackgroundColor = UIColor.FromRGB (63, 171, 171);

			var titleLabel = new UILabel () {
				Frame = new CoreGraphics.CGRect (0, 100, View.Bounds.Width, 30),
				TextColor = UIColor.White,
				BackgroundColor = UIColor.FromRGB(51,51,51), // dark grey
				Font = UIFont.SystemFontOfSize (18),
				Text = "  Title"
			};
			this.View.Add (titleLabel);

			var titleEntryBox = new UITextField () {
				Frame = new CoreGraphics.CGRect (0, 130, View.Bounds.Width, 45),
				BackgroundColor = UIColor.White,
				TextColor = UIColor.DarkGray,
				Text = note.title
			};
			this.View.Add (titleEntryBox);

			var descriptionLabel = new UILabel () {
				Frame = new CoreGraphics.CGRect (0, 190, View.Bounds.Width, 30),
				TextColor = UIColor.White,
				Font = UIFont.SystemFontOfSize (18),
				BackgroundColor = UIColor.FromRGB(51,51,51), // dark grey
				Text = "  Description"
			};
			this.View.Add (descriptionLabel);

			var descriptionEntryBox = new UITextField () {
				Frame = new CoreGraphics.CGRect (0, 220, View.Bounds.Width, 100),
				BackgroundColor = UIColor.White,
				TextColor = UIColor.DarkGray,
				Text = note.description
			};
			this.View.Add (descriptionEntryBox);

			var updateButton = new UIButton () {
				Frame = new CoreGraphics.CGRect (0, this.View.Bounds.Height - 90, this.View.Bounds.Width, 45)
			};
			updateButton.SetTitle ("Save Changes", UIControlState.Normal);
			updateButton.BackgroundColor = UIColor.FromRGB(11, 130, 130); // darker teal
			updateButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.View.Add (updateButton);

			updateButton.TouchUpInside += (sender, e) => {
				if(titleEntryBox.Text.Length < 3)
					return;

				Console.WriteLine("Updated!");
				var noteToUpdate = new Note () {
					ID = note.ID,
					title = titleEntryBox.Text,
					description = descriptionEntryBox.Text,
					dateCreated = DateTime.Now,
					audioFileURL = audioFilePath.ToString()
				};
				Database.updateNote (noteToUpdate);

				this.NavigationController.PopViewController(true);
			};

			var PlayRecordedSoundButton = new UIButton () {
				Frame = new CoreGraphics.CGRect (0, 450, this.View.Bounds.Width, 45)
			};
			PlayRecordedSoundButton.SetTitle ("Play", UIControlState.Normal);
			PlayRecordedSoundButton.BackgroundColor = UIColor.FromRGB(46, 189, 71); // Green
			PlayRecordedSoundButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.View.Add (PlayRecordedSoundButton);

			var LengthOfRecordingLabel = new UILabel () {
				Frame = new CoreGraphics.CGRect (0, 495, this.View.Bounds.Width, 30),
				BackgroundColor = UIColor.FromRGB(51,51,51), // dark grey
				TextAlignment = UITextAlignment.Center,
				TextColor = UIColor.White
			};
			this.View.Add (LengthOfRecordingLabel);

			// play recorded sound wireup
			PlayRecordedSoundButton.TouchUpInside += (sender, e) => {
				if (PlayRecordedSoundButton.CurrentTitle.Equals("Play"))
				{

					try {
						Console.WriteLine("Playing Back Recording " + this.audioFilePath.ToString());

						// The following line prevents the audio from stopping 
						// when the device autolocks. will also make sure that it plays, even
						// if the device is in mute
						AudioSession.Category = AudioSessionCategory.MediaPlayback;

						this.player = new AVPlayer (this.audioFilePath);
						this.player.Play();
						PlayRecordedSoundButton.SetTitle ("Pause", UIControlState.Normal);
						PlayRecordedSoundButton.BackgroundColor = UIColor.FromRGB (255, 94, 94); // light red
					} catch (Exception ex) {
						Console.WriteLine("There was a problem playing back audio: ");
						Console.WriteLine(ex.Message);
					}
				} else if (PlayRecordedSoundButton.CurrentTitle.Equals("Pause")) {
					try {
						this.player.Pause();
						PlayRecordedSoundButton.SetTitle ("Resume", UIControlState.Normal);
						PlayRecordedSoundButton.BackgroundColor = UIColor.FromRGB (255, 201, 94); // yellow
					} catch (Exception ex) {
						Console.WriteLine("There was a problem pausing the audio: ");
						Console.WriteLine(ex.Message);
					}
				} else { // button title = Resume
					try {
						this.player.Play();
						PlayRecordedSoundButton.SetTitle ("Pause", UIControlState.Normal);
						PlayRecordedSoundButton.BackgroundColor = UIColor.FromRGB (255, 94, 94); // light red
					} catch (Exception ex) {
						Console.WriteLine("There was a problem resuming the audio: ");
						Console.WriteLine(ex.Message);
					}
				}
			};

			observer = NSNotificationCenter.DefaultCenter.AddObserver (AVPlayerItem.DidPlayToEndTimeNotification, delegate (NSNotification n) {
				try {
					player.Dispose ();
					player = null;
					PlayRecordedSoundButton.SetTitle ("Play", UIControlState.Normal);
					PlayRecordedSoundButton.BackgroundColor = UIColor.FromRGB (46, 189, 71);
				} catch {
					Console.WriteLine("Oops something happened with the observer.");
				}
			});
		}
	}
}

