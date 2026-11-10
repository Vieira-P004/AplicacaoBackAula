namespace AplicacaoBackAula.DTO
{
    public class AtendimentoAddPutDTO
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }
        public List<AtendimentoItemAddPutDTO> Itens { get; set; } = new();
    }
}
