namespace BibliotecaDigital.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public int AnioPublicacion { get; set; }
        public string Imagen { get; set; }
        public string ISBN { get; set; }
        public int Stock { get; set; }

        public int AutorId { get; set; }
        public Autor Autor { get; set; }

        public int GeneroId { get; set; }
        public Genero Genero { get; set; }
    }
}