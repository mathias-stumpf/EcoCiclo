using EcoCiclo.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoCiclo.Data;

public class EcoCicloDbContext : DbContext
{
    public EcoCicloDbContext(DbContextOptions<EcoCicloDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Residuo> Residuos => Set<Residuo>();
    public DbSet<Sinonimo> Sinonimos => Set<Sinonimo>();
    public DbSet<PontoColeta> PontosColeta => Set<PontoColeta>();
    public DbSet<ResiduoPontoColeta> ResiduosPontosColeta => Set<ResiduoPontoColeta>();
    public DbSet<Sugestao> Sugestoes => Set<Sugestao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("categorias");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            entity.Property(x => x.IdentificacaoVisual).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Residuo>(entity =>
        {
            entity.ToTable("residuos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            entity.Property(x => x.NomeNormalizado).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(170).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.InstrucoesPreparacao).HasMaxLength(4000);
            entity.Property(x => x.InstrucoesDescarte).HasMaxLength(4000);
            entity.Property(x => x.OQueNaoFazer).HasMaxLength(3000);
            entity.Property(x => x.Cuidados).HasMaxLength(3000);
            entity.Property(x => x.DestinoRecomendado).HasMaxLength(2000);
            entity.Property(x => x.ImpactoAmbiental).HasMaxLength(3000);
            entity.Property(x => x.FonteInformacao).HasMaxLength(1000);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.NomeNormalizado);
            entity.HasIndex(x => new { x.CategoriaId, x.Ativo });
            entity.HasOne(x => x.Categoria).WithMany(x => x.Residuos)
                .HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sinonimo>(entity =>
        {
            entity.ToTable("sinonimos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            entity.Property(x => x.NomeNormalizado).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.NomeNormalizado);
            entity.HasOne(x => x.Residuo).WithMany(x => x.Sinonimos)
                .HasForeignKey(x => x.ResiduoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PontoColeta>(entity =>
        {
            entity.ToTable("pontos_coleta");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Endereco).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Bairro).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Cidade).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(2).IsRequired();
            entity.Property(x => x.HorarioFuncionamento).HasMaxLength(500);
            entity.Property(x => x.Telefone).HasMaxLength(40);
            entity.Property(x => x.FonteInformacao).HasMaxLength(1000);
            entity.Property(x => x.UrlMapa).HasMaxLength(1000);
            entity.HasIndex(x => new { x.Estado, x.Cidade, x.Ativo });
        });

        modelBuilder.Entity<ResiduoPontoColeta>(entity =>
        {
            entity.ToTable("residuos_pontos_coleta");
            entity.HasKey(x => new { x.ResiduoId, x.PontoColetaId });
            entity.Property(x => x.FonteConfirmacao).HasMaxLength(1000);
            entity.HasOne(x => x.Residuo).WithMany(x => x.PontosColeta)
                .HasForeignKey(x => x.ResiduoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PontoColeta).WithMany(x => x.ResiduosAceitos)
                .HasForeignKey(x => x.PontoColetaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sugestao>(entity =>
        {
            entity.ToTable("sugestoes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NomeResiduo).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Descricao).HasMaxLength(2000);
            entity.Property(x => x.EmailContato).HasMaxLength(254);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => new { x.Status, x.DataEnvio });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NomeUsuario).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SenhaHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.NomeUsuario).IsUnique();
        });
    }
}
