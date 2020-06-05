using System;
using System.ComponentModel;
using System.Collections.Generic;

using CirnoLib.Jass;

namespace CirnoLib.Format
{
    [Serializable]
    public class W3Object : IArrayable
    {
        public int FileVersion;
        public OriginTable Origin;
        public CustomTable Custom;
        /// <summary>
        /// w3b, w3h, w3t, w3u = false / w3a, w3d, w3q = true
        /// </summary>
        public readonly bool IsExtended;
        public W3Object(bool IsExtended) => this.IsExtended = IsExtended;

        public abstract class Table : List<Defination>
        {
            public abstract Defination this[string ID] { get; set; }
            public abstract Defination this[byte[] ID] { get; set; }
        }

        public class OriginTable : Table
        {
            public override Defination this[string ID] {
                get => Find(item => item.OriginID == RawCode.Numberize(ID));
                set => this[FindIndex(item => item.OriginID == RawCode.Numberize(ID))] = value;
            }
            public override Defination this[byte[] ID] {
                get => Find(item => item.OriginID.GetRawCode().Compare(ID));
                set => this[FindIndex(item => item.OriginID.GetRawCode().Compare(ID))] = value;
            }
        }

        public class CustomTable : Table
        {
            public override Defination this[string ID] {
                get => Find(item => item.NewID == RawCode.Numberize(ID));
                set => this[FindIndex(item => item.NewID == RawCode.Numberize(ID))] = value;
            }
            public override Defination this[byte[] ID] {
                get => Find(item => item.NewID.GetRawCode().Compare(ID));
                set => this[FindIndex(item => item.NewID.GetRawCode().Compare(ID))] = value;
            }
        }

        public class Defination : List<Modification>
        {
            public int OriginID;    // RawCode
            public int NewID;       // RawCode

            public Modification this[string ModifyID] {
                get => Find(item => item.ModifyID == RawCode.Numberize(ModifyID));
                set => this[FindIndex(item => item.ModifyID == RawCode.Numberize(ModifyID))] = value;
            }
            public Modification this[byte[] ModifyID] {
                get => Find(item => item.ModifyID.GetRawCode().Compare(ModifyID));
                set => this[FindIndex(item => item.ModifyID.GetRawCode().Compare(ModifyID))] = value;
            }

