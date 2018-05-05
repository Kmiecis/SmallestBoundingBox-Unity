using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Enums;

public class SBBModel : MonoBehaviour
{
    public Text countText;
    public InputField countInputField;
    public Slider countInputSlider;
    public Button switchButton;
    public Text switchText;
    public Dropdown dataCloudTypeDropdown;
    public Button recreateButton;
    public SBBEngine engine;

    EnvironmentSpecs environment;


    private void Awake()
    {
        environment = new EnvironmentSpecs();

        fillDataCloudTypeDropdown(Enum.GetNames(environment.dataCloud3Type.GetType()));
    }


    private void Start()
    {
        setCountText();

        switchButtonAction = onSwitchButtonClick;
        switchButton.onClick.RemoveAllListeners();
        switchButton.onClick.AddListener(switchButtonAction);

        recreateButtonAction = onRecreateButtonClick;
        recreateButton.onClick.RemoveAllListeners();
        recreateButton.onClick.AddListener(recreateButtonAction);

        countInputSliderAction = onCountInputSliderChange;
        countInputSlider.onValueChanged.RemoveAllListeners();
        countInputSlider.onValueChanged.AddListener(countInputSliderAction);

        dropdownAction = onDropdownInputChange;
        dataCloudTypeDropdown.onValueChanged.RemoveAllListeners();
        dataCloudTypeDropdown.onValueChanged.AddListener(dropdownAction);

        countInputFieldAction = onCountInputFieldEndEdit;
        countInputField.onEndEdit.RemoveAllListeners();
        countInputField.onEndEdit.AddListener(countInputFieldAction);
    }


    private const int defaultCount = 20;
    private const int minCount = 4;
    private const int maxCount = 10000;
    void setCountText()
    {
        countText.text = string.Format("Count: [{0} - {1}]", minCount, maxCount);
        countInputField.text = defaultCount.ToString();
    }

    UnityAction switchButtonAction;
    void onSwitchButtonClick()
    {
        switch (environment.dataDimension)
        {
            default:
            case DataDimension._2D:
                {   // Switch to 3D
                    switchText.text = "Switch to 2D";
                    environment.dataDimension = DataDimension._3D;
                    fillDataCloudTypeDropdown(Enum.GetNames(typeof(DataCloud2Type)));
                    break;
                }
            case DataDimension._3D:
                {   // Switch to 2D
                    switchText.text = "Switch to 3D";
                    environment.dataDimension = DataDimension._2D;
                    fillDataCloudTypeDropdown(Enum.GetNames(typeof(DataCloud3Type)));
                    break;
                }
        }

        dataCloudTypeDropdown.value = 0;
        onDropdownInputChange(0);
    }


    void fillDataCloudTypeDropdown(string[] data)
    {
        List<string> options = new List<string>(data);

        dataCloudTypeDropdown.ClearOptions();
        dataCloudTypeDropdown.AddOptions(options);
    }


    UnityAction<string> countInputFieldAction;
    void onCountInputFieldEndEdit(string inputValue)
    {
        int countValue;
        if (int.TryParse(inputValue, out countValue))
        {
            float sliderPercent = Mathf.InverseLerp(minCount, maxCount, countValue);
            countInputSlider.value = sliderPercent;
        }
    }


    UnityAction<float> countInputSliderAction;
    void onCountInputSliderChange(float sliderPercent)
    {
        int countValue = (int)(sliderPercent * (maxCount - minCount)) + minCount;

        countInputField.text = countValue.ToString();
    }


    UnityAction<int> dropdownAction;
    void onDropdownInputChange(int inputValue)
    {
        switch (environment.dataDimension)
        {
            default:
            case DataDimension._2D:
                {   // Switch to 3D
                    environment.dataCloud2Type = (DataCloud2Type)inputValue;
                    break;
                }
            case DataDimension._3D:
                {   // Switch to 2D
                    environment.dataCloud3Type = (DataCloud3Type)inputValue;
                    break;
                }
        }
    }


    UnityAction recreateButtonAction;
    void onRecreateButtonClick()
    {
        int count;
        if (!int.TryParse(countInputField.text, out count))
        {
            count = defaultCount;
        }

        engine.Refresh(count, environment);
    }
}