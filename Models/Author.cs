using System;
using System.Collections.Generic;

namespace BibliotecaDigital.Models
{
  // Esta clase define la estructura de datos para los autores de los libros
  public class Autor
  {
    // Identificador onico para cada autor (pk)
    public int Id { get; set; }

    // Nombre completo del autor
    public string Nombre { get; set; }

    // Pais de origen del autor
    public string Nacionalidad { get; set; }

    // Fecha de nacimiento del autor
    public DateTime FechaNacimiento { get; set; }

    // Coleccion de libros escritos por este autor
    // Esta es una propiedad de navegacion que permite acceder a todos los libros
    // de un autor desde un objeto autor (N-N)
    public ICollection<Libro> Libros { get; set; } = new List<Libro>();
  }
}