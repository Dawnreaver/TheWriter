using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriterKeyBehaviour : MonoBehaviour {

    public int m_keyIndex;
    public List<string> m_characters = new List<string>();
    Animation m_keyAnimation;
    public bool m_highlighted = false;
    Color m_keyColour;
    
    void Start() {
        Initilise();
    }

    void Update() {
        
    }

    // Animate Key
    // Change Colour

    public void AnimateKey() {
       m_keyAnimation.Play();
    }

    public void HighlightKey(Color highlightColour) {
        // if(!m_highlighted) {
        //     m_highlighted = true;
            StartCoroutine(ChangeColour(highlightColour, 0.25f));
        //}
    }
    public void RevertKeyColour() {
        m_highlighted = false;
        StartCoroutine(ChangeColour(m_keyColour, 0.25f));    
    }

    void Initilise(){
        if(!m_keyAnimation) {
           m_keyAnimation = GetComponent<Animation>();
        }
        m_keyColour = GetComponent<Renderer>().material.color;
    }

    IEnumerator ChangeColour(Color newColor, float duration) {
        Color currentColor = GetComponent<Renderer>().material.color;
        float counter = 0.0f;
        while(counter < duration) {
           counter += Time.deltaTime;
           Color colour = Color.Lerp(currentColor, newColor, counter/duration);
           GetComponent<Renderer>().material.color = colour;
           yield return null;
        }
    }
}
