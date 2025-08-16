namespace POS_ModernUI.Helpers;

public enum PermissionType
{
    Casher = 1,
    Sales = 2,
    Purchases = 4,
    Products = 8,
    Customers = 16,
    Debts = 32,
    Settings = 64,
    All = -1 // All permissions combined
}

public static class PermissionHelper
{
    public static bool HasPermission(this int userPermissions, PermissionType requiredPermission)
    {
        if (userPermissions == -1 ) // If user has all permissions
            return true;

        return (userPermissions & (int)requiredPermission) == (int)requiredPermission;
    }

    public static int GetPermissionValue(PermissionType permissionType)
    {
        return (int)permissionType;
    }
}
