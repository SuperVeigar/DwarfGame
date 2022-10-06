using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasicButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private CommonSoundManager_DontDest m_soundManager;
    private InputField m_inputfield;
    private Button m_button;

    // Start is called before the first frame update
    void Start()
    {
        m_soundManager = FindObjectOfType<CommonSoundManager_DontDest>();
        m_inputfield = gameObject.GetComponent<InputField>();
        m_button = gameObject.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {

    }    

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_soundManager != null &&
            isAvailable())
        {
            m_soundManager.PlayMouseOverSound();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_soundManager != null &&
            isAvailable())
        {
            m_soundManager.PlayButtonClickSound();
        }
    }

    private bool isAvailable()
    {
        if (m_inputfield != null &&
            m_inputfield.interactable) return true;
        else if (m_button != null &&
            m_button.interactable) return true;

        return false;

    }
}