            #region [    Unit Modification    ]
            public Modification GetUnitTypeDefaultActiveAbility {
                get => this[ModifyFieldID.UnitTypeDefaultActiveAbility];
                set => this[ModifyFieldID.UnitTypeDefaultActiveAbility] = value;
            }
            public Modification GetUnitTypeHeroAbilities {
                get => this[ModifyFieldID.UnitTypeHeroAbilities];
                set => this[ModifyFieldID.UnitTypeHeroAbilities] = value;
            }
            public Modification GetUnitTypeAbilities {
                get => this[ModifyFieldID.UnitTypeAbilities];
                set => this[ModifyFieldID.UnitTypeAbilities] = value;
            }
            public Modification GetUnitTypeAllowCustomTeamColor {
                get => this[ModifyFieldID.UnitTypeAllowCustomTeamColor];
                set => this[ModifyFieldID.UnitTypeAllowCustomTeamColor] = value;
            }
            public Modification GetUnitTypeBlendTime {
                get => this[ModifyFieldID.UnitTypeBlendTime];
                set => this[ModifyFieldID.UnitTypeBlendTime] = value;
            }
            public Modification GetUnitTypeCastBackswing {
                get => this[ModifyFieldID.UnitTypeCastBackswing];
                set => this[ModifyFieldID.UnitTypeCastBackswing] = value;
            }
            public Modification GetUnitTypeCastPoint {
                get => this[ModifyFieldID.UnitTypeCastPoint];
                set => this[ModifyFieldID.UnitTypeCastPoint] = value;
            }
            public Modification GetUnitTypeRunSpeed {
                get => this[ModifyFieldID.UnitTypeRunSpeed];
                set => this[ModifyFieldID.UnitTypeRunSpeed] = value;
            }
            public Modification GetUnitTypeWalkSpeed {
                get => this[ModifyFieldID.UnitTypeWalkSpeed];
                set => this[ModifyFieldID.UnitTypeWalkSpeed] = value;
            }
            public Modification GetUnitTypeButtonPositionX {
                get => this[ModifyFieldID.UnitTypeButtonPositionX];
                set => this[ModifyFieldID.UnitTypeButtonPositionX] = value;
            }
            public Modification GetUnitTypeButtonPositionY {
                get => this[ModifyFieldID.UnitTypeButtonPositionY];
                set => this[ModifyFieldID.UnitTypeButtonPositionY] = value;
            }
            public Modification GetUnitTypeDeathTime {
                get => this[ModifyFieldID.UnitTypeDeathTime];
                set => this[ModifyFieldID.UnitTypeDeathTime] = value;
            }
            public Modification GetUnitTypeElevationSamplePoints {
                get => this[ModifyFieldID.UnitTypeElevationSamplePoints];
                set => this[ModifyFieldID.UnitTypeElevationSamplePoints] = value;
            }
            public Modification GetUnitTypeElevationSampleRadius {
                get => this[ModifyFieldID.UnitTypeElevationSampleRadius];
                set => this[ModifyFieldID.UnitTypeElevationSampleRadius] = value;
            }
            public Modification GetUnitTypeFogOfWarSampleRadius {
                get => this[ModifyFieldID.UnitTypeFogOfWarSampleRadius];
                set => this[ModifyFieldID.UnitTypeFogOfWarSampleRadius] = value;
            }
            public Modification GetUnitTypeGroundTexture {
                get => this[ModifyFieldID.UnitTypeGroundTexture];
                set => this[ModifyFieldID.UnitTypeGroundTexture] = value;
            }
            public Modification GetUnitTypeHasWaterShadow {
                get => this[ModifyFieldID.UnitTypeHasWaterShadow];
                set => this[ModifyFieldID.UnitTypeHasWaterShadow] = value;
            }
            public Modification GetUnitTypeIcon {
                get => this[ModifyFieldID.UnitTypeIcon];
                set => this[ModifyFieldID.UnitTypeIcon] = value;
            }
            public Modification GetUnitTypeScoreScreenIcon {
                get => this[ModifyFieldID.UnitTypeScoreScreenIcon];
                set => this[ModifyFieldID.UnitTypeScoreScreenIcon] = value;
            }
            public Modification GetUnitTypeMaxPitch {
                get => this[ModifyFieldID.UnitTypeMaxPitch];
                set => this[ModifyFieldID.UnitTypeMaxPitch] = value;
            }
            public Modification GetUnitTypeMaxRoll {
                get => this[ModifyFieldID.UnitTypeMaxRoll];
                set => this[ModifyFieldID.UnitTypeMaxRoll] = value;
            }
            public Modification GetUnitTypeModel {
                get => this[ModifyFieldID.UnitTypeModel];
                set => this[ModifyFieldID.UnitTypeModel] = value;
            }
            public Modification GetUnitTypeModelExtraVersions {
                get => this[ModifyFieldID.UnitTypeModelExtraVersions];
                set => this[ModifyFieldID.UnitTypeModelExtraVersions] = value;
            }
            public Modification GetUnitTypeOcculderHeight {
                get => this[ModifyFieldID.UnitTypeOcculderHeight];
                set => this[ModifyFieldID.UnitTypeOcculderHeight] = value;
            }
            public Modification GetUnitTypeOrientationInterpolation {
                get => this[ModifyFieldID.UnitTypeOrientationInterpolation];
                set => this[ModifyFieldID.UnitTypeOrientationInterpolation] = value;
            }
            public Modification GetUnitTypeSwimProjectileImpactZ {
                get => this[ModifyFieldID.UnitTypeSwimProjectileImpactZ];
                set => this[ModifyFieldID.UnitTypeSwimProjectileImpactZ] = value;
            }
            public Modification GetUnitTypeProjectileImpactZ {
                get => this[ModifyFieldID.UnitTypeProjectileImpactZ];
                set => this[ModifyFieldID.UnitTypeProjectileImpactZ] = value;
            }
            public Modification GetUnitTypeProjectileLaunchX {
                get => this[ModifyFieldID.UnitTypeProjectileLaunchX];
                set => this[ModifyFieldID.UnitTypeProjectileLaunchX] = value;
            }
            public Modification GetUnitTypeSwimProjectileLaunchZ {
                get => this[ModifyFieldID.UnitTypeSwimProjectileLaunchZ];
                set => this[ModifyFieldID.UnitTypeSwimProjectileLaunchZ] = value;
            }
            public Modification GetUnitTypeProjectileLaunchZ {
                get => this[ModifyFieldID.UnitTypeProjectileLaunchZ];
                set => this[ModifyFieldID.UnitTypeProjectileLaunchZ] = value;
            }
            public Modification GetUnitTypePropulsionWindow {
                get => this[ModifyFieldID.UnitTypePropulsionWindow];
                set => this[ModifyFieldID.UnitTypePropulsionWindow] = value;
            }
            public Modification GetUnitTypeRequiredAnimationNames {
                get => this[ModifyFieldID.UnitTypeRequiredAnimationNames];
                set => this[ModifyFieldID.UnitTypeRequiredAnimationNames] = value;
            }
            public Modification GetUnitTypeRequiredAnimationAttachments {
                get => this[ModifyFieldID.UnitTypeRequiredAnimationAttachments];
                set => this[ModifyFieldID.UnitTypeRequiredAnimationAttachments] = value;
            }
            public Modification GetUnitTypeRequiredAnimationLinkNames {
                get => this[ModifyFieldID.UnitTypeRequiredAnimationLinkNames];
                set => this[ModifyFieldID.UnitTypeRequiredAnimationLinkNames] = value;
            }
            public Modification GetUnitTypeRequiredBoneNames {
                get => this[ModifyFieldID.UnitTypeRequiredBoneNames];
                set => this[ModifyFieldID.UnitTypeRequiredBoneNames] = value;
            }
            public Modification GetUnitTypeScaleProjectiles {
                get => this[ModifyFieldID.UnitTypeScaleProjectiles];
                set => this[ModifyFieldID.UnitTypeScaleProjectiles] = value;
            }
            public Modification GetUnitTypeScale {
                get => this[ModifyFieldID.UnitTypeScale];
                set => this[ModifyFieldID.UnitTypeScale] = value;
            }
            public Modification GetUnitTypeSelectionZ {
                get => this[ModifyFieldID.UnitTypeSelectionZ];
                set => this[ModifyFieldID.UnitTypeSelectionZ] = value;
            }
            public Modification GetUnitTypeSelectionOnWater {
                get => this[ModifyFieldID.UnitTypeSelectionOnWater];
                set => this[ModifyFieldID.UnitTypeSelectionOnWater] = value;
            }
            public Modification GetUnitTypeSelectionScale {
                get => this[ModifyFieldID.UnitTypeSelectionScale];
                set => this[ModifyFieldID.UnitTypeSelectionScale] = value;
            }
            public Modification GetUnitTypeShadowImage {
                get => this[ModifyFieldID.UnitTypeShadowImage];
                set => this[ModifyFieldID.UnitTypeShadowImage] = value;
            }
            public Modification GetUnitTypeShadowImageCenterX {
                get => this[ModifyFieldID.UnitTypeShadowImageCenterX];
                set => this[ModifyFieldID.UnitTypeShadowImageCenterX] = value;
            }
            public Modification GetUnitTypeShadowImageCenterY {
                get => this[ModifyFieldID.UnitTypeShadowImageCenterY];
                set => this[ModifyFieldID.UnitTypeShadowImageCenterY] = value;
            }
            public Modification GetUnitTypeShadowImageHeight {
                get => this[ModifyFieldID.UnitTypeShadowImageHeight];
                set => this[ModifyFieldID.UnitTypeShadowImageHeight] = value;
            }
            public Modification GetUnitTypeShadowImageWidth {
                get => this[ModifyFieldID.UnitTypeShadowImageWidth];
                set => this[ModifyFieldID.UnitTypeShadowImageWidth] = value;
            }
            public Modification GetUnitTypeShadowTexture {
                get => this[ModifyFieldID.UnitTypeShadowTexture];
                set => this[ModifyFieldID.UnitTypeShadowTexture] = value;
            }
            public Modification GetUnitTypeSpecialArt {
                get => this[ModifyFieldID.UnitTypeSpecialArt];
                set => this[ModifyFieldID.UnitTypeSpecialArt] = value;
            }
            public Modification GetUnitTypeTargetArt {
                get => this[ModifyFieldID.UnitTypeTargetArt];
                set => this[ModifyFieldID.UnitTypeTargetArt] = value;
            }
            public Modification GetUnitTypeTeamColor {
                get => this[ModifyFieldID.UnitTypeTeamColor];
                set => this[ModifyFieldID.UnitTypeTeamColor] = value;
            }
            public Modification GetUnitTypeRedTint {
                get => this[ModifyFieldID.UnitTypeRedTint];
                set => this[ModifyFieldID.UnitTypeRedTint] = value;
            }
            public Modification GetUnitTypeGreenTint {
                get => this[ModifyFieldID.UnitTypeGreenTint];
                set => this[ModifyFieldID.UnitTypeGreenTint] = value;
            }
            public Modification GetUnitTypeBlueTint {
                get => this[ModifyFieldID.UnitTypeBlueTint];
                set => this[ModifyFieldID.UnitTypeBlueTint] = value;
            }
            public Modification GetUnitTypeUseExtendedLineOfSight {
                get => this[ModifyFieldID.UnitTypeUseExtendedLineOfSight];
                set => this[ModifyFieldID.UnitTypeUseExtendedLineOfSight] = value;
            }
            public Modification GetUnitTypeAcquisitionRange {
                get => this[ModifyFieldID.UnitTypeAcquisitionRange];
                set => this[ModifyFieldID.UnitTypeAcquisitionRange] = value;
            }
            public Modification GetUnitTypeArmorType {
                get => this[ModifyFieldID.UnitTypeArmorType];
                set => this[ModifyFieldID.UnitTypeArmorType] = value;
            }
            public Modification GetUnitTypeBackswingPoint1 {
                get => this[ModifyFieldID.UnitTypeBackswingPoint1];
                set => this[ModifyFieldID.UnitTypeBackswingPoint1] = value;
            }
            public Modification GetUnitTypeDamagePoint1 {
                get => this[ModifyFieldID.UnitTypeDamagePoint1];
                set => this[ModifyFieldID.UnitTypeDamagePoint1] = value;
            }
            public Modification GetUnitTypeAreaOfEffectFull1 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectFull1];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectFull1] = value;
            }
            public Modification GetUnitTypeAreaOfEffectMedium1 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectMedium1];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectMedium1] = value;
            }
            public Modification GetUnitTypeAreaOfEffectSmall1 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectSmall1];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectSmall1] = value;
            }
            public Modification GetUnitTypeAreaOfEffectTargets1 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectTargets1];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectTargets1] = value;
            }
            public Modification GetUnitTypeAttackType1 {
                get => this[ModifyFieldID.UnitTypeAttackType1];
                set => this[ModifyFieldID.UnitTypeAttackType1] = value;
            }
            public Modification GetUnitTypeCooldown1 {
                get => this[ModifyFieldID.UnitTypeCooldown1];
                set => this[ModifyFieldID.UnitTypeCooldown1] = value;
            }
            public Modification GetUnitTypeDamageBase1 {
                get => this[ModifyFieldID.UnitTypeDamageBase1];
                set => this[ModifyFieldID.UnitTypeDamageBase1] = value;
            }
            public Modification GetUnitTypeDamageFactorMedium1 {
                get => this[ModifyFieldID.UnitTypeDamageFactorMedium1];
                set => this[ModifyFieldID.UnitTypeDamageFactorMedium1] = value;
            }
            public Modification GetUnitTypeDamageFactorSmall1 {
                get => this[ModifyFieldID.UnitTypeDamageFactorSmall1];
                set => this[ModifyFieldID.UnitTypeDamageFactorSmall1] = value;
            }
            public Modification GetUnitTypeDamageLossFactor1 {
                get => this[ModifyFieldID.UnitTypeDamageLossFactor1];
                set => this[ModifyFieldID.UnitTypeDamageLossFactor1] = value;
            }
            public Modification GetUnitTypeDamageNumberOfDice1 {
                get => this[ModifyFieldID.UnitTypeDamageNumberOfDice1];
                set => this[ModifyFieldID.UnitTypeDamageNumberOfDice1] = value;
            }
            public Modification GetUnitTypeDamageSidesPerDie1 {
                get => this[ModifyFieldID.UnitTypeDamageSidesPerDie1];
                set => this[ModifyFieldID.UnitTypeDamageSidesPerDie1] = value;
            }
            public Modification GetUnitTypeDamageSpillDistance1 {
                get => this[ModifyFieldID.UnitTypeDamageSpillDistance1];
                set => this[ModifyFieldID.UnitTypeDamageSpillDistance1] = value;
            }
            public Modification GetUnitTypeDamageSpillRadius1 {
                get => this[ModifyFieldID.UnitTypeDamageSpillRadius1];
                set => this[ModifyFieldID.UnitTypeDamageSpillRadius1] = value;
            }
            public Modification GetUnitTypeDamageUpgradeAmount1 {
                get => this[ModifyFieldID.UnitTypeDamageUpgradeAmount1];
                set => this[ModifyFieldID.UnitTypeDamageUpgradeAmount1] = value;
            }
            public Modification GetUnitTypeMaximumTargets1 {
                get => this[ModifyFieldID.UnitTypeMaximumTargets1];
                set => this[ModifyFieldID.UnitTypeMaximumTargets1] = value;
            }
            public Modification GetUnitTypeProjectileArc1 {
                get => this[ModifyFieldID.UnitTypeProjectileArc1];
                set => this[ModifyFieldID.UnitTypeProjectileArc1] = value;
            }
            public Modification GetUnitTypeProjectileArt1 {
                get => this[ModifyFieldID.UnitTypeProjectileArt1];
                set => this[ModifyFieldID.UnitTypeProjectileArt1] = value;
            }
            public Modification GetUnitTypeProjectileHoming1 {
                get => this[ModifyFieldID.UnitTypeProjectileHoming1];
                set => this[ModifyFieldID.UnitTypeProjectileHoming1] = value;
            }
            public Modification GetUnitTypeProjectileSpeed1 {
                get => this[ModifyFieldID.UnitTypeProjectileSpeed1];
                set => this[ModifyFieldID.UnitTypeProjectileSpeed1] = value;
            }
            public Modification GetUnitTypeRange1 {
                get => this[ModifyFieldID.UnitTypeRange1];
                set => this[ModifyFieldID.UnitTypeRange1] = value;
            }
            public Modification GetUnitTypeRangeMotionBuffer1 {
                get => this[ModifyFieldID.UnitTypeRangeMotionBuffer1];
                set => this[ModifyFieldID.UnitTypeRangeMotionBuffer1] = value;
            }
            public Modification GetUnitTypeShowUI1 {
                get => this[ModifyFieldID.UnitTypeShowUI1];
                set => this[ModifyFieldID.UnitTypeShowUI1] = value;
            }
            public Modification GetUnitTypeTargetsAllowed1 {
                get => this[ModifyFieldID.UnitTypeTargetsAllowed1];
                set => this[ModifyFieldID.UnitTypeTargetsAllowed1] = value;
            }
            public Modification GetUnitTypeWeaponSound1 {
                get => this[ModifyFieldID.UnitTypeWeaponSound1];
                set => this[ModifyFieldID.UnitTypeWeaponSound1] = value;
            }
            public Modification GetUnitTypeWeaponType1 {
                get => this[ModifyFieldID.UnitTypeWeaponType1];
                set => this[ModifyFieldID.UnitTypeWeaponType1] = value;
            }
            public Modification GetUnitTypeBackswingPoint2 {
                get => this[ModifyFieldID.UnitTypeBackswingPoint2];
                set => this[ModifyFieldID.UnitTypeBackswingPoint2] = value;
            }
            public Modification GetUnitTypeDamagePoint2 {
                get => this[ModifyFieldID.UnitTypeDamagePoint2];
                set => this[ModifyFieldID.UnitTypeDamagePoint2] = value;
            }
            public Modification GetUnitTypeAreaOfEffectFull2 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectFull2];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectFull2] = value;
            }
            public Modification GetUnitTypeAreaOfEffectMedium2 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectMedium2];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectMedium2] = value;
            }
            public Modification GetUnitTypeAreaOfEffectSmall2 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectSmall2];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectSmall2] = value;
            }
            public Modification GetUnitTypeAreaOfEffectTargets2 {
                get => this[ModifyFieldID.UnitTypeAreaOfEffectTargets2];
                set => this[ModifyFieldID.UnitTypeAreaOfEffectTargets2] = value;
            }
            public Modification GetUnitTypeAttackType2 {
                get => this[ModifyFieldID.UnitTypeAttackType2];
                set => this[ModifyFieldID.UnitTypeAttackType2] = value;
            }
            public Modification GetUnitTypeCooldown2 {
                get => this[ModifyFieldID.UnitTypeCooldown2];
                set => this[ModifyFieldID.UnitTypeCooldown2] = value;
            }
            public Modification GetUnitTypeDamageBase2 {
                get => this[ModifyFieldID.UnitTypeDamageBase2];
                set => this[ModifyFieldID.UnitTypeDamageBase2] = value;
            }
            public Modification GetUnitTypeDamageFactorMedium2 {
                get => this[ModifyFieldID.UnitTypeDamageFactorMedium2];
                set => this[ModifyFieldID.UnitTypeDamageFactorMedium2] = value;
            }
            public Modification GetUnitTypeDamageFactorSmall2 {
                get => this[ModifyFieldID.UnitTypeDamageFactorSmall2];
                set => this[ModifyFieldID.UnitTypeDamageFactorSmall2] = value;
            }
            public Modification GetUnitTypeDamageLossFactor2 {
                get => this[ModifyFieldID.UnitTypeDamageLossFactor2];
                set => this[ModifyFieldID.UnitTypeDamageLossFactor2] = value;
            }
            public Modification GetUnitTypeDamageNumberOfDice2 {
                get => this[ModifyFieldID.UnitTypeDamageNumberOfDice2];
                set => this[ModifyFieldID.UnitTypeDamageNumberOfDice2] = value;
            }
            public Modification GetUnitTypeDamageSidesPerDie2 {
                get => this[ModifyFieldID.UnitTypeDamageSidesPerDie2];
                set => this[ModifyFieldID.UnitTypeDamageSidesPerDie2] = value;
            }
            public Modification GetUnitTypeDamageSpillDistance2 {
                get => this[ModifyFieldID.UnitTypeDamageSpillDistance2];
                set => this[ModifyFieldID.UnitTypeDamageSpillDistance2] = value;
            }
            public Modification GetUnitTypeDamageSpillRadius2 {
                get => this[ModifyFieldID.UnitTypeDamageSpillRadius2];
                set => this[ModifyFieldID.UnitTypeDamageSpillRadius2] = value;
            }
            public Modification GetUnitTypeDamageUpgradeAmount2 {
                get => this[ModifyFieldID.UnitTypeDamageUpgradeAmount2];
                set => this[ModifyFieldID.UnitTypeDamageUpgradeAmount2] = value;
            }
            public Modification GetUnitTypeMaximumTargets2 {
                get => this[ModifyFieldID.UnitTypeMaximumTargets2];
                set => this[ModifyFieldID.UnitTypeMaximumTargets2] = value;
            }
            public Modification GetUnitTypeProjectileArc2 {
                get => this[ModifyFieldID.UnitTypeProjectileArc2];
                set => this[ModifyFieldID.UnitTypeProjectileArc2] = value;
            }
            public Modification GetUnitTypeProjectileArt2 {
                get => this[ModifyFieldID.UnitTypeProjectileArt2];
                set => this[ModifyFieldID.UnitTypeProjectileArt2] = value;
            }
            public Modification GetUnitTypeProjectileHoming2 {
                get => this[ModifyFieldID.UnitTypeProjectileHoming2];
                set => this[ModifyFieldID.UnitTypeProjectileHoming2] = value;
            }
            public Modification GetUnitTypeProjectileSpeed2 {
                get => this[ModifyFieldID.UnitTypeProjectileSpeed2];
                set => this[ModifyFieldID.UnitTypeProjectileSpeed2] = value;
            }
            public Modification GetUnitTypeRange2 {
                get => this[ModifyFieldID.UnitTypeRange2];
                set => this[ModifyFieldID.UnitTypeRange2] = value;
            }
            public Modification GetUnitTypeRangeMotionBuffer2 {
                get => this[ModifyFieldID.UnitTypeRangeMotionBuffer2];
                set => this[ModifyFieldID.UnitTypeRangeMotionBuffer2] = value;
            }
            public Modification GetUnitTypeShowUI2 {
                get => this[ModifyFieldID.UnitTypeShowUI2];
                set => this[ModifyFieldID.UnitTypeShowUI2] = value;
            }
            public Modification GetUnitTypeTargetsAllowed2 {
                get => this[ModifyFieldID.UnitTypeTargetsAllowed2];
                set => this[ModifyFieldID.UnitTypeTargetsAllowed2] = value;
            }
            public Modification GetUnitTypeWeaponSound2 {
                get => this[ModifyFieldID.UnitTypeWeaponSound2];
                set => this[ModifyFieldID.UnitTypeWeaponSound2] = value;
            }
            public Modification GetUnitTypeWeaponType2 {
                get => this[ModifyFieldID.UnitTypeWeaponType2];
                set => this[ModifyFieldID.UnitTypeWeaponType2] = value;
            }
            public Modification GetUnitTypeAttacksEnabled {
                get => this[ModifyFieldID.UnitTypeAttacksEnabled];
                set => this[ModifyFieldID.UnitTypeAttacksEnabled] = value;
            }
            public Modification GetUnitTypeDeathType {
                get => this[ModifyFieldID.UnitTypeDeathType];
                set => this[ModifyFieldID.UnitTypeDeathType] = value;
            }
            public Modification GetUnitTypeDefenseBase {
                get => this[ModifyFieldID.UnitTypeDefenseBase];
                set => this[ModifyFieldID.UnitTypeDefenseBase] = value;
            }
            public Modification GetUnitTypeDefenseType {
                get => this[ModifyFieldID.UnitTypeDefenseType];
                set => this[ModifyFieldID.UnitTypeDefenseType] = value;
            }
            public Modification GetUnitTypeDefenseUpgradeBonus {
                get => this[ModifyFieldID.UnitTypeDefenseUpgradeBonus];
                set => this[ModifyFieldID.UnitTypeDefenseUpgradeBonus] = value;
            }
            public Modification GetUnitTypeMinimumAttackRange {
                get => this[ModifyFieldID.UnitTypeMinimumAttackRange];
                set => this[ModifyFieldID.UnitTypeMinimumAttackRange] = value;
            }
            public Modification GetUnitTypeTargetedAs {
                get => this[ModifyFieldID.UnitTypeTargetedAs];
                set => this[ModifyFieldID.UnitTypeTargetedAs] = value;
            }
            public Modification GetUnitTypeDropItemsOnDeath {
                get => this[ModifyFieldID.UnitTypeDropItemsOnDeath];
                set => this[ModifyFieldID.UnitTypeDropItemsOnDeath] = value;
            }
            public Modification GetUnitTypeCategoryCampaign {
                get => this[ModifyFieldID.UnitTypeCategoryCampaign];
                set => this[ModifyFieldID.UnitTypeCategoryCampaign] = value;
            }
            public Modification GetUnitTypeCategorySpecial {
                get => this[ModifyFieldID.UnitTypeCategorySpecial];
                set => this[ModifyFieldID.UnitTypeCategorySpecial] = value;
            }
            public Modification GetUnitTypeDisplayAsNeutralHostile {
                get => this[ModifyFieldID.UnitTypeDisplayAsNeutralHostile];
                set => this[ModifyFieldID.UnitTypeDisplayAsNeutralHostile] = value;
            }
            public Modification GetUnitTypeHasTilesetSpecificData {
                get => this[ModifyFieldID.UnitTypeHasTilesetSpecificData];
                set => this[ModifyFieldID.UnitTypeHasTilesetSpecificData] = value;
            }
            public Modification GetUnitTypePlaceableInEditor {
                get => this[ModifyFieldID.UnitTypePlaceableInEditor];
                set => this[ModifyFieldID.UnitTypePlaceableInEditor] = value;
            }
            public Modification GetUnitTypeTilesets {
                get => this[ModifyFieldID.UnitTypeTilesets];
                set => this[ModifyFieldID.UnitTypeTilesets] = value;
            }
            public Modification GetUnitTypeUseClickHelper {
                get => this[ModifyFieldID.UnitTypeUseClickHelper];
                set => this[ModifyFieldID.UnitTypeUseClickHelper] = value;
            }
            public Modification GetUnitTypeGroupSeparationEnabled {
                get => this[ModifyFieldID.UnitTypeGroupSeparationEnabled];
                set => this[ModifyFieldID.UnitTypeGroupSeparationEnabled] = value;
            }
            public Modification GetUnitTypeGroupSeparationGroupNumber {
                get => this[ModifyFieldID.UnitTypeGroupSeparationGroupNumber];
                set => this[ModifyFieldID.UnitTypeGroupSeparationGroupNumber] = value;
            }
            public Modification GetUnitTypeGroupSeparationParameter {
                get => this[ModifyFieldID.UnitTypeGroupSeparationParameter];
                set => this[ModifyFieldID.UnitTypeGroupSeparationParameter] = value;
            }
            public Modification GetUnitTypeGroupSeparationPriority {
                get => this[ModifyFieldID.UnitTypeGroupSeparationPriority];
                set => this[ModifyFieldID.UnitTypeGroupSeparationPriority] = value;
            }
            public Modification GetUnitTypeFlyHeight {
                get => this[ModifyFieldID.UnitTypeFlyHeight];
                set => this[ModifyFieldID.UnitTypeFlyHeight] = value;
            }
            public Modification GetUnitTypeMinimumHeight {
                get => this[ModifyFieldID.UnitTypeMinimumHeight];
                set => this[ModifyFieldID.UnitTypeMinimumHeight] = value;
            }
            public Modification GetUnitTypeSpeedBase {
                get => this[ModifyFieldID.UnitTypeSpeedBase];
                set => this[ModifyFieldID.UnitTypeSpeedBase] = value;
            }
            public Modification GetUnitTypeSpeedMaximum {
                get => this[ModifyFieldID.UnitTypeSpeedMaximum];
                set => this[ModifyFieldID.UnitTypeSpeedMaximum] = value;
            }
            public Modification GetUnitTypeSpeedMinimum {
                get => this[ModifyFieldID.UnitTypeSpeedMinimum];
                set => this[ModifyFieldID.UnitTypeSpeedMinimum] = value;
            }
            public Modification GetUnitTypeTurnRate {
                get => this[ModifyFieldID.UnitTypeTurnRate];
                set => this[ModifyFieldID.UnitTypeTurnRate] = value;
            }
            public Modification GetUnitTypeMoveType {
                get => this[ModifyFieldID.UnitTypeMoveType];
                set => this[ModifyFieldID.UnitTypeMoveType] = value;
            }
            public Modification GetUnitTypeAIPlacementRadius {
                get => this[ModifyFieldID.UnitTypeAIPlacementRadius];
                set => this[ModifyFieldID.UnitTypeAIPlacementRadius] = value;
            }
            public Modification GetUnitTypeAIPlacementType {
                get => this[ModifyFieldID.UnitTypeAIPlacementType];
                set => this[ModifyFieldID.UnitTypeAIPlacementType] = value;
            }
            public Modification GetUnitTypeCollisionSize {
                get => this[ModifyFieldID.UnitTypeCollisionSize];
                set => this[ModifyFieldID.UnitTypeCollisionSize] = value;
            }
            public Modification GetUnitTypePathingMap {
                get => this[ModifyFieldID.UnitTypePathingMap];
                set => this[ModifyFieldID.UnitTypePathingMap] = value;
            }
            public Modification GetUnitTypePlacementPreventedBy {
                get => this[ModifyFieldID.UnitTypePlacementPreventedBy];
                set => this[ModifyFieldID.UnitTypePlacementPreventedBy] = value;
            }
            public Modification GetUnitTypePlacementRequires {
                get => this[ModifyFieldID.UnitTypePlacementRequires];
                set => this[ModifyFieldID.UnitTypePlacementRequires] = value;
            }
            public Modification GetUnitTypePlacementRequiresWaterRadius {
                get => this[ModifyFieldID.UnitTypePlacementRequiresWaterRadius];
                set => this[ModifyFieldID.UnitTypePlacementRequiresWaterRadius] = value;
            }
            public Modification GetUnitTypeBuildSound {
                get => this[ModifyFieldID.UnitTypeBuildSound];
                set => this[ModifyFieldID.UnitTypeBuildSound] = value;
            }
            public Modification GetUnitTypeSoundLoopFadeInRate {
                get => this[ModifyFieldID.UnitTypeSoundLoopFadeInRate];
                set => this[ModifyFieldID.UnitTypeSoundLoopFadeInRate] = value;
            }
            public Modification GetUnitTypeSoundLoopFadeOutRate {
                get => this[ModifyFieldID.UnitTypeSoundLoopFadeOutRate];
                set => this[ModifyFieldID.UnitTypeSoundLoopFadeOutRate] = value;
            }
            public Modification GetUnitTypeMoveSound {
                get => this[ModifyFieldID.UnitTypeMoveSound];
                set => this[ModifyFieldID.UnitTypeMoveSound] = value;
            }
            public Modification GetUnitTypeRandomSound {
                get => this[ModifyFieldID.UnitTypeRandomSound];
                set => this[ModifyFieldID.UnitTypeRandomSound] = value;
            }
            public Modification GetUnitTypeSoundSet {
                get => this[ModifyFieldID.UnitTypeSoundSet];
                set => this[ModifyFieldID.UnitTypeSoundSet] = value;
            }
            public Modification GetUnitTypeAgilityPerLevel {
                get => this[ModifyFieldID.UnitTypeAgilityPerLevel];
                set => this[ModifyFieldID.UnitTypeAgilityPerLevel] = value;
            }
            public Modification GetUnitTypeBuildTime {
                get => this[ModifyFieldID.UnitTypeBuildTime];
                set => this[ModifyFieldID.UnitTypeBuildTime] = value;
            }
            public Modification GetUnitTypeCanBeBuiltOn {
                get => this[ModifyFieldID.UnitTypeCanBeBuiltOn];
                set => this[ModifyFieldID.UnitTypeCanBeBuiltOn] = value;
            }
            public Modification GetUnitTypeCanBuildOn {
                get => this[ModifyFieldID.UnitTypeCanBuildOn];
                set => this[ModifyFieldID.UnitTypeCanBuildOn] = value;
            }
            public Modification GetUnitTypeCanFlee {
                get => this[ModifyFieldID.UnitTypeCanFlee];
                set => this[ModifyFieldID.UnitTypeCanFlee] = value;
            }
            public Modification GetUnitTypeFoodCost {
                get => this[ModifyFieldID.UnitTypeFoodCost];
                set => this[ModifyFieldID.UnitTypeFoodCost] = value;
            }
            public Modification GetUnitTypeFoodProduced {
                get => this[ModifyFieldID.UnitTypeFoodProduced];
                set => this[ModifyFieldID.UnitTypeFoodProduced] = value;
            }
            public Modification GetUnitTypeFormationRank {
                get => this[ModifyFieldID.UnitTypeFormationRank];
                set => this[ModifyFieldID.UnitTypeFormationRank] = value;
            }
            public Modification GetUnitTypeGoldBountyBase {
                get => this[ModifyFieldID.UnitTypeGoldBountyBase];
                set => this[ModifyFieldID.UnitTypeGoldBountyBase] = value;
            }
            public Modification GetUnitTypeGoldBountyNumberOfDice {
                get => this[ModifyFieldID.UnitTypeGoldBountyNumberOfDice];
                set => this[ModifyFieldID.UnitTypeGoldBountyNumberOfDice] = value;
            }
            public Modification GetUnitTypeGoldBountySidesPerDie {
                get => this[ModifyFieldID.UnitTypeGoldBountySidesPerDie];
                set => this[ModifyFieldID.UnitTypeGoldBountySidesPerDie] = value;
            }
            public Modification GetUnitTypeGoldCost {
                get => this[ModifyFieldID.UnitTypeGoldCost];
                set => this[ModifyFieldID.UnitTypeGoldCost] = value;
            }
            public Modification GetUnitTypeHideHeroDeathMessage {
                get => this[ModifyFieldID.UnitTypeHideHeroDeathMessage];
                set => this[ModifyFieldID.UnitTypeHideHeroDeathMessage] = value;
            }
            public Modification GetUnitTypeHideHeroInterfaceIcon {
                get => this[ModifyFieldID.UnitTypeHideHeroInterfaceIcon];
                set => this[ModifyFieldID.UnitTypeHideHeroInterfaceIcon] = value;
            }
            public Modification GetUnitTypeHideHeroMinimapDisplay {
                get => this[ModifyFieldID.UnitTypeHideHeroMinimapDisplay];
                set => this[ModifyFieldID.UnitTypeHideHeroMinimapDisplay] = value;
            }
            public Modification GetUnitTypeHideMinimapDisplay {
                get => this[ModifyFieldID.UnitTypeHideMinimapDisplay];
                set => this[ModifyFieldID.UnitTypeHideMinimapDisplay] = value;
            }
            public Modification GetUnitTypeHitPointsMaximum {
                get => this[ModifyFieldID.UnitTypeHitPointsMaximum];
                set => this[ModifyFieldID.UnitTypeHitPointsMaximum] = value;
            }
            public Modification GetUnitTypeHitPointsRegeneration {
                get => this[ModifyFieldID.UnitTypeHitPointsRegeneration];
                set => this[ModifyFieldID.UnitTypeHitPointsRegeneration] = value;
            }
            public Modification GetUnitTypeHitPointsRegenerationType {
                get => this[ModifyFieldID.UnitTypeHitPointsRegenerationType];
                set => this[ModifyFieldID.UnitTypeHitPointsRegenerationType] = value;
            }
            public Modification GetUnitTypeIntelligencePerLevel {
                get => this[ModifyFieldID.UnitTypeIntelligencePerLevel];
                set => this[ModifyFieldID.UnitTypeIntelligencePerLevel] = value;
            }
            public Modification GetUnitTypeIsABuilding {
                get => this[ModifyFieldID.UnitTypeIsABuilding];
                set => this[ModifyFieldID.UnitTypeIsABuilding] = value;
            }
            public Modification GetUnitTypeLevel {
                get => this[ModifyFieldID.UnitTypeLevel];
                set => this[ModifyFieldID.UnitTypeLevel] = value;
            }
            public Modification GetUnitTypeLumberBountyBase {
                get => this[ModifyFieldID.UnitTypeLumberBountyBase];
                set => this[ModifyFieldID.UnitTypeLumberBountyBase] = value;
            }
            public Modification GetUnitTypeLumberBountyNumberOfDice {
                get => this[ModifyFieldID.UnitTypeLumberBountyNumberOfDice];
                set => this[ModifyFieldID.UnitTypeLumberBountyNumberOfDice] = value;
            }
            public Modification GetUnitTypeLumberBountySidesPerDie {
                get => this[ModifyFieldID.UnitTypeLumberBountySidesPerDie];
                set => this[ModifyFieldID.UnitTypeLumberBountySidesPerDie] = value;
            }
            public Modification GetUnitTypeLumberCost {
                get => this[ModifyFieldID.UnitTypeLumberCost];
                set => this[ModifyFieldID.UnitTypeLumberCost] = value;
            }
            public Modification GetUnitTypeManaInitialAmount {
                get => this[ModifyFieldID.UnitTypeManaInitialAmount];
                set => this[ModifyFieldID.UnitTypeManaInitialAmount] = value;
            }
            public Modification GetUnitTypeManaMaximum {
                get => this[ModifyFieldID.UnitTypeManaMaximum];
                set => this[ModifyFieldID.UnitTypeManaMaximum] = value;
            }
            public Modification GetUnitTypeManaRegeneration {
                get => this[ModifyFieldID.UnitTypeManaRegeneration];
                set => this[ModifyFieldID.UnitTypeManaRegeneration] = value;
            }
            public Modification GetUnitTypeShowNeutralBuildingIcon {
                get => this[ModifyFieldID.UnitTypeShowNeutralBuildingIcon];
                set => this[ModifyFieldID.UnitTypeShowNeutralBuildingIcon] = value;
            }
            public Modification GetUnitTypeValidAsRandomNeutralBuilding {
                get => this[ModifyFieldID.UnitTypeValidAsRandomNeutralBuilding];
                set => this[ModifyFieldID.UnitTypeValidAsRandomNeutralBuilding] = value;
            }
            public Modification GetUnitTypePointValue {
                get => this[ModifyFieldID.UnitTypePointValue];
                set => this[ModifyFieldID.UnitTypePointValue] = value;
            }
            public Modification GetUnitTypePrimaryAttribute {
                get => this[ModifyFieldID.UnitTypePrimaryAttribute];
                set => this[ModifyFieldID.UnitTypePrimaryAttribute] = value;
            }
            public Modification GetUnitTypePriority {
                get => this[ModifyFieldID.UnitTypePriority];
                set => this[ModifyFieldID.UnitTypePriority] = value;
            }
            public Modification GetUnitTypeRace {
                get => this[ModifyFieldID.UnitTypeRace];
                set => this[ModifyFieldID.UnitTypeRace] = value;
            }
            public Modification GetUnitTypeRepairGoldCost {
                get => this[ModifyFieldID.UnitTypeRepairGoldCost];
                set => this[ModifyFieldID.UnitTypeRepairGoldCost] = value;
            }
            public Modification GetUnitTypeRepairLumberCost {
                get => this[ModifyFieldID.UnitTypeRepairLumberCost];
                set => this[ModifyFieldID.UnitTypeRepairLumberCost] = value;
            }
            public Modification GetUnitTypeRepairTime {
                get => this[ModifyFieldID.UnitTypeRepairTime];
                set => this[ModifyFieldID.UnitTypeRepairTime] = value;
            }
            public Modification GetUnitTypeSightRadiusDay {
                get => this[ModifyFieldID.UnitTypeSightRadiusDay];
                set => this[ModifyFieldID.UnitTypeSightRadiusDay] = value;
            }
            public Modification GetUnitTypeSightRadiusNight {
                get => this[ModifyFieldID.UnitTypeSightRadiusNight];
                set => this[ModifyFieldID.UnitTypeSightRadiusNight] = value;
            }
            public Modification GetUnitTypeSleeps {
                get => this[ModifyFieldID.UnitTypeSleeps];
                set => this[ModifyFieldID.UnitTypeSleeps] = value;
            }
            public Modification GetUnitTypeStartingAgility {
                get => this[ModifyFieldID.UnitTypeStartingAgility];
                set => this[ModifyFieldID.UnitTypeStartingAgility] = value;
            }
            public Modification GetUnitTypeStartingIntelligence {
                get => this[ModifyFieldID.UnitTypeStartingIntelligence];
                set => this[ModifyFieldID.UnitTypeStartingIntelligence] = value;
            }
            public Modification GetUnitTypeStartingStrength {
                get => this[ModifyFieldID.UnitTypeStartingStrength];
                set => this[ModifyFieldID.UnitTypeStartingStrength] = value;
            }
            public Modification GetUnitTypeStockMaximum {
                get => this[ModifyFieldID.UnitTypeStockMaximum];
                set => this[ModifyFieldID.UnitTypeStockMaximum] = value;
            }
            public Modification GetUnitTypeStockReplenishInterval {
                get => this[ModifyFieldID.UnitTypeStockReplenishInterval];
                set => this[ModifyFieldID.UnitTypeStockReplenishInterval] = value;
            }
            public Modification GetUnitTypeStockStartDelay {
                get => this[ModifyFieldID.UnitTypeStockStartDelay];
                set => this[ModifyFieldID.UnitTypeStockStartDelay] = value;
            }
            public Modification GetUnitTypeStrengthPerLevel {
                get => this[ModifyFieldID.UnitTypeStrengthPerLevel];
                set => this[ModifyFieldID.UnitTypeStrengthPerLevel] = value;
            }
            public Modification GetUnitTypeTransportedSize {
                get => this[ModifyFieldID.UnitTypeTransportedSize];
                set => this[ModifyFieldID.UnitTypeTransportedSize] = value;
            }
            public Modification GetUnitTypeUnitClassification {
                get => this[ModifyFieldID.UnitTypeUnitClassification];
                set => this[ModifyFieldID.UnitTypeUnitClassification] = value;
            }
            public Modification GetUnitTypeDependencyEquivalents {
                get => this[ModifyFieldID.UnitTypeDependencyEquivalents];
                set => this[ModifyFieldID.UnitTypeDependencyEquivalents] = value;
            }
            public Modification GetUnitTypeHeroRevivalLocations {
                get => this[ModifyFieldID.UnitTypeHeroRevivalLocations];
                set => this[ModifyFieldID.UnitTypeHeroRevivalLocations] = value;
            }
            public Modification GetUnitTypeItemsMade {
                get => this[ModifyFieldID.UnitTypeItemsMade];
                set => this[ModifyFieldID.UnitTypeItemsMade] = value;
            }
            public Modification GetUnitTypeItemsSold {
                get => this[ModifyFieldID.UnitTypeItemsSold];
                set => this[ModifyFieldID.UnitTypeItemsSold] = value;
            }
            public Modification GetUnitTypeRequirements {
                get => this[ModifyFieldID.UnitTypeRequirements];
                set => this[ModifyFieldID.UnitTypeRequirements] = value;
            }
            public Modification GetUnitTypeRequirementsLevels {
                get => this[ModifyFieldID.UnitTypeRequirementsLevels];
                set => this[ModifyFieldID.UnitTypeRequirementsLevels] = value;
            }
            public Modification GetUnitTypeRequirementsTier2 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier2];
                set => this[ModifyFieldID.UnitTypeRequirementsTier2] = value;
            }
            public Modification GetUnitTypeRequirementsTier3 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier3];
                set => this[ModifyFieldID.UnitTypeRequirementsTier3] = value;
            }
            public Modification GetUnitTypeRequirementsTier4 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier4];
                set => this[ModifyFieldID.UnitTypeRequirementsTier4] = value;
            }
            public Modification GetUnitTypeRequirementsTier5 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier5];
                set => this[ModifyFieldID.UnitTypeRequirementsTier5] = value;
            }
            public Modification GetUnitTypeRequirementsTier6 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier6];
                set => this[ModifyFieldID.UnitTypeRequirementsTier6] = value;
            }
            public Modification GetUnitTypeRequirementsTier7 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier7];
                set => this[ModifyFieldID.UnitTypeRequirementsTier7] = value;
            }
            public Modification GetUnitTypeRequirementsTier8 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier8];
                set => this[ModifyFieldID.UnitTypeRequirementsTier8] = value;
            }
            public Modification GetUnitTypeRequirementsTier9 {
                get => this[ModifyFieldID.UnitTypeRequirementsTier9];
                set => this[ModifyFieldID.UnitTypeRequirementsTier9] = value;
            }
            public Modification GetUnitTypeRequirementsTiersUsed {
                get => this[ModifyFieldID.UnitTypeRequirementsTiersUsed];
                set => this[ModifyFieldID.UnitTypeRequirementsTiersUsed] = value;
            }
            public Modification GetUnitTypeStructuresBuilt {
                get => this[ModifyFieldID.UnitTypeStructuresBuilt];
                set => this[ModifyFieldID.UnitTypeStructuresBuilt] = value;
            }
            public Modification GetUnitTypeResearchesAvailable {
                get => this[ModifyFieldID.UnitTypeResearchesAvailable];
                set => this[ModifyFieldID.UnitTypeResearchesAvailable] = value;
            }
            public Modification GetUnitTypeRevivesDeadHeroes {
                get => this[ModifyFieldID.UnitTypeRevivesDeadHeroes];
                set => this[ModifyFieldID.UnitTypeRevivesDeadHeroes] = value;
            }
            public Modification GetUnitTypeUnitsSold {
                get => this[ModifyFieldID.UnitTypeUnitsSold];
                set => this[ModifyFieldID.UnitTypeUnitsSold] = value;
            }
            public Modification GetUnitTypeUnitsTrained {
                get => this[ModifyFieldID.UnitTypeUnitsTrained];
                set => this[ModifyFieldID.UnitTypeUnitsTrained] = value;
            }
            public Modification GetUnitTypeUpgradesTo {
                get => this[ModifyFieldID.UnitTypeUpgradesTo];
                set => this[ModifyFieldID.UnitTypeUpgradesTo] = value;
            }
            public Modification GetUnitTypeUpgradesUsed {
                get => this[ModifyFieldID.UnitTypeUpgradesUsed];
                set => this[ModifyFieldID.UnitTypeUpgradesUsed] = value;
            }
            public Modification GetUnitTypeDescription {
                get => this[ModifyFieldID.UnitTypeDescription];
                set => this[ModifyFieldID.UnitTypeDescription] = value;
            }
            public Modification GetUnitTypeHotkey {
                get => this[ModifyFieldID.UnitTypeHotkey];
                set => this[ModifyFieldID.UnitTypeHotkey] = value;
            }
            public Modification GetUnitTypeName {
                get => this[ModifyFieldID.UnitTypeName];
                set => this[ModifyFieldID.UnitTypeName] = value;
            }
            public Modification GetUnitTypeNameEditorSuffix {
                get => this[ModifyFieldID.UnitTypeNameEditorSuffix];
                set => this[ModifyFieldID.UnitTypeNameEditorSuffix] = value;
            }
            public Modification GetUnitTypeProperNames {
                get => this[ModifyFieldID.UnitTypeProperNames];
                set => this[ModifyFieldID.UnitTypeProperNames] = value;
            }
            public Modification GetUnitTypeProperNamesUsed {
                get => this[ModifyFieldID.UnitTypeProperNamesUsed];
                set => this[ModifyFieldID.UnitTypeProperNamesUsed] = value;
            }
            public Modification GetUnitTypeAwakenTooltip {
                get => this[ModifyFieldID.UnitTypeAwakenTooltip];
                set => this[ModifyFieldID.UnitTypeAwakenTooltip] = value;
            }
            public Modification GetUnitTypeTooltip {
                get => this[ModifyFieldID.UnitTypeTooltip];
                set => this[ModifyFieldID.UnitTypeTooltip] = value;
            }
            public Modification GetUnitTypeUbertip {
                get => this[ModifyFieldID.UnitTypeUbertip];
                set => this[ModifyFieldID.UnitTypeUbertip] = value;
            }
            public Modification GetUnitTypeReviveTooltip {
                get => this[ModifyFieldID.UnitTypeReviveTooltip];
                set => this[ModifyFieldID.UnitTypeReviveTooltip] = value;
            }
            #endregion

            #region [    Item Modification    ]
            public Modification GetItemTypeAbilities {
                get => this[ModifyFieldID.ItemTypeAbilities];
                set => this[ModifyFieldID.ItemTypeAbilities] = value;
            }
            public Modification GetItemTypeArmorType {
                get => this[ModifyFieldID.ItemTypeArmorType];
                set => this[ModifyFieldID.ItemTypeArmorType] = value;
            }
            public Modification GetItemTypeIcon {
                get => this[ModifyFieldID.ItemTypeIcon];
                set => this[ModifyFieldID.ItemTypeIcon] = value;
            }
            public Modification GetItemTypeButtonPositionX {
                get => this[ModifyFieldID.ItemTypeButtonPositionX];
                set => this[ModifyFieldID.ItemTypeButtonPositionX] = value;
            }
            public Modification GetItemTypeButtonPositionY {
                get => this[ModifyFieldID.ItemTypeButtonPositionY];
                set => this[ModifyFieldID.ItemTypeButtonPositionY] = value;
            }
            public Modification GetItemTypeClass {
                get => this[ModifyFieldID.ItemTypeClass];
                set => this[ModifyFieldID.ItemTypeClass] = value;
            }
            public Modification GetItemTypeColorBlue {
                get => this[ModifyFieldID.ItemTypeColorBlue];
                set => this[ModifyFieldID.ItemTypeColorBlue] = value;
            }
            public Modification GetItemTypeColorGreen {
                get => this[ModifyFieldID.ItemTypeColorGreen];
                set => this[ModifyFieldID.ItemTypeColorGreen] = value;
            }
            public Modification GetItemTypeColorRed {
                get => this[ModifyFieldID.ItemTypeColorRed];
                set => this[ModifyFieldID.ItemTypeColorRed] = value;
            }
            public Modification GetItemTypeCooldownGroup {
                get => this[ModifyFieldID.ItemTypeCooldownGroup];
                set => this[ModifyFieldID.ItemTypeCooldownGroup] = value;
            }
            public Modification GetItemTypeDescription {
                get => this[ModifyFieldID.ItemTypeDescription];
                set => this[ModifyFieldID.ItemTypeDescription] = value;
            }
            public Modification GetItemTypeDropsOnDeath {
                get => this[ModifyFieldID.ItemTypeDropsOnDeath];
                set => this[ModifyFieldID.ItemTypeDropsOnDeath] = value;
            }
            public Modification GetItemTypeDroppable {
                get => this[ModifyFieldID.ItemTypeDroppable];
                set => this[ModifyFieldID.ItemTypeDroppable] = value;
            }
            public Modification GetItemTypeModel {
                get => this[ModifyFieldID.ItemTypeModel];
                set => this[ModifyFieldID.ItemTypeModel] = value;
            }
            public Modification GetItemTypeGoldCost {
                get => this[ModifyFieldID.ItemTypeGoldCost];
                set => this[ModifyFieldID.ItemTypeGoldCost] = value;
            }
            public Modification GetItemTypeHotkey {
                get => this[ModifyFieldID.ItemTypeHotkey];
                set => this[ModifyFieldID.ItemTypeHotkey] = value;
            }
            public Modification GetItemTypeHealth {
                get => this[ModifyFieldID.ItemTypeHealth];
                set => this[ModifyFieldID.ItemTypeHealth] = value;
            }
            public Modification GetItemTypeIgnoreCooldown {
                get => this[ModifyFieldID.ItemTypeIgnoreCooldown];
                set => this[ModifyFieldID.ItemTypeIgnoreCooldown] = value;
            }
            public Modification GetItemTypeLevel {
                get => this[ModifyFieldID.ItemTypeLevel];
                set => this[ModifyFieldID.ItemTypeLevel] = value;
            }
            public Modification GetItemTypeLumberCost {
                get => this[ModifyFieldID.ItemTypeLumberCost];
                set => this[ModifyFieldID.ItemTypeLumberCost] = value;
            }
            public Modification GetItemTypeIsTransformable {
                get => this[ModifyFieldID.ItemTypeIsTransformable];
                set => this[ModifyFieldID.ItemTypeIsTransformable] = value;
            }
            public Modification GetItemTypeName {
                get => this[ModifyFieldID.ItemTypeName];
                set => this[ModifyFieldID.ItemTypeName] = value;
            }
            public Modification GetItemTypeUnclassifiedLevel {
                get => this[ModifyFieldID.ItemTypeUnclassifiedLevel];
                set => this[ModifyFieldID.ItemTypeUnclassifiedLevel] = value;
            }
            public Modification GetItemTypeIsPawnable {
                get => this[ModifyFieldID.ItemTypeIsPawnable];
                set => this[ModifyFieldID.ItemTypeIsPawnable] = value;
            }
            public Modification GetItemTypeIsPerishable {
                get => this[ModifyFieldID.ItemTypeIsPerishable];
                set => this[ModifyFieldID.ItemTypeIsPerishable] = value;
            }
            public Modification GetItemTypeIsRandomChoice {
                get => this[ModifyFieldID.ItemTypeIsRandomChoice];
                set => this[ModifyFieldID.ItemTypeIsRandomChoice] = value;
            }
            public Modification GetItemTypeIsPowerup {
                get => this[ModifyFieldID.ItemTypeIsPowerup];
                set => this[ModifyFieldID.ItemTypeIsPowerup] = value;
            }
            public Modification GetItemTypePriority {
                get => this[ModifyFieldID.ItemTypePriority];
                set => this[ModifyFieldID.ItemTypePriority] = value;
            }
            public Modification GetItemTypeRequirements {
                get => this[ModifyFieldID.ItemTypeRequirements];
                set => this[ModifyFieldID.ItemTypeRequirements] = value;
            }
            public Modification GetItemTypeRequiredLevels {
                get => this[ModifyFieldID.ItemTypeRequiredLevels];
                set => this[ModifyFieldID.ItemTypeRequiredLevels] = value;
            }
            public Modification GetItemTypeScale {
                get => this[ModifyFieldID.ItemTypeScale];
                set => this[ModifyFieldID.ItemTypeScale] = value;
            }
            public Modification GetItemTypeIsSellable {
                get => this[ModifyFieldID.ItemTypeIsSellable];
                set => this[ModifyFieldID.ItemTypeIsSellable] = value;
            }
            public Modification GetItemTypeSelectionSize {
                get => this[ModifyFieldID.ItemTypeSelectionSize];
                set => this[ModifyFieldID.ItemTypeSelectionSize] = value;
            }
            public Modification GetItemTypeMaximumStock {
                get => this[ModifyFieldID.ItemTypeMaximumStock];
                set => this[ModifyFieldID.ItemTypeMaximumStock] = value;
            }
            public Modification GetItemTypeStockReplenishInterval {
                get => this[ModifyFieldID.ItemTypeStockReplenishInterval];
                set => this[ModifyFieldID.ItemTypeStockReplenishInterval] = value;
            }
            public Modification GetItemTypeStartingStock {
                get => this[ModifyFieldID.ItemTypeStartingStock];
                set => this[ModifyFieldID.ItemTypeStartingStock] = value;
            }
            public Modification GetItemTypeTooltip {
                get => this[ModifyFieldID.ItemTypeTooltip];
                set => this[ModifyFieldID.ItemTypeTooltip] = value;
            }
            public Modification GetItemTypeUbertip {
                get => this[ModifyFieldID.ItemTypeUbertip];
                set => this[ModifyFieldID.ItemTypeUbertip] = value;
            }
            public Modification GetItemTypeIsUsable {
                get => this[ModifyFieldID.ItemTypeIsUsable];
                set => this[ModifyFieldID.ItemTypeIsUsable] = value;
            }
            public Modification GetItemTypeUses {
                get => this[ModifyFieldID.ItemTypeUses];
                set => this[ModifyFieldID.ItemTypeUses] = value;
            }
            #endregion

            #region [    Destructable  Modification    ]
            public Modification GetDestructableTypeName {
                get => this[ModifyFieldID.DestructableTypeName];
                set => this[ModifyFieldID.DestructableTypeName] = value;
            }
            public Modification GetDestructableTypeEditorSuffix {
                get => this[ModifyFieldID.DestructableTypeEditorSuffix];
                set => this[ModifyFieldID.DestructableTypeEditorSuffix] = value;
            }
            public Modification GetDestructableTypeCategory {
                get => this[ModifyFieldID.DestructableTypeCategory];
                set => this[ModifyFieldID.DestructableTypeCategory] = value;
            }
            public Modification GetDestructableTypeTilesets {
                get => this[ModifyFieldID.DestructableTypeTilesets];
                set => this[ModifyFieldID.DestructableTypeTilesets] = value;
            }
            public Modification GetDestructableTypeIsTilesetSpecific {
                get => this[ModifyFieldID.DestructableTypeIsTilesetSpecific];
                set => this[ModifyFieldID.DestructableTypeIsTilesetSpecific] = value;
            }
            public Modification GetDestructableTypeFile {
                get => this[ModifyFieldID.DestructableTypeFile];
                set => this[ModifyFieldID.DestructableTypeFile] = value;
            }
            public Modification GetDestructableTypeIsLightweight {
                get => this[ModifyFieldID.DestructableTypeIsLightweight];
                set => this[ModifyFieldID.DestructableTypeIsLightweight] = value;
            }
            public Modification GetDestructableTypeIsFatLOS {
                get => this[ModifyFieldID.DestructableTypeIsFatLOS];
                set => this[ModifyFieldID.DestructableTypeIsFatLOS] = value;
            }
            public Modification GetDestructableTypeTextureID {
                get => this[ModifyFieldID.DestructableTypeTextureID];
                set => this[ModifyFieldID.DestructableTypeTextureID] = value;
            }
            public Modification GetDestructableTypeTextureFile {
                get => this[ModifyFieldID.DestructableTypeTextureFile];
                set => this[ModifyFieldID.DestructableTypeTextureFile] = value;
            }
            public Modification GetDestructableTypeUseClickHelper {
                get => this[ModifyFieldID.DestructableTypeUseClickHelper];
                set => this[ModifyFieldID.DestructableTypeUseClickHelper] = value;
            }
            public Modification GetDestructableTypeCanPlaceOnCliffs {
                get => this[ModifyFieldID.DestructableTypeCanPlaceOnCliffs];
                set => this[ModifyFieldID.DestructableTypeCanPlaceOnCliffs] = value;
            }
            public Modification GetDestructableTypeCanPlaceOnWater {
                get => this[ModifyFieldID.DestructableTypeCanPlaceOnWater];
                set => this[ModifyFieldID.DestructableTypeCanPlaceOnWater] = value;
            }
            public Modification GetDestructableTypeCanPlaceDead {
                get => this[ModifyFieldID.DestructableTypeCanPlaceDead];
                set => this[ModifyFieldID.DestructableTypeCanPlaceDead] = value;
            }
            public Modification GetDestructableTypeIsWalkable {
                get => this[ModifyFieldID.DestructableTypeIsWalkable];
                set => this[ModifyFieldID.DestructableTypeIsWalkable] = value;
            }
            public Modification GetDestructableTypeCliffHeight {
                get => this[ModifyFieldID.DestructableTypeCliffHeight];
                set => this[ModifyFieldID.DestructableTypeCliffHeight] = value;
            }
            public Modification GetDestructableTypeTargetType {
                get => this[ModifyFieldID.DestructableTypeTargetType];
                set => this[ModifyFieldID.DestructableTypeTargetType] = value;
            }
            public Modification GetDestructableTypeArmor {
                get => this[ModifyFieldID.DestructableTypeArmor];
                set => this[ModifyFieldID.DestructableTypeArmor] = value;
            }
            public Modification GetDestructableTypeNumVar {
                get => this[ModifyFieldID.DestructableTypeNumVar];
                set => this[ModifyFieldID.DestructableTypeNumVar] = value;
            }
            public Modification GetDestructableTypeHealth {
                get => this[ModifyFieldID.DestructableTypeHealth];
                set => this[ModifyFieldID.DestructableTypeHealth] = value;
            }
            public Modification GetDestructableTypeOcclusionHeight {
                get => this[ModifyFieldID.DestructableTypeOcclusionHeight];
                set => this[ModifyFieldID.DestructableTypeOcclusionHeight] = value;
            }
            public Modification GetDestructableTypeFlyHeight {
                get => this[ModifyFieldID.DestructableTypeFlyHeight];
                set => this[ModifyFieldID.DestructableTypeFlyHeight] = value;
            }
            public Modification GetDestructableTypeFixedRotation {
                get => this[ModifyFieldID.DestructableTypeFixedRotation];
                set => this[ModifyFieldID.DestructableTypeFixedRotation] = value;
            }
            public Modification GetDestructableTypeSelectionSize {
                get => this[ModifyFieldID.DestructableTypeSelectionSize];
                set => this[ModifyFieldID.DestructableTypeSelectionSize] = value;
            }
            public Modification GetDestructableTypeMinScale {
                get => this[ModifyFieldID.DestructableTypeMinScale];
                set => this[ModifyFieldID.DestructableTypeMinScale] = value;
            }
            public Modification GetDestructableTypeMaxScale {
                get => this[ModifyFieldID.DestructableTypeMaxScale];
                set => this[ModifyFieldID.DestructableTypeMaxScale] = value;
            }
            public Modification GetDestructableTypeCanPlaceRandScale {
                get => this[ModifyFieldID.DestructableTypeCanPlaceRandScale];
                set => this[ModifyFieldID.DestructableTypeCanPlaceRandScale] = value;
            }
            public Modification GetDestructableTypeMaxPitch {
                get => this[ModifyFieldID.DestructableTypeMaxPitch];
                set => this[ModifyFieldID.DestructableTypeMaxPitch] = value;
            }
            public Modification GetDestructableTypeMaxRoll {
                get => this[ModifyFieldID.DestructableTypeMaxRoll];
                set => this[ModifyFieldID.DestructableTypeMaxRoll] = value;
            }
            public Modification GetDestructableTypeRadius {
                get => this[ModifyFieldID.DestructableTypeRadius];
                set => this[ModifyFieldID.DestructableTypeRadius] = value;
            }
            public Modification GetDestructableTypeFogRadius {
                get => this[ModifyFieldID.DestructableTypeFogRadius];
                set => this[ModifyFieldID.DestructableTypeFogRadius] = value;
            }
            public Modification GetDestructableTypeFogVisibility {
                get => this[ModifyFieldID.DestructableTypeFogVisibility];
                set => this[ModifyFieldID.DestructableTypeFogVisibility] = value;
            }
            public Modification GetDestructableTypePathTexture {
                get => this[ModifyFieldID.DestructableTypePathTexture];
                set => this[ModifyFieldID.DestructableTypePathTexture] = value;
            }
            public Modification GetDestructableTypePathTextureDeath {
                get => this[ModifyFieldID.DestructableTypePathTextureDeath];
                set => this[ModifyFieldID.DestructableTypePathTextureDeath] = value;
            }
            public Modification GetDestructableTypeDeathSound {
                get => this[ModifyFieldID.DestructableTypeDeathSound];
                set => this[ModifyFieldID.DestructableTypeDeathSound] = value;
            }
            public Modification GetDestructableTypeShadow {
                get => this[ModifyFieldID.DestructableTypeShadow];
                set => this[ModifyFieldID.DestructableTypeShadow] = value;
            }
            public Modification GetDestructableTypeShowInMM {
                get => this[ModifyFieldID.DestructableTypeShowInMM];
                set => this[ModifyFieldID.DestructableTypeShowInMM] = value;
            }
            public Modification GetDestructableTypeMMRed {
                get => this[ModifyFieldID.DestructableTypeMMRed];
                set => this[ModifyFieldID.DestructableTypeMMRed] = value;
            }
            public Modification GetDestructableTypeMMGreen {
                get => this[ModifyFieldID.DestructableTypeMMGreen];
                set => this[ModifyFieldID.DestructableTypeMMGreen] = value;
            }
            public Modification GetDestructableTypeMMBlue {
                get => this[ModifyFieldID.DestructableTypeMMBlue];
                set => this[ModifyFieldID.DestructableTypeMMBlue] = value;
            }
            public Modification GetDestructableTypeUseMMColor {
                get => this[ModifyFieldID.DestructableTypeUseMMColor];
                set => this[ModifyFieldID.DestructableTypeUseMMColor] = value;
            }
            public Modification GetDestructableTypeBuildTime {
                get => this[ModifyFieldID.DestructableTypeBuildTime];
                set => this[ModifyFieldID.DestructableTypeBuildTime] = value;
            }
            public Modification GetDestructableTypeRepairTime {
                get => this[ModifyFieldID.DestructableTypeRepairTime];
                set => this[ModifyFieldID.DestructableTypeRepairTime] = value;
            }
            public Modification GetDestructableTypeGoldRepairCost {
                get => this[ModifyFieldID.DestructableTypeGoldRepairCost];
                set => this[ModifyFieldID.DestructableTypeGoldRepairCost] = value;
            }
            public Modification GetDestructableTypeLumberRepairCost {
                get => this[ModifyFieldID.DestructableTypeLumberRepairCost];
                set => this[ModifyFieldID.DestructableTypeLumberRepairCost] = value;
            }
            public Modification GetDestructableTypeUserList {
                get => this[ModifyFieldID.DestructableTypeUserList];
                set => this[ModifyFieldID.DestructableTypeUserList] = value;
            }
            public Modification GetDestructableTypeColorRed {
                get => this[ModifyFieldID.DestructableTypeColorRed];
                set => this[ModifyFieldID.DestructableTypeColorRed] = value;
            }
            public Modification GetDestructableTypeColorGreen {
                get => this[ModifyFieldID.DestructableTypeColorGreen];
                set => this[ModifyFieldID.DestructableTypeColorGreen] = value;
            }
            public Modification GetDestructableTypeColorBlue {
                get => this[ModifyFieldID.DestructableTypeColorBlue];
                set => this[ModifyFieldID.DestructableTypeColorBlue] = value;
            }
            public Modification GetDestructableTypeIsSelectable {
                get => this[ModifyFieldID.DestructableTypeIsSelectable];
                set => this[ModifyFieldID.DestructableTypeIsSelectable] = value;
            }
            public Modification GetDestructableTypeSelectionCircleSize {
                get => this[ModifyFieldID.DestructableTypeSelectionCircleSize];
                set => this[ModifyFieldID.DestructableTypeSelectionCircleSize] = value;
            }
            public Modification GetDestructableTypePortraitModel {
                get => this[ModifyFieldID.DestructableTypePortraitModel];
                set => this[ModifyFieldID.DestructableTypePortraitModel] = value;
            }
            #endregion
        }

        public static class ModifyFieldID
        {
            #region [    Unit Modification    ]
            public const string UnitTypeDefaultActiveAbility = "udaa";
            public const string UnitTypeHeroAbilities = "uhab";
            public const string UnitTypeAbilities = "uabi";
            public const string UnitTypeAllowCustomTeamColor = "utcc";
            public const string UnitTypeBlendTime = "uble";
            public const string UnitTypeCastBackswing = "ucbs";
            public const string UnitTypeCastPoint = "ucpt";
            public const string UnitTypeRunSpeed = "urun";
            public const string UnitTypeWalkSpeed = "uwal";
            public const string UnitTypeButtonPositionX = "ubpx";
            public const string UnitTypeButtonPositionY = "ubpy";
            public const string UnitTypeDeathTime = "udtm";
            public const string UnitTypeElevationSamplePoints = "uept";
            public const string UnitTypeElevationSampleRadius = "uerd";
            public const string UnitTypeFogOfWarSampleRadius = "ufrd";
            public const string UnitTypeGroundTexture = "uubs";
            public const string UnitTypeHasWaterShadow = "ushr";
            public const string UnitTypeIcon = "uico";
            public const string UnitTypeScoreScreenIcon = "ussi";
            public const string UnitTypeMaxPitch = "umxp";
            public const string UnitTypeMaxRoll = "umxr";
            public const string UnitTypeModel = "umdl";
            public const string UnitTypeModelExtraVersions = "uver";
            public const string UnitTypeOcculderHeight = "uocc";
            public const string UnitTypeOrientationInterpolation = "uori";
            public const string UnitTypeSwimProjectileImpactZ = "uisz";
            public const string UnitTypeProjectileImpactZ = "uimz";
            public const string UnitTypeProjectileLaunchX = "ulpx";
            public const string UnitTypeSwimProjectileLaunchZ = "ulsz";
            public const string UnitTypeProjectileLaunchZ = "ulpz";
            public const string UnitTypePropulsionWindow = "uprw";
            public const string UnitTypeRequiredAnimationNames = "uani";
            public const string UnitTypeRequiredAnimationAttachments = "uaap";
            public const string UnitTypeRequiredAnimationLinkNames = "ualp";
            public const string UnitTypeRequiredBoneNames = "ubpr";
            public const string UnitTypeScaleProjectiles = "uscb";
            public const string UnitTypeScale = "usca";
            public const string UnitTypeSelectionZ = "uslz";
            public const string UnitTypeSelectionOnWater = "usew";
            public const string UnitTypeSelectionScale = "ussc";
            public const string UnitTypeShadowImage = "ushu";
            public const string UnitTypeShadowImageCenterX = "ushx";
            public const string UnitTypeShadowImageCenterY = "ushy";
            public const string UnitTypeShadowImageHeight = "ushh";
            public const string UnitTypeShadowImageWidth = "ushw";
            public const string UnitTypeShadowTexture = "ushb";
            public const string UnitTypeSpecialArt = "uspa";
            public const string UnitTypeTargetArt = "utaa";
            public const string UnitTypeTeamColor = "utco";
            public const string UnitTypeRedTint = "uclr";
            public const string UnitTypeGreenTint = "uclg";
            public const string UnitTypeBlueTint = "uclb";
            public const string UnitTypeUseExtendedLineOfSight = "ulos";
            public const string UnitTypeAcquisitionRange = "uacq";
            public const string UnitTypeArmorType = "uarm";
            public const string UnitTypeBackswingPoint1 = "ubs1";
            public const string UnitTypeDamagePoint1 = "udp1";
            public const string UnitTypeAreaOfEffectFull1 = "ua1f";
            public const string UnitTypeAreaOfEffectMedium1 = "ua1h";
            public const string UnitTypeAreaOfEffectSmall1 = "ua1q";
            public const string UnitTypeAreaOfEffectTargets1 = "ua1p";
            public const string UnitTypeAttackType1 = "ua1t";
            public const string UnitTypeCooldown1 = "ua1c";
            public const string UnitTypeDamageBase1 = "ua1b";
            public const string UnitTypeDamageFactorMedium1 = "uhd1";
            public const string UnitTypeDamageFactorSmall1 = "uqd1";
            public const string UnitTypeDamageLossFactor1 = "udl1";
            public const string UnitTypeDamageNumberOfDice1 = "ua1d";
            public const string UnitTypeDamageSidesPerDie1 = "ua1s";
            public const string UnitTypeDamageSpillDistance1 = "usd1";
            public const string UnitTypeDamageSpillRadius1 = "usr1";
            public const string UnitTypeDamageUpgradeAmount1 = "udu1";
            public const string UnitTypeMaximumTargets1 = "utc1";
            public const string UnitTypeProjectileArc1 = "uma1";
            public const string UnitTypeProjectileArt1 = "ua1m";
            public const string UnitTypeProjectileHoming1 = "umh1";
            public const string UnitTypeProjectileSpeed1 = "ua1z";
            public const string UnitTypeRange1 = "ua1r";
            public const string UnitTypeRangeMotionBuffer1 = "urb1";
            public const string UnitTypeShowUI1 = "uwu1";
            public const string UnitTypeTargetsAllowed1 = "ua1g";
            public const string UnitTypeWeaponSound1 = "ucs1";
            public const string UnitTypeWeaponType1 = "ua1w";
            public const string UnitTypeBackswingPoint2 = "ubs2";
            public const string UnitTypeDamagePoint2 = "udp2";
            public const string UnitTypeAreaOfEffectFull2 = "ua2f";
            public const string UnitTypeAreaOfEffectMedium2 = "ua2h";
            public const string UnitTypeAreaOfEffectSmall2 = "ua2q";
            public const string UnitTypeAreaOfEffectTargets2 = "ua2p";
            public const string UnitTypeAttackType2 = "ua2t";
            public const string UnitTypeCooldown2 = "ua2c";
            public const string UnitTypeDamageBase2 = "ua2b";
            public const string UnitTypeDamageFactorMedium2 = "uhd2";
            public const string UnitTypeDamageFactorSmall2 = "uqd2";
            public const string UnitTypeDamageLossFactor2 = "udl2";
            public const string UnitTypeDamageNumberOfDice2 = "ua2d";
            public const string UnitTypeDamageSidesPerDie2 = "ua2s";
            public const string UnitTypeDamageSpillDistance2 = "usd2";
            public const string UnitTypeDamageSpillRadius2 = "usr2";
            public const string UnitTypeDamageUpgradeAmount2 = "udu2";
            public const string UnitTypeMaximumTargets2 = "utc2";
            public const string UnitTypeProjectileArc2 = "uma2";
            public const string UnitTypeProjectileArt2 = "ua2m";
            public const string UnitTypeProjectileHoming2 = "umh2";
            public const string UnitTypeProjectileSpeed2 = "ua2z";
            public const string UnitTypeRange2 = "ua2r";
            public const string UnitTypeRangeMotionBuffer2 = "urb2";
            public const string UnitTypeShowUI2 = "uwu2";
            public const string UnitTypeTargetsAllowed2 = "ua2g";
            public const string UnitTypeWeaponSound2 = "ucs2";
            public const string UnitTypeWeaponType2 = "ua2w";
            public const string UnitTypeAttacksEnabled = "uaen";
            public const string UnitTypeDeathType = "udea";
            public const string UnitTypeDefenseBase = "udef";
            public const string UnitTypeDefenseType = "udty";
            public const string UnitTypeDefenseUpgradeBonus = "udup";
            public const string UnitTypeMinimumAttackRange = "uamn";
            public const string UnitTypeTargetedAs = "utar";
            public const string UnitTypeDropItemsOnDeath = "udro";
            public const string UnitTypeCategoryCampaign = "ucam";
            public const string UnitTypeCategorySpecial = "uspe";
            public const string UnitTypeDisplayAsNeutralHostile = "uhos";
            public const string UnitTypeHasTilesetSpecificData = "utss";
            public const string UnitTypePlaceableInEditor = "uine";
            public const string UnitTypeTilesets = "util";
            public const string UnitTypeUseClickHelper = "uuch";
            public const string UnitTypeGroupSeparationEnabled = "urpo";
            public const string UnitTypeGroupSeparationGroupNumber = "urpg";
            public const string UnitTypeGroupSeparationParameter = "urpp";
            public const string UnitTypeGroupSeparationPriority = "urpr";
            public const string UnitTypeFlyHeight = "umvh";
            public const string UnitTypeMinimumHeight = "umvf";
            public const string UnitTypeSpeedBase = "umvs";
            public const string UnitTypeSpeedMaximum = "umas";
            public const string UnitTypeSpeedMinimum = "umis";
            public const string UnitTypeTurnRate = "umvr";
            public const string UnitTypeMoveType = "umvt";
            public const string UnitTypeAIPlacementRadius = "uabr";
            public const string UnitTypeAIPlacementType = "uabt";
            public const string UnitTypeCollisionSize = "ucol";
            public const string UnitTypePathingMap = "upat";
            public const string UnitTypePlacementPreventedBy = "upar";
            public const string UnitTypePlacementRequires = "upap";
            public const string UnitTypePlacementRequiresWaterRadius = "upaw";
            public const string UnitTypeBuildSound = "ubsl";
            public const string UnitTypeSoundLoopFadeInRate = "ulfi";
            public const string UnitTypeSoundLoopFadeOutRate = "ulfo";
            public const string UnitTypeMoveSound = "umsl";
            public const string UnitTypeRandomSound = "ursl";
            public const string UnitTypeSoundSet = "usnd";
            public const string UnitTypeAgilityPerLevel = "uagp";
            public const string UnitTypeBuildTime = "ubld";
            public const string UnitTypeCanBeBuiltOn = "uibo";
            public const string UnitTypeCanBuildOn = "ucbo";
            public const string UnitTypeCanFlee = "ufle";
            public const string UnitTypeFoodCost = "ufoo";
            public const string UnitTypeFoodProduced = "ufma";
            public const string UnitTypeFormationRank = "ufor";
            public const string UnitTypeGoldBountyBase = "ubba";
            public const string UnitTypeGoldBountyNumberOfDice = "ubdi";
            public const string UnitTypeGoldBountySidesPerDie = "ubsi";
            public const string UnitTypeGoldCost = "ugol";
            public const string UnitTypeHideHeroDeathMessage = "uhhd";
            public const string UnitTypeHideHeroInterfaceIcon = "uhhb";
            public const string UnitTypeHideHeroMinimapDisplay = "uhhm";
            public const string UnitTypeHideMinimapDisplay = "uhom";
            public const string UnitTypeHitPointsMaximum = "uhpm";
            public const string UnitTypeHitPointsRegeneration = "uhpr";
            public const string UnitTypeHitPointsRegenerationType = "uhrt";
            public const string UnitTypeIntelligencePerLevel = "uinp";
            public const string UnitTypeIsABuilding = "ubdg";
            public const string UnitTypeLevel = "ulev";
            public const string UnitTypeLumberBountyBase = "ulba";
            public const string UnitTypeLumberBountyNumberOfDice = "ulbd";
            public const string UnitTypeLumberBountySidesPerDie = "ulbs";
            public const string UnitTypeLumberCost = "ulum";
            public const string UnitTypeManaInitialAmount = "umpi";
            public const string UnitTypeManaMaximum = "umpm";
            public const string UnitTypeManaRegeneration = "umpr";
            public const string UnitTypeShowNeutralBuildingIcon = "unbm";
            public const string UnitTypeValidAsRandomNeutralBuilding = "unbr";
            public const string UnitTypePointValue = "upoi";
            public const string UnitTypePrimaryAttribute = "upra";
            public const string UnitTypePriority = "upri";
            public const string UnitTypeRace = "urac";
            public const string UnitTypeRepairGoldCost = "ugor";
            public const string UnitTypeRepairLumberCost = "ulur";
            public const string UnitTypeRepairTime = "urtm";
            public const string UnitTypeSightRadiusDay = "usid";
            public const string UnitTypeSightRadiusNight = "usin";
            public const string UnitTypeSleeps = "usle";
            public const string UnitTypeStartingAgility = "uagi";
            public const string UnitTypeStartingIntelligence = "uint";
            public const string UnitTypeStartingStrength = "ustr";
            public const string UnitTypeStockMaximum = "usma";
            public const string UnitTypeStockReplenishInterval = "usrg";
            public const string UnitTypeStockStartDelay = "usst";
            public const string UnitTypeStrengthPerLevel = "ustp";
            public const string UnitTypeTransportedSize = "ucar";
            public const string UnitTypeUnitClassification = "utyp";
            public const string UnitTypeDependencyEquivalents = "udep";
            public const string UnitTypeHeroRevivalLocations = "urva";
            public const string UnitTypeItemsMade = "umki";
            public const string UnitTypeItemsSold = "usei";
            public const string UnitTypeRequirements = "ureq";
            public const string UnitTypeRequirementsLevels = "urqa";
            public const string UnitTypeRequirementsTier2 = "urq1";
            public const string UnitTypeRequirementsTier3 = "urq2";
            public const string UnitTypeRequirementsTier4 = "urq3";
            public const string UnitTypeRequirementsTier5 = "urq4";
            public const string UnitTypeRequirementsTier6 = "urq5";
            public const string UnitTypeRequirementsTier7 = "urq6";
            public const string UnitTypeRequirementsTier8 = "urq7";
            public const string UnitTypeRequirementsTier9 = "urq8";
            public const string UnitTypeRequirementsTiersUsed = "urqc";
            public const string UnitTypeStructuresBuilt = "ubui";
            public const string UnitTypeResearchesAvailable = "ures";
            public const string UnitTypeRevivesDeadHeroes = "urev";
            public const string UnitTypeUnitsSold = "useu";
            public const string UnitTypeUnitsTrained = "utra";
            public const string UnitTypeUpgradesTo = "uupt";
            public const string UnitTypeUpgradesUsed = "upgr";
            public const string UnitTypeDescription = "ides";
            public const string UnitTypeHotkey = "uhot";
            public const string UnitTypeName = "unam";
            public const string UnitTypeNameEditorSuffix = "unsf";
            public const string UnitTypeProperNames = "upro";
            public const string UnitTypeProperNamesUsed = "upru";
            public const string UnitTypeAwakenTooltip = "uawt";
            public const string UnitTypeTooltip = "utip";
            public const string UnitTypeUbertip = "utub";
            public const string UnitTypeReviveTooltip = "utpr";
            #endregion

            #region [    Item Modification    ]
            public const string ItemTypeAbilities = "iabi";
            public const string ItemTypeArmorType = "iarm";
            public const string ItemTypeIcon = "iico";
            public const string ItemTypeButtonPositionX = "ubpx";
            public const string ItemTypeButtonPositionY = "ubpy";
            public const string ItemTypeClass = "icla";
            public const string ItemTypeColorBlue = "iclb";
            public const string ItemTypeColorGreen = "iclg";
            public const string ItemTypeColorRed = "iclr";
            public const string ItemTypeCooldownGroup = "icid";
            public const string ItemTypeDescription = "ides";
            public const string ItemTypeDropsOnDeath = "idrp";
            public const string ItemTypeDroppable = "idro";
            public const string ItemTypeModel = "ifil";
            public const string ItemTypeGoldCost = "igol";
            public const string ItemTypeHotkey = "uhot";
            public const string ItemTypeHealth = "ihtp";
            public const string ItemTypeIgnoreCooldown = "iicd";
            public const string ItemTypeLevel = "ilev";
            public const string ItemTypeLumberCost = "ilum";
            public const string ItemTypeIsTransformable = "imor";
            public const string ItemTypeName = "unam";
            public const string ItemTypeUnclassifiedLevel = "ilvo";
            public const string ItemTypeIsPawnable = "ipaw";
            public const string ItemTypeIsPerishable = "iper";
            public const string ItemTypeIsRandomChoice = "iprn";
            public const string ItemTypeIsPowerup = "ipow";
            public const string ItemTypePriority = "ipri";
            public const string ItemTypeRequirements = "ureq";
            public const string ItemTypeRequiredLevels = "urqa";
            public const string ItemTypeScale = "isca";
            public const string ItemTypeIsSellable = "isel";
            public const string ItemTypeSelectionSize = "issc";
            public const string ItemTypeMaximumStock = "isto";
            public const string ItemTypeStockReplenishInterval = "istr";
            public const string ItemTypeStartingStock = "isst";
            public const string ItemTypeTooltip = "utip";
            public const string ItemTypeUbertip = "utub";
            public const string ItemTypeIsUsable = "iusa";
            public const string ItemTypeUses = "iuse";
            #endregion

            #region [    Destructable  Modification    ]
            public const string DestructableTypeName = "bnam";
            public const string DestructableTypeEditorSuffix = "bsuf";
            public const string DestructableTypeCategory = "bcat";
            public const string DestructableTypeTilesets = "btil";
            public const string DestructableTypeIsTilesetSpecific = "btsp";
            public const string DestructableTypeFile = "bfil";
            public const string DestructableTypeIsLightweight = "blit";
            public const string DestructableTypeIsFatLOS = "bflo";
            public const string DestructableTypeTextureID = "btxi";
            public const string DestructableTypeTextureFile = "btxf";
            public const string DestructableTypeUseClickHelper = "buch";
            public const string DestructableTypeCanPlaceOnCliffs = "bonc";
            public const string DestructableTypeCanPlaceOnWater = "bonw";
            public const string DestructableTypeCanPlaceDead = "bcpd";
            public const string DestructableTypeIsWalkable = "bwal";
            public const string DestructableTypeCliffHeight = "bclh";
            public const string DestructableTypeTargetType = "btar";
            public const string DestructableTypeArmor = "barm";
            public const string DestructableTypeNumVar = "bvar";
            public const string DestructableTypeHealth = "bhps";
            public const string DestructableTypeOcclusionHeight = "boch";
            public const string DestructableTypeFlyHeight = "bflh";
            public const string DestructableTypeFixedRotation = "bfxr";
            public const string DestructableTypeSelectionSize = "bsel";
            public const string DestructableTypeMinScale = "bmis";
            public const string DestructableTypeMaxScale = "bmas";
            public const string DestructableTypeCanPlaceRandScale = "bcpr";
            public const string DestructableTypeMaxPitch = "bmap";
            public const string DestructableTypeMaxRoll = "bmar";
            public const string DestructableTypeRadius = "brad";
            public const string DestructableTypeFogRadius = "bfra";
            public const string DestructableTypeFogVisibility = "bfvi";
            public const string DestructableTypePathTexture = "bptx";
            public const string DestructableTypePathTextureDeath = "bptd";
            public const string DestructableTypeDeathSound = "bdsn";
            public const string DestructableTypeShadow = "bshd";
            public const string DestructableTypeShowInMM = "bsmm";
            public const string DestructableTypeMMRed = "bmmr";
            public const string DestructableTypeMMGreen = "bmmg";
            public const string DestructableTypeMMBlue = "bmmb";
            public const string DestructableTypeUseMMColor = "bumm";
            public const string DestructableTypeBuildTime = "bbut";
            public const string DestructableTypeRepairTime = "bret";
            public const string DestructableTypeGoldRepairCost = "breg";
            public const string DestructableTypeLumberRepairCost = "brel";
            public const string DestructableTypeUserList = "busr";
            public const string DestructableTypeColorRed = "bvcr";
            public const string DestructableTypeColorGreen = "bvcg";
            public const string DestructableTypeColorBlue = "bvcb";
            public const string DestructableTypeIsSelectable = "bgse";
            public const string DestructableTypeSelectionCircleSize = "bgsc";
            public const string DestructableTypePortraitModel = "bgpm";
            #endregion
        }

        public class Modification
        {
            public int ModifyID;    // RawCode
            public int VarType;
            public int LevelVar;
            public int DataPointer;
            public int VarInt;
            public float VarFloat;
            public string VarString = null;
            public int[] VarIntArray = null;
            public int OriginID;    // RawCode
        }
        public static W3Object Parse(byte[] data, string extention)
        {
            switch (extention.ToLower().Replace(".",string.Empty))
            {
                case "w3b":
                case "w3h":
                case "w3t":
                case "w3u":
                    return Parse(data, false);
                case "w3a":
                case "w3d":
                case "w3q":
                    return Parse(data, true);
                default:
                    throw new Exception("지원하지 않는 확장자입니다.");
            }
        }
        public static W3Object Parse(byte[] data, bool isExtended)
        {
            W3Object w3o = new W3Object(isExtended);
            using (ByteStream bs = new ByteStream(data))
            {
                w3o.FileVersion = bs.ReadInt32();
                for (int table = 0; table < 2; table++)
                {
                    int LoopCount = bs.ReadInt32();
                    Table t = null;
                    switch (table)
                    {
                        case 0: t = new OriginTable(); break;
                        case 1: t = new CustomTable(); break;
                    }
                    for (int i = 0; i < LoopCount; i++)
                    {
                        Defination def = new Defination
                        {
                            OriginID = bs.ReadInt32().ReverseByte(),
                            NewID = bs.ReadInt32().ReverseByte()
                        };
                        int defCount = bs.ReadInt32();
                        for (int j = 0; j < defCount; j++)
                        {
                            Modification mod = new Modification
                            {
                                ModifyID = bs.ReadInt32().ReverseByte(),
                                VarType = bs.ReadInt32()
                            };
                            if (isExtended)
                            {
                                mod.LevelVar = bs.ReadInt32();
                                mod.DataPointer = bs.ReadInt32();
                            }
                            switch (mod.VarType)
                            {
                                case 0:
                                    mod.VarInt = bs.ReadInt32();
                                    break;
                                case 1:
                                case 2:
                                    mod.VarFloat = bs.ReadSingle();
                                    break;
                                case 3:
                                    string value = bs.ReadString();
                                    int[] list = value.GetNumberizeList();
                                    if (list == null) mod.VarString = value;
                                    else mod.VarIntArray = list;
                                    break;
                            }
                            mod.OriginID = bs.ReadInt32().ReverseByte();
                            def.Add(mod);
                        }
                        t.Add(def);
                    }
                    switch (table)
                    {
                        case 0: w3o.Origin = (OriginTable)t; break;
                        case 1: w3o.Custom = (CustomTable)t; break;
                    }
                }
            }
            return w3o;
        }

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(FileVersion);
                Table table = null;
                for (int i = 0; i < 2; i++)
                {
                    switch (i)
                    {
                        case 0: table = Origin; break;
                        case 1: table = Custom; break;
                    }
                    if (table == null) bs.Write(0);
                    else
                    {
                        bs.Write(table.Count);
                        foreach (var item in table)
                        {
                            bs.Write(item.OriginID.ReverseByte());
                            bs.Write(item.NewID.ReverseByte());
                            bs.Write(item.Count);
                            foreach (var initem in item)
                            {
                                bs.Write(initem.ModifyID.ReverseByte());
                                bs.Write(initem.VarType);
                                if (IsExtended)
                                {
                                    bs.Write(initem.LevelVar);
                                    bs.Write(initem.DataPointer);
                                }
                                switch (initem.VarType)
                                {
                                    case 0:
                                        bs.Write(initem.VarInt);
                                        break;
                                    case 1:
                                    case 2:
                                        bs.Write(initem.VarFloat);
                                        break;
                                    case 3:
                                        if (initem.VarString == null)
                                        {
                                            int[] RawCodes = initem.VarIntArray;
                                            bs.Write(RawCodes.GetRawCodeList());
                                            bs.WriteByte(0);
                                        }
                                        else bs.Write(initem.VarString);
                                        break;
                                }
                                bs.Write(initem.OriginID.ReverseByte());
                            }
                        }
                    }
                }
                return bs.ToArray();
            }
        }
    }
}
