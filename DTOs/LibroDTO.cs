namespace BibliotecaDigital.DTOs
{
    // DTO para mostrar un libro
    public class LibroDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public int AnioPublicacion { get; set; }
        public string Imagen { get; set; }
        public string ISBN { get; set; }
        public int Stock { get; set; }
        public int AutorId { get; set; }
        public int GeneroId { get; set; }
    }

    // DTO para mostrar resumen de un libro
    public class LibroResumenDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int AnioPublicacion { get; set; }
        public string Imagen { get; set; }
        public bool Disponible { get; set; }
    }

    // DTO para mostrar detalle de un libro
    public class LibroDetalleDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public int AnioPublicacion { get; set; }
        public string Imagen { get; set; }
        public string ISBN { get; set; }
        public int Stock { get; set; }
        public AutorDTO Autor { get; set; }
        public GeneroDTO Genero { get; set; }
    }
}