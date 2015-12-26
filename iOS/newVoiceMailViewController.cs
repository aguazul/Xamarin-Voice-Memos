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
	public class NewVoiceMailViewController : UIViewController
	{
		// Declare Audio Recording components
		AVAudioRecorder recorder;
		AVPlayer player;
		NSDictionary settings;
		Stopwatch stopwatch = null;
		NSUrl audioFilePath = null;
		NSObject observer;

		public NewVoiceMailViewController ()
		{
			AudioSession.Initialize ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Create graphical elements here
			this.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(51,51,51); // dark grey
			this.Title = "New Voice Memo";
			this.NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes () { ForegroundColor = UIColor.White };

			this.NavigationController.NavigationBar.TintColor = UIColor.White;
			this.View.BackgroundColor = UIColor.FromRGB (63, 171, 171); // teal

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
				Placeholder = "Enter title...",
				TextColor = UIColor.DarkGray
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
				Placeholder = "Enter description...",
				TextColor = UIColor.DarkGray
			};
			this.View.Add (descriptionEntryBox);

			var recordButton = new UIButton () {
				Frame = new CoreGraphics.CGRect (0, 375, this.View.Bounds.Width, 45)
			};
			recordButton.SetTitle ("Record", UIControlState.Normal);
			recordButton.BackgroundColor = UIColor.FromRGB (255, 201, 94); // Yellow
			recordButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.View.Add (recordButton);

			var RecordingStatusLabel = new UILabel () {
				Frame = new CoreGraphics.CGRect (0, 420, this.View.Bounds.Width, 30),
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Center,
				TextColor = UIColor.White,
			};
			this.View.Add (RecordingStatusLabel);

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

			var saveButton = new UIButton () {
				Frame = new CoreGraphics.CGRect (0, this.View.Bounds.Height - 90, this.View.Bounds.Width, 45)
			};
			saveButton.SetTitle ("Save Voice Memo", UIControlState.Normal);
			saveButton.BackgroundColor = UIColor.FromRGB(11, 130, 130); // darker teal
			saveButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			this.View.Add (saveButton);

			saveButton.TouchUpInside += (sender, e) => {
				if(titleEntryBox.Text.Length < 3) // Don't allow titles < 3 characters
					return;

				var noteToSave = new Note () {
					title = titleEntryBox.Text,
					description = descriptionEntryBox.Text,
					dateCreated = DateTime.Now,
					audioFileURL = "file://" + CopyFileInBundleToDocumentsFolder(audioFilePath)
				};

				// Save the note to the database
				Database.insertNote (noteToSave);

				titleEntryBox.Text = "";
				descriptionEntryBox.Text = "";
			};

			// start recording wireup
			recordButton.TouchUpInside += (sender, e) => {
				if (recordButton.CurrentTitle.Equals("Record")) // Record button pressed - begin recording
				{
					recordButton.SetTitle ("Stop", UIControlState.Normal); // Change the button to say "Stop"
					recordButton.BackgroundColor = UIColor.FromRGB (255, 94, 94); // Light Red

					Console.WriteLine("Begin Recording");

					AudioSession.Category = AudioSessionCategory.RecordAudio;
					AudioSession.SetActive (true);

					if (!PrepareAudioRecording ()) {
						RecordingStatusLabel.Text = "Error preparing";
						RecordingStatusLabel.BackgroundColor = UIColor.FromRGB (217, 18, 18); // red
						return;
					}

					if (!recorder.Record ()) {
						RecordingStatusLabel.Text = "Error recording";
						RecordingStatusLabel.BackgroundColor = UIColor.FromRGB (217, 18, 18); // red
						return;
					}

					this.stopwatch = new Stopwatch();
					this.stopwatch.Start();
					LengthOfRecordingLabel.Text = "";
					PlayRecordedSoundButton.Enabled = false;
				} else { // Stop button pressed - stop recording
					this.recorder.Stop();
					LengthOfRecordingLabel.Text = string.Format("{0:hh\\:mm\\:ss}", this.stopwatch.Elapsed);
					this.stopwatch.Stop();
					recordButton.SetTitle ("Record", UIControlState.Normal); // Set the button back to Record
					recordButton.BackgroundColor = UIColor.FromRGB (255, 201, 94); // Yellow 
					PlayRecordedSoundButton.Enabled = true;
				}
			};

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

		public override void ViewDidUnload ()
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver (observer);

			base.ViewDidUnload ();

			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;

			//ReleaseDesignerOutlets ();
		}

		//public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		//{
		//	return true;
		//}

		bool PrepareAudioRecording ()
		{
			// You must initialize an audio session before trying to record
			var audioSession = AVAudioSession.SharedInstance ();
			var err = audioSession.SetCategory (AVAudioSessionCategory.PlayAndRecord);
			if(err != null) {
				Console.WriteLine ("audioSession: {0}", err);
				return false;
			}
			err = audioSession.SetActive (true);
			if(err != null ){
				Console.WriteLine ("audioSession: {0}", err);
				return false;
			}

			// Declare string for application temp path and tack on the file extension
			string fileName = string.Format ("Myfile{0}.aac", DateTime.Now.ToString ("yyyyMMddHHmmss"));
			string tempRecording = Path.Combine (Path.GetTempPath (), fileName);

			Console.WriteLine (tempRecording);
			this.audioFilePath = NSUrl.FromFilename(tempRecording);

			//set up the NSObject Array of values that will be combined with the keys to make the NSDictionary
			NSObject[] values = new NSObject[]
			{    
				NSNumber.FromFloat(44100.0f),
				NSNumber.FromInt32((int)AudioToolbox.AudioFormatType.MPEG4AAC),
				NSNumber.FromInt32(1),
				NSNumber.FromInt32((int)AVAudioQuality.High)
			};
			//Set up the NSObject Array of keys that will be combined with the values to make the NSDictionary
			NSObject[] keys = new NSObject[]
			{
				AVAudioSettings.AVSampleRateKey,
				AVAudioSettings.AVFormatIDKey,
				AVAudioSettings.AVNumberOfChannelsKey,
				AVAudioSettings.AVEncoderAudioQualityKey
			};			
			//Set Settings with the Values and Keys to create the NSDictionary
			settings = NSDictionary.FromObjectsAndKeys (values, keys);

			//Set recorder parameters
			NSError error;
			recorder = AVAudioRecorder.Create(this.audioFilePath, new AudioSettings(settings), out error);
			if ((recorder == null) || (error != null)) {
				Console.WriteLine (error);
				return false;
			}

			//Set Recorder to Prepare To Record
			if (!recorder.PrepareToRecord ()) {
				recorder.Dispose ();
				recorder = null;
				return false;
			}

			recorder.FinishedRecording += delegate (object sender, AVStatusEventArgs e) {
				recorder.Dispose ();
				recorder = null;
				Console.WriteLine ("Done Recording (status: {0})", e.Status);
			};

			return true;
		}

		// save audio file
		public string CopyFileInBundleToDocumentsFolder(NSUrl filePath)
		{
			// extract filename from NSUrl
			var filename = filePath.LastPathComponent;

			//---path to Documents folder---
			var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			//---destination path for file in the Documents
			// folder---
			var destinationPath = Path.Combine(documentsFolderPath, filename);

			var sourcePath = filePath.AbsoluteString;

			sourcePath = sourcePath.Substring (7, sourcePath.Length - 7);

			//---print for verfications---
			Console.WriteLine(destinationPath);
			Console.WriteLine(sourcePath);

			try {
				//---copy only if file does not exist---
				if (!File.Exists(destinationPath))
				{
					File.Copy(sourcePath, destinationPath);
					Console.WriteLine("File copied to Documents");
					return destinationPath;
				}  else {
					Console.WriteLine("File already exists");
					return "error";
				}
			}  catch (Exception e) {
				Console.WriteLine(e.Message);
				return "error";
			}
		}
	}
}

