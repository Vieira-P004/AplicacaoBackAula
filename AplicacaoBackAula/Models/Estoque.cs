using System;
using System.Collections.Generic;

namespace AplicacaoBackAula.Models;

public partial class Estoque
{
    public int Iditem { get; set; }

    public int QuantidadeEstoque { get; set; }

    public virtual ICollection<EntradaProduto> EntradaProdutos { get; set; } = new List<EntradaProduto>();

    public virtual Produto IditemNavigation { get; set; } = null!;
}
