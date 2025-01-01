/*using UnityEngine;
using Units; // Assuming your Unit class is in this namespace

public class Projectile : MonoBehaviour
{
    private Unit _target;
    private float _speed;
    private int _damage;
    private bool _initialized = false;

    public void Initialize(Unit target, float speed, int damage)
    {
        _target = target;
        _speed = speed;
        _damage = damage;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || _target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 direction = (_target.transform.position - transform.position).normalized;
        
        transform.position += direction * _speed * Time.deltaTime;
        transform.forward = direction;
        
        float distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
        if (distanceToTarget < 0.5f) 
        {
            _target.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}*/