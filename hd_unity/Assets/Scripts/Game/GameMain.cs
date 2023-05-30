using System.Collections;
using System.Collections.Generic;
using System.Resources;
using FrameWork.Asset;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var gameMain = ResourcesManager.Instance.LoadResource<GameObject>("Prefabs/UI/GameMain",true);

        Instantiate(gameMain);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
