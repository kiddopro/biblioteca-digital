namespace BibliotecaDigital.Models
{
    // Definimos la estructura de datos para los libros
    public class Libro
    {
        // Identificador unico para cada libro (primary key)
        public int Id { get; set; }

        // Título del libro
        public string Titulo { get; set; }

        // Resumen o sinopsis del contenido del libro
        public string Resumen { get; set; }

        // Anio en que se publico el libro
        public int AnioPublicacion { get; set; }

        // URL de la imagen de portada del libro
        public string Imagen { get; set; }

        // codigo ISBN unico que identifica internacionalmente al libro
        public string ISBN { get; set; }

        // Disponibilidad en stock
        public int Stock { get; set; }

        // Identificador del autor del libro (fk)
        // Esto conecta / vincula cada libro con su autor en la base de datos
        public int AutorId { get; set; }

        // Propiedad de navegacion que permite acceder a los datos del autor
        // desde un objeto libro (relacion "muchos a uno")
        public Autor Autor { get; set; }

        // Identificador del genero del libro (fk)
        // Esto conecta cada libro con su genero en la base de datos
        public int GeneroId { get; set; }

        // Propiedad de navegacion que permite acceder a los datos del genero
        // desde un objeto libro (N-N)
        public Genero Genero { get; set; }
    }
}