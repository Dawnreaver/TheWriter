using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriterKeyBehaviour : MonoBehaviour {

    public int m_keyIndex;
    public List<string> m_characters = new List<string>();
    Animation m_keyAnimation;
    
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

    void Initilise(){
        if(!m_keyAnimation) {
           m_keyAnimation = GetComponent<Animation>();
        }
    }
}
