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

    /// automatic typing
    bool m_isTypingAutonomously = false;
    string m_testString = "qweasdzxc zcsda edc edws asdqw qweasdzxc qweasdzxc /return asdqwe qwe qwe asdzxc qweasd asdqw a asdwe zxcasdqwe qweasd zxcasd ed ws qa zxcasd asdqw asdzxc";

    ////// word processing
    List<string> m_targetWordLetters = new List<string>(); 
    string m_typedWord ="";
    string m_targetWord = "was";

    ////// paper feed

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
                }
                else{
                    Debug.Log("Unable to add value: "+temp.m_characters[b]);
                }
            }
        }
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
            }else if(hit.collider.tag == "carriageReturnLever") {
                // animate lever
                m_typewriterReturnLever.GetComponent<Animation>().Play();
                // return carriage
                ResetCarriage();
                // roll up paper one line
            }else if(hit.collider.tag == "rollerKnob") {
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
            m_typedWord +=character;
        }
        else if(character != " " && character != "") {
            m_typewriterAudio.clip = m_typewriterSounds[Random.Range(0,5)];
            m_typewriterLetterbars[index-1].GetComponent<Animation>().Play();
            m_typedWord +=character;
            ValidateTypedWord(index-1);
        }
        else {
            if(m_typewriterAudio.clip) {
                m_typewriterAudio.clip = null;
            }
            Debug.Log("Unexpected character: "+character);
        }
        // if(key.m_highlighted) {
        //     key.RevertKeyColour();
        // }
        key.AnimateKey();
        m_typewriterAudio.Play();
        m_tempCharacterCount += 1;
        if(m_tempCharacterCount == m_maxCharacterCount) {
            m_typewriterCarriage.GetComponent<AudioSource>().clip = m_typewriterSounds[6];
            m_typewriterCarriage.GetComponent<AudioSource>().Play();
            
            // Need to give feedack that the player needs to return the typewriter carriage
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
        if(keyboardInput != "" && keyboardInput != "r"  && keyboardInput != "h" && keyboardInput != "t" && m_tempCharacterCount < m_maxCharacterCount && !m_isMoving) {
            int a;
            m_charaterMapping.TryGetValue(keyboardInput, out a);
            PressedKey(a, keyboardInput);
        } 
        else if (keyboardInput == "r") { // testing only, needs to be removed once the whole keyboard is in use
            ResetCarriage();
        } 
        else if (keyboardInput == "h") { // testing only, needs to be removed once the whole keyboard is in use
            SetTargetWord(m_targetWord);
            HighlightKeys();
        }
        else if (keyboardInput == "t") { // testing only, needs to be removed once the whole keyboard is in use
            StartCoroutine(WriteStringAutomatically(m_testString));
        }
    }

    void ResetCarriage() {
        float baseReturnTime = 0.5f;

        Debug.Log("a = "+m_tempCharacterCount+" b = "+m_maxCharacterCount+ "a/b = "+m_tempCharacterCount/m_maxCharacterCount);
        float returnTime = baseReturnTime*((float)m_tempCharacterCount/(float)m_maxCharacterCount);
        Debug.Log(returnTime);
        StartCoroutine(MoveCarriage(0.0f,returnTime,true));
        m_typewriterReturnLever.GetComponent<Animation>().Play();
        m_tempCharacterCount = 0;
    }

    IEnumerator MoveCarriage(float targetZpos, float duration, bool reset = false) {

        if(m_isMoving) {
            yield break;
        }
        m_isMoving = true;

        float counter = 0.0f;
        Vector3 currentPos = m_typewriterCarriage.transform.position;
        Vector3 movedPos = m_typewriterCarriage.transform.position;
        if(reset) {
            movedPos = new Vector3(currentPos.x,currentPos.y, m_carriageMoveMin);
        } 
        else {
            movedPos = new Vector3(currentPos.x,currentPos.y, currentPos.z+targetZpos);
        }
        
        while (counter < duration) {
            counter += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(currentPos, movedPos, counter/duration);
            m_typewriterCarriage.transform.position = newPos;
            yield return null;
        }
        if(reset) {
            RollToNextLine();
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

    void UnHighlightKeys() {
        for (int a = 0; a < m_typewriterKeys.Count; a++) {
            m_typewriterKeys[a].GetComponent<TypeWriterKeyBehaviour>().RevertKeyColour();
        }
        Debug.Log("Reset all key highlighting");
    }

    public IEnumerator WriteStringAutomatically(string automatedText, float typeTime = 0.15f, float carriageReturnTime = 1.0f) {
        m_isTypingAutonomously = true;
        string[] words = automatedText.Split();
        int wordCount = words.Length;
        for(int a = 0; a < wordCount; a++) {
            if(words[a] != "/return") {
                char[] letters = words[a].ToCharArray();
                // check if the word fits on page
                if(m_tempCharacterCount+letters.Length+1 <= m_maxCharacterCount) {
                    foreach (char l in letters) {
                    
                        int b;
                        string character = l.ToString();
                        m_charaterMapping.TryGetValue(character, out b);
                        PressedKey(b, character);

                        yield return new WaitForSeconds(typeTime);
                    }
                    PressedKey(10, " ");
                    yield return new WaitForSeconds(typeTime);
                }
                else {
                    Debug.Log("Need to go to next line");
                    ResetCarriage();
                    a--;
                    yield return new WaitForSeconds(carriageReturnTime);
                }
            }
            else {
                 Debug.Log("Found '/return'.");
                ResetCarriage();
                yield return new WaitForSeconds(carriageReturnTime);
            }
        }
    }

    public void SetTargetWord(string word) {
        m_targetWordLetters.Clear();
        char[] letters = word.ToCharArray();
        Debug.Log("Number of letters in "+word+": "+letters.Length);
        foreach( char s in letters) {
            m_targetWordLetters.Add(""+s);
        }
        Debug.Log(m_targetWordLetters.Count);
        HighlightKeys();
    }

    public void ValidateTypedWord(int index) {
        if(!m_isTypingAutonomously) {
            bool correctWord = true;
            for(int l = 0; l < m_typedWord.Length; l++) {
                if(m_typedWord[l].ToString() != m_targetWord[l].ToString()) {
                    correctWord = false;
                    SetTargetWord(m_targetWord);
                    HighlightKeys();
                    m_typedWord ="";
                    Debug.Log("Wrong key pressed");
                    return;
                }
                else if (m_typedWord[l].ToString() == m_targetWord[l].ToString()){
                    Debug.Log("Make it blue!");
                    // need to disable the highlight on the key
                    m_typewriterKeys[index].GetComponent<TypeWriterKeyBehaviour>().HighlightKey(Color.blue);
                    Debug.Log(l);
                    if(l == m_targetWord.Length-1) {
                        Debug.Log("Word was spelled correctly!");
                        UnHighlightKeys();
                        // trigger event when the right word was spelled
                        // talk to the game event manager?
                    }
                }
            }
        }
    }

    public void RollToNextLine() { // May want to find a nicer name
        Debug.Log("Rolled to next line.");
        // Rotate roller knob
        // play rotation sound
        // move piece of paper
        // indicate if we reached the end of the paper
    }
}