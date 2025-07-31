using System;
using System.Collections.Generic;

namespace localizeBackendAPI.Models;

public partial class Usuario
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SenhaHash { get; set; } = null!;

    public bool Ativo { get; set; }

    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}
