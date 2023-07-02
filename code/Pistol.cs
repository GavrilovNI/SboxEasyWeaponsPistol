using EasyWeapons.Bullets.Spawners;
using EasyWeapons.Weapons.Modules.Attack.ShootingModes;
using EasyWeapons.Weapons.Modules.Attack;
using EasyWeapons.Weapons;
using Sandbox;
using EasyWeapons.Sounds;
using EasyWeapons.Inventories;
using EasyWeapons.Weapons.Modules.Reload;

namespace EasyWeapons.Demo.Weapons;

[Spawnable]
[Library("ew_pistol")]
public partial class Pistol : Weapon
{
    public const int DefaultMaxAmmoInClip = 8;
    public const float Force = 150f;
    public const float Spread = 0.05f;
    public const float Damage = 9f;
    public const float Distance = 5000f;
    public const float BulletSize = 3f;
    public const float ReloadTime = 2.3f;

    [Net, Local]
    protected BulletSpawner BulletSpawner { get; private set; }

    [Net, Local]
    protected OneTypeAmmoInventory Clip { get; private set; }


    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    public Pistol()
    {
        if(Game.IsServer)
        {
            DefaultLocalAimRay = new Ray(new(11f, 0f, 4.6f), Vector3.Forward);
            DeploySound = new DelayedSound("rust_pistol.deploy");
            DeployTime = 0.5f;
            BulletSpawner = new TraceBulletSpawner(Spread, Force, Damage, Distance, BulletSize, this);
            Clip = OneTypeAmmoInventory.Full("pistol", DefaultMaxAmmoInClip);

            var attackModule = new SimpleAttackModule(Clip, BulletSpawner, new SemiShootingMode())
            {
                AttackSound = new DelayedSound("rust_pistol.shoot"),
                DryfireSound = new DelayedSound("rust_pistol.dryfire"),
                FireRate = 10f
            };

            var reloadModule = new SimpleReloadModule(Clip)
            {
                ReloadTime = ReloadTime,
                ReloadSound = DelayedSoundList.AllFromStart(new DelayedSound("rust_pistol.eject_clip", 0.2f), new DelayedSound("rust_pistol.grab_clip", 0.4f), new DelayedSound("rust_pistol.insert_clip", 1.35f)),
                ReloadFailSound = new DelayedSound("no_ammo"),

            };

            Components.Add(attackModule);
            Components.Add(reloadModule);
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
