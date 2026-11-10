using System;
using System.Collections.Generic;

namespace AplicacaoBackAula.Models;

public partial class EntradaProduto
{
    public int IdentradaProduto { get; set; }

    public int Iditem { get; set; }

    public DateOnly DataEntradaProduto { get; set; }

    public int QuantidadeEntrada { get; set; }

    public string Unidade { get; set; } = null!;

    public decimal ValorUnitarioEntrada { get; set; }

    public virtual Estoque IditemNavigation { get; set; } = null!;
}
