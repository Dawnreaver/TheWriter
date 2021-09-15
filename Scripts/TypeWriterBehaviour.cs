using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeWriterBehaviour : MonoBehaviour {
 
    public GameObject m_typewriterCarriage;
    public GameObject m_typewriterReturnLever;
    [SerializeField]
    bool m_isMoving = false;
    bool m_shiftIsActive = false;
    float m_carriageMoveMin = -0.13f;
    float m_carriageMoveMax = 0.13f;
    int m_maxCharacterCount = 68;
    [SerializeField]
    int m_tempCharacterCount = 0;
    [SerializeField]
    AudioSource m_typewriterAudio;
    public List<AudioClip> m_typewriterSounds = new List<AudioClip>();
    public List<GameObject> m_typewriterKeys = new List<GameObject>();
    public List<GameObject> m_typewriterLetterbars = new List<GameObject>();
    Dictionary<string,int> m_charaterMapping = new Dictionary<string,int>();
    ///// key highlighting
    public Color m_highlightColour = Color.red;

    ////// word processing
    List<string> m_targetWordLetters = new List<string>(); 

    void Start() {
        Initialise();
    }

    void Update() {
        if(Input.GetButtonDown("Fire1") && m_tempCharacterCount <= m_maxCharacterCount) {
            ReadMouseInput();
        }
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

    void ReadMouseInput() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100)) {
            if(hit.collider.tag == "key") {
                TypeWriterKeyBehaviour key = hit.collider.gameObject.GetComponent<TypeWriterKeyBehaviour>();
                string character ="";
                if(!m_shiftIsActive) {
                    character = key.m_characters[0];
                }
                else {
                    character = key.m_characters[1];
                }
                PressedKey(key.m_keyIndex, character);
            }else if(hit.collider.tag == "carriageReturnLever"){
                // animate lever
                m_typewriterReturnLever.GetComponent<Animation>().Play();
                // return carriage
                ResetCarriage();
                // roll up paper one line
            }else if(hit.collider.tag == "rollerKnob"){
                RollToNextLine();
            }
        }
    }

    void PressedKey(int index, string character ="") {
        TypeWriterKeyBehaviour key = m_typewriterKeys[index-1].GetComponent<TypeWriterKeyBehaviour>();
        if(m_tempCharacterCount < m_maxCharacterCount) {
        }
        if(character == " "){
            m_typewriterAudio.clip = m_typewriterSounds[5];
        }
        else if(character != " " && character != "") {
            m_typewriterAudio.clip = m_typewriterSounds[Random.Range(0,5)];
            m_typewriterLetterbars[index-1].GetComponent<Animation>().Play();
        }
        else {
            if(m_typewriterAudio.clip) {
                m_typewriterAudio.clip = null;
            }
            Debug.Log("Unexpected character: "+character);
        }
        if(key.m_highlighted) {
            key.RevertKeyColour();
        }
        key.AnimateKey();
        m_typewriterAudio.Play();
        m_tempCharacterCount += 1;
        if(m_tempCharacterCount == m_maxCharacterCount) {
            m_typewriterCarriage.GetComponent<AudioSource>().clip = m_typewriterSounds[6];
            m_typewriterCarriage.GetComponent<AudioSource>().Play();
        }

        if(m_targetWordLetters.Count != 0) {
            for(int a = 0; a < m_targetWordLetters.Count; a++) {
                if(m_targetWordLetters[a] == character) {
                    m_targetWordLetters.RemoveAt(a);
                    HighlightKeys();
                    break;
                }
            }
        }
        StartCoroutine(MoveCarriage(0.3f/m_maxCharacterCount,0.1f, false));   
    }
    
    void ReadKeyboardInput() { 
        string keyboardInput ="";
        foreach (char c in Input.inputString) {
            keyboardInput += c;
        }
        if(keyboardInput != "" && keyboardInput != "r"  && keyboardInput != "h" && m_tempCharacterCount < m_maxCharacterCount && !m_isMoving) {
            int a;
            m_charaterMapping.TryGetValue(keyboardInput, out a);
            PressedKey(a, keyboardInput);
        } else if (keyboardInput == "r") {
            ResetCarriage();
        } else if (keyboardInput == "h") {
            SetTargetWord("deseased");
            HighlightKeys();
        }
    }

    void ResetCarriage() {
        m_tempCharacterCount = 0;
        StartCoroutine(MoveCarriage(0.0f,0.5f,true));
    }

    IEnumerator MoveCarriage(float distance, float duration, bool reset = false) {

        if(m_isMoving) {
            yield break;
        }
        m_isMoving = true;

        float counter = 0.0f;
        Vector3 currentPos = m_typewriterCarriage.transform.position;
        Vector3 movedPos = m_typewriterCarriage.transform.position;
        if(reset) {
            movedPos = new Vector3(currentPos.x,currentPos.y, m_carriageMoveMin);
        } else {
            movedPos = new Vector3(currentPos.x,currentPos.y, currentPos.z+distance);
        }
        
        while (counter < duration) {
            counter += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(currentPos, movedPos, counter/duration);
            m_typewriterCarriage.transform.position = newPos;
            yield return null;
        }
        m_isMoving = false;
    }

    public void HighlightKeys() {
        if(m_targetWordLetters.Count > 0) {
            int l;
            TypeWriterKeyBehaviour key;
            for (int a = 0; a < m_targetWordLetters.Count; a++) {
                
                if(m_charaterMapping.TryGetValue(m_targetWordLetters[a], out l)){
                m_typewriterKeys[l-1].GetComponent<TypeWriterKeyBehaviour>().HighlightKey(m_highlightColour);
                }
                else {
                    Debug.Log("Failed to get entry from dictionary for string: "+m_targetWordLetters[a]);
                }
            }
        }
    }

    public void WriteStringAutomatically(string automatedText) { // Maybe an I enumerator?
        // Devide automated text in separate strings
        // check string
        // perform actions for special string e.g. "return" will return the carriage and go to the next line 
        // start and wait for other coroutine to finish?
        // type characters
        // keep track of type caracters
        // return carriage when max characters reached
    }

    public void SetTargetWord(string word) {
        // make sure the list of target letters is empty
        m_targetWordLetters.Clear();
        // split the word into letters
        char[] letters = word.ToCharArray();
        Debug.Log("Number of letters in "+word+": "+letters.Length);
        foreach( char s in letters) {
            m_targetWordLetters.Add(""+s);
        }
        Debug.Log(m_targetWordLetters.Count);
        HighlightKeys();
    }

    public void RollToNextLine() { // May want to find a nicer name

    }
}