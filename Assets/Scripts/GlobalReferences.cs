using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance{get;set;}
    public GameObject bulletImpactEffectPrefab;
    public GameObject grenadeExplosionEffect;
    public GameObject smokeGreandeEffect;
    public GameObject bloodSprayEffect;
    public int waveNumber;

    private void Awake()
    {
        if(Instance!=null && Instance!=this)
        {
            Destroy(gameObject);
        }else
        {
            Instance=this;
        }
    }
}
