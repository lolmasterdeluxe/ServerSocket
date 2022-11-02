using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Skill
{
    public string name;
    public int level;
    public Skill(string name, int level)
    {
        this.name = name;
        this.level = level;
    }
}

public class SkillBox : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField skillName;
    [SerializeField]
    private Slider skillLevelSlider;
    [SerializeField]
    private TextMeshProUGUI skillLevelText;
    
    public Skill ReturnClass()
    {
        return new Skill(skillName.text, (int)skillLevelSlider.value);
    }

    public void SetUI(Skill sk)
    {
        skillName.text = sk.name;
        skillLevelSlider.value = sk.level;
    }

    public void SliderChangeUpdate(float num)
    {
        skillLevelText.text = skillLevelSlider.value.ToString();
    }
}
