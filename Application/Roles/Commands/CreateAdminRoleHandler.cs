using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Roles.Commands;

public sealed class CreateAdminRoleHandler(
    IUnitOfWork unitOfWork,
    IRoleRepository roleRepo,
    IPermissionRepository permissionRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    private const string ManagePermission = "Admin.Role.Manage";

    public async Task<Result> HandleAsync(
        CreateAdminRoleCommand cmd,
        CancellationToken ct = default)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Result.Failure(Errors.Role.PermissionDenied);

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(Errors.Role.NameRequired);

        var trimmedName = cmd.Name.Trim();

        await using var uow = await unitOfWork.BeginAsync(ct);

        var existing = await roleRepo.GetByNameAndScopeAsync(
            trimmedName, RoleScope.System, uow.Session, ct);
        if (existing is not null)
            return Result.Failure(Errors.Role.DuplicateName);

        var validPermissionIds = await permissionRepo.GetExistingIdsAsync(
            cmd.PermissionIds, uow.Session, ct);
        if (cmd.PermissionIds.Any(id => !validPermissionIds.Contains(id)))
            return Result.Failure(Errors.Role.PermissionNotFound);

        var role = new Role
        {
            Name = trimmedName,
            Description = cmd.Description?.Trim(),
            Scope = RoleScope.System,
            IsSystemReserved = cmd.IsSystemReserved,
            IsActive = true
        };

        var roleId = await roleRepo.InsertAsync(role, uow.Session, ct);

        if (cmd.PermissionIds.Count > 0)
        {
            await roleRepo.ReplacePermissionsAsync(
                roleId, cmd.PermissionIds, uow.Session, ct);
        }

        await activityLogRepo.WriteAsync(
            targetType: "Roles",
            targetId: roleId,
            actorUserId: currentUser.UserId,
            action: "CreateAdminRole",
            note: $"建立角色：{role.Name}",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
