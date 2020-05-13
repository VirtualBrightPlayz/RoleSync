using EXILED.Extensions;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleSync
{
    [HarmonyPatch(typeof(PermissionsHandler), nameof(PermissionsHandler.GetUserGroup))]
    class GetUserGroupPatch
    {
        public static bool Prefix(PermissionsHandler __instance, ref UserGroup __result, string playerId)
        {
            if (Player.GetPlayer(playerId) != null && Player.GetPlayer(playerId).serverRoles.Group != null && __instance._groups.ContainsValue(Player.GetPlayer(playerId).serverRoles.Group))
            {
                __result = Player.GetPlayer(playerId).serverRoles.Group;
                return false;
            }
            return true;
        }
    }
}
