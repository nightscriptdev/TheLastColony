using Components.Enemies;
using UnityEngine;

namespace Game
{
    public class VoidVortexEffect : SkillEffect
    {
        [SerializeField] private float rotationSpeed = -360f;

        private void Start()
        {
            Destroy(gameObject, skillData.duration);
        }

        private void Update()
        {
            transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.forward);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            other.GetComponent<EnemyComponent>().InstantKill();
        }
    }
}