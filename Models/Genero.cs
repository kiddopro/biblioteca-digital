using System.Collections.Generic;

namespace BibliotecaDigital.Models
{
  // Definimos la estructura de datos para los generos de los libros
  public class Genero
  {
    // Identificador unico para cada genero (pk)
    public int Id { get; set; }

    // Nombre del genero
    public string Nombre { get; set; }

    // Descripcion detallada del genero
    public string Descripcion { get; set; }

    // Coleccion de libros que pertenecen a este genero
    // Esta es una propiedad de navegacion que permite acceder a todos los libros
    //  (N-N)
    public ICollection<Libro> Libros { get; set; } = new List<Libro>();
  }
}