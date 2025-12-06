using UnityEngine;
using System.Collections;

public class HumanoidResponse : MonoBehaviour {
  public Whisper.Samples.MicrophoneDemo notificator;

  public float movement_force = 5f;
  public float stop_distance = 1f;

  private Rigidbody rigid;
  private bool must_move = false;

  private void Start() {
    rigid = GetComponent<Rigidbody>();
    
    if (rigid == null) {
      Debug.LogError($"{gameObject.name}: ¡Falta componente Rigidbody!");
      return;
    }

    if (notificator != null) {
      notificator.OnRegularExpression += ReactToExpression;
    } else {
      Debug.LogWarning($"{gameObject.name}: No se asignó el notificator!");
    }
  }

  void ReactToExpression(string HumanoidType) {
    // Los humanoides tipo 1 reaccionan cuando el cubo toca un humanoide tipo 2
    if (HumanoidType == "Avanza") {
      Debug.Log($"{gameObject.name} (Humanoide Tipo1) se mueve");
      must_move = true;
    }
  }

  private void FixedUpdate() {
    if (must_move && rigid != null) {
      // Calcular dirección hacia el escudo
      Vector3 direction = new Vector3(0f, 0f, -1f);
      
      // Aplicar fuerza hacia el escudo
      rigid.AddForce(direction * movement_force);

      // Limitar velocidad máxima
      if (rigid.linearVelocity.magnitude > movement_force) {
        rigid.linearVelocity = rigid.linearVelocity.normalized * movement_force;
      }
    }
  }

  private void OnDestroy() {
    if (notificator != null) {
      notificator.OnRegularExpression -= ReactToExpression;
    }
  }
}