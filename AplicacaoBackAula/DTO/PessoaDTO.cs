namespace AplicacaoBackAula.DTO
{
    public class PessoaDTO
    {
        public int IdPessoa { get; set; }
        public string? Nome { get; set;}
        public long CPF { get; set; }
        public DateOnly DataNascimento { get; set; }
        public string sexo { get; set; }
        public string email { get; set; }
        public long telefone { get; set; }
        public string AtivoPessoa { get; set; }

        public List<EnderecoDTO> Enderecos { get; set; } = new();
        

    }

    
}
