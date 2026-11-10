using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.Models;

// AJUSTE 1: troque MeuDbContext pelo nome real do seu contexto.
// AJUSTE 2: confira os nomes dos DbSet (Items, Estoques, EntradaProdutos) no arquivo do contexto.
// AJUSTE 3: confira as propriedades do modelo EntradaProduto (veja a lista no fim deste comentário).
//   Usadas aqui: IdentradaProduto, Iditem, DataEntradaProduto (DateOnly), QuantidadeEntrada (int),
//   Unidade (string), ValorUnitarioEntrada (decimal). Se DataEntradaProduto for DateTime, troque
//   DateOnly por DateTime no DTO e no controller.
namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]   // /api/EntradaProduto
    [ApiController]
    public class EntradaProdutoController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public EntradaProdutoController(BdAulaBackendContext context) => _context = context;

        // GET /api/EntradaProduto  -> histórico de entradas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntradaProdutoDto>>> Get()
        {
            return await _context.EntradaProdutos
                .AsNoTracking()
                .OrderByDescending(e => e.IdentradaProduto)
                .Select(e => new EntradaProdutoDto
                {
                    IdEntradaProduto = e.IdentradaProduto,
                    IdItem = e.Iditem,
                    DataEntrada = e.DataEntradaProduto,
                    QuantidadeEntrada = e.QuantidadeEntrada,
                    Unidade = e.Unidade,
                    ValorUnitarioEntrada = e.ValorUnitarioEntrada,
                })
                .ToListAsync();
        }

        // GET /api/EntradaProduto/101
        [HttpGet("{id}")]
        public async Task<ActionResult<EntradaProdutoDto>> Get(int id)
        {
            var e = await _context.EntradaProdutos.AsNoTracking().FirstOrDefaultAsync(x => x.IdentradaProduto == id);
            if (e is null) return NotFound();
            return Mapear(e);
        }

        // POST /api/EntradaProduto  -> registra a entrada E soma a quantidade no estoque (tudo ou nada)
        [HttpPost]
        public async Task<ActionResult<EntradaProdutoDto>> Post(EntradaProdutoDto dto)
        {
            // validações de regra de negócio
            if (dto.QuantidadeEntrada < 1) return BadRequest("A quantidade mínima é 1.");
            if (dto.ValorUnitarioEntrada <= 0) return BadRequest("Informe um valor maior que zero.");
            if (string.IsNullOrWhiteSpace(dto.Unidade) || dto.Unidade.Trim().Length > 10)
                return BadRequest("Informe a unidade (máximo de 10 caracteres).");
            if (dto.DataEntrada > DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("A data não pode ser futura.");

            var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Iditem == dto.IdItem);
            if (item is null) return NotFound("Produto não localizado.");
            if (item.TipoItem != "P") return BadRequest("Serviço não possui estoque. Selecione um produto.");
            if (item.AtivoProdutoServico != "S") return BadRequest("O produto está inativo.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            var entrada = new EntradaProduto
            {
                Iditem = dto.IdItem,
                DataEntradaProduto = dto.DataEntrada,
                QuantidadeEntrada = dto.QuantidadeEntrada,
                Unidade = dto.Unidade.Trim(),
                ValorUnitarioEntrada = dto.ValorUnitarioEntrada,
            };
            _context.EntradaProdutos.Add(entrada);

            var estoque = await _context.Estoques.FirstOrDefaultAsync(x => x.Iditem == dto.IdItem);
            if (estoque is null)
                _context.Estoques.Add(new Estoque { Iditem = dto.IdItem, QuantidadeEstoque = dto.QuantidadeEntrada });
            else
                estoque.QuantidadeEstoque += dto.QuantidadeEntrada;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(nameof(Get), new { id = entrada.IdentradaProduto }, Mapear(entrada));
        }

        // DELETE /api/EntradaProduto/101  -> remove a entrada e devolve a quantidade do estoque
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entrada = await _context.EntradaProdutos.FirstOrDefaultAsync(x => x.IdentradaProduto == id);
            if (entrada is null) return NotFound();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var estoque = await _context.Estoques.FirstOrDefaultAsync(x => x.Iditem == entrada.Iditem);
            if (estoque is not null)
            {
                if (estoque.QuantidadeEstoque < entrada.QuantidadeEntrada)
                    return Conflict("Não é possível excluir: parte desta entrada já foi vendida.");
                estoque.QuantidadeEstoque -= entrada.QuantidadeEntrada;
            }

            _context.EntradaProdutos.Remove(entrada);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return NoContent();
        }

        private static EntradaProdutoDto Mapear(EntradaProduto e) => new()
        {
            IdEntradaProduto = e.IdentradaProduto,
            IdItem = e.Iditem,
            DataEntrada = e.DataEntradaProduto,
            QuantidadeEntrada = e.QuantidadeEntrada,
            Unidade = e.Unidade,
            ValorUnitarioEntrada = e.ValorUnitarioEntrada,
        };
    }

    // JSON com os nomes que o Angular envia e espera.
    public class EntradaProdutoDto
    {
        [JsonPropertyName("idEntradaProduto")]
        public int IdEntradaProduto { get; set; }

        [JsonPropertyName("idItem")]
        public int IdItem { get; set; }

        [JsonPropertyName("dataEntrada")]
        public DateOnly DataEntrada { get; set; }

        [JsonPropertyName("quantidadeEntrada")]
        public int QuantidadeEntrada { get; set; }

        [JsonPropertyName("unidade")]
        public string Unidade { get; set; } = "";

        [JsonPropertyName("valorUnitarioEntrada")]
        public decimal ValorUnitarioEntrada { get; set; }
    }
}