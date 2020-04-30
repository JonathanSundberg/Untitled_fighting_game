using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _text_mesh;

    public event Action OnClick;
    public string Label
    {
        get => _text_mesh.text;
        set => _text_mesh.text = value;
    }
    
    void Awake()
    {
        _button.onClick.AddListener(Click);
    }

    public void Click()
    {
        OnClick?.Invoke();
    }
}
