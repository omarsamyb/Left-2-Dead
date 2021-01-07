using UnityEngine;

public class Rage : MonoBehaviour
{
    private float rageResetRef = 3f;
    private float rageDurationRef = 7f;
    private float rageReset;
    private float rageDuration;
    private int ragePoints;
    private bool canActivate;
    private TextMesh HUD_rage;

    void Start()
    {
        rageReset = 3f;
        rageDuration = 7f;
        ragePoints = 0;
    }

    void Update()
    {
        if (rageReset >= 0)
            rageReset -= Time.deltaTime;
        if (rageReset <= 0 && !canActivate)
            ragePoints = 0;

        if (GameManager.instance.inRageMode)
        {
            rageDuration -= Time.deltaTime;
            if (rageDuration <= 0f)
            {
                GameManager.instance.inRageMode = false;
                ragePoints = 0;
                canActivate = false;
                rageDuration = rageDurationRef;
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && canActivate && !GameManager.instance.inRageMode)
            GameManager.instance.inRageMode = true;
    }

    public void UpdateRage(string tag)
    {
        rageReset = rageResetRef;
        if (tag[0] == 'S')
            ragePoints = Mathf.Clamp(ragePoints + 50, 0, 100);
        else
            ragePoints = Mathf.Clamp(ragePoints + 10, 0, 100);
        if (ragePoints == 100)
            canActivate = true;
    }

    void OnGUI()
    {
        if (!HUD_rage)
        {
            try
            {
                HUD_rage = GameObject.Find("HUD_rage").GetComponent<TextMesh>();
            }
            catch (System.Exception ex)
            {
                print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
            }
        }
        if (HUD_rage)
            HUD_rage.text = ragePoints.ToString() + " - " + rageReset.ToString() + " - " + rageDuration.ToString();
    }
}
