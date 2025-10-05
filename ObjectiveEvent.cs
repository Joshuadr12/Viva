using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectiveEvent : MonoBehaviour
{
    [Header("Basics")] public bool increaseCount = true;
    public List<GameObject> enableObjects, disableObjects;
    [Header("SpaceSpam")] public bool spaceSpam = false;
    public Vector3 playerPos;
    public float playerStrength = 0.05f, resistance;
    public Sprite face;
    [Tooltip("Set to true for special use in Level2; leave false otherwise.")] public bool fish;
    [Tooltip("When set to true, the player character will rotate randomly with each button press.")] public bool spin = true;
    [Tooltip("The range within which the player character randomly relocates with each button press")] public Vector2 shakeRange;
    [Tooltip("The audio clip to play with each button press")] public AudioClip spamSound;
    [Tooltip("The list of objects to activate one at a time with each button press.")] public List<GameObject> buildList;
    public ObjectiveEvent onSuccess;
    [Header("Miscellaneous"), Tooltip("Set to true for special use in Level3; leave false otherwise.")] public bool suspense;
    public AudioClip sound;
    public List<Tilemap> darkenTiles;
    public string goToScene;
    public bool fade;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}