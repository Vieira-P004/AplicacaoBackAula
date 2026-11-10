namespace AplicacaoBackAula.DTO
{
    public class AtendimentoItemDTO
    {
        public int IdAtendimento { get; set; }
        public int IdItem { get; set; }
        public string Item { get; set; }
        public int IdFuncionario { get; set; }
        public string NomeFuncionario { get; set; }
        public string Cargo { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

    }
}
