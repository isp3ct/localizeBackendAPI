namespace localizeBackendAPI.DTOs
{    public class UpdateProfileRequest
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaAtual { get; set; }
        public string SenhaNova { get; set; }
        public string SenhaConfirmacao { get; set; }
    }
}
