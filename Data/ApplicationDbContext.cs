using BibliotecaDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaDigital.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Libro> Libros { get; set; }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Libro
            modelBuilder.Entity<Libro>()
                .HasIndex(l => l.ISBN)
                .IsUnique();

            // Configure Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            // Configure relationships
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Autor)
                .WithMany(a => a.Libros)
                .HasForeignKey(l => l.AutorId);
                
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Genero)
                .WithMany(g => g.Libros)
                .HasForeignKey(l => l.GeneroId);
        }
    }
}