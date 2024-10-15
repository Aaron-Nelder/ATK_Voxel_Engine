using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
    [SerializeField] Image frame;
    [SerializeField] Image fill;

    [SerializeField] Color fillColorFull;
    [SerializeField] Color fillColorEmpty;

    [Header("Division Settings")]
    [SerializeField] Sprite divisionSprite;
    [SerializeField] Color divisionColor;
    [SerializeField] int divisionWidth;
    [SerializeField] int divisionCount;
    [SerializeField] List<Image> divisions = new();

    public void SetFill(float fillAmount)
    {
        fill.fillAmount = fillAmount;
        fill.color = Color.Lerp(fillColorEmpty, fillColorFull, fillAmount);
    }

    [ContextMenu("Test Fill")]
    public void TestFill()
    {
        SetFill(Random.Range(0f, 1f));
    }

    [ContextMenu("Spawn Division")]
    void SpawnDivision()
    {
        for (int i = 0; i < divisions.Count; i++)
        {
            if (divisions[i] != null)
                DestroyImmediate(divisions[i].gameObject);
        }

        divisions.Clear();

        for (int i = 0; i < divisionCount; i++)
        {
            GameObject division = new GameObject("Division", typeof(RectTransform), typeof(Image));
            division.transform.SetParent(frame.transform);
            division.transform.localRotation = fill.transform.localRotation;

            Image imageComponent = division.GetComponent<Image>();
            imageComponent.sprite = divisionSprite;
            imageComponent.color = divisionColor;

            divisions.Add(imageComponent);

            RectTransform divisionTransform = division.GetComponent<RectTransform>();

            divisionTransform.sizeDelta = new Vector2(divisionWidth, fill.rectTransform.rect.height);

            divisionTransform.localPosition = new Vector2(0, 0);
            divisionTransform.localScale = Vector3.one;
        }
        PlaceDivisions();
    }

    void PlaceDivisions()
    {
        float startingX = fill.rectTransform.rect.width * 0.5f;

        float divisionSpacing = fill.rectTransform.rect.width / (divisionCount + 1);

        for (int i = 0; i < divisions.Count; i++)
        {
            RectTransform divisionTransform = divisions[i].GetComponent<RectTransform>();

            divisionTransform.localPosition = new Vector2((divisionSpacing * (i + 1)) - startingX, 0);
        }
    }

}
