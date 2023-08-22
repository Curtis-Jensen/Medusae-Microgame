using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateByRegion : ProceduralGeneration
{
    /*  🧨 For each spawn point, delete any existing walls and spawn a new object if the random value is below the prefab chance

            💥 Delete existing walls from spawn points immediately so it can be used in the inspector

            🧱 Spawn new prefab with the spawnpoint's position, the generated rotation, and with the spawn point as a parent

            🎉 Instantiate the prefab with the spawn point as its parent

            🎨 Stretch the prefab if the stretchy flag is set
         */
    /// <summary>
    /// Places the objects based off the spawn points that are laid out
    /// </summary>
    /// <param name="spawnChance">The chance of spawning a new object on each spawn point</param>
    protected override void SpawnObjects(float spawnChance)
    {
        Vector3 regionSize = new Vector3(10f, 0f, 10f); // Change this to your desired region size
        Vector3 spawnPointOffset = new Vector3(0.5f, 0f, 0.5f); // Offset to ensure objects are centered

        for (float x = 0; x < regionSize.x; x++)
        {
            for (float z = 0; z < regionSize.z; z++)
            {
                Vector3 spawnPosition = new Vector3(x, 0f, z) + spawnPointOffset;

                // Delete existing objects at the spawn position
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, 0.5f); // Adjust the radius
                foreach (var collider in colliders)
                {
                    if (collider.CompareTag("Procedural"))
                        DestroyImmediate(collider.gameObject);
                }

                if (Random.value >= spawnChance) continue;

                int randomIndex = Random.Range(0, objectPrefabs.Length);
                GameObject chosenObject = objectPrefabs[randomIndex];

                var rotation = Quaternion.Euler(Random.Range(-tiltAngle, tiltAngle),
                                                Random.Range(0, 360),
                                                Random.Range(-tiltAngle, tiltAngle));

                var newObject = Instantiate(chosenObject, spawnPosition, rotation); // No parent here

                if (stretchy)
                    newObject.transform.localScale = new Vector3(Random.Range(1, stretchAmounts),
                                                                 Random.Range(1, stretchAmounts),
                                                                 Random.Range(1, stretchAmounts));
            }
        }
    }
}
