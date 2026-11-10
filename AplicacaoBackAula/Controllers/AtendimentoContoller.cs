using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AtendimentoController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public AtendimentoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // GET: api/Atendimento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtendimentoDTO>>> GetAtendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .Select(a => new AtendimentoDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems.Select(i =>
                        new AtendimentoItemDTO
                        {
                            IdAtendimento = i.Idatendimento,
                            IdItem = i.Iditem,
                            Item = i.IditemNavigation.DescricaoItem,
                            IdFuncionario = i.Idfuncionario,    
                            NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                            Cargo = i.IdfuncionarioNavigation.Cargo,
                            Quantidade = i.QuantidadeItens,
                            ValorUnitario = i.ValorunitarioEntrada
                        }).ToList()
                })
                .ToListAsync();

            return Ok(atendimentos);
        }

        // GET: api/Atendimento/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AtendimentoDTO>> GetAtendimento(int id)
        {
            var atendimento = await _context.Atendimentos
                .Where(a => a.Idatendimento == id)
                .Select(a => new AtendimentoDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    StatusAtendimento = a.StatusAtendimento,

                    Itens = a.Atendimentoitems.Select(i =>
                        new AtendimentoItemDTO
                        {
                            IdAtendimento = i.Idatendimento,
                            IdItem = i.Iditem,
                            Item = i.IditemNavigation.DescricaoItem,
                            IdFuncionario = i.Idfuncionario,
                            NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                            Cargo = i.IdfuncionarioNavigation.Cargo,
                            Quantidade = i.QuantidadeItens,
                            ValorUnitario = i.ValorunitarioEntrada
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (atendimento == null)
                return NotFound(new
                {
                    mensagem = "Atendimento não encontrado."
                });

            return Ok(atendimento);
        }

        
         // POST: api/Atendimento
        [HttpPost]
        public async Task<ActionResult<AtendimentoDTO>> Post(AtendimentoAddPutDTO dto)
        {

            //BUSCA A PESSOA E SEUS ENDEREÇOS E ATRIBUI A VARIÁVEL PESSOA
            var pessoa = await _context.Pessoas
                .FirstOrDefaultAsync(p => p.Idpessoa == dto.IdPessoa);

            //VERIFICA SE A PESSOA EXISTE
            if (pessoa == null)
            {
                return NotFound(new
                {
                    mensagem = "Pessoa não encontrada."
                });
            }

            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = dto.DataAtendimento,
                StatusAtendimento = dto.StatusAtendimento,

                Atendimentoitems = dto.Itens.Select(i =>
                    new Atendimentoitem
                    {
                        Iditem = i.IdItem,
                        Idfuncionario = i.IdFuncionario,
                        QuantidadeItens = i.Quantidade,
                        ValorunitarioEntrada = i.ValorUnitario
                    }).ToList()
            };

            _context.Atendimentos.Add(atendimento);
            await _context.SaveChangesAsync();

            dto.IdAtendimento = atendimento.Idatendimento;

            foreach (var item in dto.Itens)
                item.IdAtendimento = atendimento.Idatendimento;

            return CreatedAtAction(
                nameof(GetAtendimento),
                new { id = atendimento.Idatendimento },
                dto);
        }

        // PUT: api/Atendimento/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, AtendimentoAddPutDTO dto)
        {
            
            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            if (atendimento == null)
                return NotFound(new
                {
                    mensagem = "Atendimento não encontrado."
                });

            atendimento.Idpessoa = dto.IdPessoa;
            atendimento.DataAtendimento = dto.DataAtendimento;
            atendimento.StatusAtendimento = dto.StatusAtendimento;

            // Identifica os itens enviados pelo cliente.
            var idsRecebidos = dto.Itens
                .Select(i => (i.IdItem, i.IdFuncionario))
                .ToHashSet();

            // Remove os itens que foram retirados do atendimento.
            var itensRemover = atendimento.Atendimentoitems
                .Where(i => !idsRecebidos.Contains(
                    (i.Iditem, i.Idfuncionario)))
                .ToList();

            _context.Atendimentoitems.RemoveRange(itensRemover);

            // Atualiza os itens existentes ou adiciona os novos.
            foreach (var itemDto in dto.Itens)
            {
                var existente = atendimento.Atendimentoitems
                    .FirstOrDefault(i =>
                        i.Iditem == itemDto.IdItem &&
                        i.Idfuncionario == itemDto.IdFuncionario);

                if (existente != null)
                {
                    existente.QuantidadeItens = itemDto.Quantidade;
                    existente.ValorunitarioEntrada = itemDto.ValorUnitario;
                }
                else
                {
                    atendimento.Atendimentoitems.Add(new Atendimentoitem
                    {
                        Idatendimento = atendimento.Idatendimento,
                        Iditem = itemDto.IdItem,
                        Idfuncionario = itemDto.IdFuncionario,
                        QuantidadeItens = itemDto.Quantidade,
                        ValorunitarioEntrada = itemDto.ValorUnitario
                    });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Atendimento/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var atendimento = await _context.Atendimentos
                .Include(a => a.Atendimentoitems)
                .FirstOrDefaultAsync(a => a.Idatendimento == id);

            if (atendimento == null)
                return NotFound(new
                {
                    mensagem = "Atendimento não encontrado."
                });

            _context.Set<Atendimentoitem>()
                .RemoveRange(atendimento.Atendimentoitems);

            _context.Atendimentos.Remove(atendimento);

            await _context.SaveChangesAsync();

            return NoContent();
        }
     
       
    }
}
