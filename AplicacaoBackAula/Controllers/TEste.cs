using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;

namespace AplicacaoBackAula.Controllers
{
    public class TEste
    {
        [HttpGet]
        public async Task<ActionResult<AtendimentoDTO>> GetAtendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .Include(a => a.IdpessoaNavigation)
                //                .Include(a => a.Idatendimento)

                .Include(a => a.Atendimentoitems)
                    .ThenInclude(ai => ai.IditemNavigation)
                .Include(a => a.Atendimentoitems)
                    .ThenInclude(ai => ai.IdfuncionarioNavigation)
                .OrderBy(a => a.Idatendimento)
                .ToListAsync();

            return Ok (atendimentos);
        }

    }
}
