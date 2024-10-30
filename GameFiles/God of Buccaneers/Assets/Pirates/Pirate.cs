using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pirate : InteractableBase
{
    // Start is called before the first frame update
    protected override void Start()
    {
        LayerSecurity();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        Debug.Log("Je suis spécial");
    }
}
