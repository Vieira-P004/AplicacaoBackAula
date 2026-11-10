using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace AplicacaoBackAula.Models;

public partial class BdAulaBackendContext : DbContext
{
    public BdAulaBackendContext()
    {
    }

    public BdAulaBackendContext(DbContextOptions<BdAulaBackendContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atendimento> Atendimentos { get; set; }

    public virtual DbSet<Atendimentoitem> Atendimentoitems { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Endereco> Enderecos { get; set; }

    public virtual DbSet<EntradaProduto> EntradaProdutos { get; set; }

    public virtual DbSet<Estoque> Estoques { get; set; }

    public virtual DbSet<Funcionario> Funcionarios { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Pessoa> Pessoas { get; set; }

    public virtual DbSet<Produto> Produtos { get; set; }

   /* 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=bd_aula_backend;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));
   */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Atendimento>(entity =>
        {
            entity.HasKey(e => e.Idatendimento).HasName("PRIMARY");

            entity.ToTable("atendimento");

            entity.HasIndex(e => e.Idpessoa, "fk_atendimento_pessoa1_idx");

            entity.Property(e => e.Idatendimento).HasColumnName("idatendimento");
            entity.Property(e => e.DataAtendimento).HasColumnName("data_atendimento");
            entity.Property(e => e.Idpessoa).HasColumnName("idpessoa");
            entity.Property(e => e.StatusAtendimento)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("status_atendimento");

            entity.HasOne(d => d.IdpessoaNavigation).WithMany(p => p.Atendimentos)
                .HasForeignKey(d => d.Idpessoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_atendimento_pessoa1");
        });

        modelBuilder.Entity<Atendimentoitem>(entity =>
        {
            entity.HasKey(e => new { e.Idatendimento, e.Iditem })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("atendimentoitem");

            entity.HasIndex(e => e.Idfuncionario, "fk_atendimentoitem_funcionario1_idx");

            entity.HasIndex(e => e.Idatendimento, "fk_item_has_atendimento_atendimento1_idx");

            entity.HasIndex(e => e.Iditem, "fk_item_has_atendimento_item1_idx");

            entity.Property(e => e.Idatendimento).HasColumnName("idatendimento");
            entity.Property(e => e.Iditem).HasColumnName("iditem");
            entity.Property(e => e.Idfuncionario).HasColumnName("idfuncionario");
            entity.Property(e => e.QuantidadeItens).HasColumnName("quantidade_itens");
            entity.Property(e => e.ValorunitarioEntrada)
                .HasPrecision(10, 2)
                .HasColumnName("valorunitario_entrada");

            entity.HasOne(d => d.IdatendimentoNavigation).WithMany(p => p.Atendimentoitems)
                .HasForeignKey(d => d.Idatendimento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_has_atendimento_atendimento1");

            entity.HasOne(d => d.IdfuncionarioNavigation).WithMany(p => p.Atendimentoitems)
                .HasForeignKey(d => d.Idfuncionario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_atendimentoitem_funcionario1");

            entity.HasOne(d => d.IditemNavigation).WithMany(p => p.Atendimentoitems)
                .HasForeignKey(d => d.Iditem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_has_atendimento_item1");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.Idcategoria).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.Property(e => e.Idcategoria).HasColumnName("idcategoria");
            entity.Property(e => e.Categoria)
                .HasMaxLength(35)
                .HasColumnName("categoria");
        });

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.HasKey(e => e.Idendereco).HasName("PRIMARY");

            entity.ToTable("endereco");

            entity.HasIndex(e => e.Idpessoa, "fk_endereco_pessoa1_idx");

            entity.Property(e => e.Idendereco).HasColumnName("idendereco");
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("cep");
            entity.Property(e => e.Complemento)
                .HasMaxLength(150)
                .HasColumnName("complemento");
            entity.Property(e => e.Idpessoa).HasColumnName("idpessoa");
            entity.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("numero");

            entity.HasOne(d => d.IdpessoaNavigation).WithMany(p => p.Enderecos)
                .HasForeignKey(d => d.Idpessoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_endereco_pessoa1");
        });

        modelBuilder.Entity<EntradaProduto>(entity =>
        {
            entity.HasKey(e => new { e.IdentradaProduto, e.Iditem })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("entrada_produto");

            entity.HasIndex(e => e.Iditem, "fk_entrada_produto_estoque1_idx");

            entity.Property(e => e.IdentradaProduto)
                .ValueGeneratedOnAdd()
                .HasColumnName("identrada_produto");
            entity.Property(e => e.Iditem).HasColumnName("iditem");
            entity.Property(e => e.DataEntradaProduto).HasColumnName("data_entrada_produto");
            entity.Property(e => e.QuantidadeEntrada).HasColumnName("quantidade_entrada");
            entity.Property(e => e.Unidade)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("unidade");
            entity.Property(e => e.ValorUnitarioEntrada)
                .HasPrecision(10, 2)
                .HasColumnName("valor_unitario_entrada");

            entity.HasOne(d => d.IditemNavigation).WithMany(p => p.EntradaProdutos)
                .HasForeignKey(d => d.Iditem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_entrada_produto_estoque1");
        });

        modelBuilder.Entity<Estoque>(entity =>
        {
            entity.HasKey(e => e.Iditem).HasName("PRIMARY");

            entity.ToTable("estoque");

            entity.HasIndex(e => e.Iditem, "fk_estoque_produto1_idx");

            entity.Property(e => e.Iditem)
                .ValueGeneratedNever()
                .HasColumnName("iditem");
            entity.Property(e => e.QuantidadeEstoque).HasColumnName("quantidade_estoque");

            entity.HasOne(d => d.IditemNavigation).WithOne(p => p.Estoque)
                .HasForeignKey<Estoque>(d => d.Iditem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_estoque_produto1");
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.Idpessoa).HasName("PRIMARY");

            entity.ToTable("funcionario");

            entity.Property(e => e.Idpessoa)
                .ValueGeneratedNever()
                .HasColumnName("idpessoa");
            entity.Property(e => e.Cargo)
                .HasMaxLength(20)
                .HasColumnName("cargo");
            entity.Property(e => e.DataAdmissao).HasColumnName("data_admissao");

            entity.HasOne(d => d.IdpessoaNavigation).WithOne(p => p.Funcionario)
                .HasForeignKey<Funcionario>(d => d.Idpessoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_funcionario_pessoa");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Iditem).HasName("PRIMARY");

            entity.ToTable("item");

            entity.HasIndex(e => e.Idcategoria, "fk_produto_servico_categoria1_idx");

            entity.Property(e => e.Iditem).HasColumnName("iditem");
            entity.Property(e => e.AtivoProdutoServico)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("ativo_produto_servico");
            entity.Property(e => e.DescricaoItem)
                .HasMaxLength(200)
                .HasColumnName("descricao_item");
            entity.Property(e => e.Idcategoria).HasColumnName("idcategoria");
            entity.Property(e => e.Item1)
                .HasMaxLength(40)
                .HasColumnName("item");
            entity.Property(e => e.Preco)
                .HasPrecision(10, 2)
                .HasColumnName("preco");
            entity.Property(e => e.TipoItem)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("tipo_item");

            entity.HasOne(d => d.IdcategoriaNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.Idcategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_produto_servico_categoria1");
        });

        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Idpessoa).HasName("PRIMARY");

            entity.ToTable("pessoa");

            entity.Property(e => e.Idpessoa).HasColumnName("idpessoa");
            entity.Property(e => e.AtivoPessoa)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("ativo_pessoa");
            entity.Property(e => e.Cpf).HasColumnName("cpf");
            entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("sexo");
            entity.Property(e => e.Telefone).HasColumnName("telefone");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Iditem).HasName("PRIMARY");

            entity.ToTable("produto");

            entity.HasIndex(e => e.Iditem, "fk_produto_item1_idx");

            entity.Property(e => e.Iditem)
                .ValueGeneratedNever()
                .HasColumnName("iditem");
            entity.Property(e => e.CodigoPreco).HasColumnName("codigo_preco");
            entity.Property(e => e.PrecoVenda)
                .HasPrecision(10, 2)
                .HasColumnName("preco_venda");
            entity.Property(e => e.Unidade)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("unidade");

            entity.HasOne(d => d.IditemNavigation).WithOne(p => p.Produto)
                .HasForeignKey<Produto>(d => d.Iditem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_produto_item1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
