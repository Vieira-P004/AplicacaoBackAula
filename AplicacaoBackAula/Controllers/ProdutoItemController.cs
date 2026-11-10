using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    //DEFINIDO ANOTAÇÕES PARA EF IDENTIFICAR QUE É UMA CONTROLLER
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoItemController : ControllerBase
    {
        //DEFININDO UM ATRIBUTO GLOBAL DO TIPO BdAulaBackendContext
        private readonly BdAulaBackendContext _context;

        //CRIADO O MÉTODO CONSTRUTOR DA CLASSE
        public ProdutoItemController(BdAulaBackendContext context)
        {
            //INSTANCIANDO O ATRIBUTO _context
            _context = context;
        }

        //LISTAR TODOS OS PRODUTOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutos()
        {
            var produtos = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .ToListAsync();

            return Ok(produtos);
        }

        //LISTAR APENAS UM PRODUTO
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDTO>> GetProduto(int id)
        {
            var produto = await _context.Produtos 
                .Include(p => p.IditemNavigation)
                .FirstOrDefaultAsync(p => p.Iditem == id);

            if(produto == null)
            {
                return NotFound(
                    new { 
                        mensagem = "PRODUTO NÃO LOCALIZADO!"
                    });
            }

            return Ok(produto);
        }

        //INSERIR DADOS NAS TABELAS ITEM E PRODUTO
        [HttpPost]
        public async Task<ActionResult> PostProduto(ProdutoDTO produtoDTO)
        {
            //ABRINDO UMA TRANSAÇÃO NO BANCO DE DADOS
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //DADOS ITEM
                var item = new Item
                {
                    Idcategoria = produtoDTO.IdCategoria,
                    Item1 = produtoDTO.Item,
                    DescricaoItem = produtoDTO.DescricaoItem,
                    Preco = (decimal) produtoDTO.Preco,
                    TipoItem = produtoDTO.TipoItem,
                    AtivoProdutoServico = produtoDTO.AtivoProdutoServico
                };
                
                //PREPARA OS DADOS PARA O BANCO
                _context.Items.Add(item);

                //SALVA OS DADOS NO BANCO
                await _context.SaveChangesAsync();

                //PEGAR O ID GERADO
                int idItemGerado = item.Iditem;

                //DADOS PRODUTO
                var produto = new Produto
                {
                    Iditem = idItemGerado,
                    CodigoPreco = produtoDTO.CodigoPreco,
                    PrecoVenda = produtoDTO.PrecoVenda,
                    Unidade = produtoDTO.Unidade
                };
                
                //PREPARA OS DADOS PARA O BANCO
                _context.Produtos.Add(produto);

                //SALVA OS DADOS NO BANCO
                await _context.SaveChangesAsync();

                //CONFIRME A TRANSAÇÃO
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetProduto),
                    new { id = produto.Iditem },
                    produto
                );
            }
            catch (Exception ex)
            {
                //CASO HAJA ALGUM ERRO EXECUTA O ROOLLBACK NAS TABELAS
                await transaction.RollbackAsync();

               return StatusCode(500, new
                {
                    mensagem = "ERRO NO CADASTRO",
                    erro = ex.Message
                });

            }
           
        }

        //ALTERAR DADO NAS TABELAS ITEM E PRODUTO
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, ProdutoDTO dados)
        {
            //ABRINDO UMA TRANSAÇÃO NO BANCO DE DADOS
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                // Busca o Produto junto com o Item
                var produtoExistente = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produtoExistente == null)
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                //DADOS DA TABELA PRODUTO
                produtoExistente.CodigoPreco = dados.CodigoPreco;
                produtoExistente.PrecoVenda = dados.PrecoVenda;
                produtoExistente.Unidade = dados.Unidade;

                //DADOS DA TABELA ITEM

                produtoExistente.IditemNavigation.Idcategoria = dados.IdCategoria;
                produtoExistente.IditemNavigation.Item1 = dados.Item;
                produtoExistente.IditemNavigation.DescricaoItem = dados.DescricaoItem;
                produtoExistente.IditemNavigation.Preco = (decimal)dados.Preco;
                produtoExistente.IditemNavigation.TipoItem = dados.TipoItem;
                produtoExistente.IditemNavigation.AtivoProdutoServico = dados.AtivoProdutoServico;

                // SALVA AS DUAS ALTERAÇÕES          
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                //CASO HAJA ALGUM ERRO EXECUTA O ROOLLBACK NAS TABELAS
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "ERRO NO ALTERAR",
                    erro = ex.Message
                });
            }

            return NoContent();
        }

        //EXCLUIR DADO NAS TABELAS ESTOQUE, PRODUTO E ITEM
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var produto = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produto == null)
                {
                    return NotFound(new
                    {
                        mensagem = "Produto não encontrado."
                    });
                }

                // Busca os registros de estoque do produto
                var estoques = await _context.Estoques
                    .Where(e => e.Iditem == id)
                    .ToListAsync();

                // Remove os registros de estoque primeiro
                if (estoques.Any())
                {
                    _context.Estoques.RemoveRange(estoques);
                    await _context.SaveChangesAsync();
                }

                // Remove o produto
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                // Busca e remove o item correspondente
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Iditem == id);

                if (item != null)
                {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensagem = "Erro ao excluir produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }




    }//FIM CLASSE

}//FIM NAMESPACE
