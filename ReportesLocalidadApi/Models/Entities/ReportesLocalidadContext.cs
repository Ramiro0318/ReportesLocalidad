using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ReportesLocalidadApi.Models.Entities;

public partial class ReportesLocalidadContext : DbContext
{
    public ReportesLocalidadContext()
    {
    }

    public ReportesLocalidadContext(DbContextOptions<ReportesLocalidadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorias> Categorias { get; set; }

    public virtual DbSet<Estados> Estados { get; set; }

    public virtual DbSet<RefreshTokens> RefreshTokens { get; set; }

    public virtual DbSet<Reportes> Reportes { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Categorias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categorias");

            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Estados>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("estados");

            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<RefreshTokens>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.FechaExpiracion, "IX_RefreshTokens_FechaExpiracion");

            entity.HasIndex(e => e.IdUsuario, "IX_RefreshTokens_IdUsuario");

            entity.HasIndex(e => e.Token, "IX_RefreshTokens_Token");

            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FechaRevocacion).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_Usuarios_IdUsuario");
        });

        modelBuilder.Entity<Reportes>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reportes");

            entity.HasIndex(e => e.ClientRequestId, "IX_Reportes_ClientRequestId");

            entity.HasIndex(e => e.FechaSubida, "IX_Reportes_FechaSubida");

            entity.HasIndex(e => e.IdCategoria, "IX_Reportes_IdCategoria");

            entity.HasIndex(e => e.IdEstado, "IX_Reportes_IdEstado");

            entity.HasIndex(e => e.IdUsuario, "IX_Reportes_IdUsuario");

            entity.Property(e => e.ClientRequestId).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Direccion).HasMaxLength(500);
            entity.Property(e => e.FechaEdicion).HasColumnType("datetime");
            entity.Property(e => e.FechaSubida).HasColumnType("datetime");
            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.Titulo).HasMaxLength(255);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Categorias_IdCategoria");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Estados_IdEstado");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Usuarios_IdUsuario");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.IdRol, "IX_Usuarios_IdRol");

            entity.Property(e => e.NombreUsuario).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.TokenFirebase).HasMaxLength(500);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles_IdRol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
