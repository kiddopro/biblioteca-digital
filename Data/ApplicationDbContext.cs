using BibliotecaDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaDigital.Data
{
    // Esta clase hace como puente entre los modelos de la aplicacion y la base de datos
    // Realiza las tareas CRUD sin tener que escribir sql
    public class ApplicationDbContext : DbContext
    {
        // Constructor recibe las opciones de configuracion
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Estas propiedades representan tablas en la base de datos
        // Cada dbset hace referencia a una tabla que almacena objetos del tipo especificado

        // Tabla de libros
        public DbSet<Libro> Libros { get; set; }

        // Tabla de autores
        public DbSet<Autor> Autores { get; set; }

        // Tabla de generos
        public DbSet<Genero> Generos { get; set; }

        // Tabla de usuarios
        public DbSet<Usuario> Usuarios { get; set; }

        // Este metodo configura las relaciones entre entidades y otras restricciones
        // Se ejecuta al momento de quee Entity Framework crea el modelo de la base de datos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuramos el ISBN como un indice unico
            modelBuilder.Entity<Libro>()
                .HasIndex(l => l.ISBN)
                .IsUnique();

            // Configuramos el Email como unico (para que se loguee el usuario y no comparta mail con otro)
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configuramos la relacion entre libro y autor
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Autor)
                .WithMany(a => a.Libros)
                .HasForeignKey(l => l.AutorId);

            // Configuramos la relacion entre Libro y Genero
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Genero)
                .WithMany(g => g.Libros)
                .HasForeignKey(l => l.GeneroId);
        }
    }
}