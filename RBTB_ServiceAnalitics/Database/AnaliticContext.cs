using Microsoft.EntityFrameworkCore;

using RBTB_ServiceAnalitics.Database.Entities;

namespace RBTB_ServiceAnalitics.Database
{
	public class AnaliticContext : DbContext
	{
		public DbSet<Tick> Ticks { get; set; }
		public DbSet<Level> Levels { get; set; }
		public AnaliticContext( DbContextOptions<AnaliticContext> options ) : base( options )
		{
		}

		protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
		{
		}
    }
}
