using EasyWeapons.Weapons.Modules.Attack.ShootingModes;
using EasyWeapons.Weapons.Modules.Attack;
using EasyWeapons.Weapons;
using Sandbox;
using EasyWeapons.Sounds;
using EasyWeapons.Inventories;
using EasyWeapons.Weapons.Modules.Reload;
using EasyWeapons.Recoiles.Modules;
using EasyWeapons.Effects;
using System.Collections.Generic;
using EasyWeapons.Effects.Animations;
using EasyWeapons.Effects.Animations.Parameters;
using EasyWeapons.Weapons.Modules.Aiming;
using EasyWeapons.Weapons.Modules.Aiming.Effects;
using static EasyWeapons.Weapons.Modules.Aiming.Effects.ViewModelPositioningEffect;
using EasyWeapons.Bullets.Spawners;
using EasyWeapons.Bullets.Datas;
using EasyWeapons.Bullets;

namespace EasyWeapons.Demo.Weapons;

[Spawnable]
[Library("ew_pistol")]
public partial class Pistol : Weapon
{
    public const string DefaultAmmoId = "pistol";

    public const int DefaultMaxAmmoInClip = 8;

    public const float HitForce = 5f;
    public const float Damage = 9f;

    public const float ReloadTime = 2.3f;

    public const float Spread = 0.05f;
    public const float Distance = 5000f;
    public const float TraceRadius = 3f;

    [Net, Local]
    protected BulletSpawner BulletSpawner { get; private set; }

    [Net, Local]
    protected OneTypeAmmoInventory Clip { get; private set; }


    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    static Pistol()
    {
        if(Game.IsServer)
        {
            var bulletsRegister = BulletsRegister.Instanse;
            if(bulletsRegister.Contains<InstantTraceBulletData>(DefaultAmmoId) == false)
                bulletsRegister.Add(DefaultAmmoId, new InstantTraceBulletData() { HitForce = HitForce, Damage = Damage });
        }
    }

    public Pistol()
    {
        if(Game.IsServer)
        {
            UseOwnerAimRay = true;
            DefaultLocalAimRay = new Ray(new(11f, 0f, 4.6f), Vector3.Forward);
            DeployEffects = new List<WeaponEffect>()
            {
                new SoundEffect()
                {
                    Side = Networking.NetworkSide.Client,
                    Sound = new DelayedSound("rust_pistol.deploy")
                },
                new OwnerAnimationEffect() { AnimationParameter = AnimationParameter.Of("b_deploy", true) }
            };
            DeployTime = 0.5f;
            BulletSpawner = new InstantTraceBulletSpawner(Spread, Distance, TraceRadius, this);
            Clip = OneTypeAmmoInventory.Full(DefaultAmmoId, DefaultMaxAmmoInClip);

            var attackModule = new SimpleAttackModule(Clip, BulletSpawner, new SemiShootingMode())
            {
                AttackEffects = new List<WeaponEffect>()
                {
                    new SoundEffect()
                    {
                        Side = Networking.NetworkSide.Server,
                        Sound = new DelayedSound("rust_pistol.shoot")
                    },
                    new ParticleEffect()
                    {
                        Name = "particles/pistol_muzzleflash.vpcf",
                        Attachment = "muzzle",
                        ShouldFollow = false
                    },
                    new ViewModelAnimationEffect() { AnimationParameter = AnimationParameter.Of("fire", true) },
                    new OwnerAnimationEffect() { AnimationParameter = AnimationParameter.Of("b_attack", true) }
                },
                DryfireEffects = new List<WeaponEffect>()
                {
                    new SoundEffect()
                    {
                        Side = Networking.NetworkSide.Server,
                        Sound = new DelayedSound("rust_pistol.dryfire")
                    }
                },
                FireRate = 10f,
                Recoil = new RandomRecoil() { XRecoil = new RangedFloat(-1, 1), YRecoil = 6 },
                NoOwnerRecoilForce = 50000
            };

            var reloadModule = new SimpleReloadModule(Clip)
            {
                ReloadTime = ReloadTime,
                ReloadEffects = new List<WeaponEffect>()
                {
                    new SoundEffect()
                    {
                        Side = Networking.NetworkSide.Server,
                        Sound = DelayedSoundList.AllFromStart(new DelayedSound("rust_pistol.eject_clip", 0.2f), new DelayedSound("rust_pistol.grab_clip", 0.4f), new DelayedSound("rust_pistol.insert_clip", 1.35f)),
                    },
                    new ViewModelAnimationEffect() { AnimationParameter = AnimationParameter.Of("reload", true) },
                    new OwnerAnimationEffect() { AnimationParameter = AnimationParameter.Of("b_reload", true) }
                },
                ReloadFailEffects = new List<WeaponEffect>()
                {
                    new SoundEffect()
                    {
                        Side = Networking.NetworkSide.Server,
                        Sound = new DelayedSound("no_ammo")
                    }
                }
            };

            var aimingModule = new AimingModule()
            {
                Effects = new List<AimingEffect>()
                {
                    new ViewModelPositioningEffect(this)
                    {
                        DefaultPositioning = new ViewModelPositioning(),
                        AimedPositioning = new ViewModelPositioning()
                        {
                            PositionOffset = new Vector3(0, 16.77f, 2.5f),
                            FieldOfView = 40f
                        }
                    },
                    new FovEffect(this)
                    {
                        FieldOfView = 50
                    }
                }
            };

            Components.Add(attackModule);
            Components.Add(reloadModule);
            Components.Add(aimingModule);
        }
        else
        {

            BulletSpawner = null!;
            Clip = null!;
        }
    }

    public override void Spawn()
    {
        base.Spawn();
        SetModel("weapons/rust_pistol/rust_pistol.vmdl");
    }
}
