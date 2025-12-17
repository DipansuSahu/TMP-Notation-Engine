using AbS;
using TMPro;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] private string data;
    [SerializeField] private TMP_Text text1, text2;

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Testing();
        }
    }

    private void Testing()
    {
        text1.text = data.ToFormatTMP();

        text2.text = text1.text.ToUnicodeTMP();
    }
}