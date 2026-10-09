using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FlowDesk.Infrastructure.Persistence.Context;

// Valorizza le colonne di audit a ogni salvataggio, così nessun chiamante può dimenticarsene
// né aggirarle usando il SaveChanges standard.
public class AuditSaveChangesInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeService dateTimeService) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();
        if (entries.Count == 0)
        {
            return;
        }

        var idUser = currentUserService.GetCurrentUserId();
        if (idUser <= 0)
        {
            throw new InvalidOperationException(
                "Impossibile salvare: la richiesta non ha un utente applicativo per l'audit.");
        }

        var now = dateTimeService.Now();
        foreach (var entry in entries)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.IdUserCreation = idUser;
                entity.DateCreation = now;
                entity.IdUserModification = idUser;
                entity.DateModification = now;
                continue;
            }

            entity.IdUserModification = idUser;
            entity.DateModification = now;
            entry.Property(e => e.IdUserModification).IsModified = true;
            entry.Property(e => e.DateModification).IsModified = true;

            // Archiviazione: registra chi e quando. Il ripristino azzera questi campi nel repository.
            if (entity is IDeletableEntity { Deleted: true } && !entity.DateDeleted.HasValue)
            {
                entity.IdUserDeleted = idUser;
                entity.DateDeleted = now;
                entry.Property(e => e.IdUserDeleted).IsModified = true;
                entry.Property(e => e.DateDeleted).IsModified = true;
            }
        }
    }
}
