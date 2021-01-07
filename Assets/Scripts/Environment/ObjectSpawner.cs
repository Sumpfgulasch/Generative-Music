using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // public
    [Header("Objects")]
    public static ObjectSpawner instance;
    public List<GameObject> objects;
    public float zSpawn = 10f;
    public float moveSpeed = 1f;
    public float spawnRhythm = 5f;
    public int objectCount = 3;

    // private
    private List<GameObject> activeObjects;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        StartCoroutine(SpawnObject());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnObject()
    {
        activeObjects = new List<GameObject>();

        // initial instatiations
        for (int i = 0; i<objectCount-1; i++)
        {
            GameObject newObj = objects[Random.Range(0, objects.Count)];
            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + i * moveSpeed * spawnRhythm * 120), Quaternion.identity);
            newObj.AddComponent<Move>();
            activeObjects.Add(newObj);
        }
        
        while (true)
        {
            // INSTANTIATE new objects
            GameObject newObj = objects[Random.Range(0, objects.Count)];
            newObj = Instantiate(newObj, new Vector3(0, 0, zSpawn + objectCount * moveSpeed * spawnRhythm * 120), Quaternion.identity);
            newObj.AddComponent<Move>();
            activeObjects.Add(newObj);

            // delete when not visible anymore
            List<GameObject> objects2destroy = new List<GameObject>();
            foreach(GameObject obj in activeObjects)
            {
                if (obj.transform.position.z < -20)
                    objects2destroy.Add(obj);
            }
            for(int i=0;i<objects2destroy.Count; i++)
            {
                activeObjects.Remove(objects2destroy[i]);
                Destroy(objects2destroy[i]);
                
            }
            yield return new WaitForSeconds(spawnRhythm);
        }
        
    }
}
