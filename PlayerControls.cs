using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerControls : MonoBehaviour
{
    public static int level = -1;

    [Header("Controls"), SerializeField] string startAnimation;
    [SerializeField] float speed = 3000;
    [SerializeField] float maxVelocity = 30;
    [SerializeField] float jumpForce = 1200;

    [Header("Level"), SerializeField] Image fadePanel;
    [SerializeField] bool fadeIn = true;
    [SerializeField] AudioClip music;
    [SerializeField] int objectives = 1;
    [SerializeField] string objectiveItem;
    [SerializeField, Tooltip("The time in seconds before the player must restart the level")] float time = 60;
    [SerializeField, Tooltip("The objective event to trigger when the objective count is met")] ObjectiveEvent eventOnCount;
    [SerializeField] string nextLevel;
    [SerializeField, Tooltip("Add events for special use in Level3; leave empty otherwise.")] List<ObjectiveEvent> animationEvents;
    [SerializeField, Tooltip("Use for special use in Level3; leave null otherwise.")] RuntimeAnimatorController suspenseAnimation;
    [SerializeField, Tooltip("The event to trigger after the start animation")] ObjectiveEvent endAnimation;
    [SerializeField, Tooltip("The event to trigger when the start animation is skipped")] ObjectiveEvent skipAnimation;

    [Header("Space Spam"), SerializeField] GameObject spaceSpamUI;
    [SerializeField] Slider spaceSpamSlider;
    [SerializeField] Transform body;
    [SerializeField] Sprite defaultFace;

    [Header("Miscellaneous"), SerializeField] TMP_Text objectiveText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] Sprite dieFace;
    [SerializeField] AudioClip jumpSound, dieSound, timeUpSound;

    Rigidbody2D rigidbody;
    Animator animator;
    float moveAxis;
    [HideInInspector] public float shake;
    int collisions = 0;
    bool levelComplete = false, enableControls = false, stopTimer = false;
    float fade = 0;
    int objectiveCount = 0;
    bool objectiveMet = false;
    ObjectiveEvent spaceSpam = null;
    float spaceSpamValue = 0;
    int timesPressed = 0;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        if (fadeIn) { fade = 1; }
        MusicSync.sync.audio.mute = false;
        MusicSync.sync.PlayMusic(music);

        //Check for start animation; only plays per level
        animator = GetComponent<Animator>();
        if (level != SceneManager.GetActiveScene().buildIndex)
        {
            if (startAnimation != "" && level < SceneManager.GetActiveScene().buildIndex) { animator.SetTrigger(startAnimation); }
            level = SceneManager.GetActiveScene().buildIndex;
        }
        else
        {
            if (skipAnimation) { TriggerEvent(skipAnimation); }
            enableControls = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        spaceSpamUI.SetActive(spaceSpam != null);
        objectiveText.gameObject.SetActive(enableControls && !stopTimer);
        timeText.gameObject.SetActive(enableControls && !stopTimer);

        //When not in space spam mode
        if (spaceSpam == null)
        {
            if (enableControls)
            {
                //Movement
                if (Input.GetButton("Horizontal"))
                {
                    moveAxis = Mathf.Sign(Input.GetAxis("Horizontal"));
                    rigidbody.AddForce(Vector2.right * moveAxis * speed * Time.deltaTime);
                    transform.localScale = new Vector2(moveAxis, 1);
                    if (Mathf.Abs(rigidbody.velocity.x) > maxVelocity) { rigidbody.velocity = new Vector2(Mathf.Clamp(rigidbody.velocity.x, -maxVelocity, maxVelocity), rigidbody.velocity.y); } //Prevents phasing through the ground/walls
                }

                //Jumping
                if (Input.GetButtonDown("Jump") && collisions > 0 && Mathf.Abs(rigidbody.velocity.y) <= 0.01f)
                {
                    rigidbody.AddForce(Vector2.up * jumpForce);
                    MusicSync.sync.PlaySound(jumpSound);
                }
                if (rigidbody.velocity.y < -maxVelocity) { rigidbody.velocity = new Vector2(rigidbody.velocity.x, -maxVelocity); }

                if (Input.GetKeyDown(KeyCode.Escape)) { SceneManager.LoadScene("Title"); }
            }
            body.position = transform.position + Vector3.right * Random.Range(-1f, 1f) * shake;
        }

        //When in space spam mode
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                spaceSpamValue += spaceSpam.playerStrength;
                if (timesPressed < spaceSpam.buildList.Count) { spaceSpam.buildList[timesPressed].SetActive(true); }
                if (spaceSpam.spamSound) { MusicSync.sync.PlaySound(spaceSpam.spamSound); }
                body.position = (Vector2)transform.position + new Vector2(Random.Range(-1f, 1f) * spaceSpam.shakeRange.x, Random.value * spaceSpam.shakeRange.y);
                if (spaceSpam.spin) { Spin(); }
                timesPressed++;
            }
            if (timesPressed > 0) { spaceSpamValue = Mathf.Max(spaceSpamValue - spaceSpam.resistance * Time.deltaTime, 0); }
            spaceSpamSlider.value = spaceSpamValue;
            if (spaceSpamValue >= 1) { StopSpaceSpam(); }
        }

        //When the level is completed
        if (levelComplete)
        {
            enableControls = false;
            fade += Time.deltaTime * 2;
            fadePanel.gameObject.SetActive(true);
            if (fade >= 1) { SceneManager.LoadScene(nextLevel); }
        }
        else
        {
            fade = Mathf.Max(fade - Time.deltaTime * 2, 0);
            if (fade <= 0) { fadePanel.gameObject.SetActive(false); }
            if (enableControls && !stopTimer) { time -= Time.deltaTime; }
            if (time <= 0 && enableControls) { Die(timeUpSound); }
        }
        fadePanel.color = Color.Lerp(Color.clear, Color.black, fade);

        //Objective update
        if (objectiveCount >= objectives && !objectiveMet)
        {
            TriggerEvent(eventOnCount);
            objectiveMet = true;
        }

        //Text update
        objectiveText.text = objectiveItem + ": " + objectiveCount.ToString() + "/" + objectives.ToString();
        timeText.text = FormatTime(time);

        //Animation
        animator.SetBool("jumping", collisions <= 0 || Mathf.Abs(rigidbody.velocity.y) > 0.01f);

        //Check party music
        if (SceneManager.GetActiveScene().name == "Party Ending" && MusicSync.sync.audio.time < 23.75f) { MusicSync.sync.audio.time = 23.75f; }

        //Check boundaries
        if (transform.position.y <= -80) { Restart(); }
    }

    string FormatTime(float totalSeconds)
    {
        string result = "";
        int minutes = Mathf.FloorToInt(Mathf.Max(totalSeconds / 60, 0));
        if (minutes < 10) { result += "0"; }
        result += minutes.ToString() + ":";

        int seconds = Mathf.FloorToInt(Mathf.Max(totalSeconds % 60, 0));
        if (seconds < 10) { result += "0"; }
        result += seconds.ToString();
        return result;
    }

    public void TriggerEvent(ObjectiveEvent e)
    {
        if (e.increaseCount) { objectiveCount++; }
        foreach (GameObject obj in e.enableObjects) { obj.SetActive(true); }
        foreach (GameObject obj in e.disableObjects) { obj.SetActive(false); }
        foreach (Tilemap tile in e.darkenTiles) { tile.color = new Color(0.25f, 0.25f, 0.25f); }

        if (e.suspense)
        {
            MusicSync.sync.audio.mute = true;
            stopTimer = true;
            foreach (Animator zombie in FindObjectsOfType<Animator>())
            {
                if (zombie.runtimeAnimatorController == suspenseAnimation) { zombie.SetTrigger("suspense"); }
            }
        }

        if (e.sound) { MusicSync.sync.PlaySound(e.sound); }
        if (e.fade) { levelComplete = true; }
        if (e.goToScene != "") { SceneManager.LoadScene(e.goToScene); }

        if (e.spaceSpam)
        {
            spaceSpam = e;
            transform.position = e.playerPos;
            if (e.face) { body.GetChild(0).GetComponent<SpriteRenderer>().sprite = e.face; }
            if (e.fish) { body.GetChild(0).GetChild(0).gameObject.SetActive(true); }
            rigidbody.velocity = Vector2.zero;
            spaceSpamValue = 0;
            timesPressed = 0;
        }
        else { Destroy(e.gameObject); }
    }

    public void StopSpaceSpam(bool success = true)
    {
        body.position = transform.position;
        body.rotation = Quaternion.identity;
        body.GetChild(0).rotation = Quaternion.identity;
        body.localScale = Vector3.one;
        body.GetChild(0).GetComponent<SpriteRenderer>().sprite = defaultFace;
        body.GetChild(0).GetChild(0).gameObject.SetActive(false);
        if (spaceSpam.onSuccess != null && success) { TriggerEvent(spaceSpam.onSuccess); }
        Destroy(spaceSpam.gameObject);
        spaceSpam = null;
    }

    public void Spin()
    {
        body.Rotate(Vector3.forward * Random.value * 360);
        body.GetChild(0).Rotate(Vector3.forward * Random.value * 360);
        body.localScale = new Vector2(Mathf.Sign(Random.Range(-1f, 1f)), Mathf.Sign(Random.Range(-1f, 1f)));
    }

    public void Die(AudioClip sound)
    {
        body.GetChild(0).GetComponent<SpriteRenderer>().sprite = dieFace;
        MusicSync.sync.PlaySound(sound);
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, maxVelocity * 2);
        animator.SetTrigger("Die");
        if (spaceSpam) { StopSpaceSpam(false); }
        enableControls = false;
        Invoke("Restart", 2);
    }

    public void ChangeExpression(Sprite face) { body.GetChild(0).GetComponent<SpriteRenderer>().sprite = face; }
    public void PlaySound(AudioClip sound) { MusicSync.sync.PlaySound(sound); }
    public void TriggerAnimationEvent(int index) { TriggerEvent(animationEvents[index]); }
    public void Force(float force)
    {
        transform.localScale = new Vector2(Mathf.Sign(force), 1);
        if (force < 20) { rigidbody.velocity = new Vector2(force, 20); }
        else { rigidbody.velocity = Vector2.right * force; }
    }
    public void Sleep()
    {
        transform.position = new Vector2(-3.925f, -0.1f);
        transform.localScale = new Vector2(-1, 1);
        rigidbody.bodyType = RigidbodyType2D.Static;
        for (int i = 1; i < body.childCount; i++) { body.GetChild(i).gameObject.SetActive(false); }
    }
    public void Wake()
    {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        for (int i = 1; i < body.childCount; i++) { body.GetChild(i).gameObject.SetActive(true); }
    }
    public void StartPlaying()
    {
        enableControls = true;
        TriggerEvent(endAnimation);
    }
    public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                collisions++;
                break;
            case "Respawn":
                Die(dieSound);
                break;
            default: break;
        }

        ObjectiveEvent objective = collision.collider.GetComponent<ObjectiveEvent>();
        if (objective != null) { TriggerEvent(objective); }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                collisions--;
                break;
            default: break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ObjectiveEvent objective = collision.GetComponent<ObjectiveEvent>();
        if (objective != null) { TriggerEvent(objective); }
    }
}