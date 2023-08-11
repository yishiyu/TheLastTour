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

        private readonly PropertyValue<float> _propertyTriggerImpulse = new PropertyValue<float>(10f);

        protected override void InitProperties()
        {
            base.InitProperties();

            Properties.Add(new MachineProperty("Trigger Impulse", _propertyTriggerImpulse));
        }

        private IEnumerator Explode()
        {
            bombMesh.GetComponent<Renderer>().enabled = false;
            
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

        // private void OnCollisionEnter(Collision collision)
        // {
        //     if (collision.impulse.magnitude > 1f && collision.collider.CompareTag("Player"))
        //     {
        //         // Explode();
        //         Debug.Log("collision.impulse.magnitude: " + collision.impulse.magnitude);
        //     }
        // }
    }
}