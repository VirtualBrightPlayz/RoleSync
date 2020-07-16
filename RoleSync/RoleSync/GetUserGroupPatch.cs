using Exiled.API.Features;
using HarmonyLib;
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
            if (Player.Get(playerId) != null && Player.Get(playerId).ReferenceHub.serverRoles.Group != null && __instance._groups.ContainsValue(Player.Get(playerId).ReferenceHub.serverRoles.Group))
            {
                __result = Player.Get(playerId).ReferenceHub.serverRoles.Group;
                return false;
            }
            return true;
        }
    }
}
