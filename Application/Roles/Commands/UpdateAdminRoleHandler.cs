using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Roles.Commands;

public sealed class UpdateAdminRoleHandler(
    IUnitOfWork unitOfWork,
    IRoleRepository roleRepo,
    IPermissionRepository permissionRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    private const string ManagePermission = "Admin.Role.Manage";

    public async Task<Result> HandleAsync(
        UpdateAdminRoleCommand cmd,
        CancellationToken ct = default)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Result.Failure(Errors.Role.PermissionDenied);

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(Errors.Role.NameRequired);

        var trimmedName = cmd.Name.Trim();

        await using var uow = await unitOfWork.BeginAsync(ct);

        var role = await roleRepo.GetByIdAsync(cmd.RoleId, uow.Session, ct);
        if (role is null || role.Scope != RoleScope.System)
            return Result.Failure(Errors.Role.NotFound);

        var duplicate = await roleRepo.GetByNameAndScopeAsync(
            trimmedName, RoleScope.System, uow.Session, ct);
        if (duplicate is not null && duplicate.Id != cmd.RoleId)
            return Result.Failure(Errors.Role.DuplicateName);

        var validPermissionIds = await permissionRepo.GetExistingIdsAsync(
            cmd.PermissionIds, uow.Session, ct);
        if (cmd.PermissionIds.Any(id => !validPermissionIds.Contains(id)))
            return Result.Failure(Errors.Role.PermissionNotFound);

        role.Name = trimmedName;
        role.Description = cmd.Description?.Trim();
        role.IsActive = cmd.IsActive;

        await roleRepo.UpdateAsync(role, uow.Session, ct);
        await roleRepo.ReplacePermissionsAsync(
            role.Id, cmd.PermissionIds, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "Roles",
            targetId: role.Id,
            actorUserId: currentUser.UserId,
            action: "UpdateAdminRole",
            note: $"更新角色：{role.Name}",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
