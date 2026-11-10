using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.Models;

// AJUSTE 1: troque MeuDbContext pelo nome real do seu contexto.
// AJUSTE 2: confira os DbSet no arquivo do contexto (aqui: Items e Categoria).
//           O modelo de categoria aparece no Swagger como "Categorium"; o DbSet costuma ser Categoria.
namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]   // /api/Item
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public ItemController(BdAulaBackendContext context) => _context = context;

        // GET /api/Item  (opcional: ?tipo=P ou ?tipo=S, ?idCategoria=3, ?ativo=S)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> Get(string? tipo, int? idCategoria, string? ativo)
        {
            var consulta = _context.Items.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(i => i.TipoItem == tipo);
            if (idCategoria.HasValue)
                consulta = consulta.Where(i => i.Idcategoria == idCategoria.Value);
            if (!string.IsNullOrWhiteSpace(ativo))
                consulta = consulta.Where(i => i.AtivoProdutoServico == ativo);

            return await consulta
                .OrderBy(i => i.Iditem)
                .Select(i => Mapear(i))
                .ToListAsync();
        }

        // RP02: GET /api/Item/22  -> consulta um produto ou serviço pelo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> Get(int id)
        {
            var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Iditem == id);
            if (item is null) return NotFound("Produto ou serviço não localizado.");
            return Mapear(item);
        }

        // POST /api/Item
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(ItemDto dto)
        {
            var erro = await Validar(dto);
            if (erro is not null) return BadRequest(erro);

            var item = new Item
            {
                Idcategoria = dto.IdCategoria,
                Item1 = dto.Item.Trim(),
                DescricaoItem = dto.DescricaoItem?.Trim() ?? "",
                Preco = dto.Preco,
                TipoItem = dto.TipoItem,
                AtivoProdutoServico = string.IsNullOrWhiteSpace(dto.AtivoProdutoServico) ? "S" : dto.AtivoProdutoServico,
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = item.Iditem }, Mapear(item));
        }

        // PUT /api/Item/22  (também serve para desativar: enviar ativoProdutoServico = "N")
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ItemDto dto)
        {
            // as telas Produtos/Serviços não enviam idItem no corpo (chega 0); só recusa se vier diferente
            if (dto.IdItem != 0 && id != dto.IdItem) return BadRequest("O id da rota difere do idItem enviado.");

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Iditem == id);
            if (item is null) return NotFound("Produto ou serviço não localizado.");

            var erro = await Validar(dto);
            if (erro is not null) return BadRequest(erro);

            item.Idcategoria = dto.IdCategoria;
            item.Item1 = dto.Item.Trim();
            item.DescricaoItem = dto.DescricaoItem?.Trim() ?? "";
            item.Preco = dto.Preco;
            item.TipoItem = dto.TipoItem;
            if (!string.IsNullOrWhiteSpace(dto.AtivoProdutoServico))
                item.AtivoProdutoServico = dto.AtivoProdutoServico;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/Item/22  (os requisitos pedem DESATIVAR; veja a etapa RP10/RP11)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Iditem == id);
            if (item is null) return NotFound("Produto ou serviço não localizado.");

            try
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Este item já está vinculado a pedidos ou estoque e não pode ser excluído. Desative-o.");
            }
            return NoContent();
        }

        private async Task<string?> Validar(ItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Item)) return "Informe o nome.";
            if (dto.Preco <= 0) return "Informe um preço maior que zero.";
            if (dto.TipoItem != "P" && dto.TipoItem != "S") return "O tipo deve ser P (produto) ou S (serviço).";
            if (!string.IsNullOrWhiteSpace(dto.AtivoProdutoServico)
                && dto.AtivoProdutoServico != "S" && dto.AtivoProdutoServico != "N")
                return "A situação deve ser S (ativo) ou N (inativo).";
            if (!await _context.Categoria.AnyAsync(c => c.Idcategoria == dto.IdCategoria))
                return "Categoria não localizada.";
            return null;
        }

        private static ItemDto Mapear(Item i) => new()
        {
            IdItem = i.Iditem,
            IdCategoria = i.Idcategoria,
            Item = i.Item1,
            DescricaoItem = i.DescricaoItem,
            Preco = i.Preco,
            TipoItem = i.TipoItem,
            AtivoProdutoServico = i.AtivoProdutoServico,
        };
    }

    // JSON com os nomes que o Angular espera (item.model.ts) e sem propriedades de navegação.
    public class ItemDto
    {
        [JsonPropertyName("idItem")]
        public int IdItem { get; set; }

        [JsonPropertyName("idCategoria")]
        public int IdCategoria { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; } = "";

        [JsonPropertyName("descricaoItem")]
        public string? DescricaoItem { get; set; }

        [JsonPropertyName("preco")]
        public decimal Preco { get; set; }

        [JsonPropertyName("tipoItem")]
        public string TipoItem { get; set; } = "P";

        [JsonPropertyName("ativoProdutoServico")]
        public string AtivoProdutoServico { get; set; } = "S";
    }
}