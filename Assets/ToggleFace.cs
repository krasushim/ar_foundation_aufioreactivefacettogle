using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

[RequireComponent(typeof(ARFaceManager))]
public class ToggleFace : MonoBehaviour
{
    [Header("Effect Configuration")]
    [SerializeField] public float vol = 1.0f;
    [SerializeField, Range(0f, 10f)] float m_gain = 10.0f; // 音量に掛ける倍率
    
    AudioClip clip;
    int head = 0;
    const int samplingFrequency = 44100;
    const int lengthSeconds = 1;
    float[] processBuffer = new float[256];
    float[] microphoneBuffer = new float[lengthSeconds * samplingFrequency];

    float m_volumeRate; // 音量(0-1)

    // face toggle
    public GameObject[] faces;

    [SerializeField]
    private ARFaceManager arFaceManager;

    [SerializeField]
    public FaceMaterial[] materials;

    private float timer;

    [SerializeField]
    private float frequency = 1.0f;

    [SerializeField]
    private float maxUntilSpawn = 1.0f;

    private int index = -1;

    void Awake()
    {
        // audio 
        clip = Microphone.Start(null, true, 1, samplingFrequency);
        while (Microphone.GetPosition(null) < 0) { }

        // face
        arFaceManager = GetComponent<ARFaceManager>();
        arFaceManager.facePrefab.GetComponent<MeshRenderer>().material = materials[0].Material;
    }

    void Update()
    {
        // audio
        var position = Microphone.GetPosition(null);
        if (position < 0 || head == position) {
            return;
        }

        clip.GetData(microphoneBuffer, 0);
        while (GetDataLength(microphoneBuffer.Length, head, position) > processBuffer.Length) {
            var remain = microphoneBuffer.Length - head;
            if (remain < processBuffer.Length) {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, remain);
                Array.Copy(microphoneBuffer, 0, processBuffer, remain, processBuffer.Length - remain);
            } else {
                Array.Copy(microphoneBuffer, head, processBuffer, 0, processBuffer.Length);
            }

            head += processBuffer.Length;
            if (head > microphoneBuffer.Length) {
                head -= microphoneBuffer.Length;
            }
        }

        float sum = 0f;
        for (int i = 0; i < processBuffer.Length; ++i)
        {
            sum += Mathf.Abs(processBuffer[i]); // データ（波形）の絶対値を足す
        }
        // データ数で割ったものに倍率をかけて音量とする
        m_volumeRate = Mathf.Clamp01(sum * m_gain / (float)processBuffer.Length);

        vol = m_volumeRate;


        // face
        if(vol > 0.4){

            if(index + 1 == materials.Length)
            {
                index = -1;
            }

            index++;

            foreach(ARFace face in arFaceManager.trackables)
            {
                face.GetComponent<MeshRenderer>().material = materials[index].Material;
            }
        }

    }

    static int GetDataLength(int bufferLength, int head, int tail) {
        if (head < tail) {
            return tail - head;
        } else {
            return bufferLength - head + tail;
        }
    }
}


[System.Serializable]
public class FaceMaterial
{
    public Material Material;
    public string Name;
}