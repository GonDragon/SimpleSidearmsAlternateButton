using System.Collections.Generic;
using System;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

using SimpleSidearms.rimworld;
using PeteTimesSix.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace GD.SSAB
{
    class SidearmStance_Gizmo : Command_Action
    {
        public Pawn parent;
        public IEnumerable<ThingWithComps> carriedWeapons;
        private readonly CompSidearmMemory pawnMemory;

        public SidearmStance_Gizmo(Pawn parent, IEnumerable<ThingWithComps> carriedWeapons)
        {
            this.parent = parent;
            this.carriedWeapons = carriedWeapons;
            this.pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(parent);

            this.action = CycleBetweenModes;
            UpdateGizmo();
        }

        override public IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {

            get
            {
                yield return new FloatMenuOption("Force Weapon", () => Find.WindowStack.Add(new FloatMenu(ForceWeaponsOptions())));


                yield return new FloatMenuOption(PrimaryWeaponMode.Ranged.ToString(), () => { ChangeStance(PrimaryWeaponMode.Ranged); });
                yield return new FloatMenuOption(PrimaryWeaponMode.Melee.ToString(), () => { ChangeStance(PrimaryWeaponMode.Melee); });
                yield return new FloatMenuOption(PrimaryWeaponMode.BySkill.ToString(), () => { ChangeStance(PrimaryWeaponMode.BySkill); });
            }
        }

        private List<FloatMenuOption> ForceWeaponsOptions()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("Unarmed", () => { pawnMemory.SetUnarmedAsForced(this.parent.Drafted); }));
            foreach (ThingWithComps weapon in carriedWeapons)
            {
                options.Add(new FloatMenuOption(weapon.LabelCap, () => { pawnMemory.SetWeaponAsForced(weapon.toThingDefStuffDefPair(), this.parent.Drafted); }));
            }

            return options;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (pawnMemory == null) return new GizmoResult(GizmoState.Clear);
            return base.GizmoOnGUI(topLeft,maxWidth,parms);
        }

        private void CycleBetweenModes()
        {
            switch (pawnMemory.primaryWeaponMode)
            {
                case PrimaryWeaponMode.Ranged:
                    ChangeStance(PrimaryWeaponMode.BySkill);
                    break;

                case PrimaryWeaponMode.BySkill:
                    ChangeStance(PrimaryWeaponMode.Melee);
                    break;

                case PrimaryWeaponMode.Melee:
                    ChangeStance(PrimaryWeaponMode.Ranged);
                    break;

                default:
                    SSAB.Error("Something went wrong changing the stance: Unknown PrimaryWeaponMode");
                    return;
            }
        }

        private void UpdateGizmo()
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(this.parent);
            string label = "Stance ({0})";
            if (pawnMemory == null)
            {
                this.defaultLabel = string.Format(label, PeteTimesSix.SimpleSidearms.SimpleSidearms.Settings.ColonistDefaultWeaponMode);
                this.icon = GD.SSAB.Textures.GetStance(PeteTimesSix.SimpleSidearms.SimpleSidearms.Settings.ColonistDefaultWeaponMode);
                this.defaultDesc = GetDescription(PeteTimesSix.SimpleSidearms.SimpleSidearms.Settings.ColonistDefaultWeaponMode);
            };

            this.defaultLabel = string.Format(label, pawnMemory.primaryWeaponMode.ToString());
            this.icon = GD.SSAB.Textures.GetStance(pawnMemory.primaryWeaponMode);
            this.defaultDesc = GetDescription(pawnMemory.primaryWeaponMode);
        }

        private void ChangeStance(PrimaryWeaponMode mode)
        {
            if (pawnMemory == null) return;
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsPreference, KnowledgeAmount.SpecificInteraction);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
            UpdateGizmo();
            pawnMemory.primaryWeaponMode = mode;
        }

        private string GetDescription(PrimaryWeaponMode mode)
        {
            switch (mode)
            {
                case PrimaryWeaponMode.Ranged:
                    return "SidearmPreference_Ranged".Translate();
                case PrimaryWeaponMode.Melee:
                    return "SidearmPreference_Melee".Translate();
                case PrimaryWeaponMode.BySkill:
                    return "SidearmPreference_Skill".Translate();
                default:
                    SSAB.Error("Something went wrong changing the stance: Unknown PrimaryWeaponMode");
                    throw new NotImplementedException();
            }
        }
    }

    class SidearmForceWeapon_Gizmo : Command_Toggle
    {
        public Pawn parent;
        public CompSidearmMemory pawnMemory;
        public IEnumerable<ThingWithComps> carriedWeapons;


        public SidearmForceWeapon_Gizmo(Pawn parent, IEnumerable<ThingWithComps> carriedWeapons)
        {
            this.parent = parent;
            this.carriedWeapons = carriedWeapons;
            this.pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(this.parent); // If this Gizmo is called, then this thing is NOT null

            this.isActive = () =>
            {
                ThingDefStuffDefPair? currentWeapon = this.parent.equipment.Primary?.toThingDefStuffDefPair();
                if (this.parent.Drafted)
                {
                    return (pawnMemory.ForcedUnarmedWhileDrafted || pawnMemory.ForcedWeaponWhileDrafted != null || pawnMemory.ForcedUnarmed || pawnMemory.ForcedWeapon != null);
                }
                else
                {
                    return (pawnMemory.ForcedUnarmed || pawnMemory.ForcedWeapon != null);
                }
            };

            this.toggleAction = () =>
            {
                if(this.isActive())
                {
                    pawnMemory.UnsetForcedWeapon(this.parent.Drafted);
                    pawnMemory.UnsetUnarmedAsForced(this.parent.Drafted);
                }
            };

            UpdateGizmo(pawnMemory);
        }

        override public IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                yield return new FloatMenuOption("Unarmed",() => { pawnMemory.SetUnarmedAsForced(this.parent.Drafted); });
                foreach(ThingWithComps weapon in carriedWeapons)
                {
                    yield return new FloatMenuOption(weapon.LabelCap, () => { pawnMemory.SetWeaponAsForced(weapon.toThingDefStuffDefPair(), this.parent.Drafted); });
                }
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(parent);
            if (pawnMemory == null) return new GizmoResult(GizmoState.Clear);
            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }

        private void UpdateGizmo(CompSidearmMemory pawnMemory)
        {
            string weaponLabel = "None";
            Texture2D new_icon = TexCommand.RemoveRoutePlannerWaypoint; // PLACEHOLDER
            if(this.isActive())
            {
                if (pawnMemory.ForcedUnarmedWhileDrafted || pawnMemory.ForcedUnarmed)
                {
                    weaponLabel = "Unarmed";
                    new_icon = TexCommand.AttackMelee;

                } else if (pawnMemory.ForcedWeaponWhileDrafted != null || pawnMemory.ForcedWeapon != null)
                {
                    ThingDefStuffDefPair forcedWeapon = (ThingDefStuffDefPair)(this.parent.Drafted ? pawnMemory.ForcedWeaponWhileDrafted ?? pawnMemory.ForcedWeapon : pawnMemory.ForcedWeapon);
                    weaponLabel = forcedWeapon.getLabelCap();
                    Graphic outerGraphic = forcedWeapon.thing.graphic;
                    if (outerGraphic is Graphic_StackCount) outerGraphic = (outerGraphic as Graphic_StackCount).SubGraphicForStackCount(this.parent.equipment.Primary.stackCount, forcedWeapon.thing);
                    Material material = outerGraphic.ExtractInnerGraphicFor(null).MatAt(forcedWeapon.thing.defaultPlacingRot, null);
                    new_icon = (Texture2D)material.mainTexture;

                } else
                {
                    SSAB.Error("Unrecognized forced mode");
                    throw new NotImplementedException();
                }
            }

            this.defaultLabel = String.Format("Forced {0}", weaponLabel);
            this.icon = new_icon;
            
        }
        
    }

    public class SidearmSmallButtons_Gizmo : Command
    {
        public Pawn parent;

        public Command_Toggle forceButton;
        public Command_Action memoryButton;

        public SidearmSmallButtons_Gizmo(Pawn parent, IEnumerable<ThingWithComps> carriedWeapons)
        {
            this.parent = parent;

            forceButton = new SidearmForceWeapon_Gizmo(parent, carriedWeapons);
            memoryButton = new Command_Action();
        }

        public override float GetWidth(float maxWidth) => 36f;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GizmoResult result1 = forceButton.GizmoOnGUIShrunk(topLeft, this.GetWidth(maxWidth), parms);
            topLeft.y += 37f;
            GizmoResult result2 = memoryButton.GizmoOnGUIShrunk(topLeft, this.GetWidth(maxWidth), parms);

            return result1.State == GizmoState.Clear ? result2 : result1;
        }
    }

}
