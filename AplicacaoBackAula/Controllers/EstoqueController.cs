using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.Models;

// AJUSTE 1: troque MeuDbContext pelo nome real do seu contexto
//           (pasta Models, arquivo terminado em "Context", ou AddDbContext<...> no Program.cs).
// AJUSTE 2: se o DbSet não se chamar Estoques, troque _context.Estoques pelo nome certo.
namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]   // /api/Estoque
    [ApiController]
    public class EstoqueController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public EstoqueController(BdAulaBackendContext context) => _context = context;

        // GET /api/Estoque  -> [{ "idItem": 22, "quantidadeEstoque": 10 }, ...]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueDto>>> Get()
        {
            return await _context.Estoques
                .AsNoTracking()
                .Select(e => new EstoqueDto { IdItem = e.Iditem, QuantidadeEstoque = e.QuantidadeEstoque })
                .ToListAsync();
        }

        // GET /api/Estoque/22  (o id é o idItem, que é a chave da tabela estoque)
        [HttpGet("{idItem}")]
        public async Task<ActionResult<EstoqueDto>> Get(int idItem)
        {
            var e = await _context.Estoques.AsNoTracking().FirstOrDefaultAsync(x => x.Iditem == idItem);
            if (e is null) return NotFound();
            return new EstoqueDto { IdItem = e.Iditem, QuantidadeEstoque = e.QuantidadeEstoque };
        }

        // POST /api/Estoque  -> cria a linha de estoque de um item
        [HttpPost]
        public async Task<ActionResult<EstoqueDto>> Post(EstoqueDto dto)
        {
            if (dto.QuantidadeEstoque < 0)
                return BadRequest("A quantidade não pode ser negativa.");

            if (await _context.Estoques.AnyAsync(x => x.Iditem == dto.IdItem))
                return Conflict("Este item já possui estoque cadastrado. Use PUT para alterar.");

            _context.Estoques.Add(new Estoque { Iditem = dto.IdItem, QuantidadeEstoque = dto.QuantidadeEstoque });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { idItem = dto.IdItem }, dto);
        }

        // PUT /api/Estoque/22  -> define a quantidade do item
        [HttpPut("{idItem}")]
        public async Task<IActionResult> Put(int idItem, EstoqueDto dto)
        {
            if (idItem != dto.IdItem) return BadRequest("O id da rota difere do idItem enviado.");
            if (dto.QuantidadeEstoque < 0) return BadRequest("A quantidade não pode ser negativa.");

            var e = await _context.Estoques.FirstOrDefaultAsync(x => x.Iditem == idItem);
            if (e is null) return NotFound();

            e.QuantidadeEstoque = dto.QuantidadeEstoque;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/Estoque/22
        [HttpDelete("{idItem}")]
        public async Task<IActionResult> Delete(int idItem)
        {
            var e = await _context.Estoques.FirstOrDefaultAsync(x => x.Iditem == idItem);
            if (e is null) return NotFound();

            _context.Estoques.Remove(e);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // DTO: só os dois campos que a API recebe e devolve. Evita erro 400 por propriedades
    // de navegação obrigatórias e já entrega o JSON com os nomes que o Angular espera.
    public class EstoqueDto
    {
        [JsonPropertyName("idItem")]
        public int IdItem { get; set; }

        [JsonPropertyName("quantidadeEstoque")]
        public int QuantidadeEstoque { get; set; }
    }
}