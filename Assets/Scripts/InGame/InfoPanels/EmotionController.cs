using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmotionController", menuName = "Dialogue System/EmotionController")]
public class EmotionController : ScriptableObject
{
    public List<Sprite> emotions;
}
