namespace AplicacaoBackAula.DTO
{
    public class AtendimentoDTO
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public string NomePessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }
        public List<AtendimentoItemDTO> Itens { get; set; } = new();


    }
}
