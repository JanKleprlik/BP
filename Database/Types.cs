﻿using System;
using System.Collections.Generic;

namespace Database
{
	public class Song
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Author { get; set; }
	}

	public class SearchData
	{
		public byte[] serializedData { get; set; }
	}
}
