using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriterBehaviour : MonoBehaviour {

    public List<GameObject> m_typewriterKeys = new List<GameObject>();
    Dictionary<string,int> m_charaterMapping = new Dictionary<string,int>();
    void Start(){
        Initialise();
    }

    void LateUpdate(){
        ReadKeyboardInput();
    }

    void Initialise(){
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

    void ReadKeyboardInput(){
        string keyboardInput ="";
        foreach (char c in Input.inputString){
            keyboardInput += c;
        }
        if(keyboardInput == " "){
            Debug.Log("Pressed space.");
        }else if(keyboardInput.Length == 1){
        Debug.Log("Input: "+keyboardInput);
        }
    }
}
