using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Test_SendBug : MonoBehaviour
{

    public static Test_SendBug Inst;

    private int index;

    public Text text;

    public Button btn;

    private SendEamilSystem _sendEamilSystem;
    private void Awake()
    {
        Inst = this;
        _sendEamilSystem = new SendEamilSystem();
        _sendEamilSystem.Init();
    }

    private void Start()
    {
        
        
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            index++;
           
            text.text = index.ToString();
            Debug.LogError("错误日志  index=="+index);
        });
    }

    private void Update()
    {
        _sendEamilSystem.UpdateData();
    }
}