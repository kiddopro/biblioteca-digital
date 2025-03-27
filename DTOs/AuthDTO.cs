namespace BibliotecaDigital.DTOs
{
    // DTO para registrar un usuario
    public class RegisterDTO
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO para iniciar sesion
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO para la respuesta en caso de que sea exitoso se envia el token
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
    }
}