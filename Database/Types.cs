﻿using System;
using System.Collections.Generic;
using SQLite;

namespace Database
{
	[Table("Songs")]
	public class Song
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int ID { get; set; }

		[Column("name"), MaxLength(20)]
		public string Name { get; set; }

		[Column("author"), MaxLength(20)]
		public string Author { get; set; }
	}

	[Table("Fingerprints")]
	public class Fingerprint
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int ID { get; set; }
		[Column("data")]
		public byte[] serializedData { get; set; }
	}
	[Table("SearchData")]
	public class SearchData
	{
		[Column("data")]
		public byte[] serializedData { get; set; }
	}
}