using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace localizeBackendAPI.Models;

public partial class LocalizeBackendContext : DbContext
{
    public LocalizeBackendContext()
    {
    }

    public LocalizeBackendContext(DbContextOptions<LocalizeBackendContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=localizeBackend;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empresas__3214EC07900C4E86");

            entity.HasIndex(e => e.Cnpj, "UQ__Empresas__AA57D6B4B5A9E9D0").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Abertura)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AtividadePrincipal)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Bairro)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Cep)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("CEP");
            entity.Property(e => e.Cnpj)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("CNPJ");
            entity.Property(e => e.Complemento)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Logradouro)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Municipio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NaturezaJuridica)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NomeEmpresarial)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NomeFantasia)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Situacao)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UF");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Empresas__Usuari__3F466844");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07FC7A5FDB");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534737EE24F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SenhaHash)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
