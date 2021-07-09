using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectsController))]
public class SpriteEffect : MonoBehaviour
{
    public List<Sprite> sprites;

    [Tooltip("The amount of seconds between each sprite change")]
    public float spriteChangeInterval = 0.25f;
    public AudioClip audioClip;
    public bool loop = false;
    public float pitchModifier = 0.1f;

    int _index = 0;
    float _timeSinceLastSpriteChange;
    EffectsController _effectsController;

    // Start is called before the first frame update
    void Start()
    {
        _effectsController = GetComponent<EffectsController>();
        _effectsController.PlayAudioClip(audioClip, 1 + (Random.value - 0.5f) * pitchModifier);
    }

    // Update is called once per frame
    void Update()
    {
        if(sprites == null)
            return;


        if(_timeSinceLastSpriteChange >= spriteChangeInterval)
        {

            if(++_index >= sprites.Count)
            {
                if(loop)
                {
                    _index = 0;
                    _effectsController.PlayAudioClip(audioClip, 1 + (0.5f - Random.value) * pitchModifier);    
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            _timeSinceLastSpriteChange = 0;
            _effectsController.ChangeSpriteTo(sprites[_index]);
        }

        _timeSinceLastSpriteChange += Time.deltaTime;
    }
}
