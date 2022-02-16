using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquidGameDoor : MonoBehaviour {

    private HealthSystem healthSystem;
    private Image healthBarImage;

    private void Awake() {
        healthBarImage = transform.Find("HealthBarCanvas").Find("Bar").GetComponent<Image>();

        transform.Find("HealthBarCanvas").gameObject.SetActive(false);

        healthSystem = new HealthSystem(100);
        healthSystem.OnDamaged += HealthSystem_OnDamaged;
        healthSystem.OnDead += HealthSystem_OnDead;
    }

    private void HealthSystem_OnDead(object sender, System.EventArgs e) {
        transform.Find("HealthBarCanvas").gameObject.SetActive(true);
        transform.Find("DoorDestroyed").gameObject.SetActive(true);
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e) {
        healthBarImage.fillAmount = healthSystem.GetHealthNormalized();
        transform.Find("HealthBarCanvas").gameObject.SetActive(true);
    }

    public void Damage() {
        if (healthSystem.IsDead()) return;
        healthSystem.Damage(1);
    }

    public bool IsDead() {
        return healthSystem.IsDead();
    }

}