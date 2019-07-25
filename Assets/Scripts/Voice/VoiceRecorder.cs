using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceRecorder : MonoBehaviour
{
    public AudioClip recording;
    
    int bufferLength;
    int sampleFrequency;
    int lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            Microphone.GetDeviceCaps(device, out int minFreq, out int maxFreq);
            Debug.Log($"Device found - {device}, [{minFreq}Hz, {maxFreq}Hz]");
        }

        bufferLength = 5;
        sampleFrequency = 44100;

        lastPosition = 0;
    }

    public void StartRecording()
    {
        recording = Microphone.Start(null, true, bufferLength, sampleFrequency);
        StartCoroutine(RetainAudioDataAsync());
    }

    public void StopRecording()
    {
        Debug.Log("Stop Recording called");
        Microphone.End(null);
    }

    public void SaveRecording()
    {
        WavUtility.FromAudioClip(recording, out var path);
        Debug.Log($"Recording saved to {path}");
    }

    private void RetainAudioData(System.IO.BinaryWriter writer)
    {
        // move data from circular audio buffer into list of buffers
        var recordingPosition = Microphone.GetPosition(null);
        var samples = lastPosition <= recordingPosition ? recordingPosition - lastPosition : recordingPosition + (recording.samples - lastPosition);
        if (samples == 0) return;

        var buffer = new float[(samples - 1) * recording.channels];
        recording.GetData(buffer, lastPosition);
        lastPosition = recordingPosition;

        foreach (float f in buffer)
            writer.Write(f);
    }

    private IEnumerator RetainAudioDataAsync()
    {
        using (var stream = new System.IO.MemoryStream())
        using (var writer = new System.IO.BinaryWriter(stream))
        {
            while (Microphone.IsRecording(null))
            {
                RetainAudioData(writer);
                yield return null;
            }

            // Clean up, do one last retain
            RetainAudioData(writer);

            // Create stitched audio file
            var samples = stream.Length / sizeof(float);
            var data = new float[samples];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            using (var reader = new System.IO.BinaryReader(stream))
            {
                for (int i = 0; i < samples; ++i)
                {
                    data[i] = reader.ReadSingle();
                }
            }

            recording = AudioClip.Create("Complete recording", (int)samples, 1, sampleFrequency, false);
            recording.SetData(data, 0);

            Debug.Log("Recording finished");
        }

    }
}
