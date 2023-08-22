using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateByRegion : ProceduralGeneration
{
    public Color gizmoColor = Color.blue;
    public Vector3 regionSize = new Vector3(10f, 1f, 10f);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, regionSize);
    }

    /// <summary>
    /// Places the objects based off the spawn points that are laid out
    /// </summary>
    /// <param name="spawnChance">The chance of spawning a new object on each spawn point</param>
    protected override void SpawnObjects(float spawnChance)
    {
        for (int i = transform.childCount - 1; i >= 0; i--) // 💥
        {
            var child = transform.GetChild(i);

            if (child.CompareTag("Procedural"))
                DestroyImmediate(child.gameObject);
        }

        for (float x = 0; x < regionSize.x; x += 10)
        {
            for (float z = 0; z < regionSize.z; z += 10)
            {
                for (float y = 0; y < regionSize.y; y += 10)
                {
                    Vector3 spawnPosition = transform.position - (regionSize/2) + new Vector3(x, y, z);

                    if (Random.value >= spawnChance) continue;

                    int randomIndex = Random.Range(0, objectPrefabs.Length);
                    GameObject chosenObject = objectPrefabs[randomIndex];

                    var rotation = Quaternion.Euler(Random.Range(-tiltAngle, tiltAngle),
                                                    Random.Range(0, 360),
                                                    Random.Range(-tiltAngle, tiltAngle));

                    var newObject = Instantiate(chosenObject, spawnPosition, rotation, transform);

                    if (stretchy)
                        newObject.transform.localScale = new Vector3(Random.Range(1, stretchAmounts),
                                                                     Random.Range(1, stretchAmounts),
                                                                     Random.Range(1, stretchAmounts));
                }
            }
        }
    }
}
