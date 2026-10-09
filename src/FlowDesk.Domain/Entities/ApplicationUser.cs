using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Domain.Entities;

// Utente applicativo (tabella Users). Viene creato al primo accesso a partire dal token di
// Entra ID. Non è un'entità auditabile: è lui stesso il riferimento delle colonne di audit.
public class ApplicationUser
{
    public int IdUser { get; set; }

    // Identificativo dell'utente in Entra ID (claim "oid"): non cambia mai, a differenza dell'email.
    public Guid EntraObjectId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    // La fonte di verità del ruolo è Entra ID (App Roles): qui resta l'ultimo valore ricevuto.
    public RoleEnum IdRole { get; set; }

    // Un utente disattivato non può usare l'applicazione anche se Microsoft lo autentica.
    public bool Enabled { get; set; } = true;

    public DateTime DateCreation { get; set; }
    public DateTime DateLastLogin { get; set; }

    public virtual Role Role { get; set; } = null!;
}
