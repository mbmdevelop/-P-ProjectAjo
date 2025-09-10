using UnityEngine;
using System.Collections;
using TMPro;

public class DamageTextPopUpController : MonoBehaviour
{
    [Header("PARAMETERS:")]
    [Header("DisappearAnim")]
    [SerializeField] private AnimationCurve AlphaCurve = null;
    [SerializeField] private AnimationCurve PosCurve = null;
    [SerializeField] private float YAddend = 0.0f;
    private Vector2 InitPos = Vector2.zero;

    private TextMeshPro DamageText;


    private void Awake()
    {
        DamageText = GetComponentInChildren<TextMeshPro>();

        InitPos = transform.position;
    }

    private void Start()
    {
        StartCoroutine(DisappearAnimRoutine());
    }

    public void SetDamageText(string Damage)
    {
        DamageText.text = Damage;
    }

    private IEnumerator DisappearAnimRoutine()
    {
        float ElapsedTime = 0.0f;

        float AlphaCurveDuration = AlphaCurve[AlphaCurve.length - 1].time;
        float PosCurveDuration = PosCurve[PosCurve.length - 1].time;
        float Duration = Mathf.Max(AlphaCurveDuration, 0.0f);

        Vector3 DestPos = new Vector3(InitPos.x, InitPos.y + YAddend);

        while (ElapsedTime < Duration)
        {
            DamageText.alpha = Mathf.Lerp(1.0f, 0.0f, AlphaCurve.Evaluate(ElapsedTime));

            transform.position = Vector3.Lerp(InitPos, DestPos, PosCurve.Evaluate(ElapsedTime));

            ElapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }
}