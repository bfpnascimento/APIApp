using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireworkRandomizer : MonoBehaviour
{
    // array of all available colors for the particles
    public Color[] particleColors;

    // particle system of the firework
    private ParticleSystem particle;

    // Start is called before the first frame update
    void Start ()
    {
        // get the particle system component
        particle = GetComponent<ParticleSystem>();

        // call the 'SetRandomColors' everytime the particle emits
        InvokeRepeating("SetRandomColors", 0.0f, particle.main.duration);
    }

    // sets the particle to be a random color gradient
    public void SetRandomColors ()
    {
        // create a list to keep track of colors we've got left to use
        List<Color> availableColors = particleColors.ToList();
        Color[] colors = new Color[2];
        
        for(int x = 0; x < colors.Length; ++x)
        {
            // get a random color
            colors[x] = availableColors[Random.Range(0, availableColors.Count)];
            availableColors.Remove(colors[x]);
        }

        // get the particle's main module and set the start color
        ParticleSystem.MainModule main = particle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(colors[0], colors[1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
