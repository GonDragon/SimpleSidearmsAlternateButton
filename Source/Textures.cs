using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace GD.SSAB
{
    public static class Textures
    {
        public static readonly Texture2D stanceRanged = ContentFinder<Texture2D>.Get("GD/UI/Gizmo/stance_ranged", true);
        public static readonly Texture2D stanceMelee = ContentFinder<Texture2D>.Get("GD/UI/Gizmo/stance_melee", true);
        public static readonly Texture2D stanceSkill = ContentFinder<Texture2D>.Get("GD/UI/Gizmo/stance_skill", true);

        public static Texture2D GetStance(PrimaryWeaponMode mode)
        {
            switch (mode)
            {
                case PrimaryWeaponMode.Ranged:
                    return stanceRanged;
                case PrimaryWeaponMode.Melee:
                    return stanceMelee;
                case PrimaryWeaponMode.BySkill:
                    return stanceSkill;
                default:
                    SSAB.Error("Icon can't be retrieved. Unrecognized stance mode.");
                    throw new NotImplementedException();
            }
        }
    }
}
