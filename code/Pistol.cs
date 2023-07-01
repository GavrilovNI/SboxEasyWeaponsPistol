using EasyWeapons.Bullets.Spawners;
using EasyWeapons.Weapons.Modules.Attack.ShootingModes;
using EasyWeapons.Weapons.Modules.Attack;
using EasyWeapons.Weapons;
using Sandbox;

namespace EasyWeapons.Demo.Weapons;

[Spawnable]
[Library("ew_pistol")]
public partial class Pistol : Weapon
{
    public const float Force = 150f;
    public const float Spread = 0.05f;
    public const float Damage = 9f;
    public const float Distance = 5000f;
    public const float BulletSize = 3f;

    [Net, Local]
    protected BulletSpawner BulletSpawner { get; private set; }


    public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    public Pistol()
    {
        SetModel("weapons/rust_pistol/rust_pistol.vmdl");

        if(Game.IsServer)
        {
            DefaultLocalAimRay = new Ray(new(11f, 0f, 4.6f), Vector3.Forward);
            DeployTime = 0.5f;
            BulletSpawner = new TraceBulletSpawner(Spread, Force, Damage, Distance, BulletSize, this);

            var attackModule = new SimpleAttackModule(/*Clip, */BulletSpawner, new SemiShootingMode())
            {
                FireRate = 10f
            };


            Components.Add(attackModule);
        }
        else
        {

            BulletSpawner = null!;
        }
    }
}
