namespace BibliotecaDigital.DTOs
{
    public class RegisterDTO
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
    }
}