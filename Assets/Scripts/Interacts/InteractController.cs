using UnityEngine;

public enum InteractType
{
    Input,
    Ricochete,
    Survival,
    None
}

public class InteractController : MonoBehaviour
{
    public static InteractController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void InteractToObject(int identity, InteractType interactType)
    {
        if (Instance == null) return;
        if (identity == 0) return;
        Debug.Log("Interagiu com o objeto!");

        GameManager.Instance.shakeController.ShakeAttackHitObject();

        switch (interactType)
        {
            case InteractType.Input:
                InteractInput(identity);
                break;
            case InteractType.Ricochete:
                Debug.Log("Interagiu com o objeto via Ricochete!");
                break;
            case InteractType.Survival:
                Debug.Log("Interagiu com o objeto via Survival!");
                break;
            default:
                Debug.Log("Tipo de interação desconhecido.");
                break;
        }
    }

    private void InteractInput(int identity)
    {
        InteractObject[] objects = GameObject.FindObjectsByType<InteractObject>(
            FindObjectsSortMode.None
        );

        InteractObject target = null;

        foreach (var obj in objects)
        {
            if (obj.identity == identity)
            {
                target = obj;
                break;
            }
        }

        if (target == null) return;

        target.isActive = true;
        target.luzFlame.intensity = target.luzIntensityTarget;
        target.colliderObject.enabled = false;
        target.particleFlame.Play();
    }
}