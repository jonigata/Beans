using UnityEngine;
using System.Collections;

public class ParticleAutoDestroy : MonoBehaviour {

    new ParticleSystem particleSystem;

    void Start () {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        if (particleSystem == null) { 
            Debug.LogErrorFormat("ParticleAutoDestroy({0}): can't find ParticleSystem", gameObject.name);
            this.enabled = false;
        }
    }

    void Update(){
        if (particleSystem == null) { 
            Debug.LogErrorFormat("ParticleAutoDestroy({0}): ParticleSystem has been broken", gameObject.name);
            this.enabled = false;
            return;
        }

        if(!particleSystem.IsAlive()){
            Destroy(this.gameObject);
        }
    }
}
