namespace proyecto.Dtos
{
    public class AuthServiceResponseDto
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public string RefreshToken { get; set; }  // Nuevo campo

    }
}
