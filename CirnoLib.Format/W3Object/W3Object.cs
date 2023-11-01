using System;
using System.Collections.Generic;

using CirnoLib.Jass;

namespace CirnoLib.Format
{
    [Serializable]
    public class W3Object : IArrayable
    {
        public int FileVersion = 2;
        public OriginTable Origin = new OriginTable();
        public CustomTable Custom = new CustomTable();
        /// <summary>
        /// w3b, w3h, w3t, w3u = false / w3a, w3d, w3q = true
        /// </summary>
        public readonly bool IsExtended;
        public W3Object(bool IsExtended) => this.IsExtended = IsExtended;

        public abstract class Table : List<Defination>
        {
            public abstract Defination this[string ID] { get; set; }
            public abstract Defination this[byte[] ID] { get; set; }

            public virtual Defination AddNew(int OriginID, int NewID)
            {
                var item = new Defination(OriginID, NewID);
                Add(item);
                return item;
            }

            public virtual Defination AddNew(string OriginID, string NewID)
            {
                var item = new Defination(OriginID, NewID);
                Add(item);
                return item;
            }
        }

        public class OriginTable : Table
        {
            public override Defination this[string ID] {
                get => Find(item => item.OriginID == ID.Numberize());
                set {
                    int index = FindIndex(item => item.OriginID == ID.Numberize());
                    if (index == -1)
                        Add(value);
                    else
                        this[index] = value;
                }
            }
            public override Defination this[byte[] ID] {
                get => Find(item => item.OriginID.GetRawCode().Compare(ID));
                set {
                    int index = FindIndex(item => item.OriginID.GetRawCode().Compare(ID));
                    if (index == -1)
                        Add(value);
                    else
                        this[index] = value;
                }
            }
        }

        public class CustomTable : Table
        {
            public override Defination this[string ID] {
                get => Find(item => item.NewID == ID.Numberize());
                set {
                    int index = FindIndex(item => item.NewID == ID.Numberize());
                    if (index == -1)
                        Add(value);
                    else
                        this[index] = value;
                }
            }
            public override Defination this[byte[] ID] {
                get => Find(item => item.NewID.GetRawCode().Compare(ID));
                set {
                    int index = FindIndex(item => item.NewID.GetRawCode().Compare(ID));
                    if (index == -1)
                        Add(value);
                    else
                        this[index] = value;
                }
            }

            public const string NameSequence = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            public int GetNewID(char Prefix, int start = 0)
            {
                char upper = char.ToUpper(Prefix);
                char lower = char.ToLower(Prefix);
                for (int i = start; i < 8192; i++)
                {
                    string index = GetRawCode(i);
                    int UpID = (upper + index).Numberize();
                    int LowID = (lower + index).Numberize();
                    if (FindIndex(item => item.NewID == UpID || item.NewID == LowID) == -1)
                        return (Prefix + index).Numberize();
                }
                return 0;
            }

            public int GetSequence(string RawCode)
            {
                int result = 0, Length = NameSequence.Length;
                foreach (var item in RawCode.Substring(1))
                {
                    int index = NameSequence.IndexOf(item);
                    result *= Length;
                    result += index == -1 ? Length : index;
                }
                return result;
            }

            public string GetRawCode(int Sequence)
            {
                string result = string.Empty;
                for (int Length = NameSequence.Length; Sequence > 0; Sequence /= Length)
                    result = NameSequence[Sequence % Length] + result;
                return result.PadLeft(3, NameSequence[0]);
            }

            public Defination AddNewByPrefix(string OriginID, char Prefix, int start = 0)
            {
                int ID = GetNewID(Prefix, start);
                if (ID == 0) return null;
                return AddNew(OriginID.Numberize(), ID);
            }
        }

        public class Defination : List<Modification>
        {
            public int OriginID;    // RawCode
            public int NewID;       // RawCode

            public Defination(int OriginID, int NewID)
            {
                this.OriginID = OriginID;
                this.NewID = NewID;
            }
            public Defination(string OriginID, string NewID)
            {
                this.OriginID = OriginID.Numberize();
                this.NewID = NewID.Numberize();
            }

            public Modification this[string ModifyID]
            {
                get => Find(item => item.ModifyID == ModifyID.Numberize());
                set {
                    int index = FindIndex(item => item.ModifyID == ModifyID.Numberize());
                    if (index == -1) Add(value);
                    else this[index] = value;
                }
            }
            public Modification this[byte[] ModifyID]
            {
                get => Find(item => item.ModifyID.GetRawCode().Compare(ModifyID));
                set {
                    int index = FindIndex(item => item.ModifyID.GetRawCode().Compare(ModifyID));
                    if (index == -1) Add(value);
                    else this[index] = value;
                }
            }

            public Modification SetValue(string ModifyID, bool? value)
            {
                var modify = this[ModifyID];
                if (modify == null)
                {
                    if (value != null)
                    {
                        modify = new Modification
                        {
                            ModifyID = ModifyID.Numberize(),
                            VarType = 0
                        };
                        Add(modify);
                    }
                    else
                        return null;
                }
                else if (value == null)
                {
                    Remove(modify);
                    return null;
                }
                modify.VarInt = value.Value ? 1 : 0;
                return modify;
            }

            public Modification SetValue(string ModifyID, int? value)
            {
                var modify = this[ModifyID];
                if (modify == null)
                {
                    if (value != null)
                    {
                        modify = new Modification
                        {
                            ModifyID = ModifyID.Numberize(),
                            VarType = 0
                        };
                        Add(modify);
                    }
                    else
                        return null;
                }
                else if (value == null)
                {
                    Remove(modify);
                    return null;
                }
                modify.VarInt = value.Value;
                return modify;
            }

            public Modification SetValue(string ModifyID, float? value, bool unreal)
            {
                var modify = this[ModifyID];
                if (modify == null)
                {
                    if (value != null)
                    {
                        modify = new Modification
                        {
                            ModifyID = ModifyID.Numberize(),
                            VarType = unreal ? 2 : 1
                        };
                        Add(modify);
                    }
                    else
                        return null;
                }
                else if (value == null)
                {
                    Remove(modify);
                    return null;
                }
                modify.VarFloat = value.Value;
                return modify;
            }

            public Modification SetValue(string ModifyID, string value)
            {
                var modify = this[ModifyID];
                if (modify == null)
                {
                    if (value != null)
                    {
                        modify = new Modification
                        {
                            ModifyID = ModifyID.Numberize(),
                            VarType = 3
                        };
                        Add(modify);
                    }
                    else
                        return null;
                }
                else if (value == null)
                {
                    Remove(modify);
                    return null;
                }
                modify.VarString = value;
                return modify;
            }

            public Modification SetValue(string ModifyID, List<int> value)
            {
                var modify = this[ModifyID];
                if (modify == null)
                {
                    if (value != null)
                    {
                        modify = new Modification
                        {
                            ModifyID = ModifyID.Numberize(),
                            VarType = 3
                        };
                        Add(modify);
                    }
                    else
                        return null;
                }
                else if (value == null)
                {
                    Remove(modify);
                    return null;
                }
                if (modify.VarIntArray == null)
                    modify.VarIntArray = new List<int>();
                else
                    modify.VarIntArray.Clear();
                modify.VarIntArray.AddRange(value);
                return modify;
            }

