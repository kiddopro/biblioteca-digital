namespace BibliotecaDigital.Models
{
    // Este enum define los posibles estados de un usuario
    // en este caso creo que se utilizara para activar o desactivar usuarios sin eliminarlos
    public enum EstadoUsuario
    {
        // Usuario activo que puede iniciar sesion y usar el sistema
        Activo,

        // Usuario inactivo que no puede iniciar sesion (se supone)
        Inactivo
    }

    // Definimos la estructura de datos para los usuarios del sistema
    public class Usuario
    {
        // Identificador unico para cada usuario (clave primaria en la base de datos)
        public int Id { get; set; }

        // Nombre completo del usuario
        public string Nombre { get; set; }

        // Correo electronico del usuario (debe ser unico)porque se usa como identificador para iniciar sesion
        public string Email { get; set; }

        // Contrasenia del usuario
        public string Password { get; set; }

        // Estado actual del usuario (activo o inactivo)
        public EstadoUsuario Estado { get; set; }
    }
}