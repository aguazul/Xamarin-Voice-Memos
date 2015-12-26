using System;
using SQLite;

#if __IOS__
using Foundation;
#endif

namespace VoiceMailer
{
	public class DataModel
	{
		public DataModel ()
		{
		}
	}

	public class Note
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set;}
		public string title { get; set; }
		public string description { get; set; }
		public DateTime dateCreated { get; set; }
		public string audioFileURL { get; set; }

	}

	// Note
	// title: note 1
	// description:  Descrip 1
	// dateCreated 12/12/2015


}

