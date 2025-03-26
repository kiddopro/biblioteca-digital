namespace BibliotecaDigital.Models
{

    public enum EstadoUsuario
    {
        Activo,
        Inactivo
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public EstadoUsuario Estado { get; set; }
    }
}