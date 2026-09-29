using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;

    private void OnCollisionEnter(Collision objectWeHit)
    {
        if (objectWeHit.gameObject.CompareTag("Target"))
        {
            print("hit " + objectWeHit.gameObject.name + "!");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
            return;
        }

        if (objectWeHit.gameObject.CompareTag("Wall"))
        {
            print("Hit the Wall");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
            return;
        }

        if (objectWeHit.gameObject.CompareTag("Beer"))
        {
            print("hit a Beer bottle");
            BeerBottle bottle = objectWeHit.gameObject.GetComponent<BeerBottle>();
            if (bottle != null)
            {
                bottle.Shatter();
            }
            return;
        }

        // Check if hit an Enemy (either on current collider or parent)
        Enemy enemy = objectWeHit.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null || objectWeHit.gameObject.CompareTag("Enemy"))
        {
            print("Hit Zombie");
            if (enemy != null && !enemy.isDead)
            {
                enemy.TakeDamage(bulletDamage);
            }
            CreateBloodSprayEffect(objectWeHit);
            Destroy(gameObject);
            return;
        }
    }

    private void CreateBloodSprayEffect(Collision objectWeHit)
    {
        if (GlobalReferences.Instance == null || GlobalReferences.Instance.bloodSprayEffect == null) return;
        if (objectWeHit.contacts == null || objectWeHit.contacts.Length == 0) return;

        ContactPoint contact = objectWeHit.contacts[0];
        GameObject bloodSprayPrefab = Instantiate(
            GlobalReferences.Instance.bloodSprayEffect,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        bloodSprayPrefab.transform.SetParent(objectWeHit.gameObject.transform);
    }

    private void CreateBulletImpactEffect(Collision objectWeHit)
    {
        if (GlobalReferences.Instance == null || GlobalReferences.Instance.bulletImpactEffectPrefab == null) return;
        if (objectWeHit.contacts == null || objectWeHit.contacts.Length == 0) return;

        ContactPoint contact = objectWeHit.contacts[0];
        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        hole.transform.SetParent(objectWeHit.gameObject.transform);
    }
}
