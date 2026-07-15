using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Roles.DTOs;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Roles.Queries;

public sealed class GetAdminRolePermissionEditHandler(
    IUnitOfWork unitOfWork,
    IRoleRepository roleRepo,
    IPermissionRepository permissionRepo)
{
    public async Task<Result<AdminRolePermissionEditDto>> HandleAsync(
        GetAdminRolePermissionEditQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var permissions = await permissionRepo.GetAllSystemPermissionsAsync(uow.Session, ct);

        if (query.RoleId == 0)
        {
            await uow.CommitAsync(ct);
            return new AdminRolePermissionEditDto
            {
                Id = 0,
                Name = string.Empty,
                Description = null,
                IsActive = true,
                IsSystemReserved = false,
                SelectedPermissionIds = [],
                Permissions = permissions
            };
        }

        var role = await roleRepo.GetByIdAsync(query.RoleId, uow.Session, ct);
        if (role is null || role.Scope != RoleScope.System)
        {
            await uow.CommitAsync(ct);
            return Domain.Exceptions.Errors.Role.NotFound;
        }

        var selectedIds = await permissionRepo.GetPermissionIdsByRoleIdAsync(role.Id, uow.Session, ct);

        await uow.CommitAsync(ct);

        return new AdminRolePermissionEditDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystemReserved = role.IsSystemReserved,
            SelectedPermissionIds = selectedIds,
            Permissions = permissions
        };
    }
}
