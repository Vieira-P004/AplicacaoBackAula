using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    //ANOTAÇÕES DE IDENTIFICAÇÃO DE CONTROLLER
    [ApiController]
    [Route("api/[controller]")]
    public class PessoaEnderecoController : ControllerBase
    {
        //DECLARAÇÃO DO ATRIBUTO DE CONTEXTO
        private readonly BdAulaBackendContext _context;

        //DECLARAÇÃO DO MÉTODO CONSTRUTOR
        public PessoaEnderecoController(BdAulaBackendContext context)
        {
            //INSTANCIA DO ATRIBUTO CONSTRUTOR
            _context = context;
        }

        //DECLARAÇÃO DO MÉTODO PARA LISTAR TODAS AS PESSOAS E SEUS ENDEREÇOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaDTO>>> GetAll()
        {
            Pessoa p = new Pessoa();
            

            var pessoas = await _context.Pessoas
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    sexo = p.Sexo,
                    email = p.Email,
                    telefone = (long)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                       IdEndereco = e.Idendereco,
                       IdPessoa = e.Idpessoa,
                       CEP = e.Cep,
                       Numero = e.Numero,
                       Complemento = e.Complemento,

                    }).ToList()

                }).ToListAsync();

            return Ok(pessoas);

        }//FIM MÉTODO GetAll

        //DECLARAÇÃO DO MÉTODO PARA LISTAR UMA A PESSOA E SEUS ENDEREÇOS
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaDTO>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoas
                .Where(p => p.Idpessoa == id)
               .Select(p => new PessoaDTO
               {
                   
                   IdPessoa = p.Idpessoa,
                   Nome = p.Nome,
                   CPF = (long)p.Cpf,
                   DataNascimento = p.DataNascimento,
                   sexo = p.Sexo,
                   email = p.Email,
                   telefone = (long)p.Telefone,
                   AtivoPessoa = p.AtivoPessoa,

                   Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                   {
                       IdEndereco = e.Idendereco,
                       IdPessoa = e.Idpessoa,
                       CEP = e.Cep,
                       Numero = e.Numero,
                       Complemento = e.Complemento,

                   }).ToList()

               }).ToListAsync();

            return Ok(pessoa);

        }//FIM MÉTODO GetPEssoa

        //MÉTODO PARA CADASTRAR UMA PESSOA
        [HttpPost]
        public async Task<ActionResult<PessoaDTO>> Post(PessoaDTO dto)
        {
            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Cpf = dto.CPF,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.sexo,
                Email = dto.email,
                Telefone = dto.telefone,
                AtivoPessoa = dto.AtivoPessoa,

                Enderecos = dto.Enderecos.Select(e => new Endereco
                {
                    Cep = e.CEP,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()
            };

            _context.Pessoas.Add(pessoa);   

            await _context.SaveChangesAsync();

            var resposta = new PessoaDTO
            {
                IdPessoa = pessoa.Idpessoa,
                Nome = pessoa.Nome,
                CPF = pessoa.Cpf,
                DataNascimento = pessoa.DataNascimento,
                sexo = pessoa.Sexo,
                email = pessoa.Email,
                telefone = pessoa.Telefone,
                AtivoPessoa = pessoa.AtivoPessoa,

                Enderecos = pessoa.Enderecos.Select(e => new EnderecoDTO
                {
                    CEP = e.Cep,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList()

            };

            return CreatedAtAction(
                nameof(GetPessoa),
                new {id = pessoa.Idpessoa},
                resposta);

        }//FIM MÉTODO Post


        //MÉTODO PARA ALTERAR UMA PESSOA
        [HttpPut("{id}")]
        public async Task<ActionResult<ActionResult<Pessoa>>> Put(int id, PessoaDTO dto)
        {
            //BUSCA A PESSOA E SEUS ENDEREÇOS E ATRIBUI A VARIÁVEL PESSOA
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            //VERIFICA SE A PESSOA EXISTE
            if (pessoa == null)
            {
                return NotFound();
            }

            //PROJETA OS DADOS DO PessoaDTO PARA O MODELO Pessoa ORIUNDA DO BANCO DE DADOS
            pessoa.Nome = dto.Nome;
            pessoa.Cpf = dto.CPF;
            pessoa.DataNascimento = dto.DataNascimento;
            pessoa.Sexo = dto.sexo;
            pessoa.Email = dto.email;
            pessoa.Telefone = dto.telefone;
            pessoa.AtivoPessoa = dto.AtivoPessoa;

            // Guarda os IDs dos endereços recebidos
            var idsRecebidos = dto.Enderecos
                .Where(e => e.IdEndereco.HasValue)
                .Select(e => e.IdEndereco!.Value)
                .ToHashSet();

            // Identifica os endereços existentes que devem ser removidos
            var enderecosRemover = pessoa.Enderecos
                .Where(e => !idsRecebidos.Contains(e.Idendereco))
                .ToList();

            // Remove apenas os endereços que já existiam no banco
            _context.Enderecos.RemoveRange(enderecosRemover);

            //PERCORRE OS ENDEREÇOS DA PESSOA
            foreach (var endPessoa in dto.Enderecos)
            {
                //VERIFICA SE O ID EXISTE COM A PESSOA
                if (endPessoa.IdEndereco != null)
                {
                    //BUSCA O ENDERECO DA PESSOA, NO ATRIBUTO DO MODELO Pessoa.
                    var endereco = pessoa.Enderecos
                        .FirstOrDefault(e => e.Idendereco == endPessoa.IdEndereco);

                    //CASO NÃO ENCONTRE EMITE UMA MENSAGEM DE QUE O ENDEREÇO NÃO PERTECE A ESSA PESSOA.
                    if (endereco == null)
                    {
                        return BadRequest($"O endereço não pertece a essa pessoa");
                    }

                    //AO LOCALIZAR PROJETA NO MODELO Endereco OS DADOS VINDO DO ATRIBUTO Endereços
                    //DO MODELO O PESSOA QUE VENHO DO PessoaDto
                    endereco.Numero = endPessoa.Numero;
                    endereco.Cep = endPessoa.CEP;
                    endereco.Complemento = endPessoa.Complemento;
                }
                else
                {
                    //CASO O ID NÃO SEJA MAIOR QUE 0(ZERO) É UM NOVO ENDEREÇO A SER ADICIONADO A PESSOA
                    pessoa.Enderecos.Add(new Endereco
                    {
                        Numero = endPessoa.Numero,
                        Cep = endPessoa.CEP,
                        Complemento = endPessoa.Complemento

                    });
                }
            }
            
            //SALVA NO BANCO DE DAOS
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //MÉTODO PARA EXCLUIR UMA PESSOA
        [HttpDelete("{id}")]
        public async Task<ActionResult<Pessoa>> Delete(int id)
        {
            //BUSCA A PESSOA E SEUS ENDEREÇOS E ATRIBUI A VARIÁVEL PESSOA
            var pessoa = await _context.Pessoas
                .Include(p => p.Enderecos)
                .FirstOrDefaultAsync(p => p.Idpessoa == id);

            //VERIFICA SE A PESSOA EXISTE
            if (pessoa == null)
            {
                return NotFound();
            }

            //PREPARA A INSTRUÇÃO PARA EXECUTAR NO BANCO DE DADOS
            _context.Pessoas.Remove(pessoa);

            //SALVA NO BANCO DE DAOS
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }//FIM DA CLASSE CONTROLLER PessoaEnderecoContoller
}//FIM DO NAMESPACE
