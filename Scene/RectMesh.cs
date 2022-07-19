using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectMesh : MonoBehaviour
{
    private Text moveText;
    private Text targetText;
    private Text totalText;
    private Image image;
    private void Awake()
    {
        var _transform = transform;
        moveText = _transform.Find("MoveText").GetComponent<Text>();
        targetText = _transform.Find("TargetText").GetComponent<Text>();
        totalText = _transform.Find("TotalText").GetComponent<Text>();
        image = _transform.GetComponent<Image>();
    }
    public void SetMoveCost(string value)
    {
        moveText.text = value;
    }
    public void SetTargetCost(string value)
    {
        targetText.text = value;
    }
    public void SetTotalCost(string value)
    {
        totalText.text = value;
    }
    public void SetImageColor(Color color)
    {
        image.color = color;
    }
}
