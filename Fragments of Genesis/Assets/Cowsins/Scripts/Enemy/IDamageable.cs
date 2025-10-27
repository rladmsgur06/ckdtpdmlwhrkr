using UnityEngine;

namespace cowsins2D
{
    // Every damageable object in 2D Platformer Engine inherits from IDamageable
    public interface IDamageable
    { 
        void Damage(float damage);
        void Die(bool condition);
    }
}
