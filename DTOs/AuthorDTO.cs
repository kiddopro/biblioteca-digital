namespace BibliotecaDigital.DTOs
{
    // DTO para mostrar un autor
    public class AutorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }

    // DTO para mostrar un autor con sus libros
    public class AutorConLibrosDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public int CantidadLibros { get; set; }
    }
}