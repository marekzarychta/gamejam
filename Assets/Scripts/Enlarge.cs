using UnityEngine;

public class Enlarge : MonoBehaviour
{
    public float scaleFactor = 2.0f;
    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            // Fallback dla obiektów z³o¿onych (szuka w dzieciach)
            myCollider = GetComponentInChildren<Collider>();
        }
    }

    void OnEnable()
    {
        if (myCollider != null)
        {
            ResizeAndReposition(scaleFactor);
        }
        else
        {
            // Fallback, jeœli nie ma collidera (stara metoda, ¿eby nie wywali³o b³êdu)
            transform.localScale *= scaleFactor;
        }
    }

    void OnDisable()
    {
        if (myCollider != null)
        {
            // Przywracamy skalê (mno¿ymy przez odwrotnoœæ, czyli 1/scaleFactor)
            ResizeAndReposition(1f / scaleFactor);
        }
        else
        {
            transform.localScale /= scaleFactor;
        }
    }

    void ResizeAndReposition(float factor)
    {
        // 1. Zapamiêtujemy, gdzie DOK£ADNIE w œwiecie jest spód obiektu przed zmian¹
        float oldBottomY = myCollider.bounds.min.y;

        // 2. Skalujemy obiekt
        transform.localScale *= factor;

        // WA¯NE: Wymuszamy odœwie¿enie fizyki/transformacji. 
        // Bez tego bounds.min.y w nastêpnej linijce mog³oby zwróciæ star¹ wartoœæ.
        Physics.SyncTransforms();

        // 3. Sprawdzamy, gdzie spód wyl¹dowa³ po skalowaniu (np. wbi³ siê pod ziemiê)
        float newBottomY = myCollider.bounds.min.y;

        // 4. Obliczamy ró¿nicê
        float difference = oldBottomY - newBottomY;

        // 5. Przesuwamy obiekt w górê/dó³ o tê ró¿nicê, 
        // ¿eby nowy spód znalaz³ siê w miejscu starego spodu.
        transform.position += Vector3.up * difference;
    }
}