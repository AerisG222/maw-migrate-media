using MawDbMigrateTool.Models.Extensions;

namespace MawDbMigrateTool;

public class TargetBuilder
{
    readonly Models.Target.Db _target = new();
    readonly Dictionary<short, Guid> _userIdMap = [];
    readonly Dictionary<short, Guid> _roleIdMap = [];

    public Models.Target.Db Build(Models.Source.Db src)
    {
        PrepareUsers(src.Users);
        PrepareRoles(src.Roles);
        PrepareUserRoles(src.UserRoles);

        return _target;
    }

    void PrepareUsers(IEnumerable<Models.Source.User> users)
    {
        foreach (var user in users)
        {
            var targetUser = user.ToTarget();
            _userIdMap.Add(user.Id, targetUser.Id);
            _target.Users.Add(targetUser);
        }
    }

    void PrepareRoles(IEnumerable<Models.Source.Role> roles)
    {
        foreach (var role in roles)
        {
            var targetRole = role.ToTarget();
            _roleIdMap.Add(role.Id, targetRole.Id);
            _target.Roles.Add(targetRole);
        }
    }

    void PrepareUserRoles(IEnumerable<Models.Source.UserRole> userRoles)
    {
        foreach (var userRole in userRoles)
        {
            var targetUserRole = new Models.Target.UserRole
            {
                UserId = _userIdMap[userRole.UserId],
                RoleId = _roleIdMap[userRole.RoleId]
            };
            _target.UserRoles.Add(targetUserRole);
        }
    }
}
