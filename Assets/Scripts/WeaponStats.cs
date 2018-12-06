using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStats : MonoBehaviour {

    public int weaponDamage;
	void OnEnable () {
        if (this.gameObject.CompareTag("SingleSword"))
            GameObject.FindGameObjectWithTag("Player").GetComponent<OnePlayerMovement>().weaponType = 0;
        if (this.gameObject.CompareTag("DualSword"))
            GameObject.FindGameObjectWithTag("Player").GetComponent<OnePlayerMovement>().weaponType = 1;
        if (this.gameObject.CompareTag("GreatSword"))
            GameObject.FindGameObjectWithTag("Player").GetComponent<OnePlayerMovement>().weaponType = 2;   
    }
}
