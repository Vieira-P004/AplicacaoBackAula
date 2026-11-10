namespace AplicacaoBackAula.DTO
{
    public class EnderecoDTO
    {
        public int? IdEndereco { get; set; }
        public int IdPessoa { get; set; }
        public string? CEP { get; set; }
        public string? Numero { get; set;}
        public string? Complemento { get; set; }
    }
}
