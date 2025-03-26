using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float _health;

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log(_health);
    }
}
