namespace AplicacaoBackAula.DTO
{
    public class AtendimentoItemAddPutDTO
    {
        public int IdAtendimento { get; set; }
        public int IdItem { get; set; }
        public int IdFuncionario { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
    }
}
