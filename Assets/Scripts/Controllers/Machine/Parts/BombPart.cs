using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public class BombPart : FixedPart
    {
        public ParticleSystem explosionFX;
        public GameObject bombMesh;

        private readonly PropertyValue<float> _propertyExplosionImpulse = new PropertyValue<float>(1000f);
        private readonly PropertyValue<float> _propertyExplosionRadius = new PropertyValue<float>(10f);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Impulse", _propertyExplosionImpulse));
            Properties.Add(new MachineProperty("Radius", _propertyExplosionRadius));
        }

        private IEnumerator Explode()
        {
            bombMesh.GetComponent<Renderer>().enabled = false;

            // 对周围刚体施加冲击力
            Collider[] colliders = Physics.OverlapSphere(
                transform.position,
                _propertyExplosionRadius.Value,
                LayerMask.GetMask("PartContour"));

            foreach (Collider col in colliders)
            {
                // 这样对每个刚体都会施加冲击力,可以实现对不同形状机器冲击力效果的差异
                if (col.transform.parent != null)
                {
                    Rigidbody rb = col.transform.parent.GetComponent<ISimulator>()?.GetSimulatorRigidbody();
                    if (rb != null && rb != SimulatorRigidbody)
                    {
                        rb.AddExplosionForce(
                            _propertyExplosionImpulse.Value,
                            transform.position,
                            _propertyExplosionRadius.Value);
                    }
                }
            }

            explosionFX.Play();
            Debug.Log("Explosion!");

            float remainTime = explosionFX.main.duration + explosionFX.main.startLifetime.constantMax;
            yield return new WaitForSeconds(remainTime);
            RemovePart(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                StartCoroutine(Explode());
            }
        }

        private void OnDrawGizmos()
        {
            if (IsDrawGizmos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _propertyExplosionRadius.Value);
            }
        }
    }
}