using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Manager: MonoBehaviour
{
    // GameObjects and Components that we get from the scene.
    public GameObject objects, sounds;
    public TMP_Dropdown dropdownObjects;
    public TMP_Dropdown dropdownSound;
    public Scrollbar sensitivityScrollbar;

    // The audiosource selected.
    private AudioSource sound;

    // Sensitivity of the scale.
    private float sensitivy = 0.5f;

    // Scale of the selected GameObject.
    [HideInInspector] public float scale;

    // Variables regarding the rotation of the selected GameObject.
    private float speed = 10;
    private float rotationValue;
    private Transform selectedObjectTransform;

    // Variables regarding sound from the microphone and 
    // changing the selected object's scale with it.
    private int numSamples = 44100;
    private int seconds = 1;
    private float timer = 0;

    // The selected sound source.
    private int soundValue = 0;

    // Singleton of the Manager Class.
    private static Manager instance = null;
    public static Manager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Manager>();
            return instance;
        }

        set => instance = value;
    }

    void Start()
    {
        selectedObjectTransform = objects.transform.GetChild(dropdownObjects.value);
        selectedObjectTransform.gameObject.SetActive(true);

        sound = sounds.transform.GetChild(0).GetComponent<AudioSource>();
        sound.clip = Microphone.Start(null, true, seconds, numSamples);

        StartCoroutine(GetSoundFile());
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            rotationValue = speed * Input.GetAxis("Mouse X");
            selectedObjectTransform.Rotate(0, rotationValue, 0);
        }

        if (soundValue == 0)
        {
            UpdateObjectScaleFromMicrophone();
        }

        if (scale < 0.1)
            scale = 0.1f;

        selectedObjectTransform.localScale = new Vector3(sensitivy + scale, sensitivy + scale, sensitivy + scale);
    }

    // Get sound file from disk and assign it to the corresponding audioclip.
    private IEnumerator GetSoundFile()
    {
        string soundDirectory = Environment.CurrentDirectory;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(soundDirectory + "\\sound.wav", AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                sounds.transform.GetChild(1).GetComponent<AudioSource>().clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }

    // Use microphone's data in order to update the scale factor for updating the object's scale.
    private void UpdateObjectScaleFromMicrophone()
    {
        timer += Time.deltaTime;
        if (timer > seconds)
            timer = 0;

        float[] data = new float[(int)Math.Floor(Time.deltaTime * numSamples)];
        sound.clip.GetData(data, (int)Math.Floor(timer * numSamples));

        float sum = 0;
        for (int i = 0; i < data.Length; i++)
            sum += data[i];
        scale = 10 * Math.Abs(sum / data.Length);
    }

    // Select object.
    public void OnObjectSelected() 
    {
        selectedObjectTransform.gameObject.SetActive(false);
        selectedObjectTransform = objects.transform.GetChild(dropdownObjects.value);
        selectedObjectTransform.gameObject.SetActive(true);
    }

    // Select sound source.
    public void OnSoundSelected()
    {
        soundValue = dropdownSound.value;

        sound.Pause();

        sound = sounds.transform.GetChild(soundValue).GetComponent<AudioSource>();

        if (soundValue == 1)
        {
            Microphone.End(null);
            sound.Play();
        }
        else
        {           
            sound.clip = Microphone.Start(null, true, seconds, numSamples);
            timer = 0;
        }
    }

    // Change sensitivity.
    public void OnSensitivityChanged()
    {
        sensitivy = sensitivityScrollbar.value;
    }
}
