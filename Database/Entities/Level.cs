﻿using System.ComponentModel.DataAnnotations;

namespace RBTB_ServiceAnalitics.Database.Entities
{
	public class Level
	{
		public int Id { get; set; }
		public string Symbol { get; set; }
		public decimal Price { get; set; }
        [ConcurrencyCheck]
        public decimal Volume { get; set; }
		public DateTime DateCreate { get; set; }
	}
}
