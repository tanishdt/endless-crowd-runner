using System;
using UnityEngine;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Weapons
{
    [Serializable]
    public class WeaponData
    {
        public WeaponType type;
        public string weaponName;
        public float fireRate = 2.0f; // Shots per second
        public int damage = 10;
        public float range = 30.0f;
        public float bulletSpeed = 40.0f;
        public int pelletsPerShot = 1;
        public float spreadAngle = 0f;
        public SoundEffectType fireSound = SoundEffectType.GunshotPistol;

        public static WeaponData GetDefault(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol:
                    return new WeaponData
                    {
                        type = WeaponType.Pistol,
                        weaponName = "Pistol",
                        fireRate = 2.0f,
                        damage = 10,
                        range = 28.0f,
                        bulletSpeed = 38.0f,
                        pelletsPerShot = 1,
                        spreadAngle = 1.0f,
                        fireSound = SoundEffectType.GunshotPistol
                    };

                case WeaponType.SMG:
                    return new WeaponData
                    {
                        type = WeaponType.SMG,
                        weaponName = "SMG",
                        fireRate = 5.0f,
                        damage = 12,
                        range = 30.0f,
                        bulletSpeed = 42.0f,
                        pelletsPerShot = 1,
                        spreadAngle = 3.5f,
                        fireSound = SoundEffectType.GunshotSMG
                    };

                case WeaponType.Shotgun:
                    return new WeaponData
                    {
                        type = WeaponType.Shotgun,
                        weaponName = "Shotgun",
                        fireRate = 1.4f,
                        damage = 15,
                        range = 22.0f,
                        bulletSpeed = 35.0f,
                        pelletsPerShot = 5,
                        spreadAngle = 14.0f,
                        fireSound = SoundEffectType.GunshotShotgun
                    };

                case WeaponType.AssaultRifle:
                    return new WeaponData
                    {
                        type = WeaponType.AssaultRifle,
                        weaponName = "Assault Rifle",
                        fireRate = 7.0f,
                        damage = 22,
                        range = 35.0f,
                        bulletSpeed = 48.0f,
                        pelletsPerShot = 1,
                        spreadAngle = 2.0f,
                        fireSound = SoundEffectType.GunshotRifle
                    };

                case WeaponType.Minigun:
                    return new WeaponData
                    {
                        type = WeaponType.Minigun,
                        weaponName = "Minigun",
                        fireRate = 15.0f,
                        damage = 28,
                        range = 38.0f,
                        bulletSpeed = 52.0f,
                        pelletsPerShot = 1,
                        spreadAngle = 4.0f,
                        fireSound = SoundEffectType.GunshotMinigun
                    };

                default:
                    return new WeaponData
                    {
                        type = WeaponType.None,
                        weaponName = "Unarmed",
                        fireRate = 0f,
                        damage = 0
                    };
            }
        }
    }
}
