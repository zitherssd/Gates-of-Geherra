using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ModifyFloats(ref float field1, ref float field2, ref float field3)
    {
        System.Random random = new System.Random();
        int randomIndex1 = random.Next(3); // Random index between 0 and 2
        int randomIndex2;

        do
        {
            randomIndex2 = random.Next(3); // Ensure randomIndex2 is different from randomIndex1
        }
        while (randomIndex2 == randomIndex1);

        if (randomIndex1 == 0)
        {
            field1 *= 1.2f;
            field2 *= 0.9f;
        }
        else if (randomIndex1 == 1)
        {
            field2 *= 1.2f;
            field3 *= 0.9f;
        }
        else
        {
            field3 *= 1.2f;
            field1 *= 0.9f;
        }
    }
}