            #region [    Unit Modification    ]
            /// <summary>능력(B) - 기본 활성 능력</summary>
            public int? UnitTypeDefaultActiveAbility
            {
                get => this[ModifyFieldID.UnitTypeDefaultActiveAbility]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeDefaultActiveAbility, value);
            }
            /// <summary>능력(B) - 영웅</summary>
            public List<int> UnitTypeHeroAbilities
            {
                get => this[ModifyFieldID.UnitTypeHeroAbilities]?.VarIntArray;
                set => SetValue(ModifyFieldID.UnitTypeHeroAbilities, value);
            }
            /// <summary>능력(B) - 일반</summary>
            public List<int> UnitTypeAbilities
            {
                get => this[ModifyFieldID.UnitTypeAbilities]?.VarIntArray;
                set => SetValue(ModifyFieldID.UnitTypeAbilities, value);
            }
            public bool? UnitTypeAllowCustomTeamColor
            {
                get {
                    int? value = this[ModifyFieldID.UnitTypeAllowCustomTeamColor]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.UnitTypeAllowCustomTeamColor, value);
            }
            public float? UnitTypeBlendTime
            {
                get => this[ModifyFieldID.UnitTypeBlendTime]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeBlendTime, value, false);
            }
            public float? UnitTypeCastBackswing
            {
                get => this[ModifyFieldID.UnitTypeCastBackswing]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeCastBackswing, value, true);
            }
            public float? UnitTypeCastPoint
            {
                get => this[ModifyFieldID.UnitTypeCastPoint]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeCastPoint, value, true);
            }
            public float? UnitTypeRunSpeed
            {
                get => this[ModifyFieldID.UnitTypeRunSpeed]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeRunSpeed, value, false);
            }
            public float? UnitTypeWalkSpeed
            {
                get => this[ModifyFieldID.UnitTypeWalkSpeed]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeWalkSpeed, value, false);
            }
            public int? UnitTypeButtonPositionX
            {
                get => this[ModifyFieldID.UnitTypeButtonPositionX]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeButtonPositionX, value);
            }
            public int? UnitTypeButtonPositionY
            {
                get => this[ModifyFieldID.UnitTypeButtonPositionY]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeButtonPositionY, value);
            }
            public float? UnitTypeDeathTime
            {
                get => this[ModifyFieldID.UnitTypeDeathTime]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeDeathTime, value, true);
            }
            public int? UnitTypeElevationSamplePoints
            {
                get => this[ModifyFieldID.UnitTypeElevationSamplePoints]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeElevationSamplePoints, value);
            }
            public float? UnitTypeElevationSampleRadius
            {
                get => this[ModifyFieldID.UnitTypeElevationSampleRadius]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeElevationSampleRadius, value, false);
            }
            public float? UnitTypeFogOfWarSampleRadius
            {
                get => this[ModifyFieldID.UnitTypeFogOfWarSampleRadius]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeFogOfWarSampleRadius, value, false);
            }
            public int? UnitTypeGroundTexture
            {
                get => this[ModifyFieldID.UnitTypeGroundTexture]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeGroundTexture, value);
            }
            public bool? UnitTypeHasWaterShadow
            {
                get {
                    int? value = this[ModifyFieldID.UnitTypeHasWaterShadow]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.UnitTypeHasWaterShadow, value);
            }
            public string UnitTypeIcon
            {
                get => this[ModifyFieldID.UnitTypeIcon]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeIcon, value);
            }
            public string UnitTypeScoreScreenIcon
            {
                get => this[ModifyFieldID.UnitTypeScoreScreenIcon]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeScoreScreenIcon, value);
            }
            public float? UnitTypeMaxPitch
            {
                get => this[ModifyFieldID.UnitTypeMaxPitch]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeMaxPitch, value, false);
            }
            public float? UnitTypeMaxRoll
            {
                get => this[ModifyFieldID.UnitTypeMaxRoll]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeMaxRoll, value, false);
            }
            public string UnitTypeModel
            {
                get => this[ModifyFieldID.UnitTypeModel]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeModel, value);
            }
            //public Modification UnitTypeModelExtraVersions {
            //    get => this[ModifyFieldID.UnitTypeModelExtraVersions];
            //    set => SetValue(ModifyFieldID.UnitTypeModelExtraVersions, value);
            //}
            //public Modification UnitTypeOcculderHeight {
            //    get => this[ModifyFieldID.UnitTypeOcculderHeight];
            //    set => SetValue(ModifyFieldID.UnitTypeOcculderHeight, value);
            //}
            //public Modification UnitTypeOrientationInterpolation {
            //    get => this[ModifyFieldID.UnitTypeOrientationInterpolation];
            //    set => SetValue(ModifyFieldID.UnitTypeOrientationInterpolation, value);
            //}
            //public Modification UnitTypeSwimProjectileImpactZ {
            //    get => this[ModifyFieldID.UnitTypeSwimProjectileImpactZ];
            //    set => SetValue(ModifyFieldID.UnitTypeSwimProjectileImpactZ, value);
            //}
            //public Modification UnitTypeProjectileImpactZ {
            //    get => this[ModifyFieldID.UnitTypeProjectileImpactZ];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileImpactZ, value);
            //}
            //public Modification UnitTypeProjectileLaunchX {
            //    get => this[ModifyFieldID.UnitTypeProjectileLaunchX];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileLaunchX, value);
            //}
            //public Modification UnitTypeSwimProjectileLaunchZ {
            //    get => this[ModifyFieldID.UnitTypeSwimProjectileLaunchZ];
            //    set => SetValue(ModifyFieldID.UnitTypeSwimProjectileLaunchZ, value);
            //}
            //public Modification UnitTypeProjectileLaunchZ {
            //    get => this[ModifyFieldID.UnitTypeProjectileLaunchZ];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileLaunchZ, value);
            //}
            //public Modification UnitTypePropulsionWindow {
            //    get => this[ModifyFieldID.UnitTypePropulsionWindow];
            //    set => SetValue(ModifyFieldID.UnitTypePropulsionWindow, value);
            //}
            public string UnitTypeRequiredAnimationNames
            {
                get => this[ModifyFieldID.UnitTypeRequiredAnimationNames]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeRequiredAnimationNames, value);
            }
            //public Modification UnitTypeRequiredAnimationAttachments {
            //    get => this[ModifyFieldID.UnitTypeRequiredAnimationAttachments];
            //    set => SetValue(ModifyFieldID.UnitTypeRequiredAnimationAttachments, value);
            //}
            //public Modification UnitTypeRequiredAnimationLinkNames {
            //    get => this[ModifyFieldID.UnitTypeRequiredAnimationLinkNames];
            //    set => SetValue(ModifyFieldID.UnitTypeRequiredAnimationLinkNames, value);
            //}
            //public Modification UnitTypeRequiredBoneNames {
            //    get => this[ModifyFieldID.UnitTypeRequiredBoneNames];
            //    set => SetValue(ModifyFieldID.UnitTypeRequiredBoneNames, value);
            //}
            //public Modification UnitTypeScaleProjectiles {
            //    get => this[ModifyFieldID.UnitTypeScaleProjectiles];
            //    set => SetValue(ModifyFieldID.UnitTypeScaleProjectiles, value);
            //}
            public float? UnitTypeScale
            {
                get => this[ModifyFieldID.UnitTypeScale]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeScale, value, false);
            }
            //public Modification UnitTypeSelectionZ {
            //    get => this[ModifyFieldID.UnitTypeSelectionZ];
            //    set => SetValue(ModifyFieldID.UnitTypeSelectionZ, value);
            //}
            //public Modification UnitTypeSelectionOnWater {
            //    get => this[ModifyFieldID.UnitTypeSelectionOnWater];
            //    set => SetValue(ModifyFieldID.UnitTypeSelectionOnWater, value);
            //}
            //public Modification UnitTypeSelectionScale {
            //    get => this[ModifyFieldID.UnitTypeSelectionScale];
            //    set => SetValue(ModifyFieldID.UnitTypeSelectionScale, value);
            //}
            //public Modification UnitTypeShadowImage {
            //    get => this[ModifyFieldID.UnitTypeShadowImage];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowImage, value);
            //}
            //public Modification UnitTypeShadowImageCenterX {
            //    get => this[ModifyFieldID.UnitTypeShadowImageCenterX];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowImageCenterX, value);
            //}
            //public Modification UnitTypeShadowImageCenterY {
            //    get => this[ModifyFieldID.UnitTypeShadowImageCenterY];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowImageCenterY, value);
            //}
            //public Modification UnitTypeShadowImageHeight {
            //    get => this[ModifyFieldID.UnitTypeShadowImageHeight];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowImageHeight, value);
            //}
            //public Modification UnitTypeShadowImageWidth {
            //    get => this[ModifyFieldID.UnitTypeShadowImageWidth];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowImageWidth, value);
            //}
            //public Modification UnitTypeShadowTexture {
            //    get => this[ModifyFieldID.UnitTypeShadowTexture];
            //    set => SetValue(ModifyFieldID.UnitTypeShadowTexture, value);
            //}
            public string UnitTypeSpecialArt
            {
                get => this[ModifyFieldID.UnitTypeSpecialArt]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeSpecialArt, value);
            }
            //public Modification UnitTypeTargetArt {
            //    get => this[ModifyFieldID.UnitTypeTargetArt];
            //    set => SetValue(ModifyFieldID.UnitTypeTargetArt, value);
            //}
            //public Modification UnitTypeTeamColor {
            //    get => this[ModifyFieldID.UnitTypeTeamColor];
            //    set => SetValue(ModifyFieldID.UnitTypeTeamColor, value);
            //}
            public int? UnitTypeRedTint
            {
                get => this[ModifyFieldID.UnitTypeRedTint]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeRedTint, value);
            }
            public int? UnitTypeGreenTint
            {
                get => this[ModifyFieldID.UnitTypeGreenTint]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeGreenTint, value);
            }
            public int? UnitTypeBlueTint
            {
                get => this[ModifyFieldID.UnitTypeBlueTint]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeBlueTint, value);
            }
            //public Modification UnitTypeUseExtendedLineOfSight {
            //    get => this[ModifyFieldID.UnitTypeUseExtendedLineOfSight];
            //    set => SetValue(ModifyFieldID.UnitTypeUseExtendedLineOfSight, value);
            //}
            public float? UnitTypeAcquisitionRange
            {
                get => this[ModifyFieldID.UnitTypeAcquisitionRange]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeAcquisitionRange, value, true);
            }
            //public Modification UnitTypeArmorType {
            //    get => this[ModifyFieldID.UnitTypeArmorType];
            //    set => SetValue(ModifyFieldID.UnitTypeArmorType, value);
            //}
            public float? UnitTypeBackswingPoint1
            {
                get => this[ModifyFieldID.UnitTypeBackswingPoint1]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeBackswingPoint1, value, true);
            }
            public float? UnitTypeDamagePoint1
            {
                get => this[ModifyFieldID.UnitTypeDamagePoint1]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeDamagePoint1, value, true);
            }
            //public Modification UnitTypeAreaOfEffectFull1 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectFull1];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectFull1, value);
            //}
            //public Modification UnitTypeAreaOfEffectMedium1 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectMedium1];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectMedium1, value);
            //}
            //public Modification UnitTypeAreaOfEffectSmall1 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectSmall1];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectSmall1, value);
            //}
            //public Modification UnitTypeAreaOfEffectTargets1 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectTargets1];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectTargets1, value);
            //}
            //public Modification UnitTypeAttackType1 {
            //    get => this[ModifyFieldID.UnitTypeAttackType1];
            //    set => SetValue(ModifyFieldID.UnitTypeAttackType1, value);
            //}
            public float? UnitTypeCooldown1
            {
                get => this[ModifyFieldID.UnitTypeCooldown1]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeCooldown1, value, true);
            }
            //public Modification UnitTypeDamageBase1 {
            //    get => this[ModifyFieldID.UnitTypeDamageBase1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageBase1, value);
            //}
            //public Modification UnitTypeDamageFactorMedium1 {
            //    get => this[ModifyFieldID.UnitTypeDamageFactorMedium1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageFactorMedium1, value);
            //}
            //public Modification UnitTypeDamageFactorSmall1 {
            //    get => this[ModifyFieldID.UnitTypeDamageFactorSmall1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageFactorSmall1, value);
            //}
            //public Modification UnitTypeDamageLossFactor1 {
            //    get => this[ModifyFieldID.UnitTypeDamageLossFactor1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageLossFactor1, value);
            //}
            //public Modification UnitTypeDamageNumberOfDice1 {
            //    get => this[ModifyFieldID.UnitTypeDamageNumberOfDice1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageNumberOfDice1, value);
            //}
            //public Modification UnitTypeDamageSidesPerDie1 {
            //    get => this[ModifyFieldID.UnitTypeDamageSidesPerDie1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSidesPerDie1, value);
            //}
            //public Modification UnitTypeDamageSpillDistance1 {
            //    get => this[ModifyFieldID.UnitTypeDamageSpillDistance1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSpillDistance1, value);
            //}
            //public Modification UnitTypeDamageSpillRadius1 {
            //    get => this[ModifyFieldID.UnitTypeDamageSpillRadius1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSpillRadius1, value);
            //}
            //public Modification UnitTypeDamageUpgradeAmount1 {
            //    get => this[ModifyFieldID.UnitTypeDamageUpgradeAmount1];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageUpgradeAmount1, value);
            //}
            //public Modification UnitTypeMaximumTargets1 {
            //    get => this[ModifyFieldID.UnitTypeMaximumTargets1];
            //    set => SetValue(ModifyFieldID.UnitTypeMaximumTargets1, value);
            //}
            //public Modification UnitTypeProjectileArc1 {
            //    get => this[ModifyFieldID.UnitTypeProjectileArc1];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileArc1, value);
            //}
            public string UnitTypeProjectileArt1
            {
                get => this[ModifyFieldID.UnitTypeProjectileArt1]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeProjectileArt1, value);
            }
            //public Modification UnitTypeProjectileHoming1 {
            //    get => this[ModifyFieldID.UnitTypeProjectileHoming1];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileHoming1, value);
            //}
            public int? UnitTypeProjectileSpeed1
            {
                get => this[ModifyFieldID.UnitTypeProjectileSpeed1]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeProjectileSpeed1, value);
            }
            public int? UnitTypeRange1
            {
                get => this[ModifyFieldID.UnitTypeRange1]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeRange1, value);
            }
            public float? UnitTypeRangeMotionBuffer1
            {
                get => this[ModifyFieldID.UnitTypeRangeMotionBuffer1]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeRangeMotionBuffer1, value, true);
            }
            //public Modification UnitTypeShowUI1 {
            //    get => this[ModifyFieldID.UnitTypeShowUI1];
            //    set => SetValue(ModifyFieldID.UnitTypeShowUI1, value);
            //}
            public string UnitTypeTargetsAllowed1
            {
                get => this[ModifyFieldID.UnitTypeTargetsAllowed1]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeTargetsAllowed1, value);
            }
            public string UnitTypeWeaponSound1
            {
                get => this[ModifyFieldID.UnitTypeWeaponSound1]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeWeaponSound1, value);
            }
            public string UnitTypeWeaponType1
            {
                get => this[ModifyFieldID.UnitTypeWeaponType1]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeWeaponType1, value);
            }
            //public Modification UnitTypeBackswingPoint2 {
            //    get => this[ModifyFieldID.UnitTypeBackswingPoint2];
            //    set => SetValue(ModifyFieldID.UnitTypeBackswingPoint2, value);
            //}
            //public Modification UnitTypeDamagePoint2 {
            //    get => this[ModifyFieldID.UnitTypeDamagePoint2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamagePoint2, value);
            //}
            //public Modification UnitTypeAreaOfEffectFull2 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectFull2];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectFull2, value);
            //}
            //public Modification UnitTypeAreaOfEffectMedium2 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectMedium2];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectMedium2, value);
            //}
            //public Modification UnitTypeAreaOfEffectSmall2 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectSmall2];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectSmall2, value);
            //}
            //public Modification UnitTypeAreaOfEffectTargets2 {
            //    get => this[ModifyFieldID.UnitTypeAreaOfEffectTargets2];
            //    set => SetValue(ModifyFieldID.UnitTypeAreaOfEffectTargets2, value);
            //}
            //public Modification UnitTypeAttackType2 {
            //    get => this[ModifyFieldID.UnitTypeAttackType2];
            //    set => SetValue(ModifyFieldID.UnitTypeAttackType2, value);
            //}
            //public Modification UnitTypeCooldown2 {
            //    get => this[ModifyFieldID.UnitTypeCooldown2];
            //    set => SetValue(ModifyFieldID.UnitTypeCooldown2, value);
            //}
            //public Modification UnitTypeDamageBase2 {
            //    get => this[ModifyFieldID.UnitTypeDamageBase2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageBase2, value);
            //}
            //public Modification UnitTypeDamageFactorMedium2 {
            //    get => this[ModifyFieldID.UnitTypeDamageFactorMedium2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageFactorMedium2, value);
            //}
            //public Modification UnitTypeDamageFactorSmall2 {
            //    get => this[ModifyFieldID.UnitTypeDamageFactorSmall2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageFactorSmall2, value);
            //}
            //public Modification UnitTypeDamageLossFactor2 {
            //    get => this[ModifyFieldID.UnitTypeDamageLossFactor2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageLossFactor2, value);
            //}
            //public Modification UnitTypeDamageNumberOfDice2 {
            //    get => this[ModifyFieldID.UnitTypeDamageNumberOfDice2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageNumberOfDice2, value);
            //}
            //public Modification UnitTypeDamageSidesPerDie2 {
            //    get => this[ModifyFieldID.UnitTypeDamageSidesPerDie2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSidesPerDie2, value);
            //}
            //public Modification UnitTypeDamageSpillDistance2 {
            //    get => this[ModifyFieldID.UnitTypeDamageSpillDistance2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSpillDistance2, value);
            //}
            //public Modification UnitTypeDamageSpillRadius2 {
            //    get => this[ModifyFieldID.UnitTypeDamageSpillRadius2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageSpillRadius2, value);
            //}
            //public Modification UnitTypeDamageUpgradeAmount2 {
            //    get => this[ModifyFieldID.UnitTypeDamageUpgradeAmount2];
            //    set => SetValue(ModifyFieldID.UnitTypeDamageUpgradeAmount2, value);
            //}
            //public Modification UnitTypeMaximumTargets2 {
            //    get => this[ModifyFieldID.UnitTypeMaximumTargets2];
            //    set => SetValue(ModifyFieldID.UnitTypeMaximumTargets2, value);
            //}
            //public Modification UnitTypeProjectileArc2 {
            //    get => this[ModifyFieldID.UnitTypeProjectileArc2];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileArc2, value);
            //}
            //public Modification UnitTypeProjectileArt2 {
            //    get => this[ModifyFieldID.UnitTypeProjectileArt2];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileArt2, value);
            //}
            //public Modification UnitTypeProjectileHoming2 {
            //    get => this[ModifyFieldID.UnitTypeProjectileHoming2];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileHoming2, value);
            //}
            //public Modification UnitTypeProjectileSpeed2 {
            //    get => this[ModifyFieldID.UnitTypeProjectileSpeed2];
            //    set => SetValue(ModifyFieldID.UnitTypeProjectileSpeed2, value);
            //}
            //public Modification UnitTypeRange2 {
            //    get => this[ModifyFieldID.UnitTypeRange2];
            //    set => SetValue(ModifyFieldID.UnitTypeRange2, value);
            //}
            //public Modification UnitTypeRangeMotionBuffer2 {
            //    get => this[ModifyFieldID.UnitTypeRangeMotionBuffer2];
            //    set => SetValue(ModifyFieldID.UnitTypeRangeMotionBuffer2, value);
            //}
            //public Modification UnitTypeShowUI2 {
            //    get => this[ModifyFieldID.UnitTypeShowUI2];
            //    set => SetValue(ModifyFieldID.UnitTypeShowUI2, value);
            //}
            //public Modification UnitTypeTargetsAllowed2 {
            //    get => this[ModifyFieldID.UnitTypeTargetsAllowed2];
            //    set => SetValue(ModifyFieldID.UnitTypeTargetsAllowed2, value);
            //}
            //public Modification UnitTypeWeaponSound2 {
            //    get => this[ModifyFieldID.UnitTypeWeaponSound2];
            //    set => SetValue(ModifyFieldID.UnitTypeWeaponSound2, value);
            //}
            //public Modification UnitTypeWeaponType2 {
            //    get => this[ModifyFieldID.UnitTypeWeaponType2];
            //    set => SetValue(ModifyFieldID.UnitTypeWeaponType2, value);
            //}
            //public Modification UnitTypeAttacksEnabled {
            //    get => this[ModifyFieldID.UnitTypeAttacksEnabled];
            //    set => SetValue(ModifyFieldID.UnitTypeAttacksEnabled, value);
            //}
            //public Modification UnitTypeDeathType {
            //    get => this[ModifyFieldID.UnitTypeDeathType];
            //    set => SetValue(ModifyFieldID.UnitTypeDeathType, value);
            //}
            //public Modification UnitTypeDefenseBase {
            //    get => this[ModifyFieldID.UnitTypeDefenseBase];
            //    set => SetValue(ModifyFieldID.UnitTypeDefenseBase, value);
            //}
            //public Modification UnitTypeDefenseType {
            //    get => this[ModifyFieldID.UnitTypeDefenseType];
            //    set => SetValue(ModifyFieldID.UnitTypeDefenseType, value);
            //}
            //public Modification UnitTypeDefenseUpgradeBonus {
            //    get => this[ModifyFieldID.UnitTypeDefenseUpgradeBonus];
            //    set => SetValue(ModifyFieldID.UnitTypeDefenseUpgradeBonus, value);
            //}
            //public Modification UnitTypeMinimumAttackRange {
            //    get => this[ModifyFieldID.UnitTypeMinimumAttackRange];
            //    set => SetValue(ModifyFieldID.UnitTypeMinimumAttackRange, value);
            //}
            //public Modification UnitTypeTargetedAs {
            //    get => this[ModifyFieldID.UnitTypeTargetedAs];
            //    set => SetValue(ModifyFieldID.UnitTypeTargetedAs, value);
            //}
            //public Modification UnitTypeDropItemsOnDeath {
            //    get => this[ModifyFieldID.UnitTypeDropItemsOnDeath];
            //    set => SetValue(ModifyFieldID.UnitTypeDropItemsOnDeath, value);
            //}
            //public Modification UnitTypeCategoryCampaign {
            //    get => this[ModifyFieldID.UnitTypeCategoryCampaign];
            //    set => SetValue(ModifyFieldID.UnitTypeCategoryCampaign, value);
            //}
            //public Modification UnitTypeCategorySpecial {
            //    get => this[ModifyFieldID.UnitTypeCategorySpecial];
            //    set => SetValue(ModifyFieldID.UnitTypeCategorySpecial, value);
            //}
            //public Modification UnitTypeDisplayAsNeutralHostile {
            //    get => this[ModifyFieldID.UnitTypeDisplayAsNeutralHostile];
            //    set => SetValue(ModifyFieldID.UnitTypeDisplayAsNeutralHostile, value);
            //}
            //public Modification UnitTypeHasTilesetSpecificData {
            //    get => this[ModifyFieldID.UnitTypeHasTilesetSpecificData];
            //    set => SetValue(ModifyFieldID.UnitTypeHasTilesetSpecificData, value);
            //}
            //public Modification UnitTypePlaceableInEditor {
            //    get => this[ModifyFieldID.UnitTypePlaceableInEditor];
            //    set => SetValue(ModifyFieldID.UnitTypePlaceableInEditor, value);
            //}
            //public Modification UnitTypeTilesets {
            //    get => this[ModifyFieldID.UnitTypeTilesets];
            //    set => SetValue(ModifyFieldID.UnitTypeTilesets, value);
            //}
            //public Modification UnitTypeUseClickHelper {
            //    get => this[ModifyFieldID.UnitTypeUseClickHelper];
            //    set => SetValue(ModifyFieldID.UnitTypeUseClickHelper, value);
            //}
            //public Modification UnitTypeGroupSeparationEnabled {
            //    get => this[ModifyFieldID.UnitTypeGroupSeparationEnabled];
            //    set => SetValue(ModifyFieldID.UnitTypeGroupSeparationEnabled, value);
            //}
            //public Modification UnitTypeGroupSeparationGroupNumber {
            //    get => this[ModifyFieldID.UnitTypeGroupSeparationGroupNumber];
            //    set => SetValue(ModifyFieldID.UnitTypeGroupSeparationGroupNumber, value);
            //}
            //public Modification UnitTypeGroupSeparationParameter {
            //    get => this[ModifyFieldID.UnitTypeGroupSeparationParameter];
            //    set => SetValue(ModifyFieldID.UnitTypeGroupSeparationParameter, value);
            //}
            //public Modification UnitTypeGroupSeparationPriority {
            //    get => this[ModifyFieldID.UnitTypeGroupSeparationPriority];
            //    set => SetValue(ModifyFieldID.UnitTypeGroupSeparationPriority, value);
            //}
            //public Modification UnitTypeFlyHeight {
            //    get => this[ModifyFieldID.UnitTypeFlyHeight];
            //    set => SetValue(ModifyFieldID.UnitTypeFlyHeight, value);
            //}
            //public Modification UnitTypeMinimumHeight {
            //    get => this[ModifyFieldID.UnitTypeMinimumHeight];
            //    set => SetValue(ModifyFieldID.UnitTypeMinimumHeight, value);
            //}
            //public Modification UnitTypeSpeedBase {
            //    get => this[ModifyFieldID.UnitTypeSpeedBase];
            //    set => SetValue(ModifyFieldID.UnitTypeSpeedBase, value);
            //}
            //public Modification UnitTypeSpeedMaximum {
            //    get => this[ModifyFieldID.UnitTypeSpeedMaximum];
            //    set => SetValue(ModifyFieldID.UnitTypeSpeedMaximum, value);
            //}
            //public Modification UnitTypeSpeedMinimum {
            //    get => this[ModifyFieldID.UnitTypeSpeedMinimum];
            //    set => SetValue(ModifyFieldID.UnitTypeSpeedMinimum, value);
            //}
            public float? UnitTypeTurnRate
            {
                get => this[ModifyFieldID.UnitTypeTurnRate]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeTurnRate, value, true);
            }
            public string UnitTypeMoveType
            {
                get => this[ModifyFieldID.UnitTypeMoveType]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeMoveType, value);
            }
            //public Modification UnitTypeAIPlacementRadius {
            //    get => this[ModifyFieldID.UnitTypeAIPlacementRadius];
            //    set => SetValue(ModifyFieldID.UnitTypeAIPlacementRadius, value);
            //}
            //public Modification UnitTypeAIPlacementType {
            //    get => this[ModifyFieldID.UnitTypeAIPlacementType];
            //    set => SetValue(ModifyFieldID.UnitTypeAIPlacementType, value);
            //}
            //public Modification UnitTypeCollisionSize {
            //    get => this[ModifyFieldID.UnitTypeCollisionSize];
            //    set => SetValue(ModifyFieldID.UnitTypeCollisionSize, value);
            //}
            //public Modification UnitTypePathingMap {
            //    get => this[ModifyFieldID.UnitTypePathingMap];
            //    set => SetValue(ModifyFieldID.UnitTypePathingMap, value);
            //}
            //public Modification UnitTypePlacementPreventedBy {
            //    get => this[ModifyFieldID.UnitTypePlacementPreventedBy];
            //    set => SetValue(ModifyFieldID.UnitTypePlacementPreventedBy, value);
            //}
            //public Modification UnitTypePlacementRequires {
            //    get => this[ModifyFieldID.UnitTypePlacementRequires];
            //    set => SetValue(ModifyFieldID.UnitTypePlacementRequires, value);
            //}
            //public Modification UnitTypePlacementRequiresWaterRadius {
            //    get => this[ModifyFieldID.UnitTypePlacementRequiresWaterRadius];
            //    set => SetValue(ModifyFieldID.UnitTypePlacementRequiresWaterRadius, value);
            //}
            //public Modification UnitTypeBuildSound {
            //    get => this[ModifyFieldID.UnitTypeBuildSound];
            //    set => SetValue(ModifyFieldID.UnitTypeBuildSound, value);
            //}
            //public Modification UnitTypeSoundLoopFadeInRate {
            //    get => this[ModifyFieldID.UnitTypeSoundLoopFadeInRate];
            //    set => SetValue(ModifyFieldID.UnitTypeSoundLoopFadeInRate, value);
            //}
            //public Modification UnitTypeSoundLoopFadeOutRate {
            //    get => this[ModifyFieldID.UnitTypeSoundLoopFadeOutRate];
            //    set => SetValue(ModifyFieldID.UnitTypeSoundLoopFadeOutRate, value);
            //}
            public string UnitTypeMoveSound
            {
                get => this[ModifyFieldID.UnitTypeMoveSound]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeMoveSound, value);
            }
            //public Modification UnitTypeRandomSound {
            //    get => this[ModifyFieldID.UnitTypeRandomSound];
            //    set => SetValue(ModifyFieldID.UnitTypeRandomSound, value);
            //}
            public string UnitTypeSoundSet
            {
                get => this[ModifyFieldID.UnitTypeSoundSet]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeSoundSet, value);
            }
            public float? UnitTypeAgilityPerLevel
            {
                get => this[ModifyFieldID.UnitTypeAgilityPerLevel]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeAgilityPerLevel, value, true);
            }
            //public Modification UnitTypeBuildTime {
            //    get => this[ModifyFieldID.UnitTypeBuildTime];
            //    set => SetValue(ModifyFieldID.UnitTypeBuildTime, value);
            //}
            //public Modification UnitTypeCanBeBuiltOn {
            //    get => this[ModifyFieldID.UnitTypeCanBeBuiltOn];
            //    set => SetValue(ModifyFieldID.UnitTypeCanBeBuiltOn, value);
            //}
            //public Modification UnitTypeCanBuildOn {
            //    get => this[ModifyFieldID.UnitTypeCanBuildOn];
            //    set => SetValue(ModifyFieldID.UnitTypeCanBuildOn, value);
            //}
            //public Modification UnitTypeCanFlee {
            //    get => this[ModifyFieldID.UnitTypeCanFlee];
            //    set => SetValue(ModifyFieldID.UnitTypeCanFlee, value);
            //}
            //public Modification UnitTypeFoodCost {
            //    get => this[ModifyFieldID.UnitTypeFoodCost];
            //    set => SetValue(ModifyFieldID.UnitTypeFoodCost, value);
            //}
            //public Modification UnitTypeFoodProduced {
            //    get => this[ModifyFieldID.UnitTypeFoodProduced];
            //    set => SetValue(ModifyFieldID.UnitTypeFoodProduced, value);
            //}
            //public Modification UnitTypeFormationRank {
            //    get => this[ModifyFieldID.UnitTypeFormationRank];
            //    set => SetValue(ModifyFieldID.UnitTypeFormationRank, value);
            //}
            //public Modification UnitTypeGoldBountyBase {
            //    get => this[ModifyFieldID.UnitTypeGoldBountyBase];
            //    set => SetValue(ModifyFieldID.UnitTypeGoldBountyBase, value);
            //}
            //public Modification UnitTypeGoldBountyNumberOfDice {
            //    get => this[ModifyFieldID.UnitTypeGoldBountyNumberOfDice];
            //    set => SetValue(ModifyFieldID.UnitTypeGoldBountyNumberOfDice, value);
            //}
            //public Modification UnitTypeGoldBountySidesPerDie {
            //    get => this[ModifyFieldID.UnitTypeGoldBountySidesPerDie];
            //    set => SetValue(ModifyFieldID.UnitTypeGoldBountySidesPerDie, value);
            //}
            //public Modification UnitTypeGoldCost {
            //    get => this[ModifyFieldID.UnitTypeGoldCost];
            //    set => SetValue(ModifyFieldID.UnitTypeGoldCost, value);
            //}
            //public Modification UnitTypeHideHeroDeathMessage {
            //    get => this[ModifyFieldID.UnitTypeHideHeroDeathMessage];
            //    set => SetValue(ModifyFieldID.UnitTypeHideHeroDeathMessage, value);
            //}
            //public Modification UnitTypeHideHeroInterfaceIcon {
            //    get => this[ModifyFieldID.UnitTypeHideHeroInterfaceIcon];
            //    set => SetValue(ModifyFieldID.UnitTypeHideHeroInterfaceIcon, value);
            //}
            //public Modification UnitTypeHideHeroMinimapDisplay {
            //    get => this[ModifyFieldID.UnitTypeHideHeroMinimapDisplay];
            //    set => SetValue(ModifyFieldID.UnitTypeHideHeroMinimapDisplay, value);
            //}
            //public Modification UnitTypeHideMinimapDisplay {
            //    get => this[ModifyFieldID.UnitTypeHideMinimapDisplay];
            //    set => SetValue(ModifyFieldID.UnitTypeHideMinimapDisplay, value);
            //}
            //public Modification UnitTypeHitPointsMaximum {
            //    get => this[ModifyFieldID.UnitTypeHitPointsMaximum];
            //    set => SetValue(ModifyFieldID.UnitTypeHitPointsMaximum, value);
            //}
            public float? UnitTypeHitPointsRegeneration
            {
                get => this[ModifyFieldID.UnitTypeHitPointsRegeneration]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeHitPointsRegeneration, value, true);
            }
            //public Modification UnitTypeHitPointsRegenerationType {
            //    get => this[ModifyFieldID.UnitTypeHitPointsRegenerationType];
            //    set => SetValue(ModifyFieldID.UnitTypeHitPointsRegenerationType, value);
            //}
            public float? UnitTypeIntelligencePerLevel
            {
                get => this[ModifyFieldID.UnitTypeIntelligencePerLevel]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeIntelligencePerLevel, value, true);
            }
            //public Modification UnitTypeIsABuilding {
            //    get => this[ModifyFieldID.UnitTypeIsABuilding];
            //    set => SetValue(ModifyFieldID.UnitTypeIsABuilding, value);
            //}
            //public Modification UnitTypeLevel {
            //    get => this[ModifyFieldID.UnitTypeLevel];
            //    set => SetValue(ModifyFieldID.UnitTypeLevel, value);
            //}
            //public Modification UnitTypeLumberBountyBase {
            //    get => this[ModifyFieldID.UnitTypeLumberBountyBase];
            //    set => SetValue(ModifyFieldID.UnitTypeLumberBountyBase, value);
            //}
            //public Modification UnitTypeLumberBountyNumberOfDice {
            //    get => this[ModifyFieldID.UnitTypeLumberBountyNumberOfDice];
            //    set => SetValue(ModifyFieldID.UnitTypeLumberBountyNumberOfDice, value);
            //}
            //public Modification UnitTypeLumberBountySidesPerDie {
            //    get => this[ModifyFieldID.UnitTypeLumberBountySidesPerDie];
            //    set => SetValue(ModifyFieldID.UnitTypeLumberBountySidesPerDie, value);
            //}
            //public Modification UnitTypeLumberCost {
            //    get => this[ModifyFieldID.UnitTypeLumberCost];
            //    set => SetValue(ModifyFieldID.UnitTypeLumberCost, value);
            //}
            //public Modification UnitTypeManaInitialAmount {
            //    get => this[ModifyFieldID.UnitTypeManaInitialAmount];
            //    set => SetValue(ModifyFieldID.UnitTypeManaInitialAmount, value);
            //}
            //public Modification UnitTypeManaMaximum {
            //    get => this[ModifyFieldID.UnitTypeManaMaximum];
            //    set => SetValue(ModifyFieldID.UnitTypeManaMaximum, value);
            //}
            //public Modification UnitTypeManaRegeneration {
            //    get => this[ModifyFieldID.UnitTypeManaRegeneration];
            //    set => SetValue(ModifyFieldID.UnitTypeManaRegeneration, value);
            //}
            //public Modification UnitTypeShowNeutralBuildingIcon {
            //    get => this[ModifyFieldID.UnitTypeShowNeutralBuildingIcon];
            //    set => SetValue(ModifyFieldID.UnitTypeShowNeutralBuildingIcon, value);
            //}
            //public Modification UnitTypeValidAsRandomNeutralBuilding {
            //    get => this[ModifyFieldID.UnitTypeValidAsRandomNeutralBuilding];
            //    set => SetValue(ModifyFieldID.UnitTypeValidAsRandomNeutralBuilding, value);
            //}
            //public Modification UnitTypePointValue {
            //    get => this[ModifyFieldID.UnitTypePointValue];
            //    set => SetValue(ModifyFieldID.UnitTypePointValue, value);
            //}
            //public Modification UnitTypePrimaryAttribute {
            //    get => this[ModifyFieldID.UnitTypePrimaryAttribute];
            //    set => SetValue(ModifyFieldID.UnitTypePrimaryAttribute, value);
            //}
            //public Modification UnitTypePriority {
            //    get => this[ModifyFieldID.UnitTypePriority];
            //    set => SetValue(ModifyFieldID.UnitTypePriority, value);
            //}
            public string UnitTypeRace
            {
                get => this[ModifyFieldID.UnitTypeRace]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeRace, value);
            }
            //public Modification UnitTypeRepairGoldCost {
            //    get => this[ModifyFieldID.UnitTypeRepairGoldCost];
            //    set => SetValue(ModifyFieldID.UnitTypeRepairGoldCost, value);
            //}
            //public Modification UnitTypeRepairLumberCost {
            //    get => this[ModifyFieldID.UnitTypeRepairLumberCost];
            //    set => SetValue(ModifyFieldID.UnitTypeRepairLumberCost, value);
            //}
            //public Modification UnitTypeRepairTime {
            //    get => this[ModifyFieldID.UnitTypeRepairTime];
            //    set => SetValue(ModifyFieldID.UnitTypeRepairTime, value);
            //}
            //public Modification UnitTypeSightRadiusDay {
            //    get => this[ModifyFieldID.UnitTypeSightRadiusDay];
            //    set => SetValue(ModifyFieldID.UnitTypeSightRadiusDay, value);
            //}
            //public Modification UnitTypeSightRadiusNight {
            //    get => this[ModifyFieldID.UnitTypeSightRadiusNight];
            //    set => SetValue(ModifyFieldID.UnitTypeSightRadiusNight, value);
            //}
            //public Modification UnitTypeSleeps {
            //    get => this[ModifyFieldID.UnitTypeSleeps];
            //    set => SetValue(ModifyFieldID.UnitTypeSleeps, value);
            //}
            public int? UnitTypeStartingAgility
            {
                get => this[ModifyFieldID.UnitTypeStartingAgility]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeStartingAgility, value);
            }
            public int? UnitTypeStartingIntelligence
            {
                get => this[ModifyFieldID.UnitTypeStartingIntelligence]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeStartingIntelligence, value);
            }
            public int? UnitTypeStartingStrength
            {
                get => this[ModifyFieldID.UnitTypeStartingStrength]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeStartingStrength, value);
            }
            //public Modification UnitTypeStockMaximum {
            //    get => this[ModifyFieldID.UnitTypeStockMaximum];
            //    set => SetValue(ModifyFieldID.UnitTypeStockMaximum, value);
            //}
            //public Modification UnitTypeStockReplenishInterval {
            //    get => this[ModifyFieldID.UnitTypeStockReplenishInterval];
            //    set => SetValue(ModifyFieldID.UnitTypeStockReplenishInterval, value);
            //}
            //public Modification UnitTypeStockStartDelay {
            //    get => this[ModifyFieldID.UnitTypeStockStartDelay];
            //    set => SetValue(ModifyFieldID.UnitTypeStockStartDelay, value);
            //}
            public float? UnitTypeStrengthPerLevel
            {
                get => this[ModifyFieldID.UnitTypeStrengthPerLevel]?.VarFloat;
                set => SetValue(ModifyFieldID.UnitTypeStrengthPerLevel, value, true);
            }
            //public Modification UnitTypeTransportedSize {
            //    get => this[ModifyFieldID.UnitTypeTransportedSize];
            //    set => SetValue(ModifyFieldID.UnitTypeTransportedSize, value);
            //}
            //public Modification UnitTypeUnitClassification {
            //    get => this[ModifyFieldID.UnitTypeUnitClassification];
            //    set => SetValue(ModifyFieldID.UnitTypeUnitClassification, value);
            //}
            //public Modification UnitTypeDependencyEquivalents {
            //    get => this[ModifyFieldID.UnitTypeDependencyEquivalents];
            //    set => SetValue(ModifyFieldID.UnitTypeDependencyEquivalents, value);
            //}
            //public Modification UnitTypeHeroRevivalLocations {
            //    get => this[ModifyFieldID.UnitTypeHeroRevivalLocations];
            //    set => SetValue(ModifyFieldID.UnitTypeHeroRevivalLocations, value);
            //}
            //public Modification UnitTypeItemsMade {
            //    get => this[ModifyFieldID.UnitTypeItemsMade];
            //    set => SetValue(ModifyFieldID.UnitTypeItemsMade, value);
            //}
            public List<int> UnitTypeItemsSold
            {
                get => this[ModifyFieldID.UnitTypeItemsSold]?.VarIntArray;
                set => SetValue(ModifyFieldID.UnitTypeItemsSold, value);
            }
            //public Modification UnitTypeRequirements {
            //    get => this[ModifyFieldID.UnitTypeRequirements];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirements, value);
            //}
            //public Modification UnitTypeRequirementsLevels {
            //    get => this[ModifyFieldID.UnitTypeRequirementsLevels];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsLevels, value);
            //}
            //public Modification UnitTypeRequirementsTier2 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier2];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier2, value);
            //}
            //public Modification UnitTypeRequirementsTier3 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier3];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier3, value);
            //}
            //public Modification UnitTypeRequirementsTier4 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier4];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier4, value);
            //}
            //public Modification UnitTypeRequirementsTier5 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier5];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier5, value);
            //}
            //public Modification UnitTypeRequirementsTier6 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier6];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier6, value);
            //}
            //public Modification UnitTypeRequirementsTier7 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier7];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier7, value);
            //}
            //public Modification UnitTypeRequirementsTier8 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier8];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier8, value);
            //}
            //public Modification UnitTypeRequirementsTier9 {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTier9];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTier9, value);
            //}
            //public Modification UnitTypeRequirementsTiersUsed {
            //    get => this[ModifyFieldID.UnitTypeRequirementsTiersUsed];
            //    set => SetValue(ModifyFieldID.UnitTypeRequirementsTiersUsed, value);
            //}
            //public Modification UnitTypeStructuresBuilt {
            //    get => this[ModifyFieldID.UnitTypeStructuresBuilt];
            //    set => SetValue(ModifyFieldID.UnitTypeStructuresBuilt, value);
            //}
            //public Modification UnitTypeResearchesAvailable {
            //    get => this[ModifyFieldID.UnitTypeResearchesAvailable];
            //    set => SetValue(ModifyFieldID.UnitTypeResearchesAvailable, value);
            //}
            //public Modification UnitTypeRevivesDeadHeroes {
            //    get => this[ModifyFieldID.UnitTypeRevivesDeadHeroes];
            //    set => SetValue(ModifyFieldID.UnitTypeRevivesDeadHeroes, value);
            //}
            //public Modification UnitTypeUnitsSold {
            //    get => this[ModifyFieldID.UnitTypeUnitsSold];
            //    set => SetValue(ModifyFieldID.UnitTypeUnitsSold, value);
            //}
            //public Modification UnitTypeUnitsTrained {
            //    get => this[ModifyFieldID.UnitTypeUnitsTrained];
            //    set => SetValue(ModifyFieldID.UnitTypeUnitsTrained, value);
            //}
            //public Modification UnitTypeUpgradesTo {
            //    get => this[ModifyFieldID.UnitTypeUpgradesTo];
            //    set => SetValue(ModifyFieldID.UnitTypeUpgradesTo, value);
            //}
            //public Modification UnitTypeUpgradesUsed {
            //    get => this[ModifyFieldID.UnitTypeUpgradesUsed];
            //    set => SetValue(ModifyFieldID.UnitTypeUpgradesUsed, value);
            //}
            public string UnitTypeDescription
            {
                get => this[ModifyFieldID.UnitTypeDescription]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeDescription, value);
            }
            public string UnitTypeHotkey
            {
                get => this[ModifyFieldID.UnitTypeHotkey]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeHotkey, value);
            }
            public string UnitTypeName
            {
                get => this[ModifyFieldID.UnitTypeName]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeName, value);
            }
            public string UnitTypeNameEditorSuffix
            {
                get => this[ModifyFieldID.UnitTypeNameEditorSuffix]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeNameEditorSuffix, value);
            }
            public string UnitTypeProperNames
            {
                get => this[ModifyFieldID.UnitTypeProperNames]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeProperNames, value);
            }
            public int? UnitTypeProperNamesUsed
            {
                get => this[ModifyFieldID.UnitTypeProperNamesUsed]?.VarInt;
                set => SetValue(ModifyFieldID.UnitTypeProperNamesUsed, value);
            }
            public string UnitTypeAwakenTooltip
            {
                get => this[ModifyFieldID.UnitTypeAwakenTooltip]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeAwakenTooltip, value);
            }
            public string UnitTypeTooltip
            {
                get => this[ModifyFieldID.UnitTypeTooltip]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeTooltip, value);
            }
            public string UnitTypeUbertip
            {
                get => this[ModifyFieldID.UnitTypeUbertip]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeUbertip, value);
            }
            public string UnitTypeReviveTooltip
            {
                get => this[ModifyFieldID.UnitTypeReviveTooltip]?.VarString;
                set => SetValue(ModifyFieldID.UnitTypeReviveTooltip, value);
            }
            #endregion

            #region [    Item Modification    ]
            /// <summary>그림(A) - 버튼 위치(X)</summary>
            public int? ItemTypeButtonPositionX
            {
                get => this[ModifyFieldID.ItemTypeButtonPositionX]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeButtonPositionX, value);
            }
            /// <summary>그림(A) - 버튼 위치(Y)</summary>
            public int? ItemTypeButtonPositionY
            {
                get => this[ModifyFieldID.ItemTypeButtonPositionY]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeButtonPositionY, value);
            }
            /// <summary>그림(A) - 사용된 모델</summary>
            public string ItemTypeModel
            {
                get => this[ModifyFieldID.ItemTypeModel]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeModel, value);
            }
            /// <summary>그림(A) - 색조 1 (빨간색)</summary>
            public int? ItemTypeColorRed
            {
                get => this[ModifyFieldID.ItemTypeColorRed]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeColorRed, value);
            }
            /// <summary>그림(A) - 색조 2 (초록색)</summary>
            public int? ItemTypeColorGreen
            {
                get => this[ModifyFieldID.ItemTypeColorGreen]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeColorGreen, value);
            }
            /// <summary>그림(A) - 색조 3 (파란색)</summary>
            public int? ItemTypeColorBlue
            {
                get => this[ModifyFieldID.ItemTypeColorBlue]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeColorBlue, value);
            }
            /// <summary>그림(A) - 선택 크기 - 에디터</summary>
            public float? ItemTypeSelectionSize
            {
                get => this[ModifyFieldID.ItemTypeSelectionSize]?.VarFloat;
                set => SetValue(ModifyFieldID.ItemTypeSelectionSize, value, false);
            }
            /// <summary>그림(A) - 인터페이스 아이콘</summary>
            public string ItemTypeIcon
            {
                get => this[ModifyFieldID.ItemTypeIcon]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeIcon, value);
            }
            /// <summary>그림(A) - 크기 조절치</summary>
            public float? ItemTypeScale
            {
                get => this[ModifyFieldID.ItemTypeScale]?.VarFloat;
                set => SetValue(ModifyFieldID.ItemTypeScale, value, false);
            }
            /// <summary>능력(B) - 능력</summary>
            public List<int> ItemTypeAbilities
            {
                get => this[ModifyFieldID.ItemTypeAbilities]?.VarIntArray;
                set => SetValue(ModifyFieldID.ItemTypeAbilities, value);
            }
            /// <summary>능력치(S) - 금 비용</summary>
            public int? ItemTypeGoldCost
            {
                get => this[ModifyFieldID.ItemTypeGoldCost]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeGoldCost, value);
            }
            /// <summary>능력치(S) - 랜덤 선택으로 포함</summary>
            public bool? ItemTypeIsRandomChoice
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsRandomChoice]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsRandomChoice, value);
            }
            /// <summary>능력치(S) - 레벨</summary>
            public int? ItemTypeLevel
            {
                get => this[ModifyFieldID.ItemTypeLevel]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeLevel, value);
            }
            /// <summary>능력치(S) - 레벨(분류 안됨)</summary>
            public int? ItemTypeUnclassifiedLevel
            {
                get => this[ModifyFieldID.ItemTypeUnclassifiedLevel]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeUnclassifiedLevel, value);
            }
            /// <summary>능력치(S) - 목재 비용</summary>
            public int? ItemTypeLumberCost
            {
                get => this[ModifyFieldID.ItemTypeLumberCost]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeLumberCost, value);
            }
            /// <summary>능력치(S) - 버릴 수 있음</summary>
            public bool? ItemTypeDroppable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeDroppable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeDroppable, value);
            }
            /// <summary>능력치(S) - 분류</summary>
            public string ItemTypeClass
            {
                get => this[ModifyFieldID.ItemTypeClass]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeClass, value);
            }
            /// <summary>능력치(S) - 사용 가능 회수</summary>
            public int? ItemTypeUses
            {
                get => this[ModifyFieldID.ItemTypeUses]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeUses, value);
            }
            /// <summary>능력치(S) - 상점에 판매 가능</summary>
            public bool? ItemTypeIsPawnable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsPawnable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsPawnable, value);
            }
            /// <summary>능력치(S) - 상점에서 판매 가능</summary>
            public bool? ItemTypeIsSellable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsSellable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsSellable, value);
            }
            /// <summary>능력치(S) - 습득 시 자동으로 사용</summary>
            public bool? ItemTypeIsPowerup
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsPowerup]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsPowerup, value);
            }
            /// <summary>능력치(S) - 썩기 쉬움</summary>
            public bool? ItemTypeIsPerishable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsPerishable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsPerishable, value);
            }
            /// <summary>능력치(S) - 우선도</summary>
            public int? ItemTypePriority
            {
                get => this[ModifyFieldID.ItemTypePriority]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypePriority, value);
            }
            /// <summary>능력치(S) - 자주 사용됨</summary>
            public bool? ItemTypeIsUsable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsUsable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsUsable, value);
            }
            /// <summary>능력치(S) - 재고 보충 기간</summary>
            public int? ItemTypeStockReplenishInterval
            {
                get => this[ModifyFieldID.ItemTypeStockReplenishInterval]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeStockReplenishInterval, value);
            }
            /// <summary>능력치(S) - 재고 시작 지연</summary>
            public int? ItemTypeStartingStock
            {
                get => this[ModifyFieldID.ItemTypeStartingStock]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeStartingStock, value);
            }
            /// <summary>능력치(S) - 적절한 변환 목표물</summary>
            public bool? ItemTypeIsTransformable
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIsTransformable]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIsTransformable, value);
            }
            /// <summary>능력치(S) - 죽으면 떨어트림</summary>
            public bool? ItemTypeDropsOnDeath
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeDropsOnDeath]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeDropsOnDeath, value);
            }
            /// <summary>능력치(S) - 체력</summary>
            public int? ItemTypeHealth
            {
                get => this[ModifyFieldID.ItemTypeHealth]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeHealth, value);
            }
            /// <summary>능력치(S) - 최대 재고</summary>
            public int? ItemTypeMaximumStock
            {
                get => this[ModifyFieldID.ItemTypeMaximumStock]?.VarInt;
                set => SetValue(ModifyFieldID.ItemTypeMaximumStock, value);
            }
            /// <summary>능력치(S) - 쿨다운 그룹</summary>
            public List<int> ItemTypeCooldownGroup
            {
                get => this[ModifyFieldID.ItemTypeCooldownGroup]?.VarIntArray;
                set => SetValue(ModifyFieldID.ItemTypeCooldownGroup, value);
            }
            /// <summary>능력치(S) - 쿨다운 무시</summary>
            public bool? ItemTypeIgnoreCooldown
            {
                get {
                    var value = this[ModifyFieldID.ItemTypeIgnoreCooldown]?.VarInt;
                    if (value == null) return null;
                    else return value > 0;
                }
                set => SetValue(ModifyFieldID.ItemTypeIgnoreCooldown, value);
            }
            /// <summary>전투(C) - 아머 형식</summary>
            public string ItemTypeArmorType
            {
                get => this[ModifyFieldID.ItemTypeArmorType]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeArmorType, value);
            }
            /// <summary>테크트리(T) - 요구 사항</summary>
            public List<int> ItemTypeRequirements
            {
                get => this[ModifyFieldID.ItemTypeRequirements]?.VarIntArray;
                set => SetValue(ModifyFieldID.ItemTypeRequirements, value);
            }
            /// <summary>테크트리(T) - 요구사항 - 레벨</summary>
            public List<int> ItemTypeRequiredLevels
            {
                get => this[ModifyFieldID.ItemTypeRequiredLevels]?.VarIntArray;
                set => SetValue(ModifyFieldID.ItemTypeRequiredLevels, value);
            }
            /// <summary>텍스트(X) - 단축키</summary>
            public string ItemTypeHotkey
            {
                get => this[ModifyFieldID.ItemTypeHotkey]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeHotkey, value);
            }
            /// <summary>텍스트(T) - 도구 도움말 - 기본</summary>
            public string ItemTypeTooltip
            {
                get => this[ModifyFieldID.ItemTypeTooltip]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeTooltip, value);
            }
            /// <summary>텍스트(T) - 도구 도움말 - 확장</summary>
            public string ItemTypeUbertip
            {
                get => this[ModifyFieldID.ItemTypeUbertip]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeUbertip, value);
            }
            /// <summary>텍스트(X) - 설명</summary>
            public string ItemTypeDescription
            {
                get => this[ModifyFieldID.ItemTypeDescription]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeDescription, value);
            }
            /// <summary>텍스트(X) - 이름</summary>
            public string ItemTypeName
            {
                get => this[ModifyFieldID.ItemTypeName]?.VarString;
                set => SetValue(ModifyFieldID.ItemTypeName, value);
            }
            #endregion

            #region [    Destructable  Modification    ]
            //public Modification DestructableTypeName {
            //    get => this[ModifyFieldID.DestructableTypeName];
            //    set => SetValue(ModifyFieldID.DestructableTypeName, value);
            //}
            //public Modification DestructableTypeEditorSuffix {
            //    get => this[ModifyFieldID.DestructableTypeEditorSuffix];
            //    set => SetValue(ModifyFieldID.DestructableTypeEditorSuffix, value);
            //}
            //public Modification DestructableTypeCategory {
            //    get => this[ModifyFieldID.DestructableTypeCategory];
            //    set => SetValue(ModifyFieldID.DestructableTypeCategory, value);
            //}
            //public Modification DestructableTypeTilesets {
            //    get => this[ModifyFieldID.DestructableTypeTilesets];
            //    set => SetValue(ModifyFieldID.DestructableTypeTilesets, value);
            //}
            //public Modification DestructableTypeIsTilesetSpecific {
            //    get => this[ModifyFieldID.DestructableTypeIsTilesetSpecific];
            //    set => SetValue(ModifyFieldID.DestructableTypeIsTilesetSpecific, value);
            //}
            //public Modification DestructableTypeFile {
            //    get => this[ModifyFieldID.DestructableTypeFile];
            //    set => SetValue(ModifyFieldID.DestructableTypeFile, value);
            //}
            //public Modification DestructableTypeIsLightweight {
            //    get => this[ModifyFieldID.DestructableTypeIsLightweight];
            //    set => SetValue(ModifyFieldID.DestructableTypeIsLightweight, value);
            //}
            //public Modification DestructableTypeIsFatLOS {
            //    get => this[ModifyFieldID.DestructableTypeIsFatLOS];
            //    set => SetValue(ModifyFieldID.DestructableTypeIsFatLOS, value);
            //}
            //public Modification DestructableTypeTextureID {
            //    get => this[ModifyFieldID.DestructableTypeTextureID];
            //    set => SetValue(ModifyFieldID.DestructableTypeTextureID, value);
            //}
            //public Modification DestructableTypeTextureFile {
            //    get => this[ModifyFieldID.DestructableTypeTextureFile];
            //    set => SetValue(ModifyFieldID.DestructableTypeTextureFile, value);
            //}
            //public Modification DestructableTypeUseClickHelper {
            //    get => this[ModifyFieldID.DestructableTypeUseClickHelper];
            //    set => SetValue(ModifyFieldID.DestructableTypeUseClickHelper, value);
            //}
            //public Modification DestructableTypeCanPlaceOnCliffs {
            //    get => this[ModifyFieldID.DestructableTypeCanPlaceOnCliffs];
            //    set => SetValue(ModifyFieldID.DestructableTypeCanPlaceOnCliffs, value);
            //}
            //public Modification DestructableTypeCanPlaceOnWater {
            //    get => this[ModifyFieldID.DestructableTypeCanPlaceOnWater];
            //    set => SetValue(ModifyFieldID.DestructableTypeCanPlaceOnWater, value);
            //}
            //public Modification DestructableTypeCanPlaceDead {
            //    get => this[ModifyFieldID.DestructableTypeCanPlaceDead];
            //    set => SetValue(ModifyFieldID.DestructableTypeCanPlaceDead, value);
            //}
            //public Modification DestructableTypeIsWalkable {
            //    get => this[ModifyFieldID.DestructableTypeIsWalkable];
            //    set => SetValue(ModifyFieldID.DestructableTypeIsWalkable, value);
            //}
            //public Modification DestructableTypeCliffHeight {
            //    get => this[ModifyFieldID.DestructableTypeCliffHeight];
            //    set => SetValue(ModifyFieldID.DestructableTypeCliffHeight, value);
            //}
            //public Modification DestructableTypeTargetType {
            //    get => this[ModifyFieldID.DestructableTypeTargetType];
            //    set => SetValue(ModifyFieldID.DestructableTypeTargetType, value);
            //}
            //public Modification DestructableTypeArmor {
            //    get => this[ModifyFieldID.DestructableTypeArmor];
            //    set => SetValue(ModifyFieldID.DestructableTypeArmor, value);
            //}
            //public Modification DestructableTypeNumVar {
            //    get => this[ModifyFieldID.DestructableTypeNumVar];
            //    set => SetValue(ModifyFieldID.DestructableTypeNumVar, value);
            //}
            //public Modification DestructableTypeHealth {
            //    get => this[ModifyFieldID.DestructableTypeHealth];
            //    set => SetValue(ModifyFieldID.DestructableTypeHealth, value);
            //}
            //public Modification DestructableTypeOcclusionHeight {
            //    get => this[ModifyFieldID.DestructableTypeOcclusionHeight];
            //    set => SetValue(ModifyFieldID.DestructableTypeOcclusionHeight, value);
            //}
            //public Modification DestructableTypeFlyHeight {
            //    get => this[ModifyFieldID.DestructableTypeFlyHeight];
            //    set => SetValue(ModifyFieldID.DestructableTypeFlyHeight, value);
            //}
            //public Modification DestructableTypeFixedRotation {
            //    get => this[ModifyFieldID.DestructableTypeFixedRotation];
            //    set => SetValue(ModifyFieldID.DestructableTypeFixedRotation, value);
            //}
            //public Modification DestructableTypeSelectionSize {
            //    get => this[ModifyFieldID.DestructableTypeSelectionSize];
            //    set => SetValue(ModifyFieldID.DestructableTypeSelectionSize, value);
            //}
            //public Modification DestructableTypeMinScale {
            //    get => this[ModifyFieldID.DestructableTypeMinScale];
            //    set => SetValue(ModifyFieldID.DestructableTypeMinScale, value);
            //}
            //public Modification DestructableTypeMaxScale {
            //    get => this[ModifyFieldID.DestructableTypeMaxScale];
            //    set => SetValue(ModifyFieldID.DestructableTypeMaxScale, value);
            //}
            //public Modification DestructableTypeCanPlaceRandScale {
            //    get => this[ModifyFieldID.DestructableTypeCanPlaceRandScale];
            //    set => SetValue(ModifyFieldID.DestructableTypeCanPlaceRandScale, value);
            //}
            //public Modification DestructableTypeMaxPitch {
            //    get => this[ModifyFieldID.DestructableTypeMaxPitch];
            //    set => SetValue(ModifyFieldID.DestructableTypeMaxPitch, value);
            //}
            //public Modification DestructableTypeMaxRoll {
            //    get => this[ModifyFieldID.DestructableTypeMaxRoll];
            //    set => SetValue(ModifyFieldID.DestructableTypeMaxRoll, value);
            //}
            //public Modification DestructableTypeRadius {
            //    get => this[ModifyFieldID.DestructableTypeRadius];
            //    set => SetValue(ModifyFieldID.DestructableTypeRadius, value);
            //}
            //public Modification DestructableTypeFogRadius {
            //    get => this[ModifyFieldID.DestructableTypeFogRadius];
            //    set => SetValue(ModifyFieldID.DestructableTypeFogRadius, value);
            //}
            //public Modification DestructableTypeFogVisibility {
            //    get => this[ModifyFieldID.DestructableTypeFogVisibility];
            //    set => SetValue(ModifyFieldID.DestructableTypeFogVisibility, value);
            //}
            //public Modification DestructableTypePathTexture {
            //    get => this[ModifyFieldID.DestructableTypePathTexture];
            //    set => SetValue(ModifyFieldID.DestructableTypePathTexture, value);
            //}
            //public Modification DestructableTypePathTextureDeath {
            //    get => this[ModifyFieldID.DestructableTypePathTextureDeath];
            //    set => SetValue(ModifyFieldID.DestructableTypePathTextureDeath, value);
            //}
            //public Modification DestructableTypeDeathSound {
            //    get => this[ModifyFieldID.DestructableTypeDeathSound];
            //    set => SetValue(ModifyFieldID.DestructableTypeDeathSound, value);
            //}
            //public Modification DestructableTypeShadow {
            //    get => this[ModifyFieldID.DestructableTypeShadow];
            //    set => SetValue(ModifyFieldID.DestructableTypeShadow, value);
            //}
            //public Modification DestructableTypeShowInMM {
            //    get => this[ModifyFieldID.DestructableTypeShowInMM];
            //    set => SetValue(ModifyFieldID.DestructableTypeShowInMM, value);
            //}
            //public Modification DestructableTypeMMRed {
            //    get => this[ModifyFieldID.DestructableTypeMMRed];
            //    set => SetValue(ModifyFieldID.DestructableTypeMMRed, value);
            //}
            //public Modification DestructableTypeMMGreen {
            //    get => this[ModifyFieldID.DestructableTypeMMGreen];
            //    set => SetValue(ModifyFieldID.DestructableTypeMMGreen, value);
            //}
            //public Modification DestructableTypeMMBlue {
            //    get => this[ModifyFieldID.DestructableTypeMMBlue];
            //    set => SetValue(ModifyFieldID.DestructableTypeMMBlue, value);
            //}
            //public Modification DestructableTypeUseMMColor {
            //    get => this[ModifyFieldID.DestructableTypeUseMMColor];
            //    set => SetValue(ModifyFieldID.DestructableTypeUseMMColor, value);
            //}
            //public Modification DestructableTypeBuildTime {
            //    get => this[ModifyFieldID.DestructableTypeBuildTime];
            //    set => SetValue(ModifyFieldID.DestructableTypeBuildTime, value);
            //}
            //public Modification DestructableTypeRepairTime {
            //    get => this[ModifyFieldID.DestructableTypeRepairTime];
            //    set => SetValue(ModifyFieldID.DestructableTypeRepairTime, value);
            //}
            //public Modification DestructableTypeGoldRepairCost {
            //    get => this[ModifyFieldID.DestructableTypeGoldRepairCost];
            //    set => SetValue(ModifyFieldID.DestructableTypeGoldRepairCost, value);
            //}
            //public Modification DestructableTypeLumberRepairCost {
            //    get => this[ModifyFieldID.DestructableTypeLumberRepairCost];
            //    set => SetValue(ModifyFieldID.DestructableTypeLumberRepairCost, value);
            //}
            //public Modification DestructableTypeUserList {
            //    get => this[ModifyFieldID.DestructableTypeUserList];
            //    set => SetValue(ModifyFieldID.DestructableTypeUserList, value);
            //}
            //public Modification DestructableTypeColorRed {
            //    get => this[ModifyFieldID.DestructableTypeColorRed];
            //    set => SetValue(ModifyFieldID.DestructableTypeColorRed, value);
            //}
            //public Modification DestructableTypeColorGreen {
            //    get => this[ModifyFieldID.DestructableTypeColorGreen];
            //    set => SetValue(ModifyFieldID.DestructableTypeColorGreen, value);
            //}
            //public Modification DestructableTypeColorBlue {
            //    get => this[ModifyFieldID.DestructableTypeColorBlue];
            //    set => SetValue(ModifyFieldID.DestructableTypeColorBlue, value);
            //}
            //public Modification DestructableTypeIsSelectable {
            //    get => this[ModifyFieldID.DestructableTypeIsSelectable];
            //    set => SetValue(ModifyFieldID.DestructableTypeIsSelectable, value);
            //}
            //public Modification DestructableTypeSelectionCircleSize {
            //    get => this[ModifyFieldID.DestructableTypeSelectionCircleSize];
            //    set => SetValue(ModifyFieldID.DestructableTypeSelectionCircleSize, value);
            //}
            //public Modification DestructableTypePortraitModel {
            //    get => this[ModifyFieldID.DestructableTypePortraitModel];
            //    set => SetValue(ModifyFieldID.DestructableTypePortraitModel, value);
            //}
            #endregion

            public Dictionary<string, dynamic> ToDictionary()
            {
                var dic = new Dictionary<string, dynamic>();
                foreach (var item in this)
                {
                    switch (item.VarType)
                    {
                        case 0:
                            dic.Add(item.ModifyID.GetRawString(), item.VarInt);
                            break;

                        case 1:
                        case 2:
                            dic.Add(item.ModifyID.GetRawString(), item.VarFloat);
                            break;

                        case 3:
                            if (item.VarIntArray == null)
                                dic.Add(item.ModifyID.GetRawString(), item.VarString);
                            else
                                dic.Add(item.ModifyID.GetRawString(), item.VarIntArray.ToArray().GetRawStringList());
                            break;
                    }
                }
                return dic;
            }
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
            public List<int> VarIntArray = null;
            public int OriginID;    // RawCode
        }
        public static W3Object Parse(byte[] data, string extention)
        {
            switch (extention.ToLower().Replace(".", string.Empty))
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
                        Defination def = new Defination(bs.ReadInt32().ReverseByte(), bs.ReadInt32().ReverseByte());
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
                                    else mod.VarIntArray = new List<int>(list);
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
                                            int[] RawCodes = initem.VarIntArray.ToArray();
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
