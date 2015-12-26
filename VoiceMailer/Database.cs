using System;
using System.IO;
using SQLite;
using System.Collections.Generic;


#if __IOS__
using Foundation;
#endif

namespace VoiceMailer
{
	public class Database
	{
		public Database ()
		{
			createDatabase ();
		}

		// return a filepath to documents on both android and iOS
		public static string documentsFolder () 
		{
			string path;

			#if __IOS__
			path = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User) [0].Path;
			#endif

			#if __ANDROID__
			path = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
			#endif

			Directory.CreateDirectory (path);
			return path;
		}

		public static void createDatabase () 
		{
			var conn = new SQLiteConnection (System.IO.Path.Combine (documentsFolder (), "database.db"), false);
			conn.CreateTable <Note> ();
			conn.Close ();
		}

		public static void insertNote (Note note)
		{
			var conn = new SQLiteConnection (System.IO.Path.Combine (documentsFolder (), "database.db"), false);
			conn.Insert (note);
			conn.Close ();
		}	

		public static void deleteNote (Note note)
		{
			var conn = new SQLiteConnection (System.IO.Path.Combine (documentsFolder (), "database.db"), false);
			conn.Query <Note> ("DELETE FROM Note WHERE dateCreated=?", note.dateCreated);
			conn.Close ();
			// Don't forget to delete the audio file here!
		}

		public static List<Note> getNotes ()
		{
			var conn = new SQLiteConnection (System.IO.Path.Combine (documentsFolder (), "database.db"), false);
			var results = conn.Query<Note> ("SELECT * FROM Note");
			conn.Close ();
			return results;
		}

		public static void updateNote (Note note)
		{
			var conn = new SQLiteConnection (System.IO.Path.Combine (documentsFolder (), "database.db"), false);
			conn.Update (note);
			conn.Close ();
		}

	}
}

