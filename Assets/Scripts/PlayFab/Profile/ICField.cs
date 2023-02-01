using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class IC
{
    public string Property;
    public string Identification;
    public IC(string property, string identification)
    {
        Property = property;
        Identification = identification;
    }
}
public class ICField : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI propertyName, identificationName;
    [SerializeField]
    private TMP_InputField identificationField;

    public IC ReturnClass()
    {
        return new IC(propertyName.text, identificationField.text);
    }

    public void SetUI(IC ic)
    {
        identificationField.text = ic.Identification;
        identificationName.text = ic.Identification;
        propertyName.text = ic.Property;
    }
}
