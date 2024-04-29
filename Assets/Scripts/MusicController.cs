using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [SerializeField]
    GameObject square1;

    AudioSource m_MyAudioSource;

    int qSamples = 1024;
    float refValue = 0.1f;
    float threshold = 0.02f;
    float rmsValue;
    float dbValue;
    float pitchValue;

    private float[] samples;
    private float[] spectrum;
    private float fSample;
    void Start()
    {
        samples = new float[qSamples];
        spectrum = new float[qSamples];
        fSample = AudioSettings.outputSampleRate;


        m_MyAudioSource = GetComponent<AudioSource>();
    }

    void AnalyzeSound() 
    {
        m_MyAudioSource.GetOutputData(samples, 0);
        int i;
        float sum = 0;
        for (i = 0; i < qSamples; i++)
        {
            sum += samples[i] * samples[i];
        }

        rmsValue = Mathf.Sqrt(sum / qSamples);
        dbValue = 20 * Mathf.Log10(rmsValue / refValue);
        if (dbValue < -160) dbValue = -160;

        m_MyAudioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        float maxV = 0;
        int maxN = 0;

        for (i = 0; i < qSamples; i++)
        {
            if (spectrum[i] > maxV && spectrum[i] > threshold)
            {
                maxV = spectrum[i];
                maxN = i;
            }
        }

        float freqN = maxN;
        if (maxN > 0 && maxN < qSamples - 1)
        {
            float dL = spectrum[maxN - 1] / spectrum[maxN];
            float dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += (float)(0.5 * (dR * dR - dL * dL));
        }
        pitchValue = freqN * (fSample / 2) / qSamples;
    }
    void Update()
    {
        AnalyzeSound();
        float xpos = square1.GetComponent<Rigidbody2D>().position.x;
        square1.GetComponent<Rigidbody2D>().position = new Vector2(xpos, 2 * (dbValue - 10));
        if (Mathf.Round(pitchValue) % 10 == 0)
        {
            Instantiate(Resources.Load("Projectile"), square1.transform.position, Quaternion.identity);
        }

    }
}
