using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriterBehaviour : MonoBehaviour {

    
    [SerializeField]
    int m_maxCharacterCount = 68;
    int m_tempCharacterCount = 0;
    [SerializeField]
    AudioSource m_typewriterAudio;
    public List<AudioClip> m_typewriterSounds = new List<AudioClip>();
    public List<GameObject> m_typewriterKeys = new List<GameObject>();
    Dictionary<string,int> m_charaterMapping = new Dictionary<string,int>();
    void Start() {
        Initialise();
    }

    void LateUpdate() {
        ReadKeyboardInput();
    }

    void Initialise() {
        if(!m_typewriterAudio) {
            m_typewriterAudio = GetComponent<AudioSource>();
        }
        for(int a = 0; a < m_typewriterKeys.Count; a++) {
            TypeWriterKeyBehaviour temp = m_typewriterKeys[a].GetComponent<TypeWriterKeyBehaviour>();
            int v;
            for(int b = 0; b < temp.m_characters.Count; b++) {

                if(!m_charaterMapping.TryGetValue(temp.m_characters[b], out v)) {
                    m_charaterMapping.Add(temp.m_characters[b],temp.m_keyIndex);
                }else{
                    Debug.Log("Unable to add value: "+temp.m_characters[b]);
                }
            }
        }
        Debug.Log("Dictionary length: "+m_charaterMapping.Count);
    }

    void ReadKeyboardInput() {
        string keyboardInput ="";
        foreach (char c in Input.inputString) {
            keyboardInput += c;
        }
        if(keyboardInput != "" && m_tempCharacterCount < m_maxCharacterCount) {
            int a;
            m_charaterMapping.TryGetValue(keyboardInput, out a);
            m_typewriterKeys[a-1].GetComponent<TypeWriterKeyBehaviour>().AnimateKey();
            if(keyboardInput != " ") {
                m_typewriterAudio.clip = m_typewriterSounds[Random.Range(0,5)];
            }else if(keyboardInput == " ") {
                m_typewriterAudio.clip = m_typewriterSounds[5];
            }
            m_typewriterAudio.Play();
            m_tempCharacterCount += 1;
            //Debug.Log("Input: "+keyboardInput+" Index: "+a);
        }
    }
}
